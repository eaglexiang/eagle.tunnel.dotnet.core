using System;
using System.IO;
using System.Text;
using System.Net;
using System.Linq.Expressions;
using System.Threading;
using System.Collections;

namespace eagle.tunnel.dotnet.core
{
    public class Conf
    {
        private static string allConf = "";
        public static string confPath { get; set;} = "./config.txt";
        public static bool Dirty { get; private set;} = false;

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
            if (Dirty)
            {
                SaveAll(confPath);
            }
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
            try
            {
                File.WriteAllText(confPath, allConf);
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine(uae.Message);
            }
        }

        /// <summary>
        /// Read all values for one specific key from configurations
        /// </summary>
        /// <param name="key">configuration key</param>
        /// <returns>configuration value, or null if key not found</returns>
        public static string[] ReadValue(string key)
        {
            return ReadValue(allConf, key);
        }

        /// <summary>
        /// Read all values for one specific key from arg
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string[] ReadValue(string arg, string key)
        {
            StringReader sr = new StringReader(arg);
            string line;
            ArrayList valueList = new ArrayList();
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(":"))
                {
                    if (line.Substring(0, line.IndexOf(':')) == key)
                    {
                        string newLine = line.Substring(line.IndexOf(':') + 1);
                        valueList.Add(newLine);
                    }
                }
            }
            sr.Close();
            string[] re = valueList.ToArray(typeof(string)) as string[];
            return re;
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
            string newLine = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(":"))
                {
                    if (line.Substring(0, line.IndexOf(':')) == key)
                    {
                        newLine = line.Substring(0, line.IndexOf(':') + 1) + value;
                        break;
                    }
                }
            }
            sr.Close();
            if (newLine == null)
            {
                newLine = key + ':' + value;
                sb.AppendLine(newLine);
            }
            else
            {
                sb.Replace(line, newLine);
            }
            allConf = sb.ToString();
            Dirty = true;
        }
    }
}