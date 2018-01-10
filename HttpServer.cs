using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;

namespace eagle.tunnel.dotnet.core
{
    public class HttpServer
    {
        private string ServerIP { get; set;}
        private int ServerHttpPort { get; set;}
        public bool Running { get; set;}

        public HttpServer(string serverIP, int serverHttpPort)
        {
            ServerIP = serverIP;
            ServerHttpPort = serverHttpPort;
        }

        public void Start()
        {
            Thread startThread = new Thread(_Start);
            startThread.IsBackground = true;
            startThread.Start();
        }

        private void _Start()
        {
            TcpListener server;
            while(true)
            {
                try
                {
                    if(!IPAddress.TryParse(ServerIP, out IPAddress ipa))
                    {
                        return;
                    }
                    server = new TcpListener(ipa, ServerHttpPort);
                    server.Start(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(5000);
                    continue;
                }
                Console.WriteLine("http server started: " + ServerIP + ":" + ServerHttpPort);
                break;
            }

            Running = true;
            while(Running)
            {
                TcpClient client = server.AcceptTcpClient();
                string ip =client.Client.RemoteEndPoint.ToString().Split(':')[0];
                Console.WriteLine("new client connected: from " + ip);
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(client);
            }
            Thread.Sleep(1000);
            server.Stop();
            Console.WriteLine("Server Stopped");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private void HandleClient(object clientObj)
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
                IPAddress[] ipas = Dns.GetHostAddresses(host);
                string ip = ipas[0].ToString();
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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