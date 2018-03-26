using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {
    public class Authen_ClientRelay : ClientRelay {
        public Authen_ClientRelay (IPEndPoint[] remoteIPEPs, IPEndPoint localIPEP):
            base (remoteIPEPs, localIPEP) { }

        protected override bool Authenticate (Socket socket2Server) {
            bool result = false;
            if (socket2Server != null) {
                if (base.Authenticate (socket2Server)) {
                    if (Conf.Users.Count > 0) {
                        TunnelUser firstUser = Conf.Users.Values.First ();
                        string id = firstUser.ID;
                        string pswd = firstUser.Password;
                        WriteStr (socket2Server, id + ':' + pswd);
                        string reply = ReadStr (socket2Server);
                        result = reply == "valid";
                    }
                }
            }
            return result;
        }

        protected override void PrintServerInfo (IPEndPoint localIPEP) {
            Console.WriteLine ("Authen Client Relay started: " + serverIPEP.ToString ());
        }
    }
}