using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace eagle.tunnel.dotnet.core
{
    public class Authen_ServerRelay : ServerRelay
    {
        public Authen_ServerRelay(IPEndPoint ipep) : base(ipep) { }

        protected override bool Authenticate(Socket client, out string user)
        {
            user = null;
            if (Conf.allConf.ContainsKey("users"))
            {
                string req = ReadStr(client);
                if (req != null)
                {
                    string[] reqUser = req.Split(':');
                    if (reqUser.Length == 2)
                    {
                        string id = reqUser[0];
                        string pswd = reqUser[1];
                        if (Authenticate(id, pswd))
                        {
                            user = id;
                            return WriteStr(client, "valid");
                        }
                        else
                        {
                            WriteStr(client, "invalid");
                        }
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool Authenticate(string id, string pswd)
        {
            string rightPswd;
            try
            {
                rightPswd = Conf.Users[id].Password;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }

            return rightPswd == pswd;
        }

        protected override void PrintServerInfo(IPEndPoint localIPEP)
        {
            Console.WriteLine ("Authen Server Relay started: " + serverIPEP.ToString ());
        }
    }
}