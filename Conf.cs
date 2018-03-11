using System;
using System.IO;
using System.Text;
using System.Net;
using System.Linq.Expressions;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Conf
    {
        private static string allConf = "";
        public static string confPath {get; set; } = "./config.txt";

        public static void Init()
        {
            ReadAll(confPath);
            while(confPath.Contains("\r\n"))
            {
                confPath = confPath.Replace("\r\n", "\n");
            }
        }

        public static void Close()
        {
            SaveAll(confPath);
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Read all configurations from file
        /// </summary>
        /// <param name="confPath">path of conf file</param>
        private static void ReadAll(string confPath)
        {
            if (File.Exists(confPath))
            {
                allConf = File.ReadAllText(confPath);
            }
        }

        private static void SaveAll(string confPath)
        {
            File.WriteAllText(confPath, allConf);
        }

        /// <summary>
        /// Read value from configurations
        /// </summary>
        /// <param name="key">configuration key</param>
        /// <returns>configuration value, or null if key not found</returns>
        public static string ReadValue(string key)
        {
            StringReader sr = new StringReader(allConf);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(":"))
                {
                    if (line.Substring(0, line.IndexOf(':')) == key)
                    {
                        sr.Close();
                        return line.Substring(line.IndexOf(':') + 1);
                    }
                }
            }
            sr.Close();
            return null;
        }

        /// <summary>
        /// Change value of specific key, if not key found, create it
        /// </summary>
        /// <param name="key">key specific</param>
        /// <param name="value">value expected</param>
        public static void WriteValue(string key, string value)
        {
            StringReader sr = new StringReader(allConf);
            StringBuilder sb = new StringBuilder(allConf);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(":"))
                {
                    if (line.Substring(0, line.IndexOf(':')) == key)
                    {
                        sr.Close();
                        string newLine = line.Substring(0, line.IndexOf(':') + 1) + value;
                        sb.Replace(line, newLine);
                        allConf = sb.ToString();
                        return;
                    }
                }
            }
            sr.Close();
            line = key + ":" + value;
            sb.AppendLine(line);
            allConf = sb.ToString();
        }
    }
}