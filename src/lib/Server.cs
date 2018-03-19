using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace eagle.tunnel.dotnet.core
{
    public class Server
    {
        protected IPEndPoint serverIPEP;
        public bool Running { get; set;}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverIP">Server IP</param>
        /// <param name="serverPort">Server Port</param>
        public Server(string serverip, int serverport)
        {
            IPAddress ipa = IPAddress.Parse(serverip);
            serverIPEP = new IPEndPoint(ipa, serverport);
            Running = false;
        }

        public Server(IPEndPoint ipep)
        {
            serverIPEP = ipep;
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
            TcpListener server;
            while(true)
            {
                try
                {
                    server = new TcpListener(serverIPEP);
                    server.Start(100);
                    break;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    Console.WriteLine("Waiting for 30s...");
                    Thread.Sleep(30000);
                }
            }
            Console.WriteLine("server started: " + serverIPEP.ToString());

            Listen(server);

            Thread.Sleep(1000);
            server.Stop();
            Console.WriteLine("Server Stopped");
        }

        protected virtual void Listen(TcpListener server)
        {
            Running = true;
            while(Running)
            {
                TcpClient client;
                try
                {
                    client = server.AcceptTcpClient();
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
        }

        protected virtual void HandleClient(object clientObj) { }

        protected static string ReadStr(TcpClient client)
        {
            int count;
            byte[] buffer;
            try
            {
                NetworkStream ns = client.GetStream();
                buffer = new byte[1024];
                count = ns.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                return null;
            }
            string str = Encoding.UTF8.GetString(buffer, 0, count);
            return str;
        }

        protected static void WriteStr(TcpClient client, string str)
        {
            try
            {
                NetworkStream ns = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                ns.Write(buffer, 0, buffer.Length);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }
    }
}