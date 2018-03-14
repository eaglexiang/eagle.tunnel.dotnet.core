using System;
using System.Threading;
using System.Text.RegularExpressions;

namespace eagle.tunnel.dotnet.core
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                if (Regex.IsMatch(arg, @"\bc=*"))
                {
                    Conf.confPath = arg.Substring(2);
                }
            }
            Conf.Init();
            foreach (string arg in args)
            {
                MyConsole.Run(arg);
            }
            Conf.Close();
            MyConsole.Wait();
            MyConsole.Run("close");
        }
    }
}
