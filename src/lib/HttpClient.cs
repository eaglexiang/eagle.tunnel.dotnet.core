using System;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core
{
    public class HttpClient : Authen_ClientRelay
    {
        public HttpClient(IPEndPoint[] remoteIPEPs, IPEndPoint localIPEP) : base(remoteIPEPs, localIPEP) { }

        protected override bool WorkFlow(Pipe pipeClient2Server, Pipe pipeServer2Client)
        {
            string request = pipeClient2Server.ReadString();
            IPEndPoint reqIPEP = GetIPEndPoint(request);
            if (SendReqEndPoint(pipeClient2Server.SocketTo ,reqIPEP))
            {
                if(reqIPEP.Port == 443)
                {
                    string re443 = "HTTP/1.1 OK\r\n\r\n";
                    return pipeServer2Client.Write(re443);
                }
                else
                {
                    return pipeClient2Server.Write(request);
                }
            }
            return false;
        }

        protected override void PrintServerInfo(IPEndPoint localIPEP)
        {
            Console.WriteLine ("HTTP Client started: " + serverIPEP.ToString ());
        }

        private IPEndPoint GetIPEndPoint(string request)
        {
            string host = GetHost(request);
            int port = GetPort(request);
            if(host != null & port != 0)
            {
                IPHostEntry iphe;
                try
                {
                    iphe = Dns.GetHostEntry(host);
                }
                catch { return null;}
                IPAddress ipa = null;
                foreach ( IPAddress tmp in iphe.AddressList)
                {
                    if(tmp.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipa = tmp;
                    }
                }
                if(ipa != null)
                {
                    return new IPEndPoint(ipa, port);
                }
            }
            return null;
        }

        private string GetURI(string request)
        {
            if (request != null)
            {
                string line = request.Split('\n')[0];
                int ind0 = line.IndexOf(' ');
                int ind1 = line.LastIndexOf(' ');
                if(ind0 == ind1)
                {
                    ind1 = line.Length;
                }
                string uri = request.Substring(ind0 + 1, ind1 - ind0 -1);
                return uri;
            }
            else
            {
                return null;
            }
        }

        private bool IsNum(char c)
        {
            return '0' <= c && c <= '9';
        }

        private string GetHost(string request)
        {
            string uristr = GetURI(request);
            string host;
            if (uristr != null)
            {
                if(uristr.Contains(":") &&
                    IsNum(uristr[uristr.IndexOf(":") + 1]))
                {
                    int ind = uristr.LastIndexOf(":");
                    host = uristr.Substring(0, ind);
                }
                else
                {
                    try
                    {
                        Uri uri = new Uri(uristr);
                        host = uri.Host;
                    }
                    catch { host = null;}
                }
            }
            else
            {
                host = null;
            }
            return host;
        }

        private int GetPort(string request)
        {
            string uristr = GetURI(request);
            int port;
            if (uristr != null)
            {
                if(uristr.Contains(":") &&
                    IsNum(uristr[uristr.IndexOf(":") + 1]))
                {
                    int ind = uristr.IndexOf(":");
                    string _port = uristr.Substring(ind + 1);
                    if (!int.TryParse(_port, out port))
                    {
                        port = 0;
                    }
                }
                else
                {
                    try
                    {
                        Uri uri = new Uri(uristr);
                        port = uri.Port;
                    }catch { port = 0;}
                }
            }
            else
            {
                port = 0;
            }
            return port;
        }
    }
}