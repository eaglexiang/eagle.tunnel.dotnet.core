using System;
using System.IO;
using System.Text;
using System.Net;
using System.Linq.Expressions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace eagle.tunnel.dotnet.core
{
    public class Conf
    {
        public static Dictionary<string, string[]> allConf = new Dictionary<string, string[]>();
        public static string confPath { get; set;} = "./config.txt";
        public static bool Dirty { get; private set;} = false;
        public static Dictionary<string, TunnelUser> Users = new Dictionary<string, TunnelUser>();

        public static void Init()
        {
            ReadAll(confPath);
            ImportUsers();
        }

        private static void ImportUsers()
        {
            string[] pathOfUsersConf = allConf["users"];
            if (File.Exists(pathOfUsersConf[0]))
            {
                string usersText = File.ReadAllText(pathOfUsersConf[0]);
                usersText = usersText.Replace("\r\n", "\n");
                string[] usersArray = usersText.Split('\n');
                string[][] users = ReadStrs_Split(usersArray);
                foreach (string[] user in users)
                {
                    if (user.Length >= 1)
                    {
                        TunnelUser newUser = new TunnelUser(user[0]);
                        if (user.Length >= 2)
                        {
                            newUser.Password = user[1];
                        }
                        if (user.Length >= 3)
                        {
                            if (int.TryParse(user[2], out int speedlimit))
                            {
                                newUser.SpeedLimit = speedlimit;
                            }
                        }
                        Conf.Users.Add(newUser.ID, newUser);
                    }
                }
            }
        }

        public static string[][] ReadStrs_Split(string[] src)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < src.Length; ++i)
            {
                string[] tmp = src[i].Split(':');
                list.Add(tmp);
            }
            return list.ToArray(typeof(string[])) as string[][];
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
                string allConfText = File.ReadAllText(confPath);
                allConfText = allConfText.Replace("\r\n", "\n");
                string[] lines = allConfText.Split('\n');
                foreach (string line in lines)
                {
                    string[] args = line.Split(':');
                    if (args.Length >= 2)
                    {
                        string key = args[0];
                        string[] remainingArgs = new string[args.Length - 1];
                        args.CopyTo(remainingArgs, 1);
                        allConf.Add(key, remainingArgs);
                    }
                }
            }
        }

        private static void SaveAll(string confPath)
        {
            string allConfText = "";
            foreach (string key in allConf.Keys)
            {
                string line = "";
                foreach (string arg in allConf[key])
                {
                    line += (arg + ':');
                }
                allConfText += (line.Remove(line.Length - 1));
            }
            try
            {
                File.WriteAllText(confPath, allConfText);
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine(uae.Message);
            }
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
    }
}