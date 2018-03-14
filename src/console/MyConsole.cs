using System;
using System.IO;
using System.Net;
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

        private IPEndPoint[] remoteHttpIPEPs;
        private IPEndPoint[] remoteSocksIPEPs;
        private IPEndPoint[] localHttpIPEPs;
        private IPEndPoint[] localSocksIPEPs;

        public MyConsole() { }
        ~MyConsole()
        {
            CloseAll();
        }

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
            if (remoteHttpIPEPs.Length > 0)
            {
                httpServer = new HttpServer(remoteHttpIPEPs[0]);
                httpServer.Start();
            }
        }

        private void StartHttpClient()
        {
            if (remoteHttpIPEPs.Length > 0 && localHttpIPEPs.Length > 0)
            {
                httpClient = new Client(
                    remoteHttpIPEPs,
                    localHttpIPEPs[0]
                );
                httpClient.Start();
            }
        }

        private void StartSocksServer()
        {
            if (remoteSocksIPEPs.Length > 0)
            {
                socksServer = new SocksServer(remoteSocksIPEPs[0]);
                socksServer.Start();
            }
        }

        private void StartSocksClient()
        {
            if (remoteSocksIPEPs.Length > 0 && localSocksIPEPs.Length > 0)
            {
                socksClient = new Client(
                    remoteSocksIPEPs,
                    localSocksIPEPs[0]
                );
                socksClient.Start();
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

        public void Wait()
        {
            while (true)
            {
                Thread.Sleep(100000);
            }
        }

        public void Init(string arg)
        {
            if (Regex.IsMatch(arg, @"\bc=*"))
            {
                Conf.confPath = arg.Substring(2);
            }
            Conf.Init();

            string[][] remoteHttpAddresses = ReadStrss("Remote HTTP Address");
            string[][] localHttpAddresses = ReadStrss("Local HTTP Address");
            string[][] remoteSocksAddresses = ReadStrss("Remote SOCKS Address");
            string[][] localSocksAddresses = ReadStrss("Local SOCKS Address");

            remoteHttpIPEPs = CreateEndPoints(remoteHttpAddresses);
            remoteSocksIPEPs = CreateEndPoints(remoteSocksAddresses);
            localHttpIPEPs = CreateEndPoints(localHttpAddresses);
            localSocksIPEPs = CreateEndPoints(localSocksAddresses);
        }

        public void Close()
        {
            Conf.Close();
        }

        private static IPEndPoint[] CreateEndPoints(string[][] addresses)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < addresses.Length; ++i)
            {
                if (IPAddress.TryParse(addresses[i][0], out IPAddress ipa))
                {
                    if (int.TryParse(addresses[i][1], out int port))
                    {
                        IPEndPoint ipep = new IPEndPoint(ipa, port);
                        list.Add(ipep);
                    }
                }
            }
            return list.ToArray(typeof(IPEndPoint)) as IPEndPoint[];
        }
    }
}