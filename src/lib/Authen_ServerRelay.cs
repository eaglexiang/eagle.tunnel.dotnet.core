using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {
    public class Authen_ServerRelay : ServerRelay {
        public Authen_ServerRelay (IPEndPoint ipep) : base (ipep) { }

        protected override bool Authenticate (Socket client, out string user) {
            bool result = false;
            if (base.Authenticate (client, out user)) {
                if (Conf.allConf.ContainsKey ("UsersConf")) {
                    string req = ReadStr (client);
                    if (req != "") {
                        string[] reqUser = req.Split (':');
                        if (reqUser.Length == 2) {
                            string id = reqUser[0];
                            string pswd = reqUser[1];
                            if (Authenticate (id, pswd)) {
                                user = id;
                                result = WriteStr (client, "valid");
                            } else {
                                WriteStr (client, "invalid");
                            }
                        }
                    }
                } else {
                    result = true;
                }
            } 
            return result;
        }

        public static bool Authenticate (string id, string pswd) {
            if (Conf.Users.ContainsKey (id)) {
                return Conf.Users[id].Password == pswd;
            } else {
                return false;
            }
        }

        protected override void PrintServerInfo (IPEndPoint localIPEP) {
            Console.WriteLine ("Authen Server Relay started: " + serverIPEP.ToString ());
        }
    }
}