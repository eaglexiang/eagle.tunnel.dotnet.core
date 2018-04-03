using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {
    public class EagleTunnelHandler {
        public enum EagleTunnelRequestType {
            TCP,
            UDP,
            DNS,
            Unknown
        }

        EagleTunnelRequestType RequestType { get; set; }

        public static Tunnel Handle (string firstMsg, Socket socket2Client) {
            Tunnel result = null;
            if (!string.IsNullOrEmpty (firstMsg) &&
                socket2Client != null) {
                Tunnel tunnel = CheckVersion (firstMsg, socket2Client);
                if (tunnel != null) {
                    if (CheckAuthen (tunnel)) {
                        string req = tunnel.ReadStringL ();
                        if (!string.IsNullOrEmpty (req)) {
                            EagleTunnelRequestType type = GetType (req);
                            bool done = false;
                            switch (type) {
                                case EagleTunnelRequestType.DNS:
                                    HandleDNSReq (req, tunnel);
                                    // no need to continue;
                                    break;
                                case EagleTunnelRequestType.TCP:
                                    done = TCPReqHandle (req, tunnel);
                                    break;
                            }
                            if (done) {
                                result = tunnel;
                            } else {
                                tunnel.Close ();
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static Tunnel CheckVersion (string firstMsg, Socket socket2Client) {
            Tunnel result = null;
            string[] args = firstMsg.Split (' ');
            if (args.Length >= 3) {
                string reply = "";
                bool valid = args[0] == "eagle_tunnel";
                reply = valid ? "valid" : "invalid";
                bool valid1 = args[1] == "1.0";
                valid &= valid1;
                reply += valid1 ? " valid" : " invalid";
                valid1 = args[2] == "simple";
                valid &= valid1;
                reply += valid1 ? " valid" : " invalid";
                if (valid) {
                    byte[] buffer = System.Text.Encoding.ASCII.GetBytes (reply);
                    int written = socket2Client.Send (buffer);
                    if (written > 0) {
                        result = new Tunnel (socket2Client);
                        result.EncryptL = true;
                    }
                }
            }
            return result;
        }

        private static bool CheckAuthen (Tunnel tunnel) {
            bool result = false;
            if (!Conf.allConf.ContainsKey ("user-conf")) {
                result = true;
            } else {
                byte[] buffer = new byte[100];
                string req = tunnel.ReadStringL ();
                if (!string.IsNullOrEmpty (req)) {
                    if (EagleTunnelUser.TryParse (req, out EagleTunnelUser user)) {
                        if (Conf.Users.ContainsKey (user.ID)) {
                            result = Conf.Users[user.ID].CheckAuthen (user.Password);
                        }
                    }
                }
                string reply = result ? "valid" : "invalid";
                result &= tunnel.WriteL (reply);
            }
            return result;
        }

        private static EagleTunnelRequestType GetType (string msg) {
            EagleTunnelRequestType result = EagleTunnelRequestType.Unknown;
            string[] args = msg.Split (' ');
            if (!System.Enum.TryParse (args[0], out result)) {
                result = EagleTunnelRequestType.Unknown;
            }
            return result;
        }

        private static void HandleDNSReq (string msg, Tunnel tunnel) {
            if (!string.IsNullOrEmpty (msg) && tunnel != null) {
                string[] args = msg.Split (' ');
                if (args.Length >= 2) {
                    string domain = args[1];
                    string ip = ResolvDNS (domain);
                    tunnel.WriteL (ip);
                }
            }
        }

        private static string ResolvDNS (string url) {
            IPHostEntry iphe;
            try {
                iphe = Dns.GetHostEntry (url);
            } catch { return null; }
            IPAddress ipa = null;
            foreach (IPAddress tmp in iphe.AddressList) {
                if (tmp.AddressFamily == AddressFamily.InterNetwork) {
                    ipa = tmp;
                }
            }
            return ipa != null ? ipa.ToString () : "nok";
        }

        private static bool TCPReqHandle (string msg, Tunnel tunnel) {
            bool result = false;
            if (msg != null && tunnel != null) {
                string[] args = msg.Split (' ');
                if (args.Length >= 3) {
                    string ip = args[1];
                    string _port = args[2];
                    if (int.TryParse (_port, out int port)) {
                        if (IPAddress.TryParse (ip, out IPAddress ipa)) {
                            IPEndPoint ipeReq = new IPEndPoint (ipa, port);
                            Socket socket2Server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            try {
                                socket2Server.Connect (ipeReq);
                                result = true;
                            } catch {; }
                            if (result) {
                                tunnel.SocketR = socket2Server;
                                result = tunnel.WriteL ("ok");
                            } else {
                                tunnel.WriteL ("nok");
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}