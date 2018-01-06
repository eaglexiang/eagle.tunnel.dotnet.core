using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Client
    {
        private TcpListener localServer;
        private string ServerHost { get; set;}
        private int ServerPort { get; set;}

        public Client()
        {
            ;
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

        public void Start(
            string serverhost,int serverport,
            string localhost, int localport, int localbacklog
        )
        {
            ServerHost = serverhost;
            ServerPort = serverport;

            IPAddress ipa = IPAddress.Parse(localhost);
            localServer = new TcpListener(ipa, localport);
            localServer.Start(localbacklog);

            Console.WriteLine("local server started");

            while(true)
            {
                TcpClient client = localServer.AcceptTcpClient();
                NetworkStream stream2Client = client.GetStream();
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(stream2Client);
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