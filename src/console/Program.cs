using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static void Main(string[] args)
        {
            MyConsole console = new MyConsole();
            console.Init(args[0]);
            foreach (string arg in args)
            {
                console.Run(arg);
            }
            console.Close();
            console.Wait();
        }
    }
}
