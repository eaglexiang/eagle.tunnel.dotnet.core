using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static HttpClient httpClient;
        static HttpServer httpServer;

        static void Main(string[] args)
        {
            string choice = args[0];
            switch (choice)
            {
                case "server":
                case "s":
                    StartHttpServer();
                    break;
                case "client":
                case "c":
                    StartHttpClient();
                    break;
                default:
                    Console.WriteLine("no specific mode (server/client ?)");
                    return;
            }

            if (
                choice == "clinet" ||
                choice == "c"
            )
            {
                string input = "";
                do
                {
                    Console.Write("input q to quit: ");
                    input = Console.ReadLine();
                }while(input != "q");
                if(httpClient != null)
                {
                    httpClient.Stop();
                }
            }
        }

        static void StartHttpServer()
        {
            Conf.ReadConfiguration(Conf.UpType.HttpServer);
            
            Console.WriteLine("Server IP: " + Conf.RemoteIP);
            Console.WriteLine("Server Http Port: " + Conf.RemoteHttpPort);
            
            httpServer = new HttpServer(Conf.RemoteIP, Conf.RemoteHttpPort);
            httpServer.Start();
        }

        static void StartHttpClient()
        {
            Conf.ReadConfiguration(Conf.UpType.HttpClient);
            
            Console.WriteLine("Server IP: " + Conf.RemoteIP);
            Console.WriteLine("Server Http Port: " + Conf.RemoteHttpPort);
            Console.WriteLine("Local IP: " + Conf.LocalIP);
            Console.WriteLine("Local Http Port: " + Conf.LocalHttpPort);

            httpClient = new HttpClient(
                Conf.RemoteIP, Conf.RemoteHttpPort,
                Conf.LocalIP, Conf.LocalHttpPort
            );
            
            try
            {
                httpClient.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
