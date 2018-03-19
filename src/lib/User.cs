using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class TunnelUser
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
        private const int defaultSpeedLimit = 0;
        private object lockSignal = new object();
        public int SpeedSignal { get; set;}

        public TunnelUser(string id)
        {
            ID = id;
            Password = null;
            SpeedLimit = defaultSpeedLimit;
            SpeedSignal = 0;
        }

        public void CheckSpeed(int count)
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
}