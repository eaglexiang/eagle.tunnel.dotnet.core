using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Server
    {
        protected string ServerIP { get; set;}
        protected int ServerPort { get; set;}
        public bool Running { get; set;}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverIP">Server IP</param>
        /// <param name="serverPort">Server Port</param>
        public Server(string serverIP, int serverPort)
        {
            ServerIP = serverIP;
            ServerPort = serverPort;
            Running = false;
        }

        /// <summary>
        /// Start Server on new background thread.
        /// </summary>
        public void Start()
        {
            Thread startThread = new Thread(_Start);
            startThread.IsBackground = true;
            startThread.Start();
        }

        /// <summary>
        /// realization for function Start
        /// </summary>
        private void _Start()
        {
            if(IPAddress.TryParse(ServerIP, out IPAddress ipa))
            {
                TcpListener server;
                while(true)
                {
                    try
                    {
                        server = new TcpListener(ipa, ServerPort);
                        server.Start(100);
                        break;
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine(se.Message);
                        Console.WriteLine("Waiting for 10s...");
                        Thread.Sleep(10000);
                    }
                }
                Console.WriteLine("server started: " + ServerIP + ":" + ServerPort);

                Running = true;
                while(Running)
                {
                    TcpClient client;
                    try
                    {
                        client = server.AcceptTcpClient();
                        // string ip =client.Client.RemoteEndPoint.ToString().Split(':')[0];
                        // Console.WriteLine("new client connected: from " + ip);
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine(se.Message);
                        continue;
                    }
                    
                    Thread handleClientThread = new Thread(HandleClient);
                    handleClientThread.IsBackground = true;
                    handleClientThread.Start(client);
                }
                Thread.Sleep(1000);
                server.Stop();
                Console.WriteLine("Server Stopped");
            }
            else
            {
                Console.WriteLine("invalid Server IP");
            }
        }

        protected virtual void HandleClient(object clientObj) { }
    }
}