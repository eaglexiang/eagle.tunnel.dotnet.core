using System.Net;

namespace eagle.tunnel.dotnet.core {
    public class EagleTunnelArgs {
        public string Domain { get; set; }
        public IPAddress IP { get; set; }
        public IPEndPoint EndPoint { get; set; }
    }
}