using System;

namespace Rhinox.Pulse
{
    [Serializable]
    public class ServerInfo : IEquatable<ServerInfo>
    {
        public string IP;
        public ushort Port;
        public int Players;
        public int MaxPlayers;

        public ServerInfo(string ip, ushort port, int players, int maxPlayers)
        {
            IP = ip;
            Port = port;
            Players = players;
            MaxPlayers = maxPlayers;
        }

        public override string ToString()
        {
            //ex. "127.0.0.1:7777   2/8"
            return $"{IP}:{Port.ToString()}\t{Players.ToString()}/{MaxPlayers.ToString()}";
        }

        public bool Equals(ServerInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IP == other.IP && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((IP != null ? IP.GetHashCode() : 0) * 397) ^ Port.GetHashCode();
            }
        }

        public static bool operator ==(ServerInfo left, ServerInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServerInfo left, ServerInfo right)
        {
            return !Equals(left, right);
        }
    }
}