using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class ClientRelay : Relay
    {
        protected IPEndPoint[] remoteAddresses;
        private int indexOfRemoteAddresses;
        private object lockOfIndex;

        public ClientRelay(
            IPEndPoint[] remoteaddresses,
            IPEndPoint localaddress
        ) : base(localaddress)
        {
            remoteAddresses = remoteaddresses;
            indexOfRemoteAddresses = 0;
            lockOfIndex = new object();
        }

        protected int GetIndexOfRemoteAddresses()
        {
            if (remoteAddresses.Length > 1)
            {
                lock(lockOfIndex)
                {
                    indexOfRemoteAddresses += 1;
                    indexOfRemoteAddresses %= remoteAddresses.Length;
                }
            }
            return indexOfRemoteAddresses;
        }

        protected IPEndPoint GetRemoteIPEndPoint()
        {
            return remoteAddresses[GetIndexOfRemoteAddresses()];
        }

        public override void Start(int backlog)
        {
            Console.WriteLine("Find Remote Server(s): {0}", remoteAddresses.Length);
            base.Start(backlog);
        }

        protected override void HandleClient(object socketObj)
        {
            Socket socket2Client = socketObj as Socket;
            Socket socket2Server = CreateSocketConnect(
                    GetRemoteIPEndPoint());

            if (Authenticate(socket2Server))
            {
                Pipe pipeClient2Server = new Pipe(socket2Client,
                    socket2Server);
                pipeClient2Server.EncryptTo = true;
                Pipe pipeServer2Client = new Pipe(socket2Server,
                    socket2Client);
                pipeServer2Client.EncryptFrom = true;

                if (WorkFlow(pipeClient2Server, pipeServer2Client))
                {
                    pipeClient2Server.Flow();
                    pipeServer2Client.Flow();
                }
                else
                {
                    pipeClient2Server.Close();
                    pipeServer2Client.Close();
                }
            }
            else
            {
                socket2Client.Close();
            }
        }

        protected virtual bool Authenticate(Socket socket2Server) { return socket2Server != null;}

        protected override bool WorkFlow(Pipe pipeClient2Server, Pipe pipeServer2Client) { return true;}

        protected override void PrintServerInfo(IPEndPoint localIPEP)
        {
            Console.WriteLine ("Client Relay started: " + serverIPEP.ToString ());
        }
    }
}