using System.Collections;
using System.Net;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace Rhinox.Pulse
{
    public static class HeartBeatServer
    {
        private static ManagedCoroutine _heartbeatRoutine;
        private static ServerInfo _serverInfo;

        public static bool Run(IPEndPoint endPoint, int maxConnectionCount)
        {
            if (_heartbeatRoutine != null)
            {
                PLog.Error<PulseLogger>($"HeartBeatServer already running, please kill the current running routine before starting a new one");
                return false;
            }

            _serverInfo = new ServerInfo(endPoint.Address.ToString(), (ushort) endPoint.Port, 0, maxConnectionCount);
            _heartbeatRoutine = ManagedCoroutine.Begin(ServerRoutine());
            return true;
        }

        public static bool KillCurrent()
        {
            if (_heartbeatRoutine == null)
            {
                PLog.Warn<PulseLogger>($"HeartBeatServer wasn't running, please start a new running routine before trying to kill it...");
                return false;
            }
            
            _heartbeatRoutine.Stop();
            _heartbeatRoutine = null;
            return true;
        }
        
        public static bool UpdateConnectionCount(int currentConnectionCount)
        {
            if (_heartbeatRoutine == null)
            {
                PLog.Warn<PulseLogger>($"Cannot update current connection count, HeartBeatServer wasn't running");
                return false;
            }
            
            if (currentConnectionCount < 0 || currentConnectionCount > _serverInfo.MaxPlayers)
            {
                PLog.Error<PulseLogger>($"Cannot update current connection count to an illegal value: {currentConnectionCount}");
                return false;
            }
            
            _serverInfo.Players = currentConnectionCount;
            return true;
        }

        private static IEnumerator ServerRoutine()
        {
            while (true)
            {
                var payload = JsonUtility.ToJson(_serverInfo);
                yield return new WaitForSeconds(1);
                var hubUri = PulseConfig.Instance.GetHubUri();
                using (UnityWebRequest www = UnityWebRequest.Post(hubUri, payload))
                {
                    yield return new WaitForSeconds(5);
                    yield return www.SendWebRequest();

                    if (www.responseCode != 200)
                    {
                        PLog.Error<PulseLogger>(
                            $"ServerHeart::Coroutine_Heartbeat() -> response code from: {hubUri} is {www.responseCode.ToString()}");
                    }
                }
            }
        }
    }
}