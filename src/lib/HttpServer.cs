using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;

namespace eagle.tunnel.dotnet.core
{
    public class HttpServer : Server
    {
        

        public HttpServer(string serverIP, int serverPort) : base(serverIP, serverPort) { }
        public HttpServer(IPEndPoint ipep) : base(ipep) { }

        protected override void HandleClient(object clientObj)
        {
            TcpClient socket2Client = clientObj as TcpClient;

            Pipe pipe0;
            Pipe pipe1 = null;
            pipe0 = new Pipe(
                socket2Client,
                null
            );
            pipe0.EncryptFrom = true;
            try
            {
                byte[] buffer = pipe0.Read();
                if(buffer == null)
                {
                    return;
                }
                string request = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                string host = GetHost(request);
                int port = GetPort(request);
                if(host == "" || port == 0)
                {
                    return;
                }
                IPHostEntry iphe = Dns.GetHostEntry(host);
                IPAddress ipa = null;
                foreach ( IPAddress tmp in iphe.AddressList)
                {
                    if(tmp.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipa = tmp;
                    }
                }
                if(ipa == null)
                {
                    return;
                }
                string ip = ipa.ToString();
                Console.WriteLine("connect to " + host + ":" + port);
                
                TcpClient client2Server = new TcpClient(ip, port);

                pipe1 = new Pipe(
                    client2Server,
                    socket2Client
                );
                pipe1.EncryptTo = true;

                pipe0.ClientTo = client2Server;

                if(port == 443)
                {
                    string re443 = "HTTP/1.1 OK\r\n\r\n";
                    buffer = Encoding.UTF8.GetBytes(re443);
                    pipe1.Write(buffer);
                }
                else
                {
                    pipe0.Write(buffer);
                }

                pipe0.Flow();
                pipe1.Flow();
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                pipe0.Close();
                if(pipe1 != null)
                {
                    pipe1.Close();
                }
            }
        }

        private string GetURI(string request)
        {
            StringReader reader = new StringReader(request);
            string line = reader.ReadLine();
            int ind0 = line.IndexOf(' ');
            int ind1 = line.LastIndexOf(' ');
            if(ind0 == ind1)
            {
                ind1 = line.Length;
            }
            string uri = request.Substring(ind0 + 1, ind1 - ind0 -1);
            return uri;
        }

        private bool IsNum(char c)
        {
            return '0' <= c && c <= '9';
        }

        private string GetHost(string request)
        {
            string uristr = GetURI(request);
            string host;
            if(
                uristr.Contains(":") &&
                IsNum(uristr[uristr.IndexOf(":") + 1]))
            {
                int ind = uristr.LastIndexOf(":");
                host = uristr.Substring(0, ind);
            }
            else
            {
                Uri uri = new Uri(uristr);
                host = uri.Host;
            }
            return host;
        }

        private int GetPort(string request)
        {
            string uristr = GetURI(request);
            int port;
            if(
                uristr.Contains(":") &&
                IsNum(uristr[uristr.IndexOf(":") + 1]))
            {
                int ind = uristr.IndexOf(":");
                string _port = uristr.Substring(ind + 1);
                port = int.Parse(_port);
            }
            else
            {
                Uri uri = new Uri(uristr);
                port = uri.Port;
            }
            return port;
        }

        public void Stop()
        {
            Console.WriteLine("quiting...");
            Running = false;
        }
    }
}