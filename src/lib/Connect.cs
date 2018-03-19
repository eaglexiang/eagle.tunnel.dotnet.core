using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core
{
    public class Connect
    {
        public TcpClient client;
        public string userFrom;

        public Connect(TcpClient _client, string _user)
        {
            client = _client;
            userFrom = _user;
        }
    }
}