using System;
using System.IO;

namespace eagle.tunnel.dotnet.core
{
    public class MyConsole
    {
        private static HttpServer httpServer;
        private static Client httpClient;
        private static SocksServer socksServer;
        private static Client socksClient;

        public static void Run(string command)
        {
            Console.WriteLine("command:\t{0}", command);
            
            switch (command)
            {
            case "hs":
            case "httpserver":
                StartHttpServer();
                break;
            case "hc":
            case "httpclient":
                StartHttpClient();
                break;
            case "ss":
            case "socksserver":
                StartSocksServer();
                break;
            case "sc":
            case "socksclient":
                StartSocksClient();
                break;
            case "edit":
                StartEdit();
                break;
            case "close":
                CloseAll();
                break;
            default:
                Console.WriteLine("invalid command.");
                break;
            }
        }

        private static void CloseAll()
        {
            if (httpServer != null)
            {
                httpServer.Stop();
            }
            if (httpClient != null)
            {
                httpClient.Stop();
            }
            if (socksServer != null)
            {
                socksServer.Stop();
            }
            if (socksClient != null)
            {
                socksClient.Stop();
            }
        }

        private static void StartEdit()
        {
            while(true)
            {
                Console.WriteLine(
                    "1. Remote HTTP IP.\n" +
                    "2. Remote HTTP Port.\n" +
                    "3. Remote SOCKS IP.\n" +
                    "4. Remote SOCKS Port.\n" +
                    "5. Local HTTP IP.\n" +
                    "6. Local HTTP Port.\n" +
                    "7. Local SOCKS IP.\n" +
                    "8. Local SOCKS Port.\n" +
                    "0. Quit\n" +
                    "Please Choose: "
                );
                string choice = Console.ReadLine();
                string value = "";
                if (choice == "0")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("New value: ");
                    value = Console.ReadLine();
                }
                switch (choice)
                {
                    case "1":
                        Conf.WriteValue("Remote HTTP IP", value);
                        break;
                    case "2":
                        Conf.WriteValue("Remote HTTP Port", value);
                        break;
                    case "3":
                        Conf.WriteValue("Remote SOCKS IP", value);
                        break;
                    case "4":
                        Conf.WriteValue("Remote SOCKS Port", value);
                        break;
                    case "5":
                        Conf.WriteValue("Local HTTP IP", value);
                        break;
                    case "6":
                        Conf.WriteValue("Local HTTP Port", value);
                        break;
                    case "7":
                        Conf.WriteValue("Local SOCKS IP", value);
                        break;
                    case "8":
                        Conf.WriteValue("Local SOCKS Port", value);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void StartHttpServer()
        {
            string remoteHttpIP;
            int remoteHttpPort;
            if ((remoteHttpIP = ReadStr("Remote HTTP IP")) != null)
            {
                if ((remoteHttpPort = ReadInt("Remote HTTP Port")) != -1)
                {
                    httpServer = new HttpServer(remoteHttpIP, remoteHttpPort);
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
            }
        }

        private static void StartHttpClient()
        {
            string remoteHttpIP;
            int remoteHttpPort;
            string localHttpIP;
            int localHttpPort;

            if ((remoteHttpIP = ReadStr("Remote HTTP IP")) != null)
            {
                if ((remoteHttpPort = ReadInt("Remote HTTP Port")) != -1)
                {
                    if ((localHttpIP = ReadStr("Local HTTP IP")) != null)
                    {
                        if ((localHttpPort = ReadInt("Local HTTP Port")) != -1)
                        {
                            httpClient = new Client(
                                remoteHttpIP, remoteHttpPort,
                                localHttpIP, localHttpPort
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
                    }
                }
            }
        }

        private static void StartSocksServer()
        {
            string remoteSocksIP;
            int remoteSocksPort;
            if ((remoteSocksIP = ReadStr("Remote SOCKS IP")) != null)
            {
                if ((remoteSocksPort = ReadInt("Remote SOCKS Port")) != -1)
                {
                    socksServer = new SocksServer(remoteSocksIP, remoteSocksPort);
                    try
                    {
                        socksServer.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Remote SOCKS Server Stop.");
                    }
                        }
                    }
        }

        static void StartSocksClient()
        {
            string remoteSocksIP;
            int remoteSocksPort;
            string localSocksIP;
            int localSocksPort;

            if ((remoteSocksIP = ReadStr("Remote SOCKS IP")) != null)
            {
                if ((remoteSocksPort = ReadInt("Remote SOCKS Port")) != -1)
                {
                    if ((localSocksIP = ReadStr("Local SOCKS IP")) != null)
                    {
                        if ((localSocksPort = ReadInt("Local SOCKS Port")) != -1)
                        {
                            socksClient = new Client(
                                remoteSocksIP, remoteSocksPort,
                                localSocksIP, localSocksPort
                            );
                            try
                            {
                                socksClient.Start();
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine("Local SOCKS Server Stop.");
                            }
                        }
                    }
                }
            }
        }

        private static string ReadStr(string key)
        {
            string value = Conf.ReadValue(key);
            if (value == null)
            {
                Console.WriteLine("{0} not found.", key);
            }
            else
            {
                Console.WriteLine("{0}:{1}", key, value);
            }
            return value;
        }

        private static int ReadInt(string key)
        {
            string _value = ReadStr(key);
            if (int.TryParse(_value, out int value))
            {
                return value;
            }
            else
            {
                Console.WriteLine("invalid number:{0}", _value);
                return -1;
            }
        }
    }
}