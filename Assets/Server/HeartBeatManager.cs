using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        private HttpListener _listener = new HttpListener();
        private Thread _listenerThread;
        private Dictionary<ServerInfo, float> _availableServers = new Dictionary<ServerInfo, float>();
        private Dictionary<string, float> _deceasedServers = new Dictionary<string, float>();

        private float _timePassed = 0f;
        private float _timeOut = 10f;
        private float _updateTime = 1f;
        private float _deceasedTimeOut = 120f;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _timeOut = HeartBeatHubConfig.Instance.HeartBeatTimeOut;
            _updateTime = HeartBeatHubConfig.Instance.UpdateTime;
            _deceasedTimeOut = HeartBeatHubConfig.Instance.DeceasedTimeOut;

            if (_availableServers == null)
                _availableServers = new Dictionary<ServerInfo, float>();
            Init(HeartBeatHubConfig.Instance.GetHostUri());
        }

        private void Init(Uri hostAddress)
        {
            if (hostAddress == null)
            {
                PLog.Error<PulseLogger>("hostAddress was NULL, config file settings were INVALID!");
                return;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add(hostAddress.ToString());
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.Start();

            _listenerThread = new Thread(StartListener);
            _listenerThread.Start();

            PLog.Info<PulseLogger>("Started listener");
            PLog.Info<PulseLogger>("\n======================");
            PLog.Info<PulseLogger>("|| Started listener ||");
            PLog.Info<PulseLogger>("======================\n\n");
        }

        protected override void Update()
        {
            base.Update();

            _timePassed += Time.deltaTime;
            if (_timePassed >= _updateTime)
            {
                _timePassed -= _updateTime;
                Console.Clear();
                // Console.
                CleanupHeartBeats();
                LogHeartBeats();
            }
        }

        private void CleanupHeartBeats()
        {
            if (_availableServers.Count == 0) return;

            foreach (var entry in _availableServers.Keys.ToArray())
            {
                _availableServers[entry] += _updateTime; // Increment time
                if (_availableServers[entry] >= _timeOut)
                {
                    _deceasedServers.Add($"{entry.IP}:{entry.Port.ToString()}", 0f);
                    _availableServers.Remove(entry);
                }
            }

            if (_deceasedServers.Count == 0) return;

            foreach (var deceasedServer in _deceasedServers.Keys.ToArray())
            {
                _deceasedServers[deceasedServer] += _updateTime; // Increment time
                if (_deceasedServers[deceasedServer] >= _deceasedTimeOut)
                {
                    _deceasedServers.Remove(deceasedServer);
                }
            }
        }

        private void LogHeartBeats()
        {
            PLog.Info<PulseLogger>($"\nLive Servers: {_availableServers.Count.ToString()}");

            foreach (var entry in _availableServers)
            {
                PLog.Info<PulseLogger>(
                    $"Server: {entry.Key}\t|\tlast heartbeat: {entry.Value.ToString(CultureInfo.InvariantCulture)} seconds");
            }

            PLog.Info<PulseLogger>($"\nFLAT-LINED: {_deceasedServers.Count.ToString()}");
            foreach (var deceasedServer in _deceasedServers)
            {
                //± \u00b1
                PLog.Info<PulseLogger>($"±\t{deceasedServer.Key}");
            }
            PLog.Info<PulseLogger>("\n\n");
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
            HttpListener listenerResult = (HttpListener)result.AsyncState;

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
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        var text = reader.ReadToEnd();
                        ServerInfo info = JsonUtility.FromJson<ServerInfo>(text);
                        if (info == null)
                            return;

                        if (_availableServers.ContainsKey(info))
                            _availableServers[info] = 0;
                        else
                        {
                            _availableServers.Add(info, 0);
                            //if server sprung back to life, then it should be removed from the deceased list
                            _deceasedServers.RemoveFirst(x => x.Key == $"{info.IP}:{info.Port.ToString()}");
                        }
                    }

                    string responseString = "Pulse received";
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