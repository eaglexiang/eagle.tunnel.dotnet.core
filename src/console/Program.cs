using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    class Program {
        private static string version = "2.1.0";
        public static void Main (string[] args) {
            if (args.Length >= 1) {
                if (args[0] == "-v") {
                    PrintVersion();
                }
                else
                {
                    Conf.Init (args[0]);
                    Server.Start (Conf.LocalAddresses);
                }
            } else {
                PrintGuide();
            }
        }

        public static string Version () {
            return version;
        }

        private static void PrintVersion () {
            Console.WriteLine("Eagle Tunnel\n");
            Console.WriteLine ("UI Version: {0}", Version ());
            Console.WriteLine ("Lib Version: {0}\n", Server.Version ());
        }

        private static void PrintGuide () {
            PrintVersion();
            Console.WriteLine("usage: ");
            Console.WriteLine("dotnet eagle.tunnel.dotnet.dll [option]\n");
            Console.WriteLine("options:");
            Console.WriteLine("[file path]\trun eagle tunnel with specific configuration file.");
            Console.WriteLine("-h\tshow this guide.");
            Console.WriteLine("-v\tshow version.");
        }
    }
}