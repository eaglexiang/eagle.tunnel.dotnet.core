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
        private static Random rand = new Random();

        public SocksServer(string serverIP, int serverSocksPort)
        {
            ServerIP = serverIP;
            ServerSocksPort = serverSocksPort;
        }

        public void Start()
        {
            Thread startTCPThread = new Thread(_StartTCP);
            startTCPThread.IsBackground = true;
            startTCPThread.Start();
        }

        private void _StartTCP()
        {
            TcpListener tcpServer;
            while(true)
            {
                try
                {
                    if(!IPAddress.TryParse(ServerIP, out IPAddress ipa))
                    {
                        return;
                    }
                    tcpServer = new TcpListener(ipa, ServerSocksPort);
                    tcpServer.Start(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Retrying...");
                    Thread.Sleep(5000);
                    continue;
                }
                Console.WriteLine(
                    "socks5 TCP server started: " +
                    ServerIP + ":" + ServerSocksPort
                );
                break;
            }

            Running = true;
            while(Running)
            {
                TcpClient client = tcpServer.AcceptTcpClient();
                string ip =client.Client.RemoteEndPoint.ToString().Split(':')[0];
                Console.WriteLine("new client connected: from " + ip);
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(client);
            }
            Thread.Sleep(1000);
            tcpServer.Stop();
            Console.WriteLine("Server Stopped");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        enum CMDType
        {
            Null,
            Connect,
            Bind,
            Udp
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
                byte[] request = client2Server.Read();
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

                request = client2Server.Read();
                if(request == null)
                {
                    return;
                }

                CMDType cmdType = (CMDType)request[1];

                if(cmdType == CMDType.Connect)
                {
                    HandleTCPReq(request, server2Client, client2Server);
                }
                else if(cmdType == CMDType.Udp)
                {
                    HandleUDPReq(request, server2Client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client2Server.Close();
                server2Client.Close();
            }
        }

        private void HandleTCPReq(byte[] request, Pipe server2Client, Pipe client2Server)
        {
            string ip = GetIP(request);
            int port = GetPort(request);
            string reply;
            if(ip == null)
            {
                string str = Encoding.UTF8.GetString(request);
                return;
            }
            TcpClient tcpClient2Server;
            try
            {
                tcpClient2Server = new TcpClient(ip, port);
                reply = "\u0005\u0000\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                server2Client.Write(reply);
                client2Server.ClientTo = tcpClient2Server;
                server2Client.ClientFrom = tcpClient2Server;
                client2Server.Flow();
                server2Client.Flow();
            }
            catch
            {
                CloseRequest(server2Client);
            }
        }

        private void HandleUDPReq(byte[] request, Pipe server2Client)
        {
            string clientIp = server2Client.ClientTo.Client.RemoteEndPoint.ToString().Split(':')[0];
            int clientPort = GetPort(request);
            byte[] data = GetUDPData(request);
            if(data == null)
            {
                CloseRequest(server2Client);
            }
            else
            {
                try
                {
                    int bondPort = rand.Next(5000, 10000);
                    UdpPipe pipeFromClient = new UdpPipe(clientIp, clientPort, ServerIP, bondPort);
                    pipeFromClient.Flow();
                    IPAddress.TryParse(ServerIP, out IPAddress ipa);
                    int high = bondPort / 0x100;
                    int low = bondPort % 0x100;
                    string reply = "\x05\x01\x00\x01" + ipa.GetAddressBytes() + high + low;
                    server2Client.Write(reply);
                }
                catch
                {
                    CloseRequest(server2Client);
                }
            }
        }

        private void CloseRequest(Pipe server2Client)
        {
            string reply = "\u0005\u0001\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
            server2Client.Write(reply);
        }

        public static string GetIP(byte[] request)
        {
            try
            {
                int destype = request[3];
                string ip;
                switch (destype)
                {
                case 1:
                    ip = request[4].ToString();
                    ip += '.' + request[5].ToString();
                    ip += '.' + request[6].ToString();
                    ip += '.' + request[7].ToString();
                    break;
                case 3:
                    int len = request[4];
                    char[] hostChars = new char[len];
                    for(int i = 0; i<len; ++i)
                    {
                        hostChars[i] = (char)request[5 + i];
                    }
                    string host = new string(hostChars);
                    // if host is ip but not real domain name
                    if(IPAddress.TryParse(host, out IPAddress ipa))
                    {
                        ip = host;
                    }
                    else
                    {
                        IPHostEntry iphe = Dns.GetHostEntry(host);
                        ip = iphe.AddressList[0].ToString();
                    }
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

        public static int GetPort(byte[] request)
        {
            try
            {
                int destype = request[3];
                int port;
                int high;
                int low;
                switch (destype)
                {
                case 1:
                    high = request[8];
                    low = request[9];
                    port = high * 0x100 + low;
                    break;
                case 3:
                    int len = request[4];
                    high = request[5 + len];
                    low = request[6 + len];
                    port = high * 0x100 + low;
                    break;
                default:
                    port = 0;
                    break;
                }
                return port;
            }
            catch
            {
                return 0;
            }
        }

        public static byte[] GetUDPData(byte[] request)
        {
            try
            {
                int destype = request[3];
                byte[] data;
                switch (destype)
                {
                case 1:
                    data = new byte[request.Length - 10];
                    request.CopyTo(data, 10);
                    break;
                case 3:
                    int len = request[4];
                    data = new byte[request.Length - 7 - len];
                    request.CopyTo(data, 7 + len);
                    break;
                default:
                    data = null;
                    break;
                }
                return data;
            }
            catch
            {
                return null;
            }
        }
    }
}