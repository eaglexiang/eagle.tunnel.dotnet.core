using System.Collections;

namespace eagle.tunnel.dotnet.core {
    public class EagleTunnelUser {
        public string ID { get; }
        public string Password { get; set; }
        public int SpeedLimit { get; set; } // KB/s

        private ArrayList tunnels;
        private object lockOfTunnels;

        public EagleTunnelUser (string id, string password) {
            ID = id;
            Password = password;
            SpeedLimit = 0;
            tunnels = new ArrayList ();
            lockOfTunnels = new object ();
        }

        public static bool TryParse (string parameter, out EagleTunnelUser user) {
            user = null;
            if (parameter != null) {
                string[] args = parameter.Split (':');
                if (args.Length >= 2) {
                    user = new EagleTunnelUser (args[0], args[1]);
                    if (args.Length >= 3) {
                        if (int.TryParse (args[2], out int speed)) {
                            user.SpeedLimit = speed;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void AddTunnel (Tunnel tunnel2Add) {
            lock (lockOfTunnels) {
                tunnels.Add (tunnel2Add);
            }
        }

        public double Speed () {
            double speed = 0; // KB/s
            lock (lockOfTunnels) {
                ArrayList newTunnels = new ArrayList ();
                foreach (Tunnel item in tunnels) {
                    if (item.IsWorking) {
                        speed += item.Speed () / 1024;
                        newTunnels.Add (item);
                    }
                }
                tunnels = newTunnels;
            }
            return speed;
        }

        public void LimitSpeed () {
            if (SpeedLimit > 0) {
                double speed = Speed();
                if (speed > SpeedLimit) {
                    lock (lockOfTunnels) {
                        foreach (Tunnel item in tunnels) {
                            item.IsWaiting = true;
                        }
                    }
                }
            }
        }

        public override string ToString () {
            return ID + ':' + Password;
        }

        public bool CheckAuthen (string pswd) {
            if (ID == "anonymous") {
                return false;
            } else {
                return pswd == Password;
            }
        }
    }
}