using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace eagle.tunnel.dotnet.core
{
    public class Server
    {
        public Server()
        {
            ;
        }

        public bool Start(int port)
        {
            try
            {
                string host = Dns.GetHostName();
                IPAddress[] ipas = Dns.GetHostAddresses(host);
                foreach (IPAddress ipa in ipas)
                {
                    if(ipa.AddressFamily == AddressFamily.InterNetwork)
                    {
                        TcpListener server = new TcpListener(ipa, port);
                        server.Start();
                        Thread handleServerThread = new Thread(HandleServer);
                        handleServerThread.IsBackground = true;
                        handleServerThread.Start(server);
                    }
                }
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

            int count = stream2client.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, count);
            string url = GetURL(request);
            IPAddress[] ipas = Dns.GetHostAddresses(url);
            
            TcpClient client = new TcpClient(ipas[0].ToString(), 80);
            NetworkStream stream2Server = client.GetStream();
            stream2Server.Write(buffer, 0, count);
            count = stream2Server.Read(buffer, 0, buffer.Length);

            stream2client.Write(buffer, 0, count);
        }

        private string GetURL(string request)
        {
            Console.WriteLine("Request: " + request);
            Console.WriteLine("URL: ");
            return "";
        }
    }
}