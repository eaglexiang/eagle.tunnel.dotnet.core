using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace eagle.tunnel.dotnet.core {
    public class EagleTunnelSender {
        private static ConcurrentDictionary<string, IPAddress> dnsCache =
            new ConcurrentDictionary<string, IPAddress> ();

        public static Tunnel Handle (EagleTunnelHandler.EagleTunnelRequestType type, EagleTunnelArgs e) {
            Tunnel result = null;
            if (type != EagleTunnelHandler.EagleTunnelRequestType.Unknown &&
                e != null) {
                IPEndPoint ipeOfServer = Conf.GetRemoteIPEndPoint ();
                Socket socket2Server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try {
                    socket2Server.Connect (ipeOfServer);
                } catch { socket2Server = null; }
                Tunnel tunnel = CheckVersion (socket2Server);
                if (tunnel != null) {
                    bool done = CheckUser (tunnel);
                    if (done) {
                        switch (type) {
                            case EagleTunnelHandler.EagleTunnelRequestType.DNS:
                                DNSReqSender (tunnel, e);
                                done = false; // no need to continue;
                                break;
                            case EagleTunnelHandler.EagleTunnelRequestType.TCP:
                                done = TCPReqSender (tunnel, e);
                                break;
                        }
                    }
                    if (done) {
                        result = tunnel;
                    } else {
                        tunnel.Close ();
                    }
                }
            }
            return result;
        }

        private static Tunnel CheckVersion (Socket socket2Server) {
            Tunnel result = null;
            if (socket2Server != null) {
                string req = "eagle_tunnel 1.0 simple";
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
            if (Conf.LocalUser != null) {
                bool done = tunnel.WriteR (Conf.LocalUser.ToString ());
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
                e.IP = null;
                if (e.Domain != null) {
                    if (dnsCache.ContainsKey (e.Domain)) {
                        e.IP = dnsCache[e.Domain];
                    } else {
                        if (IPAddress.TryParse (e.Domain, out IPAddress ip0)) {;
                            // e.Domain is IP but not domain
                            e.IP = ip0;
                        } else {
                            string req = EagleTunnelHandler.EagleTunnelRequestType.DNS.ToString ();
                            req += " " + e.Domain;
                            bool done = tunnel.WriteR (req);
                            if (done) {
                                string reply = tunnel.ReadStringR ();
                                if (!string.IsNullOrEmpty (reply) && reply != "nok") {
                                    if (IPAddress.TryParse (reply, out IPAddress ip1)) {
                                        e.IP = ip1;
                                    }
                                }
                            }
                        }
                        try {
                            dnsCache.TryAdd (e.Domain, e.IP);
                        } catch {; }
                    }
                }
            }
        }

        private static bool TCPReqSender (Tunnel tunnel, EagleTunnelArgs e) {
            bool result = false;
            if (tunnel != null && e != null) {
                if (e.EndPoint != null) {
                    string req = EagleTunnelHandler.EagleTunnelRequestType.TCP.ToString ();
                    req += ' ' + e.EndPoint.Address.ToString ();
                    req += ' ' + e.EndPoint.Port.ToString ();
                    bool done = tunnel.WriteR (req);
                    if (done) {
                        string reply = tunnel.ReadStringR ();
                        result = reply == "ok";
                    }
                }
            }
            return result;
        }
    }
}