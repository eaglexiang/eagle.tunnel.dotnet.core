using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core {
    class Program {
        static void Main (string[] args) {
            if (args.Length >= 1) {
                Conf.Init (args[0]);
            } else {
                Conf.Init ();
            }
            Server.Start (Conf.localAddresses);
        }
    }
}