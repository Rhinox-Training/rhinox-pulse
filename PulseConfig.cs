using System;
using System.Net;
using Rhinox.Utilities;

namespace Rhinox.Pulse
{
    
    public class PulseConfig : ConfigFile<PulseConfig>
    {
        public string HeartbeatHubIP = "127.0.0.1";
        public ushort HeartbeatHubPort = 7777;
        public bool HeartbeatHubSecure = false;
        
        public Uri GetHubUri()
        {
            if (string.IsNullOrWhiteSpace(HeartbeatHubIP) || HeartbeatHubPort <= 0)
                return null;

            if (!IPAddress.TryParse(HeartbeatHubIP, out _))
                return null;
            
            var uri = new UriBuilder(HeartbeatHubSecure ? "https" : "http", HeartbeatHubIP, HeartbeatHubPort);
            return uri.Uri;
        }
    }
}