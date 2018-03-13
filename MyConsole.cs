using System;
using System.IO;
using System.Threading;

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
                StartHttpServer();
                break;
            case "hc":
                StartHttpClient();
                break;
            case "ss":
                StartSocksServer();
                break;
            case "sc":
                StartSocksClient();
                break;
            case "edit":
                StartEdit();
                break;
            case "close":
                CloseAll();
                break;
            default:
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
                    "1. Remote HTTP Address.\n" +
                    "2. Remote SOCKS Address.\n" +
                    "3. Local HTTP Address.\n" +
                    "4. Local SOCKS Address.\n" +
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
                        Conf.WriteValue("Remote HTTP Address", value);
                        break;
                    case "2":
                        Conf.WriteValue("Remote SOCKS Adress", value);
                        break;
                    case "3":
                        Conf.WriteValue("Local HTTP Address", value);
                        break;
                    case "4":
                        Conf.WriteValue("Local SOCKS Address", value);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void StartHttpServer()
        {
            string[] remoteHttpAddress = ReadStrs("Remote HTTP Address");
            if (remoteHttpAddress.Length != 2)
            {
                return; 
            }

            if (int.TryParse(remoteHttpAddress[1], out int remoteHttpPort))
            {
                httpServer = new HttpServer(remoteHttpAddress[0], remoteHttpPort);
                httpServer.Start();
            }
        }

        private static void StartHttpClient()
        {
            string[] remoteHttpAddress = ReadStrs("Remote HTTP Address");
            string[] localHttpAddress = ReadStrs("Local HTTP Address");
            if (remoteHttpAddress.Length != 2 || localHttpAddress.Length != 2)
            {
                return;
            }

            if (int.TryParse(remoteHttpAddress[1], out int remoteHttpPort))
            {
                if (int.TryParse(localHttpAddress[1], out int localHttpPort))
                {
                    httpClient = new Client(
                        remoteHttpAddress[0], remoteHttpPort,
                        localHttpAddress[0], localHttpPort
                    );
                    httpClient.Start();
                }
            }
        }

        private static void StartSocksServer()
        {
            string[] remoteSocksAddress = ReadStrs("Remote SOCKS Address");
            if (remoteSocksAddress.Length != 2)
            {
                return;
            }

            if (int.TryParse(remoteSocksAddress[1], out int remoteSocksPort))
            {
                socksServer = new SocksServer(remoteSocksAddress[0], remoteSocksPort);
                socksServer.Start();
            }
        }

        static void StartSocksClient()
        {
            string[] remoteSocksAddress = ReadStrs("Remote SOCKS Address");
            string[] localSocksAddress = ReadStrs("Local SOCKS Address");
            if (remoteSocksAddress.Length != 2 || localSocksAddress.Length != 2)
            {
                return ;
            }

            if (int.TryParse(remoteSocksAddress[1], out int remoteSocksPort))
            {
                if (int.TryParse(localSocksAddress[1], out int localSocksPort))
                {
                    socksClient = new Client(
                        remoteSocksAddress[0], remoteSocksPort,
                        localSocksAddress[0], localSocksPort
                    );
                    socksClient.Start();
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

        private static string[] ReadStrs(string key)
        {
            string value = ReadStr(key);
            string[] values = value.Split(':');
            return values;
        }

        public static void Wait()
        {
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}