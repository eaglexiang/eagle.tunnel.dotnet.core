using System;

namespace eagle.tunnel.dotnet.core {
    public enum Request_Type {
        ERROR,
        SOCKS5,
        HTTP
    }

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

    public enum SOCKS5_CMDType {
        ERROR,
        Connect,
        Bind,
        Udp
    }

    public enum HOST_Type {
        ERROR,
        URL,
        IPV4,
        IPV6
    }

    public class HTTPReqArgs {
        public HTTP_Request_Type HTTP_Request_Type;
        public Uri Uri { get; set; }

        private HTTPReqArgs () {
            HTTP_Request_Type = HTTP_Request_Type.ERROR;
            Uri = null;
        }

        public static HTTPReqArgs Create (string request) {
            HTTPReqArgs e = new HTTPReqArgs ();
            if (request != null) {
                if (request.Contains (" ")) {
                    string typeStr = request.Substring (0, request.IndexOf (' '));
                    if (!Enum.TryParse (typeStr, out e.HTTP_Request_Type)) {
                        e.HTTP_Request_Type = HTTP_Request_Type.ERROR;
                    } else {
                        string desStr = request.Split (' ') [1];
                        e.Uri = new Uri (desStr);
                    }
                }
            }
            return e;
        }

        public static string CreateRequest (string request) {
            string newReq = "";
            string[] lines = request.Replace ("\r\n", "\n").Split ('\n');
            string[] args = lines[0].Split (' ');
            if (args.Length > 1) {
                string des = args[1];
                Uri uri = new Uri (des);
                if (uri.HostNameType != UriHostNameType.Unknown) {
                    string line = args[0];
                    line += ' ' + uri.AbsolutePath;
                    line += ' ' + args[2];
                    newReq = line;
                    for (int i = 1; i < lines.Length; ++i) {
                        line = lines[i];
                        newReq += "\r\n" + line;
                    }
                }
            }
            return newReq;
        }
    }
}