using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class HttpClient
    {
        private TcpListener localServer;
        public string ServerHost { get; set;}
        public int ServerPort { get; set;}
        public string LocalHost { get; set;}
        public int LocalPort { get; set;}

        public HttpClient(
            string serverhost,
            int serverport,
            string localhost,
            int localport
        )
        {
            ServerHost = serverhost;
            ServerPort = serverport;
            LocalHost = localhost;
            LocalPort = localport;
        }

        private NetworkStream Connect2Server(string serverhost, int port)
        {
            TcpClient client;
            try
            {
                client = new TcpClient(serverhost, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error:\tfail to connect to server");
                Console.WriteLine(ex.Message);
                return null;
            }

            NetworkStream stream2Server = client.GetStream();
            return stream2Server;
        }

        public void Start()
        {
            Thread startThread = new Thread(_Start);
            startThread.IsBackground = true;
            startThread.Start();
        }

        public void _Start()
        {
            TcpClient client = null;
            NetworkStream stream2Client = null;

            try
            {
                IPAddress ipa = IPAddress.Parse(LocalHost);
                localServer = new TcpListener(ipa, LocalPort);
                localServer.Start(100);

                while(true)
                {
                    client = localServer.AcceptTcpClient();
                    stream2Client = client.GetStream();
                    Thread handleClientThread = new Thread(HandleClient);
                    handleClientThread.IsBackground = true;
                    handleClientThread.Start(stream2Client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if(stream2Client != null)
                {
                    stream2Client.Close();
                }
                if(client != null)
                {
                    client.Close();
                }
                if(localServer != null)
                {
                    localServer.Stop();
                }
            }
        }

        private void HandleClient(object streamObj)
        {
            NetworkStream stream2Server = Connect2Server(ServerHost, ServerPort);
            if(stream2Server == null)
            {
                return;
            }
            NetworkStream stream2Client = streamObj as NetworkStream;

            Pipe pipe0 = new Pipe(stream2Client, stream2Server);
            pipe0.EncryptTo = true;
            Pipe pipe1 = new Pipe(stream2Server, stream2Client);
            pipe1.EncryptFrom = true;

            pipe0.Flow();
            pipe1.Flow();
        }
    }
}