using System.Net;
using System.Net.Sockets;
using System.Text;

namespace eagle.tunnel.dotnet.core {
    public class EagleTunnelSender {
        public static Tunnel Handle (EagleTunnelHandler.EagleTunnelRequestType type, EagleTunnelArgs e) {
            Tunnel result = null;
            if (type != EagleTunnelHandler.EagleTunnelRequestType.Unknown &&
                e != null) {
                IPEndPoint ipe2Server = Conf.GetRemoteIPEndPoint ();
                Socket socket2Server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try {
                    socket2Server.Connect (ipe2Server);
                } catch { socket2Server = null; }
                result = CheckVersion (socket2Server);
                if (result != null) {
                    if (CheckUser (result)) {
                        bool done = false;
                        switch (type) {
                            case EagleTunnelHandler.EagleTunnelRequestType.DNS:
                                DNSReqSender (result, e);
                                break;
                            case EagleTunnelHandler.EagleTunnelRequestType.TCP:
                                done = TCPReqSender (result, e);
                                break;
                        }
                        if (!done) {
                            result.Close ();
                            result = null;
                        }
                    }
                }
            }
            return result;
        }

        private static Tunnel CheckVersion (Socket socket2Server) {
            Tunnel result = null;
            if (socket2Server != null) {
                string req = "eagle_tunnel 1.0 null";
                byte[] buffer = Encoding.ASCII.GetBytes (req);
                int written;
                try {
                    written = socket2Server.Send (buffer);
                } catch { written = 0; }
                if (written > 0) {
                    buffer = new byte[100];
                    int read;
                    try {
                        read = socket2Server.Receive (buffer);
                    } catch { read = 0; }
                    if (read > 0) {
                        string reply = Encoding.UTF8.GetString (buffer, 0, read);
                        if (reply == "valid valid valid") {
                            result = new Tunnel (null, socket2Server);
                            result.EncryptR = true;
                        }
                    }
                }
            }
            return result;
        }

        private static bool CheckUser (Tunnel tunnel) {
            bool result = false;
            if (Conf.allConf.ContainsKey ("user-conf")) {
                string user_pswd = Conf.allConf["user"][0];
                bool done = tunnel.WriteR (user_pswd);
                if (done) {
                    string reply = tunnel.ReadStringR ();
                    if (!string.IsNullOrEmpty (reply)) {
                        result = reply == "valid";
                    }
                }
            } else {
                result = true;
            }
            return result;
        }

        private static void DNSReqSender (Tunnel tunnel, EagleTunnelArgs e) {
            if (tunnel != null && e != null) {
                if (e.Domain != null) {
                    string req = EagleTunnelHandler.EagleTunnelRequestType.DNS.ToString ();
                    req += " " + e.Domain;
                    bool done = tunnel.WriteR (req);
                    if (done) {
                        string reply = tunnel.ReadStringR ();
                        if (!string.IsNullOrEmpty (reply) && reply != "nok") {
                            if (IPAddress.TryParse (reply, out IPAddress ip)) {
                                e.IP = ip;
                            }
                        }
                    }
                }
            }
        }

        private static bool TCPReqSender (Tunnel tunnel, EagleTunnelArgs e) {
            bool result = false;
            if (tunnel != null && e != null) {
                if (e.EndPoint != null) {
                    IPEndPoint ipeReq = e.EndPoint;
                    string req = EagleTunnelHandler.EagleTunnelRequestType.TCP.ToString ();
                    req += ' ' + ipeReq.Address.ToString ();
                    req += ' ' + ipeReq.Port.ToString ();
                    bool done = tunnel.WriteR (req);
                    string reply = tunnel.ReadStringR ();
                    result = reply == "ok";
                }
            }
            return result;
        }
    }
}