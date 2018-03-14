using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;

namespace eagle.tunnel.dotnet.core
{
    public class MyConsole
    {
        private HttpServer httpServer;
        private Client httpClient;
        private SocksServer socksServer;
        private Client socksClient;

        private string[][] remoteHttpAddresses;
        private string[][] localHttpAddresses;
        private string[][] remoteSocksAddresses;
        private string[][] localSocksAddresses;

        public void Run(string command)
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

        private void CloseAll()
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
                        Conf.WriteValue("Remote SOCKS Address", value);
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

        private void StartHttpServer()
        {
            if (remoteHttpAddresses[0].Length != 2)
            {
                return; 
            }

            if (int.TryParse(remoteHttpAddresses[0][1], out int remoteHttpPort))
            {
                httpServer = new HttpServer(remoteHttpAddresses[0][1], remoteHttpPort);
                httpServer.Start();
            }
        }

        private void StartHttpClient()
        {
            if (remoteHttpAddresses[0].Length != 2 || localHttpAddresses[0].Length != 2)
            {
                return;
            }

            if (int.TryParse(remoteHttpAddresses[0][1], out int remoteHttpPort))
            {
                if (int.TryParse(localHttpAddresses[0][1], out int localHttpPort))
                {
                    httpClient = new Client(
                        remoteHttpAddresses[0][0], remoteHttpPort,
                        localHttpAddresses[0][0], localHttpPort
                    );
                    httpClient.Start();
                }
            }
        }

        private void StartSocksServer()
        {
            string[][] remoteSocksAddresses = ReadStrss("Remote SOCKS Address");
            if (remoteSocksAddresses[0].Length != 2)
            {
                return;
            }

            if (int.TryParse(remoteSocksAddresses[0][1], out int remoteSocksPort))
            {
                socksServer = new SocksServer(remoteSocksAddresses[0][0], remoteSocksPort);
                socksServer.Start();
            }
        }

        private void StartSocksClient()
        {
            if (remoteSocksAddresses[0].Length != 2 || localSocksAddresses[0].Length != 2)
            {
                return ;
            }

            if (int.TryParse(remoteSocksAddresses[0][1], out int remoteSocksPort))
            {
                if (int.TryParse(localSocksAddresses[0][1], out int localSocksPort))
                {
                    socksClient = new Client(
                        remoteSocksAddresses[0][0], remoteSocksPort,
                        localSocksAddresses[0][0], localSocksPort
                    );
                    socksClient.Start();
                }
            }
        }

        private static string[] ReadStrs(string key)
        {
            string[] value = Conf.ReadValue(key);
            if (value.Length == 0)
            {
                Console.WriteLine("{0} not found.", key);
            }
            return value;
        }

        private static string[][] ReadStrss(string key)
        {
            ArrayList list = new ArrayList();
            string[] values = ReadStrs(key);
            for (int i = 0; i < values.Length; ++i)
            {
                string[] tmp = values[i].Split(':');
                list.Add(tmp);
            }

            return list.ToArray(typeof(string[])) as string[][];
        }

        public static void Wait()
        {
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        public void Init(string arg)
        {
            if (Regex.IsMatch(arg, @"\bc=*"))
            {
                Conf.confPath = arg.Substring(2);
            }
            Conf.Init();

            remoteHttpAddresses = ReadStrss("Remote HTTP Address");
            localHttpAddresses = ReadStrss("Local HTTP Address");
            remoteSocksAddresses = ReadStrss("Remote SOCKS Address");
            localSocksAddresses = ReadStrss("Local SOCKS Address");
        }

        public void Close()
        {
            Conf.Close();
        }
    }
}