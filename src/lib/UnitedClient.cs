using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace eagle.tunnel.dotnet.core {
    public class UnitedClient : Authen_ClientRelay {

        public UnitedClient (IPEndPoint[] remoteIPEPs, IPEndPoint localIPEP) : base (remoteIPEPs, localIPEP) { }

        protected override bool WorkFlow (Pipe pipeClient2Server, Pipe pipeServer2Client) {
            bool result = false;
            byte[] request = pipeClient2Server.ReadByte ();
            if (request != null) {
                if (request[0] == 5) {
                    result = HandleSOCKSReq (request, pipeClient2Server, pipeServer2Client);
                } else {
                    string requestStr = Encoding.ASCII.GetString (request);
                    result = HandleHTTPReq (requestStr, pipeClient2Server, pipeServer2Client);
                }
            }
            return result;
        }

        private bool HandleSOCKSReq (byte[] request, Pipe pipeClient2Server, Pipe pipeServer2Client) {
            bool result = false;
            int version = request[0];
            // check if is socks version 5
            if (version == '\u0005') {
                string reply = "\u0005\u0000";
                pipeServer2Client.Write (reply);

                request = pipeClient2Server.ReadByte ();
                if (request != null) {
                    SOCKS5_CMDType cmdType = (SOCKS5_CMDType) request[1];
                    if (cmdType == SOCKS5_CMDType.Connect) {
                        result = HandleTCPReq (request, pipeServer2Client, pipeClient2Server);
                    } else if (cmdType == SOCKS5_CMDType.Udp) {
                        // result = HandleUDPReq(request, pipeServer2Client, pipeClient2Server);
                    }
                }
            }
            return result;
        }

        private bool HandleHTTPReq (string request, Pipe pipeClient2Server, Pipe pipeServer2Client) {
            bool result = false;
            HTTPReqArgs e = HTTPReqArgs.Create (request);
            if (e.HTTP_Request_Type != HTTP_Request_Type.ERROR) {
                IPEndPoint reqEP = GetIPEndPoint (request);
                if (SendReqEndPoint (pipeClient2Server.SocketTo, reqEP)) {
                    if (e.HTTP_Request_Type == HTTP_Request_Type.CONNECT) {
                        string re443 = "HTTP/1.0 200 Connection Established\r\n\r\n";
                        result = pipeServer2Client.Write (re443);
                    } else {
                        string newReq = HTTPReqArgs.CreateRequest (request);
                        result = pipeClient2Server.Write (newReq);
                    }
                }
            }
            return result;
        }

        private IPEndPoint GetIPEndPoint (string request) {
            string host = GetHost (request);
            int port = GetPort (request);
            if (host != null & port != 0) {
                IPHostEntry iphe;
                try {
                    iphe = Dns.GetHostEntry (host);
                } catch { return null; }
                IPAddress ipa = null;
                foreach (IPAddress tmp in iphe.AddressList) {
                    if (tmp.AddressFamily == AddressFamily.InterNetwork) {
                        ipa = tmp;
                    }
                }
                if (ipa != null) {
                    return new IPEndPoint (ipa, port);
                }
            }
            return null;
        }

        protected override void PrintServerInfo (IPEndPoint localIPEP) {
            Console.WriteLine ("Socks Relay started: " + serverIPEP.ToString ());
        }

        private string GetURI (string request) {
            if (request != null) {
                string line = request.Split ('\n') [0];
                int ind0 = line.IndexOf (' ');
                int ind1 = line.LastIndexOf (' ');
                if (ind0 == ind1) {
                    ind1 = line.Length;
                }
                string uri = request.Substring (ind0 + 1, ind1 - ind0 - 1);
                return uri;
            } else {
                return null;
            }
        }

        private bool IsNum (char c) {
            return '0' <= c && c <= '9';
        }

        private string GetHost (string request) {
            string uristr = GetURI (request);
            string host;
            if (uristr != null) {
                if (uristr.Contains (":") &&
                    IsNum (uristr[uristr.IndexOf (":") + 1])) {
                    int ind = uristr.LastIndexOf (":");
                    host = uristr.Substring (0, ind);
                } else {
                    try {
                        Uri uri = new Uri (uristr);
                        host = uri.Host;
                    } catch { host = null; }
                }
            } else {
                host = null;
            }
            return host;
        }

        private int GetPort (string request) {
            string uristr = GetURI (request);
            int port;
            if (uristr != null) {
                if (uristr.Contains (":") &&
                    IsNum (uristr[uristr.IndexOf (":") + 1])) {
                    int ind = uristr.IndexOf (":");
                    string _port = uristr.Substring (ind + 1);
                    if (!int.TryParse (_port, out port)) {
                        port = 0;
                    }
                } else {
                    try {
                        Uri uri = new Uri (uristr);
                        port = uri.Port;
                    } catch { port = 0; }
                }
            } else {
                port = 0;
            }
            return port;
        }

        private bool HandleTCPReq (byte[] request, Pipe pipeServer2Client, Pipe pipeClient2Server) {
            string ip = GetIP (request);
            int port = GetPort (request);
            if (ip != null && port != 0) {
                if (IPAddress.TryParse (ip, out IPAddress ipa)) {
                    IPEndPoint reqIPEP = new IPEndPoint (ipa, port);
                    string reply;
                    bool reqReply = SendReqEndPoint (pipeClient2Server.SocketTo, reqIPEP);
                    if (reqReply) {
                        reply = "\u0005\u0000\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                    } else {
                        reply = "\u0005\u0001\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                    }
                    pipeServer2Client.Write (reply);
                    return reqReply;
                }
            }
            return false;
        }

        public static string GetIP (byte[] request) {
            try {
                int destype = request[3];
                string ip;
                switch (destype) {
                    case 1:
                        ip = request[4].ToString ();
                        ip += '.' + request[5].ToString ();
                        ip += '.' + request[6].ToString ();
                        ip += '.' + request[7].ToString ();
                        break;
                    case 3:
                        int len = request[4];
                        char[] hostChars = new char[len];
                        for (int i = 0; i < len; ++i) {
                            hostChars[i] = (char) request[5 + i];
                        }
                        string host = new string (hostChars);
                        // if host is real ip but not domain name
                        if (IPAddress.TryParse (host, out IPAddress ipa)) {
                            ip = host;
                        } else {
                            IPHostEntry iphe = Dns.GetHostEntry (host);
                            ip = null;
                            foreach (IPAddress tmp in iphe.AddressList) {
                                if (tmp.AddressFamily == AddressFamily.InterNetwork) {
                                    ip = tmp.ToString ();
                                }
                            }
                        }
                        break;
                    default:
                        ip = null;
                        break;
                }
                return ip;
            } catch {
                return null;
            }
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

        public static byte[] GetUDPData (byte[] request) {
            try {
                int destype = request[3];
                byte[] data;
                switch (destype) {
                    case 1:
                        data = new byte[request.Length - 10];
                        request.CopyTo (data, 10);
                        break;
                    case 3:
                        int len = request[4];
                        data = new byte[request.Length - 7 - len];
                        request.CopyTo (data, 7 + len);
                        break;
                    default:
                        data = null;
                        break;
                }
                return data;
            } catch {
                return null;
            }
        }
    }
}