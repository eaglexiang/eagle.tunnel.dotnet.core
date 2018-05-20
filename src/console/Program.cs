using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    class Program {
        public static string Version { get; } = "2.1.0";
        public static void Main (string[] args) {
            if (args.Length >= 1) {
                switch (args[0]) {
                    case "-v":
                        PrintVersion ();
                        break;
                    case "-h":
                        PrintGuide ();
                        break;
                    default:
                        Conf.Init (args[0]);
                        Server.Start (Conf.LocalAddresses);
                        break;
                }
            } else {
                PrintGuide ();
            }
        }

        private static void PrintVersion () {
            Console.WriteLine ("Eagle Tunnel\n");
            Console.WriteLine ("UI Version: {0}", Version);
            Console.WriteLine ("Lib Version: {0}\n", Server.Version);
        }

        private static void PrintGuide () {
            PrintVersion ();
            Console.WriteLine ("usage: ");
            Console.WriteLine ("dotnet eagle.tunnel.dotnet.dll [option]\n");
            Console.WriteLine ("options:");
            Console.WriteLine ("[file path]\trun eagle-tunnel with specific configuration file.");
            Console.WriteLine ("-h\tshow this guide.");
            Console.WriteLine ("-v\tshow version.");
        }
    }
}