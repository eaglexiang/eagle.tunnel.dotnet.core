using System;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Eagle Tunnel");
            Console.WriteLine("");
            Console.WriteLine("1. Server");
            Console.WriteLine("2. Client");
            Console.WriteLine("0. quit");
            Console.WriteLine("");
            while(true)
            {
                int choice = ReadNum();

                switch (choice)
                {
                    case 0:
                        return;
                    case 1:
                        StartServer();
                        break;
                    case 2:
                        StartClient();
                        break;
                    default:
                        break;
                }
            }
        }

        static void StartServer()
        {
            Console.Write("IP: ");
            string ip = Console.ReadLine();
            Console.Write("Port: ");
            int port = ReadNum();
            Server server = new Server();
            server.Start(ip, port, "./ssl.key");
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
                serverIP, serverPort, "./ssl.csr",
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
