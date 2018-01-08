using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace eagle.tunnel.dotnet.core
{
    public class SocketServer
    {
        private string ServerIP { get; set;}
        private int ServerSocketPort { get; set;}
        public bool Running { get; set;}

        public SocketServer(string serverIP, int serverSocketPort)
        {
            ServerIP = serverIP;
            ServerSocketPort = serverSocketPort;
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
                    server = new TcpListener(ipa, ServerSocketPort);
                    server.Start(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(5000);
                    continue;
                }
                Console.WriteLine("http server started: " + ServerIP + ":" + ServerSocketPort);
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
            Console.WriteLine("Server Stopped");
            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        private void HandleClient(object clientObj)
        {
            TcpClient socket2Client = clientObj as TcpClient;

            Pipe pipe0 = new Pipe(
                socket2Client,
                null
            );
            Pipe pipe1 = new Pipe(
                null,
                socket2Client
            );
            pipe0.EncryptFrom = true;
            pipe1.EncryptTo = true;
            try
            {
                string request = pipe0.ReadString();
                if(request == null)
                {
                    return;
                }
                // not socket 5 request
                string version = request.Substring(0,4);
                if(version != "\\x05")
                {
                    return;
                }

                string reply = "\\x05\\x00";
                pipe1.Write(reply);

                request = pipe0.ReadString();
                if(request == null)
                {
                    return;
                }
                if(request.Substring(4, 4) != "\\x01")
                {
                    return;
                }
                string ip = GetIP(request);
                int port = GetPort(request);
                if(ip == null || port == 0)
                {
                    return;
                }

                TcpClient client2Server = new TcpClient(ip, port);

                pipe0.Flow();
                pipe1.Flow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                pipe0.Close();
                pipe1.Close();
            }
        }

        private string GetIP(string request)
        {
            try
            {
                string destype = request.Substring(9, 3);
                string ip;
                int ind = request.IndexOf("\\x", 16);
                switch (destype)
                {
                case "\\x01":
                    ip = request.Substring(16, ind - 16);
                    break;
                case "\\x3":
                    string host = request.Substring(16, ind - 16);
                    IPHostEntry iphe = Dns.GetHostEntry(host);
                    ip = iphe.AddressList[0].ToString();
                    break;
                default:
                    ip = null;
                    break;
                }
                return ip;
            }
            catch
            {
                return null;
            }
        }

        private int GetPort(string request)
        {
            try
            {
                int ind = request.IndexOf("\\x", 16);
                string _high = request.Substring(ind + 2, 2);
                string _low = request.Substring(ind + 6, 2);
                int high = Convert.ToInt32(_high, 16);
                int low = Convert.ToInt32(_low, 16);
                int port = high * 0xFF + low;
                return port;
            }
            catch
            {
                return 0;
            }
        }
    }
}