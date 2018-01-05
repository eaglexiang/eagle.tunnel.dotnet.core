using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static void Main(string[] args)
        {
            
            while(true)
            {
                int choice;
                if(args.Length == 0)
                {
                    Console.WriteLine("Eagle Tunnel");
                    Console.WriteLine("");
                    Console.WriteLine("1. Server");
                    Console.WriteLine("2. Client");
                    Console.WriteLine("0. quit");
                    Console.WriteLine("");
                    choice = ReadNum();
                }
                else
                {
                    choice = 1;
                }

                switch (choice)
                {
                    case 0:
                        return;
                    case 1:
                        StartServer(args[0], args[1]);
                        break;
                    case 2:
                        StartClient();
                        break;
                    default:
                        break;
                }
            }
        }

        static void StartServer(string ip, string _port)
        {
            int port = int.Parse(_port);
            Server server = new Server();
            server.Start(
                ip,
                port,
                100
            );
        }

        static void StartClient()
        {
            Console.Write("Server IP: ");
            string serverIP = Console.ReadLine();
            Console.Write("Server Port: ");
            int serverPort = ReadNum();
            Console.Write("Local IP: ");
            string localIP = Console.ReadLine();
            Console.Write("Local Port: ");
            int localPort = ReadNum();
            Client client = new Client();
            client.Start(
                serverIP, serverPort,
                localIP, localPort, 100
            );
        }

        static int ReadNum()
        {
            int num;
            string str;
            do
            {
                str = Console.ReadLine();
            }while(!int.TryParse(str, out num));
            return num;
        }
    }
}
