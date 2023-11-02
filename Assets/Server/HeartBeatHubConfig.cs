using System;
using System.Net;
using Rhinox.Utilities;

namespace Rhinox.Pulse
{
    public class HeartBeatHubConfig : LoadableConfigFile<HeartBeatHubConfig, ConfigFileIniLoader>
    {
        public override string RelativeFilePath => "config.ini";

        [ConfigSection("SERVER"), ConfigCommandArg("ip")]
        public string HostIP;
        
        [ConfigSection("SERVER"), ConfigCommandArg("port")]
        public ushort HostPort;
        
        [ConfigSection("SERVER"), ConfigCommandArg("secure")]
        public bool HTTPS = false;

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