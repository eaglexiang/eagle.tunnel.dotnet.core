using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace eagle.tunnel.dotnet.core
{
    public class SocksServer
    {
        private string ServerIP { get; set;}
        private int ServerSocksPort { get; set;}
        public bool Running { get; set;}

        public SocksServer(string serverIP, int serverSocksPort)
        {
            ServerIP = serverIP;
            ServerSocksPort = serverSocksPort;
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
                    server = new TcpListener(ipa, ServerSocksPort);
                    server.Start(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Retrying...");
                    Thread.Sleep(5000);
                    continue;
                }
                Console.WriteLine(
                    "socks5 server started: " +
                    ServerIP + ":" + ServerSocksPort
                );
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

            Pipe client2Server = new Pipe(
                socket2Client,
                null
            );
            Pipe server2Client = new Pipe(
                null,
                socket2Client
            );
            // pipe0.EncryptFrom = true;
            // pipe1.EncryptTo = true;
            try
            {
                string request = client2Server.ReadString();
                if(request == null)
                {
                    return;
                }
                // not socket 5 request
                int version = request[0];
                if(version != '\u0005')
                {
                    return;
                }

                string reply = "\u0005\u0000";
                server2Client.Write(reply);

                request = client2Server.ReadString();
                if(request == null)
                {
                    return;
                }
                if(request[1] != '\u0001')
                {
                    return;
                }
                string ip = GetIP(request);
                int port = GetPort(request);
                if(ip == null || port == 0)
                {
                    return;
                }

                TcpClient tcpClient2Server;
                try
                {
                    tcpClient2Server = new TcpClient(ip, port);
                    reply = "\u0005\u0000\u0000\u0001" + (char)127 + (char)0 + (char)0 + (char)1 + (char)0 + (char)8080;
                    server2Client.Write(reply);
                }
                catch
                {
                    reply = "\u0005\u0001\u0000\u0001" + (char)127 + (char)0 + (char)0 + (char)1 + (char)0 + (char)8080;
                    server2Client.Write(reply);
                    return;
                }
                client2Server.ClientTo = tcpClient2Server;
                server2Client.ClientFrom = tcpClient2Server;

                client2Server.Flow();
                server2Client.Flow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client2Server.Close();
                server2Client.Close();
            }
        }

        private string GetIP(string request)
        {
            try
            {
                int destype = request[3];
                string ip;
                switch (destype)
                {
                case 1:
                    ip = ((int)request[4]).ToString();
                    ip += '.' + ((int)request[5]).ToString();
                    ip += '.' + ((int)request[6]).ToString();
                    ip += '.' + ((int)request[7]).ToString();
                    break;
                case 3:
                    int ind = request.IndexOf('\0', 3);
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
                int ind = request.IndexOf('\0', 3);
                // string _high = request.Substring(ind + 2, 2);
                // string _low = request.Substring(ind + 6, 2);
                // int high = Convert.ToInt32(_high, 16);
                // int low = Convert.ToInt32(_low, 16);
                // int port = high * 0xFF + low;
                int port = request[ind + 1];
                return port;
            }
            catch
            {
                return 0;
            }
        }
    }
}