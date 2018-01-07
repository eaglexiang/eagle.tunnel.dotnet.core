using System;
using System.IO;
using System.Net;
using System.Text;

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
                    StartServer();
                    break;
                case "client":
                case "c":
                    StartClient();
                    break;
                default:
                    Console.WriteLine("no specific mode (server/client ?)");
                    return;
            }

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
            if(httpServer != null)
            {
                httpServer.Stop();
            }
        }

        static void StartServer()
        {
            Conf.ReadConfiguration(Conf.UpType.HttpServer);
            
            Console.WriteLine("Server IP: " + Conf.RemoteIP);
            Console.WriteLine("Server Http Port: " + Conf.RemoteHttpPort);
            Console.WriteLine("Server Socket Port: " + Conf.RemoteSocketPort);
            
            httpServer = new HttpServer(Conf.RemoteIP, Conf.RemoteHttpPort);
            httpServer.Start();
        }

        static void StartClient()
        {
            Conf.ReadConfiguration(Conf.UpType.HttpClient);
            
            Console.WriteLine("Server IP: " + Conf.RemoteIP);
            Console.WriteLine("Server Http Port: " + Conf.RemoteHttpPort);
            Console.WriteLine("Server Socket Port: " + Conf.RemoteSocketPort);
            Console.WriteLine("Local IP: " + Conf.LocalIP);
            Console.WriteLine("Local Http Port: " + Conf.LocalIP);
            Console.WriteLine("Local Socekt Port: " + Conf.LocalSocketPort);

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
