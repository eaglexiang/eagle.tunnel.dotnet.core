using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Eagle Tunnel");
            Server server = new Server();
            Console.Write("Server IP: ");
            string ip = Console.ReadLine();
            Console.Write("Server Port: ");
            string _port = Console.ReadLine();
            int port = int.Parse(_port);
            server.Start(ip, port);
            while(true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}
