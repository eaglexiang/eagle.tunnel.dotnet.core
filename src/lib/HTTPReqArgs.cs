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

        public static HTTPReqArgs Create (string request) {
            HTTPReqArgs e = new HTTPReqArgs ();
            if (request != null) {
                if (request.Contains (" ")) {
                    string typeStr = request.Substring (0, request.IndexOf (' '));
                    if (!Enum.TryParse (typeStr, out e.HTTP_Request_Type)) {
                        e.HTTP_Request_Type = HTTP_Request_Type.ERROR;
                    } else {
                        e.Host = GetHost (request);
                        e.Port = GetPort (request);
                    }
                }
            }
            return e;
        }

        public static string CreateRequest (string request) {
            string newReq = "";
            string[] lines = request.Replace ("\r\n", "\n").Split ('\n');
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
                string host = e0.Host;
                int port = e0.Port;
                if (host != null & port != 0) {
                    EagleTunnelArgs e1 = new EagleTunnelArgs ();
                    e1.Domain = host;
                    EagleTunnelSender.Handle (EagleTunnelHandler.EagleTunnelRequestType.DNS, e1);
                    if (e1.IP != null) {
                        result = new IPEndPoint (e1.IP, port);
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