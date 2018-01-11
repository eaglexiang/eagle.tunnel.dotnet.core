using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static Client httpClient;
        static Client socksClient;
        static HttpServer httpServer;
        static SocksServer socksServer;

        static void Main(string[] args)
        {
            string allArgs = "";
            foreach (string arg in args)
            {
                allArgs += arg;
            }
            if(
                allArgs.Contains("httpserver") ||
                allArgs.Contains("hs")
            )
            {
                StartHttpServer();
            }
            if(allArgs.Contains("httpclient") ||
                allArgs.Contains("hc")
            )
            {
                StartHttpClient();
            }
            if(allArgs.Contains("socksserver") ||
                allArgs.Contains("ss")
            )
            {
                StartSocksServer();
            }
            if(allArgs.Contains("socksclient") ||
                allArgs.Contains("sc")
            )
            {
                StartSocksClient();
            }

            while(true)
            {
                Thread.Sleep(60000);
            }
        }

        static void StartHttpServer()
        {
            Console.WriteLine("Read conf for Remote HTTP Server.");
            Conf.ReadConfiguration(Conf.UpType.HttpServer);
            
            Console.WriteLine("Server IP: " + Conf.RemoteIP);
            Console.WriteLine("Server Http Port: " + Conf.RemoteHttpPort);
            
            httpServer = new HttpServer(Conf.RemoteIP, Conf.RemoteHttpPort);
            try
            {
                httpServer.Start();
            }
            catch ( Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Remote HTTP Server stopped");
            }
        }

        static void StartHttpClient()
        {
            Console.WriteLine("Read conf for Local HTTP Server.");
            Conf.ReadConfiguration(Conf.UpType.HttpClient);
            
            Console.WriteLine("Server IP: " + Conf.RemoteIP);
            Console.WriteLine("Server Http Port: " + Conf.RemoteHttpPort);
            Console.WriteLine("Local IP: " + Conf.LocalIP);
            Console.WriteLine("Local Http Port: " + Conf.LocalHttpPort);

            httpClient = new Client(
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
                Console.WriteLine("Local HTTP Server Stop.");
            }
        }

        static void StartSocksServer()
        {
            Console.WriteLine("Read conf for Remote Socks Server.");
            Conf.ReadConfiguration(Conf.UpType.SocksServer);

            Console.WriteLine("Remote IP: " + Conf.RemoteIP);
            Console.WriteLine("Remote Socks Port: " + Conf.RemoteSocksPort);

            socksServer = new SocksServer(Conf.RemoteIP, Conf.RemoteSocksPort);

            try
            {
                socksServer.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Remote Socks Server Stop.");
            }
        }

        static void StartSocksClient()
        {
            Console.WriteLine("Read conf for Local Socks Server.");
            Conf.ReadConfiguration(Conf.UpType.SocksClient);
            
            Console.WriteLine("Remote IP: " + Conf.RemoteIP);
            Console.WriteLine("Remote Socks Port: " + Conf.RemoteSocksPort);
            Console.WriteLine("Local IP: " + Conf.LocalIP);
            Console.WriteLine("Local Socks Port: " + Conf.LocalSocksPort);

            socksClient = new Client(
                Conf.RemoteIP, Conf.RemoteSocksPort,
                Conf.LocalIP, Conf.LocalSocksPort
            );

            try
            {
                socksClient.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Local Socks Server Stop.");
            }
        }
    }
}
