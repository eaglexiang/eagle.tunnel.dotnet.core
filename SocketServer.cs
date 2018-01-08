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
                string version = request.Substring(0,3);
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
                string ip = GetIP(request);
                int port = GetPort(request);


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
            string destype = request.Substring(9, 3);
            string ip = null;
            int ind = request.IndexOf("\\x", 12);
            switch (destype)
            {
            case "\\x01":
                ip = request.Substring(12, ind - 12);
                break;
            case "\\x3":
                string host = request.Substring(12, ind - 12);
                IPHostEntry iphe = Dns.GetHostEntry(host);
                ip = iphe.AddressList[0].ToString();
                break;
            default:
                break;
            }
            return ip;
        }

        private int GetPort(string request)
        {
            int ind = request.IndexOf("\\x", 12);
        }
    }
}