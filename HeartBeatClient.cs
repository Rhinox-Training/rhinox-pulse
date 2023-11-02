using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace Rhinox.Pulse
{
    public class HeartBeatClient : Singleton<HeartBeatClient>
    {
        private static ManagedCoroutine _heartbeatRoutine;
        private static List<ServerInfo> _serverListOptions;

        public delegate void ServerListUpdatedDelegate();

        public static event ServerListUpdatedDelegate ServerListUpdated;

        public static bool RunClient()
        {
            if (_heartbeatRoutine != null)
            {
                PLog.Error<PulseLogger>($"HeartBeatClient already running, please kill the current running routine before starting a new one");
                return false;
            }
            
            _heartbeatRoutine = ManagedCoroutine.Begin(ClientRoutine());
            return true;
        }

        public static bool KillCurrent()
        {
            if (_heartbeatRoutine == null)
            {
                PLog.Warn<PulseLogger>($"HeartBeatClient wasn't running, please start a new running routine before trying to kill it...");
                return false;
            }
            
            _heartbeatRoutine.Stop();
            _heartbeatRoutine = null;
            return true;
        }

        private static IEnumerator ClientRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                using (UnityWebRequest www = UnityWebRequest.Get(PulseConfig.Instance.GetHubUri()))
                {
                    yield return new WaitForSeconds(5);
                    yield return www.SendWebRequest();

                    if (!www.IsRequestValid(out string error))
                    {
                        UpdateServerInfoList(Array.Empty<ServerInfo>());
                        Debug.LogError($"ServerHeartbeatChecker::Coroutine_Check() -> failed, reason: {error}");
                    }
                    else
                    {
                        var serverOptions = JsonHelper.FromJson<ServerInfo>(www.downloadHandler.text);
                        UpdateServerInfoList(serverOptions);
                    }
                }
            }
        }


        private static void UpdateServerInfoList(ServerInfo[] input)
        {
            if (_serverListOptions == null)
                _serverListOptions = new List<ServerInfo>();
            _serverListOptions.Clear();

            if (input.Length <= 0)
                return;

            _serverListOptions.AddRange(input);

            ServerListUpdated?.Invoke();
        }
    }
}