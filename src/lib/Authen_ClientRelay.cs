using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {
    public class Authen_ClientRelay : ClientRelay {
        public Authen_ClientRelay (IPEndPoint[] remoteIPEPs, IPEndPoint localIPEP):
            base (remoteIPEPs, localIPEP) { }

        protected override bool Authenticate (Socket socket2Server) {
            if (socket2Server != null) {
                TunnelUser firstUser = Conf.Users.Values.First ();
                string id = firstUser.ID;
                string pswd = firstUser.Password;
                WriteStr (socket2Server, id + ':' + pswd);
                string result = ReadStr (socket2Server);
                return result == "valid";
            } else {
                return false;
            }
        }

        protected override void PrintServerInfo(IPEndPoint localIPEP)
        {
            Console.WriteLine ("Authen Client Relay started: " + serverIPEP.ToString ());
        }
    }
}