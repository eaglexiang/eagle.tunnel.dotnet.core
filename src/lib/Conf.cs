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
        public static Dictionary<string, List<string[]>> allConf =
            new Dictionary<string, List<string[]>>();
        public static string confPath { get; set;} = "./config.txt";
        public static bool Dirty { get; private set;} = false;
        public static Dictionary<string, TunnelUser> Users =
            new Dictionary<string, TunnelUser>();

        public static void Init()
        {
            ReadAll(confPath);
            if (allConf.ContainsKey("users"))
            {
                ImportUsers();
                Console.WriteLine("find user(s): {0}", Users.Count);
            }
        }

        private static void ImportUsers()
        {
            if (allConf["users"].Count >= 1)
            {
                if (allConf["users"][0].Length == 1)
                {
                    string pathOfUsersConf = allConf["users"][0][0] as string;
                    if (File.Exists(pathOfUsersConf))
                    {
                        string usersText = File.ReadAllText(pathOfUsersConf);
                        usersText = usersText.Replace("\r\n", "\n");
                        string[] usersArray = usersText.Split('\n');
                        // usersArray = RemoveNotes(usersArray);
                        string[][] users = ReadStrs_Split(usersArray);
                        foreach (string[] user in users)
                        {
                            if (user.Length >= 2)
                            {
                                TunnelUser newUser = new TunnelUser(user[0], user[1]);
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
                    string[] arg = line.Split('=');
                    if (arg.Length == 2)
                    {
                        string key = arg[0];
                        string[] args = arg[1].Split(':');
                        if (allConf.ContainsKey(key))
                        {
                            allConf[key].Add(args);
                        }
                        else
                        {
                            List<string[]> tmpList = new List<string[]>();
                            tmpList.Add(args);
                            allConf.Add(key, tmpList);
                        }
                    }
                }
            }
        }

        private static string[] RemoveNotes(string[] lines)
        {
            ArrayList newLines = new ArrayList();
            foreach (string line in lines)
            {
                string validline = line;
                if (validline.Contains("#"))
                {
                    int index = validline.IndexOf("#");
                    validline = validline.Substring(0, index);
                }
                validline = validline.Trim();
                newLines.Add(validline);
            }
            return newLines.ToArray(typeof(string)) as string[];
        }

        private static void SaveAll(string confPath)
        {
            string allConfText = "";
            foreach (string key in allConf.Keys)
            {
                string values = "";
                foreach (string[] args in allConf[key])
                {
                    string line = "";
                    foreach (string arg in args)
                    {
                        line += (':' + arg);
                    }
                    values += key + line + '\n';
                }
                allConfText += values;
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
    }
}