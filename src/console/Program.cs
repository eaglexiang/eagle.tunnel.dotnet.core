using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    class Program {
        private static string version = "2.0.0";
        public static void Main (string[] args) {
            if (args.Length >= 1) {
                Conf.Init (args[0]);
            } else {
                Conf.Init ();
            }
            Server.Start (Conf.LocalAddresses);
        }

        public static string Version()
        {
            return version;
        }

        private static void PrintVersion()
        {
            Console.WriteLine("UI Version: {0}", Version());
            Console.WriteLine("Lib Version: {0}", Server.Version());
        }
    }
}