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
                Console.WriteLine("invalid " + msg);
            }
            return key;
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
            }

            // read ip
            result &= FixReadIP(out RemoteIP);
            result &= FixReadIP(out LocalIP);

            // read port
            result &= FixReadInt(out RemoteHttpPort);
            if(uptype == UpType.HttpClient)
            {
                result &= FixReadInt(out LocalHttpPort);
            }

            result &= FixReadInt(out RemoteSocketPort);
            if(uptype == UpType.SocketClient)
            {
                result &= FixReadInt(out LocalSocketPort);
            }

            return result;
        }

        static bool FixReadIP(out string value)
        {
            bool need2fix;
            string key = nameof(value);
            need2fix = CheckType(
                IPAddress.TryParse(
                    value = ReadConf(conf, key),
                    out IPAddress ipa
                ),
                key
            );
            
            if(need2fix)
            {
                Console.WriteLine("please input new " + key + ":");
                value = Console.ReadLine();
                WriteConf(ref conf, key, value);
            }
            return need2fix;
        }

        static bool FixReadInt(out int value)
        {
            bool need2fix;
            string key = nameof(value);
            need2fix = CheckType(
                int.TryParse(
                    ReadConf(conf, key),
                    out value
                ),
                key
            );
            
            if(need2fix)
            {
                Console.WriteLine("please input new " + key + ":");
                string newValue = Console.ReadLine();
                int.TryParse(newValue, out value);
                WriteConf(ref conf, key, newValue);
            }
            return need2fix;
        }
    }
}