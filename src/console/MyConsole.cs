using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    public class MyConsole {
        private UnitedClient localRelay;
        private Authen_ServerRelay remoteRelay;

        private IPEndPoint[] remoteIPEPs;
        private IPEndPoint[] localIPEPs;

        public MyConsole () { }

        public void Run (string command) {
            Console.WriteLine ("command:\t{0}", command);

            switch (command) {
                case "s":
                    StartServerRelay ();
                    break;
                case "c":
                    StartLocalRelay();
                    break;
                case "close":
                    CloseAll ();
                    break;
                default:
                    break;
            }
        }

        private void CloseAll () {
            if (localRelay != null) {
                localRelay.Close ();
            }
            if (remoteRelay != null) {
                remoteRelay.Close ();
            }
        }

        private void StartServerRelay () {
            if (remoteIPEPs.Length > 0) {
                remoteRelay = new Authen_ServerRelay (remoteIPEPs[0]);
                remoteRelay.Start (100);
            }
        }

        private void StartLocalRelay () {
            if (remoteIPEPs.Length > 0) {
                localRelay = new UnitedClient (remoteIPEPs, localIPEPs[0]);
                localRelay.Start (100);
            }
        }

        public static void Wait () {
            while (true) {
                Thread.Sleep (100000);
            }
        }

        public void Init (string arg) {
            if (Regex.IsMatch (arg, @"\bc=*")) {
                Conf.confPath = arg.Substring (2);
            }
            Conf.Init ();

            try {
                List<string> remoteAddresses = Conf.allConf["Remote Address"];
                remoteIPEPs = CreateEndPoints (remoteAddresses);
            } catch (KeyNotFoundException) {
                Console.WriteLine ("Remote Address not found.");
            }
            try {
                List<string> localAddresses = Conf.allConf["Local Address"];
                localIPEPs = CreateEndPoints (localAddresses);

            } catch (KeyNotFoundException) {
                Console.WriteLine ("Local Address not found");
            }
        }

        private static IPEndPoint[] CreateEndPoints (List<string> addresses) {
            ArrayList list = new ArrayList ();
            foreach (string address in addresses) {
                string[] endpoints = address.Split (':');
                if (endpoints.Length >= 2) {
                    if (IPAddress.TryParse (endpoints[0], out IPAddress ipa)) {
                        if (int.TryParse (endpoints[1], out int port)) {
                            IPEndPoint ipep = new IPEndPoint (ipa, port);
                            list.Add (ipep);
                        }
                    }
                }
            }
            return list.ToArray (typeof (IPEndPoint)) as IPEndPoint[];
        }
    }
}