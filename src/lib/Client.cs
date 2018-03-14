using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Client : Server
    {
        public string remoteIP { get; set;}
        public int remotePort { get; set;}

        public Client(
            string serverhost, int serverport,
            string localhost, int localport
        ) : base(localhost, localport)
        {
            remoteIP = serverhost;
            remotePort = serverport;
        }

        private TcpClient Connect2Server(string serverhost, int port)
        {
            try
            {
                TcpClient client = new TcpClient(serverhost, port);
                return client;
            }
            catch (SocketException se)
            {
                Console.WriteLine("error:\tfail to connect to server");
                Console.WriteLine(se.Message);
                return null;
            }
        }

        protected override void HandleClient(object clientObj)
        {
            TcpClient client2Client = clientObj as TcpClient;
            TcpClient client2Server = Connect2Server(remoteIP, remotePort);
            if(client2Server == null)
            {
                return;
            }

            Pipe pipe0 = new Pipe(
                client2Client,
                client2Server
            );
            pipe0.EncryptTo = true;
            Pipe pipe1 = new Pipe(
                client2Server,
                client2Client
                );
            pipe0.EncryptTo = true;
            pipe1.EncryptFrom = true;

            pipe0.Flow();
            pipe1.Flow();
        }

        public void Stop()
        {
            Running = false;
            Console.WriteLine("quitting...");
        }
    }
}