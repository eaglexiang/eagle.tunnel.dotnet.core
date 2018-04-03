using System;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {

    public enum HTTP_Request_Type {
        ERROR,
        OPTIONS,
        HEAD,
        GET,
        POST,
        PUT,
        DELETE,
        TRACE,
        CONNECT
    }

    public class HTTPReqArgs {
        public HTTP_Request_Type HTTP_Request_Type;
        public string Host { get; set; }
        public int Port { get; set; }

        private HTTPReqArgs () {
            HTTP_Request_Type = HTTP_Request_Type.ERROR;
            Host = "";
            Port = 0;
        }

        public static bool TryParse (string request, out HTTPReqArgs e) {
            bool result = false;
            e = new HTTPReqArgs ();
            if (request != null) {
                string[] args = request.Split (' ');
                if (args.Length >= 2) {
                    if (Enum.TryParse (args[0], out HTTP_Request_Type type)) {
                        string host = GetHost (request);
                        int port = GetPort (request);
                        if (!string.IsNullOrEmpty (host) && port != 0) {
                            e.HTTP_Request_Type = type;
                            e.Host = host;
                            e.Port = port;
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        public static string CreateNewRequest (string oldRequest) {
            string newReq = "";
            string[] lines = oldRequest.Replace ("\r\n", "\n").Split ('\n');
            string[] args = lines[0].Split (' ');
            if (args.Length >= 2) {
                string des = args[1];
                Uri uri = new Uri (des);
                if (uri.HostNameType != UriHostNameType.Unknown) {
                    string line = args[0];
                    line += ' ' + uri.AbsolutePath;
                    newReq = line;
                    if (args.Length >= 3) {
                        newReq += ' ' + args[2];
                        for (int i = 1; i < lines.Length; ++i) {
                            line = lines[i];
                            newReq += "\r\n" + line;
                        }
                    }
                }
            }
            return newReq;
        }

        public static IPEndPoint GetIPEndPoint (HTTPReqArgs e0) {
            IPEndPoint result = null;
            if (e0 != null) {
                if (e0.Host != null & e0.Port != 0) {
                    // resolv ip of domain name by EagleTunnel Sender
                    EagleTunnelArgs e1 = new EagleTunnelArgs ();
                    e1.Domain = e0.Host;
                    EagleTunnelSender.Handle (EagleTunnelHandler.EagleTunnelRequestType.DNS, e1);
                    // resolv successfully
                    if (e1.IP != null) {
                        result = new IPEndPoint (e1.IP, e0.Port);
                    }
                }
            }
            return result;
        }

        private static string GetURI (string request) {
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

        private static bool IsNum (char c) {
            return '0' <= c && c <= '9';
        }

        private static string GetHost (string request) {
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

        private static int GetPort (string request) {
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
    }
}