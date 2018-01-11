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
        public string ServerHost { get; set;}
        public int ServerPort { get; set;}
        public string LocalHost { get; set;}
        public int LocalPort { get; set;}
        private bool Running { get; set;}

        public Client(
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

        private TcpClient Connect2Server(string serverhost, int port)
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
            return client;
        }

        public void Start()
        {
            Thread startThread = new Thread(_Start);
            startThread.IsBackground = true;
            Running = true;
            startThread.Start();
        }

        public void _Start()
        {
            TcpClient client = null;

            try
            {
                while(true)
                {
                    try
                    {
                        if(!IPAddress.TryParse(LocalHost, out IPAddress ipa))
                        {
                            Console.WriteLine("invalid local ip: " + LocalHost);
                            return;
                        }
                        localServer = new TcpListener(ipa, LocalPort);
                        localServer.Start(100);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Retrying...");
                        Thread.Sleep(5000);
                        continue;
                    }
                    break;
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if(localServer != null)
                {
                    localServer.Stop();
                }
                return;
            }

            
            Console.WriteLine("Local Server Started.");
            try
            {
                while(Running)
                {
                    client = localServer.AcceptTcpClient();
                    Thread handleClientThread = new Thread(HandleClient);
                    handleClientThread.IsBackground = true;
                    handleClientThread.Start(client);
                }
                Thread.Sleep(1000);
                Console.WriteLine("Local Server stopped.");
            }
            catch
            {
                ;
            }
            localServer.Stop();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private void HandleClient(object clientObj)
        {
            TcpClient client2Client = clientObj as TcpClient;
            TcpClient client2Server = Connect2Server(ServerHost, ServerPort);
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