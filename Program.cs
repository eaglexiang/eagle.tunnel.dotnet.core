using System;
using System.IO;
using System.Net;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static string confPath = "./config.cfg";
        static string serverIP;
        static string localIP;
        static int serverHttpPort;
        static int serverSocketPort;
        static int localHttpPort;
        static int localSocketPort;

        static void Main(string[] args)
        {
            string choice = args[0];
            switch (choice)
            {
                case "server":
                case "s":
                    StartServer(args[0], args[1]);
                    break;
                case "client":
                case "c":
                    StartClient();
                    break;
                default:
                    Console.WriteLine("no specific mode (server/client ?)");
                    break;
            }
        }

        static void StartServer(string ip, string _port)
        {
            int port = int.Parse(_port);
            HttpServer server = new HttpServer();
            server.Start(
                ip,
                port,
                100
            );
        }

        static void StartClient()
        {
            ReadConfiguration();
            
            Console.WriteLine("Server IP: " + serverIP);
            Console.WriteLine("Server Http Port: " + serverHttpPort);
            Console.WriteLine("Server Socket Port: " + serverSocketPort);
            Console.WriteLine("Local IP: " + localIP);
            Console.WriteLine("Local Http Port: " + localHttpPort);
            Console.WriteLine("Local Socekt Port: " + localSocketPort);

            HttpClient httpClient = new HttpClient(
                serverIP, serverHttpPort,
                localIP, localHttpPort
            );
            
            try
            {
                httpClient.Start();
                
                Console.WriteLine();
                Console.WriteLine("Local Server Started.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            string input = "";
            do
            {
                Console.WriteLine("Tunnel Number: " + Pipe.ThreadNum);
                Console.Write("input q to quit: ");
                input = Console.ReadLine();
            }while(input != "q");
        }

        /// <summary>
        /// read configuration from conf file
        /// </summary>
        /// <returns>if conf read sucessfully</returns>
        static bool ReadConfiguration()
        {
            if(!File.Exists(confPath))
            {
                Console.WriteLine("no configuration file exsits");
                return false;
            }

            string conf = File.ReadAllText(confPath);
            bool result = true;

            string type = "server ip";
            serverIP = ReadConf(conf, type);
            result &= CheckType(
                IPAddress.TryParse(
                    serverIP,
                    out IPAddress ipa
                ),
                type
            );

            type = "server http port";
            result &= CheckType(
                int.TryParse(
                    ReadConf(conf, type),
                    out serverHttpPort
                ),
                type
            );

            type = "server socket port";
            result &= CheckType(
                int.TryParse(
                    ReadConf(conf, type),
                    out serverSocketPort
                ),
                type
            );

            type = "local ip";
            localIP = ReadConf(conf, type);
            result &= CheckType(
                IPAddress.TryParse(
                    localIP,
                    out ipa
                ),
                type
            );

            type = "local http port";
            result &= CheckType(
                int.TryParse(
                    ReadConf(conf, "local http port"),
                    out localHttpPort
                ),
                type
            );

            type = "local socket port";
            result &= CheckType(
                int.TryParse(
                    ReadConf(conf, "local socket port"),
                    out localSocketPort
                ),
                type
            );

            return result;
        }

        /// <summary>
        /// read single conf as string from all confs
        /// </summary>
        /// <param name="conf">all confs</param>
        /// <param name="key">key of single conf</param>
        /// <returns>value of specific conf</returns>
        static string ReadConf(string conf, string key)
        {
            string value;
            try
            {
                int ind = conf.IndexOf(key) + key.Length + 1;
                value = conf.Substring(
                    ind,
                    conf.IndexOf('\n', ind) - ind
                );
            }
            catch
            {
                return "";
            }
            return value;
        }

        /// <summary>
        /// If key is false, output "invalid " + msg
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msg"></param>
        /// <returns>unchanged value of key</returns>
        static bool CheckType(bool key, string msg)
        {
            if(!key)
            {
                Console.WriteLine("invalid " + key);
            }
            return key;
        }
    }
}
