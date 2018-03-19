using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
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
                    "Please Choose to add: "
                );
                string choice = Console.ReadLine();
                string value;
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
                        Conf.AddValue("Remote HTTP Address", value);
                        break;
                    case "2":
                        Conf.AddValue("Remote SOCKS Address", value);
                        break;
                    case "3":
                        Conf.AddValue("Local HTTP Address", value);
                        break;
                    case "4":
                        Conf.AddValue("Local SOCKS Address", value);
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
                if (Conf.allConf.ContainsKey("users"))
                {
                    httpClient = new AuthenticationClient(
                        remoteHttpIPEPs,
                        localHttpIPEPs[0]
                    );
                }
                else
                {
                    httpClient = new Client(
                        remoteHttpIPEPs,
                        localHttpIPEPs[0]
                    );
                }
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
                if (Conf.allConf.ContainsKey("users"))
                {
                    socksClient = new AuthenticationClient(
                        remoteSocksIPEPs,
                        localSocksIPEPs[0]
                    );
                }
                else
                {
                    socksClient = new Client(
                        remoteSocksIPEPs,
                        localSocksIPEPs[0]
                    );
                }
                socksClient.Start();
            }
        }

        public static void Wait()
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

            try
            {
                List<ArrayList> remoteHttpAddresses = Conf.allConf["Remote HTTP Address"];
                remoteHttpIPEPs = CreateEndPoints(remoteHttpAddresses);
            }
            catch (KeyNotFoundException knfe)
            {
                Console.WriteLine(knfe.Message + " Remote HTTP Address");
            }
            try
            {
                List<ArrayList> localHttpAddresses = Conf.allConf["Local HTTP Address"];
                localHttpIPEPs = CreateEndPoints(localHttpAddresses);
                
            }
            catch (KeyNotFoundException knfe)
            {
                Console.WriteLine(knfe.Message + " Local HTTP Address");
            }
            try
            {
                List<ArrayList> remoteSocksAddresses = Conf.allConf["Remote SOCKS Address"];
                remoteSocksIPEPs = CreateEndPoints(remoteSocksAddresses);
            }
            catch (KeyNotFoundException knfe)
            {
                Console.WriteLine(knfe.Message + " Remote SOCKS Address");
            }
            try
            {
                List<ArrayList> localSocksAddresses = Conf.allConf["Local SOCKS Address"];
                localSocksIPEPs = CreateEndPoints(localSocksAddresses);
            }
            catch (KeyNotFoundException knfe)
            {
                Console.WriteLine(knfe.Message + " Local SOCKS Address");
            }
        }

        public void Close()
        {
            Conf.Close();
        }

        private static IPEndPoint[] CreateEndPoints(List<ArrayList> addresses)
        {
            ArrayList list = new ArrayList();
            foreach (ArrayList addressList in addresses)
            {
                string[] address = addressList.ToArray(typeof(string)) as string[];
                if (address.Length >= 2)
                {
                    if (IPAddress.TryParse(address[0], out IPAddress ipa))
                    {
                        if (int.TryParse(address[1], out int port))
                        {
                            IPEndPoint ipep = new IPEndPoint(ipa, port);
                            list.Add(ipep);
                        }
                    }
                }
            }
            return list.ToArray(typeof(IPEndPoint)) as IPEndPoint[];
        }
    }
}