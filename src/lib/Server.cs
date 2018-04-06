using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    public class Server {
        private static Queue<Tunnel> clients;
        private static List<Socket> servers;
        private static bool IsRunning { get; set; } // Server will keep running.
        public static bool IsWorking {get; private set;} // Server has started working.

        public static void Start (IPEndPoint[] localAddress) {
            if (!IsRunning) {
                if (localAddress != null) {
                    clients = new Queue<Tunnel> ();
                    servers = new List<Socket> ();
                    IsRunning = true;

                    Thread threadLimitCheck = new Thread (LimitSpeed);
                    threadLimitCheck.IsBackground = true;
                    threadLimitCheck.Start ();

                    Socket server;
                    for (int i = 1; i < localAddress.Length; ++i) {
                        server = CreateServer (localAddress[i]);
                        Thread thread = new Thread (Listen);
                        thread.IsBackground = true;
                        thread.Start (server);
                    }
                    server = CreateServer (localAddress[0]);
                    IsWorking = true;
                    Listen (server);
                }
            }
        }

        private static Socket CreateServer (IPEndPoint ipep) {
            Socket server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool done = false;
            do {
                try {
                    server.Bind (ipep);
                    done = true;
                } catch {
                    Thread.Sleep (60000); // wait for 60s to retry.
                }
                break;
            } while (done);
            lock (servers) {
                servers.Add (server);
            }
            Console.WriteLine ("Server Started: {0}", ipep.ToString ());
            return server;
        }

        private static void Listen (object socket2Listen) {
            Socket server = socket2Listen as Socket;
            server.Listen (100);
            while (IsRunning) {
                try {
                    Socket client = server.Accept ();
                    HandleClient (client);
                } catch { break; }
            }
        }

        private static void HandleClient (Socket socket2Client) {
            lock (clients) {
                while (clients.Count > Conf.maxClientsCount) {
                    Tunnel tunnel2Close = clients.Dequeue ();
                    tunnel2Close.Close ();
                }
            }
            Thread threadHandleClient = new Thread (_handleClient);
            threadHandleClient.IsBackground = true;
            threadHandleClient.Start (socket2Client);
        }

        private static void _handleClient (object socket2ClientObj) {
            Socket socket2Client = socket2ClientObj as Socket;
            byte[] buffer = new byte[1024];
            int read;
            try {
                read = socket2Client.Receive (buffer);
            } catch { read = 0; }
            if (read > 0) {
                byte[] req = new byte[read];
                Array.Copy (buffer, req, read);
                Tunnel tunnel = RequestHandler.Handle (req, socket2Client);
                if (tunnel != null) {
                    tunnel.Flow ();
                    lock (clients) {
                        clients.Enqueue (tunnel);
                    }
                }
            }
        }

        public static double Speed () {
            double speed = 0;
            if (Conf.Users != null) {
                foreach (EagleTunnelUser item in Conf.Users.Values) {
                    speed += item.Speed ();
                }
            }
            if(Conf.LocalUser!=null)
            {
                speed += Conf.LocalUser.Speed();
            }
            return speed;
        }

        private static void LimitSpeed () {
            if (Conf.allConf.ContainsKey ("speed-check")) {
                if (Conf.allConf["speed-check"][0] == "on") {
                    while (IsRunning) {
                        foreach (EagleTunnelUser item in Conf.Users.Values) {
                            item.LimitSpeed ();
                        }
                        Thread.Sleep (5000);
                    }
                }
            }
        }

        public static void Close () {
            if (IsRunning) {
                IsRunning = false;
                Thread.Sleep (1000);
                // stop listening
                lock (servers) {
                    foreach (Socket item in servers) {
                        try {
                            item.Close ();
                        } catch {; }
                    }
                }
                // shut down all connections
                lock (clients) {
                    while (clients.Count > 0) {
                        Tunnel tunnel2Close = clients.Dequeue ();
                        if (tunnel2Close.IsWorking) {
                            tunnel2Close.Close ();
                        }
                    }
                }
            }
        }
    }
}