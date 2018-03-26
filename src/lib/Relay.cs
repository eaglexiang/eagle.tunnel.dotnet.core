using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    public abstract class Relay {
        private Thread thread_Main;
        protected IPEndPoint serverIPEP;
        public bool IsRunning { get; set; }
        private int clientsCount;
        private object lock_ClientsCount;

        public Relay (IPEndPoint ipep, int worker = 100) {
            serverIPEP = ipep;
            IsRunning = false;

            thread_Main = new Thread (_Start);
            thread_Main.IsBackground = true;
            clientsCount = 0;
            lock_ClientsCount = new object ();
        }

        /// <summary>
        /// Start Server on new background thread.
        /// </summary>
        public virtual void Start (int backlog = 200) {
            IsRunning = true;
            thread_Main.Start (backlog);
        }

        /// <summary>
        /// realization for function Start
        /// </summary>
        private void _Start (object _backlog) {
            int backlog = Convert.ToInt32 (_backlog);

            Socket serverSocket = CreateSocketListen (serverIPEP, backlog);
            while (serverSocket == null) {
                Console.WriteLine ("Waiting for 60s...");
                Thread.Sleep (60000);
                serverSocket = CreateSocketListen (serverIPEP, backlog);
            }
            PrintServerInfo (serverIPEP);

            Listen (serverSocket);

            if (serverSocket.Connected) {
                serverSocket.Shutdown (SocketShutdown.Both);
                Thread.Sleep (10);
                serverSocket.Close ();
            }
            Console.WriteLine ("Server Stopped");
        }

        protected virtual void PrintServerInfo (IPEndPoint localIPEP) {
            Console.WriteLine ("Relay started: " + serverIPEP.ToString ());
        }

        protected virtual void Listen (Socket server) {
            while (IsRunning) {
                Socket client;
                try {
                    client = server.Accept ();
                } catch (SocketException) { break; }

                Thread thread_HandleClient = new Thread (_HandleClient);
                thread_HandleClient.IsBackground = true;
                while (clientsCount > Conf.maxClientHandleThreads) { Thread.Sleep (1000); }
                thread_HandleClient.Start (client);
                lock (lock_ClientsCount) {
                    clientsCount++;
                }
            }
        }

        public void Close () {
            IsRunning = false;
        }

        private void _HandleClient (object socketObj) {
            Socket socket = socketObj as Socket;
            HandleClient (socket);
            lock (lock_ClientsCount) {
                clientsCount--;
            }
        }

        protected abstract void HandleClient (Socket socket2Client);

        protected abstract bool WorkFlow (Pipe pipeClient2Server, Pipe pipeServer2Client);

        protected static string ReadStr (Socket client) {
            string result = "";
            if (client != null) {
                int count;
                byte[] buffer = new byte[1024];
                try {
                    count = client.Receive (buffer);
                } catch (SocketException) {
                    count = 0;
                }
                if (count != 0) {
                    result = Encoding.UTF8.GetString (buffer, 0, count);
                }
            }
            return result;
        }

        protected static bool WriteStr (Socket client, string str) {
            bool result = false;
            if (client != null) {
                byte[] buffer = Encoding.UTF8.GetBytes (str);
                try {
                    client.Send (buffer);
                    result = true;
                } catch (SocketException se) {
                    Console.WriteLine (se.Message);
                }
            }
            return result;
        }

        protected Socket CreateSocketConnect (IPEndPoint remoteIPEP) {
            Socket client = new Socket (AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            try {
                client.Connect (remoteIPEP);
            } catch (SocketException se) {
                Console.WriteLine (se.Message);
                client = null;
            }
            return client;
        }

        protected static Socket CreateSocketListen (IPEndPoint localIPEP, int backlog = 100) {
            Socket server = new Socket (AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            try {
                server.Bind (localIPEP);
                server.Listen (backlog);
                return server;
            } catch (SocketException se) {
                Console.WriteLine (se.Message);
                return null;
            }
        }

        protected IPEndPoint ReceiveReqEndPoint (Socket socket2Client) {
            if (socket2Client != null) {
                string req = ReadStr (socket2Client);
                if (req != "") {
                    string[] address = req.Split (':');
                    if (address.Length == 2) {
                        if (IPAddress.TryParse (address[0], out IPAddress ipa)) {
                            if (int.TryParse (address[1], out int port)) {
                                if (WriteStr (socket2Client, "req_ep_ok")) {
                                    return new IPEndPoint (ipa, port);
                                }
                            }
                        }
                    }
                    WriteStr (socket2Client, "req_ep_nok");
                }
            }
            return null;
        }

        protected bool SendReqEndPoint (Socket socket2Server, IPEndPoint reqEndPoint) {
            if (reqEndPoint != null) {
                if (WriteStr (socket2Server, reqEndPoint.ToString ())) {
                    string reply = ReadStr (socket2Server);
                    return reply == "req_ep_ok";
                }
            }
            return false;
        }
    }
}