using System;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {
    public class ServerRelay : Relay {
        public ServerRelay (IPEndPoint serverIPEP) : base (serverIPEP) { }

        protected override void HandleClient (Socket socket2Client) {
            if (socket2Client != null) {
                if (Authenticate (socket2Client, out string user)) {
                    Pipe pipeClient2Server = new Pipe (socket2Client,
                        null, user);
                    pipeClient2Server.EncryptFrom = true;
                    Pipe pipeServer2Client = new Pipe (null,
                        socket2Client,
                        user);
                    pipeServer2Client.EncryptTo = true;

                    IPEndPoint reqIPEP = ReceiveReqEndPoint (socket2Client);
                    if (reqIPEP != null) {
                        Socket socket2Server = CreateSocketConnect (reqIPEP);
                        if (socket2Server != null) {
                            pipeClient2Server.SocketTo = socket2Server;
                            pipeServer2Client.SocketFrom = socket2Server;

                            if (WorkFlow (pipeClient2Server, pipeServer2Client)) {
                                pipeClient2Server.Flow ();
                                pipeServer2Client.Flow ();
                            } else {
                                pipeClient2Server.Close ();
                                pipeServer2Client.Close ();
                            }
                        } else {
                            pipeClient2Server.Close ();
                            pipeServer2Client.Close ();
                        }
                    } else {
                        pipeClient2Server.Close ();
                        pipeServer2Client.Close ();
                    }
                } else {
                    socket2Client.Close ();
                }
            }
        }

        protected virtual bool Authenticate (Socket socket2Client, out string user) {
            user = null;
            bool result = false;
            if (socket2Client != null) {
                string req = ReadStr (socket2Client);
                if (req == "eagle_tunnel_v2.0") {
                    if (WriteStr (socket2Client, "version_ok")) {
                        req = ReadStr (socket2Client);
                        if (req == "req_type_tcp") {
                            return WriteStr (socket2Client, "req_type_ok");
                        } else {
                            WriteStr (socket2Client, "req_type_invalid");
                        }
                    }
                } else {
                    WriteStr (socket2Client, "version_invalid");
                }
            }
            return result;
        }

        protected override bool WorkFlow (Pipe pipeClient2Server, Pipe pipeServer2Client) { return true; }

        protected override void PrintServerInfo (IPEndPoint localIPEP) {
            Console.WriteLine ("Server Relay started: " + serverIPEP.ToString ());
        }
    }
}