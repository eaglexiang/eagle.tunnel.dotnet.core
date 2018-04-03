using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    public class Server {
        private static Queue<Tunnel> clients;

        public static void Start (IPEndPoint[] localAddress) {
            if (localAddress != null) {
                clients = new Queue<Tunnel> ();
                Socket server;
                for (int i = 1; i < localAddress.Length; ++i) {
                    server = CreateServer (localAddress[i]);
                    Thread thread = new Thread (Listen);
                    thread.IsBackground = true;
                    thread.Start (server);
                }
                server = CreateServer (localAddress[0]);
                Listen (server);
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
            Console.WriteLine ("Server Started: {0}", ipep.ToString ());
            return server;
        }

        private static void Listen (object socket2Listen) {
            Socket server = socket2Listen as Socket;
            server.Listen (100);
            while (true) {
                Socket client = server.Accept ();
                HandleClient (client);
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
            byte[] buffer = new byte[100];
            int read;
            try {
                read = socket2Client.Receive (buffer);
            } catch { read = 0; }
            Tunnel tunnel;
            if (read > 0) {
                byte[] req = new byte[read];
                Array.Copy (buffer, req, read);
                tunnel = RequestHandler.Handle (req, socket2Client);
                if (tunnel != null) {
                    tunnel.Flow();
                    lock (clients) {
                        clients.Enqueue (tunnel);
                    }
                }
            }
        }
    }
}