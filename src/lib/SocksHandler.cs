using System.Net;
using System.Net.Sockets;
using System.Text;

namespace eagle.tunnel.dotnet.core {
    public class SocksHandler {
        public enum SOCKS5_CMDType {
            ERROR,
            Connect,
            Bind,
            Udp
        }

        public static Tunnel Handle (byte[] request, Socket socket2Client) {
            Tunnel result = null;
            if (request != null && socket2Client != null) {
                int version = request[0];
                // check if is socks version 5
                if (version == '\u0005') {
                    string reply = "\u0005\u0000";
                    byte[] buffer = Encoding.ASCII.GetBytes (reply);
                    int written;
                    try {
                        written = socket2Client.Send (buffer);
                    } catch { written = 0; }
                    if (written > 0) {
                        buffer = new byte[100];
                        int read;
                        try {
                            read = socket2Client.Receive (buffer);
                        } catch { read = 0; }
                        if (read > 0) {
                            SOCKS5_CMDType cmdType = (SOCKS5_CMDType) buffer[1];
                            switch (cmdType) {
                                case SOCKS5_CMDType.Connect:
                                    result = HandleTCPReq (buffer, socket2Client);
                                    break;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static Tunnel HandleTCPReq (byte[] request, Socket socket2Client) {
            Tunnel result = null;
            if (request != null && socket2Client != null) {
                IPAddress ip = GetIP (request);
                int port = GetPort (request);
                if (ip != null && port != 0) {
                    IPEndPoint reqIPEP = new IPEndPoint (ip, port);
                    string reply;
                    EagleTunnelArgs e = new EagleTunnelArgs ();
                    e.EndPoint = reqIPEP;
                    result = EagleTunnelSender.Handle (EagleTunnelHandler.EagleTunnelRequestType.TCP, e);
                    if (result != null) {
                        reply = "\u0005\u0000\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                    } else {
                        reply = "\u0005\u0001\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                    }
                    byte[] buffer = Encoding.ASCII.GetBytes (reply);
                    int written;
                    try {
                        written = socket2Client.Send (buffer);
                    } catch { written = 0; }
                    if (result != null) {
                        if (written > 0) {
                            result.SocketL = socket2Client;
                        } else {
                            result.Close ();
                            result = null;
                        }
                    }
                }
            }
            return result;
        }

        public static IPAddress GetIP (byte[] request) {
            IPAddress ip = null;
            try {
                int destype = request[3];
                string ip_str;
                switch (destype) {
                    case 1:
                        ip_str = request[4].ToString ();
                        ip_str += '.' + request[5].ToString ();
                        ip_str += '.' + request[6].ToString ();
                        ip_str += '.' + request[7].ToString ();
                        if (IPAddress.TryParse (ip_str, out IPAddress ipa0)) {
                            ip = ipa0;
                        }
                        break;
                    case 3:
                        int len = request[4];
                        char[] hostChars = new char[len];
                        for (int i = 0; i < len; ++i) {
                            hostChars[i] = (char) request[5 + i];
                        }
                        string host = new string (hostChars);
                        // if host is real ip but not domain name
                        if (IPAddress.TryParse (host, out IPAddress ipa1)) {
                            ip = ipa1;
                        } else {
                            EagleTunnelArgs e = new EagleTunnelArgs ();
                            e.Domain = host;
                            EagleTunnelSender.Handle (EagleTunnelHandler.EagleTunnelRequestType.DNS, e);
                            ip = e.IP;
                        }
                        break;
                }
            } catch { ip = null; }
            return ip;
        }

        public static int GetPort (byte[] request) {
            try {
                int destype = request[3];
                int port;
                int high;
                int low;
                switch (destype) {
                    case 1:
                        high = request[8];
                        low = request[9];
                        port = high * 0x100 + low;
                        break;
                    case 3:
                        int len = request[4];
                        high = request[5 + len];
                        low = request[6 + len];
                        port = high * 0x100 + low;
                        break;
                    default:
                        port = 0;
                        break;
                }
                return port;
            } catch {
                return 0;
            }
        }
    }
}