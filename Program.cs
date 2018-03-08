using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;
            Conf.Init();
            foreach (string arg in args)
            {
                MyConsole.Run(arg);
            }
            Conf.Close();
            Console.WriteLine("Press X to quit.");
            while (true)
            {
                cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.X)
                {
                    break;
                }
            }
            MyConsole.Run("close");
            Thread.Sleep(2000);
        }
    }
}
