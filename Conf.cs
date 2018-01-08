using System;
using System.IO;
using System.Text;
using System.Net;
using System.Linq.Expressions;

namespace eagle.tunnel.dotnet.core
{
    public class Conf
    {
        public enum UpType
        {
            HttpServer,
            HttpClient,
            SocketServer,
            SocketClient
        }
        private static string confPath = "./config.cfg";
        private static string conf;
        public static string RemoteIP;
        public static string LocalIP;
        public static int RemoteHttpPort;
        public static int RemoteSocketPort;
        public static int LocalHttpPort;
        public static int LocalSocketPort;

        /// <summary>
        /// read single conf as string from all confs
        /// </summary>
        /// <param name="conf">all confs</param>
        /// <param name="key">key of single conf</param>
        /// <returns>value of specific conf</returns>
        static string ReadConf(ref string conf, string key)
        {
            string value = "";
            try
            {
                int ind0 = conf.IndexOf(key);
                if(ind0 == -1)
                {
                    conf += (key + ":\n");
                }
                else
                {
                    int ind1 = ind0 + key.Length + 1;
                    value = conf.Substring(
                        ind1,
                        conf.IndexOf('\n', ind1) - ind1
                    );
                }
            }
            catch
            {
                return "";
            }
            return value;
        }

        static void WriteConf(ref string conf, string key, string value)
        {
            try
            {
                int ind0 = conf.IndexOf(key) + key.Length + 1;
                int ind1 = conf.IndexOf('\n', ind0);
                conf = conf.Substring(0, ind0) + value + conf.Substring(ind1);
            }
            catch
            {
                return;
            }
        }

        public static void ReadConfiguration(UpType uptype)
        {
            bool re = _ReadConfiguration(uptype);
            if(!re)
            {
                File.WriteAllText(confPath, conf, Encoding.UTF8);
            }
        }

        /// <summary>
        /// read configuration from conf file
        /// </summary>
        /// <returns>if conf read sucessfully</returns>
        private static bool _ReadConfiguration(UpType uptype)
        {
            bool result = true;
            if(!File.Exists(confPath))
            {
                Console.WriteLine("no configuration file exsits");
                conf = "";
                result = false;
            }
            else
            {
                conf = File.ReadAllText(confPath, Encoding.UTF8);
                conf = conf.Replace("\r", "");
            }

            // read ip
            result &= FixReadString("remote ip", out RemoteIP);
            result &= FixReadString("local ip", out LocalIP);

            // read port
            result &= FixReadInt("remote http port", out RemoteHttpPort);
            if(uptype == UpType.HttpClient)
            {
                result &= FixReadInt("local http port", out LocalHttpPort);
            }

            result &= FixReadInt("remote socket port", out RemoteSocketPort);
            if(uptype == UpType.SocketClient)
            {
                result &= FixReadInt("local socket port", out LocalSocketPort);
            }

            return result;
        }

        static bool FixReadString(string key, out string value)
        {
            bool need2fix;
            need2fix = !IPAddress.TryParse(
                value = ReadConf(ref conf, key),
                out IPAddress ipa
            );
            if(need2fix)
            {
                Console.WriteLine("invalid " + key);
            }
            
            // if(need2fix)
            // {
            //     Console.WriteLine("please input new " + key + ":");
            //     value = Console.ReadLine();
            //     WriteConf(ref conf, key, value);
            // }
            return !need2fix;
        }

        static bool FixReadInt(string key, out int value)
        {
            bool need2fix;
            need2fix = !int.TryParse(
                ReadConf(ref conf, key),
                out value
            );
            if(need2fix)
            {
                Console.WriteLine("invalid " + key);
            }
            
            if(need2fix)
            {
                Console.WriteLine("please input new " + key + ":");
                string newValue = Console.ReadLine();
                int.TryParse(newValue, out value);
                WriteConf(ref conf, key, newValue);
            }
            return !need2fix;
        }
    }
}