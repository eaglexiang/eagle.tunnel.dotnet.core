using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    public abstract class Relay {
        protected IPEndPoint serverIPEP;
        public bool IsRunning { get; set; }

        public Relay (IPEndPoint ipep) {
            serverIPEP = ipep;
            IsRunning = false;
        }

        /// <summary>
        /// Start Server on new background thread.
        /// </summary>
        public virtual void Start (int backlog) {
            Thread startThread = new Thread (_Start);
            startThread.IsBackground = true;
            startThread.Start (backlog);
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
            PrintServerInfo(serverIPEP);

            Listen (serverSocket);

            Thread.Sleep (1000);
            if (serverSocket.Connected) {
                serverSocket.Close ();
            }
            Console.WriteLine ("Server Stopped");
        }

        protected virtual void PrintServerInfo(IPEndPoint localIPEP)
        {
            Console.WriteLine ("Relay started: " + serverIPEP.ToString ());
        }

        protected virtual void Listen (Socket server) {
            IsRunning = true;
            while (IsRunning) {
                Socket client;
                try {
                    client = server.Accept ();
                } catch (SocketException) { continue; }

                Thread handleClientThread = new Thread (HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start (client);
            }
        }

        public void Close () {
            IsRunning = false;
        }

        protected abstract void HandleClient (object clientObj);

        protected abstract bool WorkFlow (Pipe pipeClient2Server, Pipe pipeServer2Client);

        protected static string ReadStr (Socket client) {
            if (client != null)
            {
                int count;
                byte[] buffer = new byte[1024];
                try {
                    count = client.Receive(buffer);
                } catch (SocketException) {
                    return null;
                }
                string str = Encoding.UTF8.GetString (buffer, 0, count);
                return str;
            }
            else
            {
                return null;
            }
        }

        protected static bool WriteStr (Socket client, string str) {
            if (client != null) {
                byte[] buffer = Encoding.UTF8.GetBytes (str);
                try {
                    client.Send (buffer);
                    return true;
                } catch (SocketException se) {
                    Console.WriteLine (se.Message);
                    return false;
                }
            } else {
                return false;
            }
        }

        protected static Socket CreateSocketConnect (IPEndPoint remoteIPEP) {
            Socket client = new Socket (AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            try {

                client.Connect (remoteIPEP);
                return client;
            } catch (SocketException se) {
                Console.WriteLine (se.Message);
                return null;
            }
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
                if (req != null) {
                    string[] address = req.Split (':');
                    if (address.Length == 2) {
                        if (IPAddress.TryParse (address[0], out IPAddress ipa)) {
                            if (int.TryParse (address[1], out int port)) {
                                if (WriteStr (socket2Client, "ok")) {
                                    return new IPEndPoint (ipa, port);
                                }
                            }
                        }
                    }
                    WriteStr (socket2Client, "nok");
                }
            }
            return null;
        }

        protected bool SendReqEndPoint (Socket socket2Server, IPEndPoint reqEndPoint) {
            if (reqEndPoint != null) {
                if (WriteStr (socket2Server, reqEndPoint.ToString ())) {
                    string reply = ReadStr (socket2Server);
                    return reply == "ok";
                }
            }
            return false;
        }
    }
}