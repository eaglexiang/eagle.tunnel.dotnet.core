using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class EagleTunnelUser
    {
        public string ID { get; }
        public string Password { get; set;}
        private int _SpeedLimit;
        public int SpeedLimit //speed limit = [value set]KB/s
        {
            get
            {
                return _SpeedLimit * 1024;
            }
            set
            {
                _SpeedLimit = value;
            }
        }
        private object lockSignal = new object();
        public int SpeedSignal { get; set;}

        public EagleTunnelUser(string id, string password)
        {
            ID = id;
            Password = password;
            SpeedLimit = 0;
            SpeedSignal = 0;
        }

        public static bool TryParse(string parameter, out EagleTunnelUser user)
        {
            user = null;
            if (parameter != null)
            {
                string[] args = parameter.Split(':');
                if (args.Length>=2)
                {
                    user = new EagleTunnelUser(args[0], args[1]);
                    if (args.Length>=3)
                    {
                        if(int.TryParse(args[2], out int speed))
                        {
                            user.SpeedLimit = speed;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void CheckSpeed(int count)
        {
            if (SpeedLimit > 0)
            {
                lock(lockSignal)
                {
                    SpeedSignal += count;
                    while (SpeedSignal > SpeedLimit)
                    {
                        Thread.Sleep(1000);
                        SpeedSignal -= SpeedLimit;
                    }
                }
            }
        }

        public override string ToString()
        {
            return ID + ':' + Password;
        }

        public bool CheckAuthen(string pswd)
        {
            return pswd == Password;
        }
    }
}