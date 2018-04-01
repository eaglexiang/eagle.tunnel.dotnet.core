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
                HTTPReqArgs e0 = HTTPReqArgs.Create (firstMsg);
                if (e0.HTTP_Request_Type != HTTP_Request_Type.ERROR) {
                    IPEndPoint reqEP = HTTPReqArgs.GetIPEndPoint (e0);
                    EagleTunnelArgs e1 = new EagleTunnelArgs ();
                    e1.EndPoint = reqEP;
                    result = EagleTunnelSender.Handle (EagleTunnelHandler.EagleTunnelRequestType.TCP, e1);
                    if (result != null) {
                        bool done;
                        if (e0.HTTP_Request_Type == HTTP_Request_Type.CONNECT) {
                            // HTTPS: reply web client;
                            string re443 = "HTTP/1.1 200 Connection Established\r\n\r\n";
                            byte[] buffer = Encoding.UTF8.GetBytes (re443);
                            int written;
                            try {
                                written = socket2Client.Send (buffer);
                            } catch { written = 0; }
                            done = written > 0;
                        } else {
                            // HTTP: relay new request to web server
                            string newReq = HTTPReqArgs.CreateRequest (firstMsg);
                            done = result.WriteR (newReq);
                        }
                        if (!done) {
                            result.Close ();
                            result = null;
                        } else {
                            result.SocketL = socket2Client;
                        }
                    }
                }
            }
            return result;
        }
    }
}