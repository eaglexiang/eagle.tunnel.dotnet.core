using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace eagle.tunnel.dotnet.core {
    public class HTTPHandler {
        public static Tunnel Handle (string firstMsg, Socket socket2Client) {
            Tunnel result = null;
            if (firstMsg != null && socket2Client != null) {
                if (HTTPReqArgs.TryParse (firstMsg, out HTTPReqArgs e0)) {
                    IPEndPoint reqEP = HTTPReqArgs.GetIPEndPoint (e0);
                    EagleTunnelArgs e1 = new EagleTunnelArgs ();
                    e1.EndPoint = reqEP;
                    Tunnel tunnel = EagleTunnelSender.Handle (EagleTunnelHandler.EagleTunnelRequestType.TCP, e1);
                    if (tunnel != null) {
                        tunnel.SocketL = socket2Client;
                        bool done;
                        if (e0.HTTP_Request_Type == HTTP_Request_Type.CONNECT) {
                            // HTTPS: reply web client;
                            // string re443 = "HTTP/1.1 200 Connection Established\r\n\r\n";
                            string re443 = "HTTP/1.1 OK\r\n\r\n";
                            done = tunnel.WriteL (re443);
                        } else {
                            // HTTP: relay new request to web server
                            string newReq = HTTPReqArgs.CreateRequest (firstMsg);
                            done = tunnel.WriteR (newReq);
                        }
                        if (done) {
                            result = tunnel;
                        } else {
                            tunnel.Close ();
                        }
                    }
                }
            }
            return result;
        }
    }
}