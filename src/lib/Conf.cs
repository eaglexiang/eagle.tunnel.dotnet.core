using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace eagle.tunnel.dotnet.core {
    public class Conf {
        public static EagleTunnelUser LocalUser { get; set; }
        public static bool EnableSOCKS { get; set; }
        public static bool EnableHTTP { get; set; }
        public static bool EnableEagleTunnel { get; set; }
        public static Dictionary<string, List<string>> allConf =
            new Dictionary<string, List<string>> (StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, EagleTunnelUser> Users =
            new Dictionary<string, EagleTunnelUser> ();
        public static int maxSocketTimeout = 5000;
        public static int maxClientsCount = 100;
        public static IPEndPoint[] localAddresses;
        private static IPEndPoint[] remoteAddresses;

        private static object lockOfIndex;

        private static int indexOfRemoteAddresses;
        private static int GetIndexOfRemoteAddresses () {
            if (remoteAddresses.Length > 1) {
                lock (lockOfIndex) {
                    indexOfRemoteAddresses += 1;
                    indexOfRemoteAddresses %= remoteAddresses.Length;
                }
            }
            return indexOfRemoteAddresses;
        }

        public static IPEndPoint GetRemoteIPEndPoint () {
            return remoteAddresses[GetIndexOfRemoteAddresses ()];
        }

        public static void Init (string confPath = "./config.txt") {
            ReadAll (confPath);

            if (allConf.ContainsKey ("user-conf")) {
                ImportUsers ();
                Console.WriteLine ("find user(s): {0}", Users.Count);
            }

            LocalUser = null;
            if (allConf.ContainsKey ("user")) {
                if (EagleTunnelUser.TryParse(allConf["user"][0], out EagleTunnelUser user))
                {
                    LocalUser = user;
                }
            }
            if (LocalUser!=null)
            {
                Console.WriteLine("User: {0}", LocalUser.ID);
            }

            if (allConf.ContainsKey ("worker")) {
                if (int.TryParse (allConf["worker"][0], out int workerCount)) {
                    maxClientsCount = workerCount;
                }
            }
            Console.WriteLine ("worker: {0}", maxClientsCount);

            try {
                List<string> remoteAddressStrs = Conf.allConf["remote address"];
                remoteAddresses = CreateEndPoints (remoteAddressStrs);
            } catch (KeyNotFoundException) {
                Console.WriteLine ("Remote Address not found.");
            }
            try {
                List<string> localAddressStrs = Conf.allConf["local address"];
                localAddresses = CreateEndPoints (localAddressStrs);
                lockOfIndex = new object ();

            } catch (KeyNotFoundException) {
                Console.WriteLine ("Local Address not found");
            }

            if (allConf.ContainsKey ("socks")) {
                if (allConf["socks"][0] == "on") {
                    EnableSOCKS = true;
                }
            }
            Console.WriteLine ("SOCKS Switch: {0}", EnableSOCKS.ToString ());

            if (allConf.ContainsKey ("http")) {
                if (allConf["http"][0] == "on") {
                    EnableHTTP = true;
                }
            }
            Console.WriteLine ("HTTP Switch: {0}", EnableHTTP.ToString ());

            if (allConf.ContainsKey ("eagle tunnel")) {
                if (allConf["eagle tunnel"][0] == "on") {
                    EnableEagleTunnel = true;
                }
            }
            Console.WriteLine ("Eagle Tunnel Switch: {0}", EnableEagleTunnel.ToString ());
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
        private static void ImportUsers () {
            if (allConf["user-conf"].Count >= 1) {
                string pathOfUsersConf = allConf["user-conf"][0];
                if (File.Exists (pathOfUsersConf)) {
                    string usersText = File.ReadAllText (pathOfUsersConf);
                    usersText = usersText.Replace ("\r\n", "\n");
                    string[] usersArray = usersText.Split ('\n');
                    usersArray = Format (usersArray);
                    foreach (string line in usersArray)
                    {
                        if(EagleTunnelUser.TryParse(line,out EagleTunnelUser user))
                        {
                            if (!Users.ContainsKey(user.ID))
                            {
                                Users.Add(user.ID, user);
                            }
                        }
                    }
                } else {
                    Console.WriteLine ("user-conf file not found: {0}", pathOfUsersConf);
                }
            }
        }

        public static string[][] SplitStrs (string[] src, char sig) {
            ArrayList list = new ArrayList ();
            for (int i = 0; i < src.Length; ++i) {
                string[] tmp = src[i].Split (sig);
                list.Add (tmp);
            }
            return list.ToArray (typeof (string[])) as string[][];
        }

        /// <summary>
        /// Read all configurations from file
        /// </summary>
        /// <param name="confPath">path of conf file</param>
        private static void ReadAll (string confPath) {
            if (File.Exists (confPath)) {
                string allConfText = File.ReadAllText (confPath);
                allConfText = allConfText.Replace ("\r\n", "\n");
                string[] lines = allConfText.Split ('\n');
                // create array for [key : value]
                string[][] arg_lines = SplitStrs (lines, '=');
                foreach (string[] arg_line in arg_lines) {
                    if (arg_line.Length == 2) {
                        AddValue (arg_line[0], arg_line[1]);
                    }
                }
            }
        }

        private static string[] Format (string[] lines) {
            ArrayList newLines = new ArrayList ();
            foreach (string line in lines) {
                string validline = line;
                if (validline.Contains ("#")) {
                    int index = validline.IndexOf ("#");
                    validline = validline.Substring (0, index);
                }
                validline = validline.Trim ();
                newLines.Add (validline);
            }
            return newLines.ToArray (typeof (string)) as string[];
        }

        private static void AddValue (string key, string value) {
            if (!allConf.ContainsKey (key)) {
                allConf.Add (key, new List<string> ());
            }
            allConf[key].Add (value);
        }
    }
}