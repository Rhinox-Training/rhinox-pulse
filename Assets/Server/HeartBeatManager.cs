using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using UnityEngine;

namespace Rhinox.Pulse
{
    [ServiceLoader(10)]
    public class HeartBeatManager : AutoService<HeartBeatManager>
    {
        //TODO: this should become a variable
        const int HEARTBEAT_TIMEOUT = 10;

        private HttpListener _listener = new HttpListener();
        private Thread _listenerThread;
        private Dictionary<ServerInfo, int> _availableServers = new Dictionary<ServerInfo, int>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (_availableServers == null)
                _availableServers = new Dictionary<ServerInfo, int>();
            Init(HeartBeatHubConfig.Instance.GetHostUri());
        }

        private void Init(Uri hostAddress)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(hostAddress.ToString());
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.Start();

            _listenerThread = new Thread(StartListener);
            _listenerThread.Start();
            PLog.Debug<PulseLogger>("Started listener");
        }

        protected override void Update()
        {
            base.Update();
            
            CleanupHeartBeats();
            LogHeartBeats();
        }

        private void CleanupHeartBeats()
        {
            foreach (var entry in _availableServers.Keys.ToArray())
            {
                _availableServers[entry] = _availableServers[entry] + 1; // Increment time
                if (_availableServers[entry] >= HEARTBEAT_TIMEOUT)
                    _availableServers.Remove(entry);
            }
        }

        private void LogHeartBeats()
        {
            PLog.Debug<PulseLogger>($"Server heartbeat timeout = {HEARTBEAT_TIMEOUT}");
            foreach (var entry in _availableServers)
                PLog.Debug<PulseLogger>($"Server: {entry.Key}\t|\tlast heartbeat: {entry.Value} seconds");
        }


        private void StartListener()
        {
            while (true)
            {
                var result = _listener.BeginGetContext(ListenerCallback, _listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            HttpListener listenerResult = (HttpListener) result.AsyncState;

            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listenerResult.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            switch (context.Request.HttpMethod)
            {
                case "GET":
                {
                    var jsonResponse = JsonHelper.ToJson(_availableServers.Keys.ToArray());
                    MakeResponse(response, System.Text.Encoding.UTF8.GetBytes(jsonResponse), 200);
                    break;
                }
                case "POST":
                {
                    string text;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        text = reader.ReadToEnd();
                        ServerInfo info = JsonUtility.FromJson<ServerInfo>(text);
                        if (_availableServers.ContainsKey(info))
                            _availableServers[info] = 0;
                        else
                            _availableServers.Add(info, 0);
                    }
                    
                    string responseString = "";
                    MakeResponse(response, System.Text.Encoding.UTF8.GetBytes(responseString), 200);
                    break;
                }
            }
        }

        private void MakeResponse(HttpListenerResponse response, byte[] buffer, int statusCode = 200)
        {
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            response.StatusCode = statusCode;
        }
    }
}