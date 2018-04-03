using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eagle.tunnel.dotnet.core {
    public class RequestHandler {

        enum RequestType {
            Unknown,
            SOCKS5,
            HTTP_Proxy,
            Eagle_Tunnel
        }

        private RequestHandler () { }

        public static Tunnel Handle (byte[] firstMsg, Socket socket2Client) {
            Tunnel result = null;
            if (firstMsg != null && socket2Client != null) {
                RequestType reqType = GetType (firstMsg);
                string msgStr = Encoding.UTF8.GetString (firstMsg);
                Tunnel tunnel = null;
                switch (reqType) {
                    case RequestType.Eagle_Tunnel:
                        if (Conf.EnableEagleTunnel) {
                            tunnel = EagleTunnelHandler.Handle (msgStr, socket2Client);
                        }
                        break;
                    case RequestType.HTTP_Proxy:
                        if (Conf.EnableHTTP) {
                            tunnel = HTTPHandler.Handle (msgStr, socket2Client);
                        }
                        break;
                    case RequestType.SOCKS5:
                        if (Conf.EnableSOCKS) {
                            tunnel = SocksHandler.Handle (firstMsg, socket2Client);
                        }
                        break;
                }
                if (tunnel == null) {
                    if (socket2Client.Connected) {
                        try {
                            socket2Client.Shutdown (SocketShutdown.Both);
                            Thread.Sleep (100);
                            socket2Client.Close ();
                        } catch {; }
                    }
                } else {
                    result = tunnel;
                }
            }
            return result;
        }

        private static RequestType GetType (byte[] msg) {
            RequestType result = RequestType.Unknown;
            if (msg[0] == 5) {
                result = RequestType.SOCKS5;
            } else {
                string msgStr = Encoding.ASCII.GetString (msg);
                string[] args = msgStr.Split (' ');
                if (args.Length >= 1) {
                    if (Enum.TryParse (args[0], out HTTP_Request_Type type)) {
                        result = RequestType.HTTP_Proxy;
                    } else if (args[0] == "eagle_tunnel") {
                        result = RequestType.Eagle_Tunnel;
                    }
                }
            }
            return result;
        }
    }
}