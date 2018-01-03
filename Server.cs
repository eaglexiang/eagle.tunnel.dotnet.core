using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;

namespace eagle.tunnel.dotnet.core
{
    public class Server
    {
        public Server()
        {
            ;
        }

        public bool Start(string ip, int port)
        {
            try
            {
                // string host = Dns.GetHostName();
                // IPAddress[] ipas = Dns.GetHostAddresses(host);
                // foreach (IPAddress ipa in ipas)
                // {
                //     if(ipa.AddressFamily == AddressFamily.InterNetwork)
                //     {
                //         Console.WriteLine("found new local ip: " + ipa.ToString());
                //         TcpListener server = new TcpListener(ipa, port);
                //         server.Start();
                //         Thread handleServerThread = new Thread(HandleServer);
                //         handleServerThread.IsBackground = true;
                //         handleServerThread.Start(server);
                //     }
                // }
                IPAddress ipa = IPAddress.Parse(ip);
                TcpListener server = new TcpListener(ipa, port);
                server.Start();
                Thread handleServerThread = new Thread(HandleServer);
                handleServerThread.IsBackground = true;
                handleServerThread.Start(server);
                Console.WriteLine("server started: " + ip + ":" + port);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void HandleServer(object serverObj)
        {
            TcpListener server = serverObj as TcpListener;
            while(true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("new client connected");
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(client);
            }
        }

        private void HandleClient(object clientObj)
        {
            TcpClient socket2Client = clientObj as TcpClient;
            NetworkStream stream2client = socket2Client.GetStream();
            HandleSocket2Client(stream2client);
        }

        private void HandleSocket2Client(NetworkStream stream2client)
        {
            byte[] buffer = new byte[102400];
            try
            {
                int count = stream2client.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, count);
                string url = GetURL(request);
                int port = GetPort(request);
                if(url == "")
                {
                    return ;
                }
                IPAddress[] ipas = Dns.GetHostAddresses(url);
                string ip = ipas[0].ToString();
                Console.WriteLine("connect to " + url + ":" + port);
                
                TcpClient client2Server = new TcpClient(ip, port);
                NetworkStream stream2server = client2Server.GetStream();
                
                Pipe pipe0 = new Pipe(stream2client, stream2server);
                Pipe pipe1 = new Pipe(stream2server, stream2client);

                pipe0.Write(buffer, 0, count);

                pipe0.Flow();
                pipe1.Flow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetURL(string request)
        {
            StringReader reader = new StringReader(request);
            string line = reader.ReadLine();
            int ind0 = line.IndexOf(' ');
            int ind1 = line.LastIndexOf(' ');
            string url = request.Substring(ind0 + 1, ind1 - ind0);
            Uri uri = new Uri(url);
            url = uri.Host;
            Console.WriteLine("URL: " + url);
            // while(line != null)
            // {
            //     if(line.Contains("Host:"))
            //     {
            //         int ind = request.IndexOf(":");
            //         string url = line.Substring(6, ind);
            //         Console.WriteLine("URL found: " + url);
            //         return url;
            //     }
            //     else
            //     {
            //         line = reader.ReadLine();
            //     }
            // }
            // Console.WriteLine("URL not found");
            return url;
        }

        private int GetPort(string request)
        {
            StringReader reader = new StringReader(request);
            string line = reader.ReadLine();
            int ind0 = line.IndexOf(' ');
            int ind1 = line.LastIndexOf(' ');
            string url = request.Substring(ind0 + 1, ind1 - ind0);
            Uri uri = new Uri(url);
            int port = uri.Port;
            return port;
            // if(line.Contains(":"))
            // {
            //     int ind = line.IndexOf(':');
            //     string _port = "";
            //     while(true)
            //     {
            //         if('0' <= line[ind] && line[ind] <= '9')
            //         {
            //             _port += line[ind];
            //         }
            //         else
            //         {
            //             break;
            //         }
            //     }
            //     int port = int.Parse(_port);
            //     return port;
            // }
            // else
            // {
            //     if(line.Contains("https"))
            //     {
            //         return 443;
            //     }
            //     else
            //     {
            //         return 80;
            //     }
            // }
            // // while(line != null)
            // // {
            // //     if(line.Contains("Host:"))
            // //     {
            // //         int ind = request.IndexOf(":");
            // //         string port = line.Substring(ind);
            // //         Console.WriteLine("Port found: " + port);
            // //         return int.Parse(port);
            // //     }
            // //     else
            // //     {
            // //         line = reader.ReadLine();
            // //     }
            // // }
            // // Console.WriteLine("Port not found");
            // // return 80;
        }
    }
}