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
        private HttpClient httpClient;
        private SocksClient socksClient;
        private Authen_ServerRelay serverRelay;

        private IPEndPoint[] remoteIPEPs;
        private IPEndPoint[] localHttpIPEPs;
        private IPEndPoint[] localSocksIPEPs;

        public MyConsole() { }

        public void Run(string command)
        {
            Console.WriteLine("command:\t{0}", command);
            
            switch (command)
            {
            case "s":
                StartServerRelay();
                break;
            case "hc":
                StartHttpClient();
                break;
            case "sc":
                StartSocksClient();
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
            if (httpClient != null)
            {
                httpClient.Close();
            }
            if (socksClient != null)
            {
                socksClient.Close();
            }
            if (serverRelay != null)
            {
                serverRelay.Close();
            }
        }

        private void StartServerRelay()
        {
            if (remoteIPEPs.Length > 0)
            {
                serverRelay = new Authen_ServerRelay(remoteIPEPs[0]);
                serverRelay.Start(100);
            }
        }

        private void StartHttpClient()
        {
            if (remoteIPEPs.Length > 0 && localHttpIPEPs.Length > 0)
            {
                httpClient = new HttpClient(remoteIPEPs, localHttpIPEPs[0]);
                httpClient.Start(100);
            }
        }

        private void StartSocksClient()
        {
            if (remoteIPEPs.Length > 0 && localSocksIPEPs.Length > 0)
            {
                socksClient = new SocksClient(remoteIPEPs, localSocksIPEPs[0]);
                socksClient.Start(100);
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
                List<ArrayList> remoteAddresses = Conf.allConf["Remote Address"];
                remoteIPEPs = CreateEndPoints(remoteAddresses);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Remote HTTP Address not found.");
            }
            try
            {
                List<ArrayList> localHttpAddresses = Conf.allConf["Local HTTP Address"];
                localHttpIPEPs = CreateEndPoints(localHttpAddresses);
                
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Local HTTP Address not found");
            }
            try
            {
                List<ArrayList> localSocksAddresses = Conf.allConf["Local SOCKS Address"];
                localSocksIPEPs = CreateEndPoints(localSocksAddresses);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Local SOCKS Address not found");
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