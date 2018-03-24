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
        private ManualResetEvent connectDone = new ManualResetEvent (false);

        public Relay (IPEndPoint ipep, int worker = 100) {
            serverIPEP = ipep;
            IsRunning = false;

            thread_Main = new Thread (_Start);
            thread_Main.IsBackground = true;
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

                Thread thread_HandleClient = new Thread(_HandleClient);
                thread_HandleClient.IsBackground = true;
                thread_HandleClient.Start(client);
            }
        }

        public void Close () {
            IsRunning = false;
        }

        private void _HandleClient(object socketObj)
        {
            Socket socket = socketObj as Socket;
            HandleClient(socket);
        }

        protected abstract void HandleClient (Socket socket2Client);

        protected abstract bool WorkFlow (Pipe pipeClient2Server, Pipe pipeServer2Client);

        protected static string ReadStr (Socket client) {
            DateTime now;
            if (Conf.IsDebug) {
                now = DateTime.Now;
            }

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

            if (Conf.IsDebug) {
                double sec = (DateTime.Now - now).TotalSeconds;
                if (sec > Conf.DebugTimeThreshold) {
                    Console.WriteLine ("Time for Relay.ReadStr is {0} s", sec);
                }
            }
            return result;
        }

        protected static bool WriteStr (Socket client, string str) {
            DateTime now;
            if (Conf.IsDebug) {
                now = DateTime.Now;
            }

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

            if (Conf.IsDebug) {
                double sec = (DateTime.Now - now).TotalSeconds;
                if (sec > Conf.DebugTimeThreshold) {
                    Console.WriteLine ("Time for Relay.WriteStr is {0} s", sec);
                }
            }

            return result;
        }

        protected Socket CreateSocketConnect (IPEndPoint remoteIPEP) {
            DateTime now;
            if (Conf.IsDebug) {
                now = DateTime.Now;
            }

            Socket client = new Socket (AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            try {
                // connectDone.Reset ();
                // client.BeginConnect (remoteIPEP, new AsyncCallback (CreateSocketConnectCallback), client);
                // connectDone.WaitOne (5000, false);
                // if (!client.Connected) {
                //     client = null;
                // }
                client.Connect(remoteIPEP);
            } catch (SocketException se) {
                Console.WriteLine (se.Message);
                client = null;
            }

            if (Conf.IsDebug) {
                double sec = (DateTime.Now - now).TotalSeconds;
                if (sec > Conf.DebugTimeThreshold) {
                    Console.WriteLine ("Time to connect to {0} is : {1} s", remoteIPEP.ToString (), sec);
                }
            }

            return client;
        }

        // private void CreateSocketConnectCallback (IAsyncResult ar) {
        //     try {
        //         Socket client = ar.AsyncState as Socket;
        //         client.EndConnect (ar);

        //     } catch {; } finally {
        //         connectDone.Set ();
        //     }
        // }

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