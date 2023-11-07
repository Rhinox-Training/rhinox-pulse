using System;
using System.Net;
using Rhinox.Utilities;


namespace Rhinox.Pulse
{
    public class HeartBeatHubConfig : LoadableConfigFile<HeartBeatHubConfig, ConfigFileIniLoader>
    {
        public override string RelativeFilePath => "config.ini";

        [ConfigSection("SERVER"), ConfigCommandArg("ip")]
        public string HostIP= "127.0.0.1";

        [ConfigSection("SERVER"), ConfigCommandArg("port")]
        public ushort HostPort= 7777;

        [ConfigSection("SERVER"), ConfigCommandArg("secure")]
        public bool HTTPS = false;


        [ConfigSection("HUB"), ConfigCommandArg("secure")]
        public float UpdateTime = 1f;

        [ConfigSection("HUB"), ConfigCommandArg("timeout")]
        public float HeartBeatTimeOut = 10f;
        
        [ConfigSection("HUB"), ConfigCommandArg("deadtimeout")]
        public float DeceasedTimeOut = 120f;

        public Uri GetHostUri()
        {
            if (string.IsNullOrWhiteSpace(HostIP) || HostPort <= 0)
                return null;

            if (!IPAddress.TryParse(HostIP, out _))
                return null;

            var uri = new UriBuilder(HTTPS ? "https" : "http", HostIP, HostPort);
            return uri.Uri;
        }
    }
}