using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace eagle.tunnel.dotnet.core
{
    public class AuthenticationServer : Server
    {
        public AuthenticationServer(IPEndPoint ipep) : base(ipep) { }

        public AuthenticationServer(string url, int port) : base(url, port) { }

        protected override void _Start()
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
                    Console.WriteLine("Waiting for 10s...");
                    Thread.Sleep(10000);
                }
            }
            Console.WriteLine("server started: " + serverIPEP.ToString());

            Running = true;
            while(Running)
            {
                TcpClient client;
                try
                {
                    client = server.AcceptTcpClient();
                    if (!Authenticate(client))
                    {
                        client.Close();
                        continue;
                    }
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

        private static bool Authenticate(TcpClient client)
        {
            string id = ReadStr(client);
            string pswd = ReadStr(client);
            bool result = Authenticate(id, pswd);
            if (result)
            {
                WriteStr(client, "valid");
            }
            else
            {
                WriteStr(client, "invalid");
            }
            return result;
        }

        public static bool Authenticate(string id, string pswd)
        {
            string rightPswd;
            try
            {
                rightPswd = Conf.Users[id];
            }
            catch(KeyNotFoundException)
            {
                return false;
            }

            return rightPswd == pswd;
        }
    }
}