using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace eagle.tunnel.dotnet.core {
    public class Conf {
        private static string confFilePath;
        private static EagleTunnelUser localUser;
        public static EagleTunnelUser LocalUser {
            get {
                return localUser;
            }
            set {
                localUser = value;
                Dirty = true;
            }
        }
        private static bool enableSOCKS;
        public static bool EnableSOCKS {
            get {
                return enableSOCKS;
            }
            set {
                enableSOCKS = value;
                Dirty = true;
            }
        }
        private static bool enableHTTP;
        public static bool EnableHTTP {
            get {
                return enableHTTP;
            }
            set {
                enableHTTP = value;
                Dirty = true;
            }
        }
        private static bool enableEagleTunnel;
        public static bool EnableEagleTunnel {
            get {
                return enableEagleTunnel;
            }
            set {
                enableEagleTunnel = value;
                Dirty = true;
            }
        }
        public static Dictionary<string, List<string>> allConf;
        public static Dictionary<string, EagleTunnelUser> Users;
        public static int maxClientsCount;
        private static IPEndPoint[] localAddresses;
        public static IPEndPoint[] LocalAddresses {
            get {
                return localAddresses;
            }
            set {
                localAddresses = value;
                Dirty = true;
            }
        }
        private static IPEndPoint[] remoteAddresses;
        public static IPEndPoint[] RemoteAddresses {
            get {
                return remoteAddresses;
            }
            set {
                remoteAddresses = value;
                Dirty = true;
            }
        }

        private static object lockOfIndex;
        private static bool Dirty { get; set; }

        public static bool LocalAddress_Set (string address) {
            bool result = false;
            if (address != null) {
                string[] args = address.Split (':');
                if (args.Length == 2) {
                    if (IPAddress.TryParse (args[0], out IPAddress ipa)) {
                        if (int.TryParse (args[1], out int port)) {
                            LocalAddresses = new IPEndPoint[1];
                            LocalAddresses[0] = new IPEndPoint (ipa, port);
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        public static bool RemoteAddress_Set (string address) {
            bool result = false;
            if (!string.IsNullOrEmpty (address)) {
                string[] args = address.Split (':');
                if (args.Length == 2) {
                    if (IPAddress.TryParse (args[0], out IPAddress ipa)) {
                        if (int.TryParse (args[1], out int port)) {
                            RemoteAddresses = new IPEndPoint[1];
                            RemoteAddresses[0] = new IPEndPoint (ipa, port);
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        private static int indexOfRemoteAddresses;
        private static int GetIndexOfRemoteAddresses () {
            int result = 0;
            if (RemoteAddresses != null) {
                if (RemoteAddresses.Length > 1) {
                    lock (lockOfIndex) {
                        indexOfRemoteAddresses += 1;
                        indexOfRemoteAddresses %= RemoteAddresses.Length;
                    }
                    result = indexOfRemoteAddresses;
                }
            }
            return result;
        }

        public static IPEndPoint GetRemoteIPEndPoint () {
            IPEndPoint result = null;
            if (RemoteAddresses != null) {
                result = RemoteAddresses[GetIndexOfRemoteAddresses ()];
            }
            return result;
        }

        public static void Init (string confPath = "/etc/eagle-tunnel.conf") {
            Dirty = false;
            allConf = new Dictionary<string, List<string>> (StringComparer.OrdinalIgnoreCase);
            confFilePath = confPath;
            ReadAll ();

            if (allConf.ContainsKey ("user-conf")) {
                Users =
                    new Dictionary<string, EagleTunnelUser> ();
                ImportUsers ();
                Console.WriteLine ("find user(s): {0}", Users.Count);
            }

            if (allConf.ContainsKey ("user")) {
                if (EagleTunnelUser.TryParse (allConf["user"][0], out EagleTunnelUser user)) {
                    localUser = user;
                }
            }
            if (LocalUser != null) {
                Console.WriteLine ("User: {0}", LocalUser.ID);
            }

            maxClientsCount = 200;
            if (allConf.ContainsKey ("worker")) {
                if (int.TryParse (allConf["worker"][0], out int workerCount)) {
                    maxClientsCount = workerCount;
                }
            }
            Console.WriteLine ("worker: {0}", maxClientsCount);

            try {
                List<string> remoteAddressStrs = Conf.allConf["relayer"];
                remoteAddresses = CreateEndPoints (remoteAddressStrs);
            } catch (KeyNotFoundException) {
                Console.WriteLine ("`Relayer` not found.");
            }
            if (RemoteAddresses != null) {
                Console.WriteLine ("Count of `Relayer`: {0}", RemoteAddresses.Length);
            }

            try {
                List<string> localAddressStrs = Conf.allConf["listen"];
                localAddresses = CreateEndPoints (localAddressStrs);
                lockOfIndex = new object ();

            } catch (KeyNotFoundException) {
                Console.WriteLine ("`Listen` not found");
            }

            if (allConf.ContainsKey ("socks")) {
                if (allConf["socks"][0] == "on") {
                    enableSOCKS = true;
                }
            }
            Console.WriteLine ("SOCKS Switch: {0}", EnableSOCKS.ToString ());

            if (allConf.ContainsKey ("http")) {
                if (allConf["http"][0] == "on") {
                    enableHTTP = true;
                }
            }
            Console.WriteLine ("HTTP Switch: {0}", EnableHTTP.ToString ());

            if (allConf.ContainsKey ("eagle tunnel")) {
                if (allConf["eagle tunnel"][0] == "on") {
                    enableEagleTunnel = true;
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
                    foreach (string line in usersArray) {
                        if (EagleTunnelUser.TryParse (line, out EagleTunnelUser user)) {
                            if (!Users.ContainsKey (user.ID)) {
                                Users.Add (user.ID, user);
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
        private static void ReadAll () {
            string confPath = confFilePath;
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
            } else {
                Console.WriteLine ("Conf file not found: {0}", confPath);
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
            key = key.Trim ();
            value = value.Trim ();
            if (!allConf.ContainsKey (key)) {
                allConf.Add (key, new List<string> ());
            }
            allConf[key].Add (value);
        }

        public static void Save () {
            if (Dirty) {
                if (confFilePath != null) {
                    string allConf = ToString ();
                    File.WriteAllText (confFilePath, allConf);
                }
            }
        }

        public static new string ToString () {
            string result = "";
            if (remoteAddresses != null) {
                result += "Relayer=" + remoteAddresses[0].ToString () + "\n";
            }
            if (localAddresses != null) {
                result += "Listen=" + localAddresses[0].ToString () + "\n";
            }
            if (enableSOCKS) {
                result += "socks=on\n";
            }
            if (enableHTTP) {
                result += "http=on\n";
            }
            if (enableEagleTunnel) {
                result += "eagle tunnel=on\n";
            }
            if (localUser != null) {
                result += "user=" + localUser.ToString () + "\n";
            }
            return result;
        }
    }
}