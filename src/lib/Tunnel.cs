using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core {
    public class Tunnel {
        private Pipe pipeL2R; // pipe from Left socket to Right socket
        private Pipe pipeR2L; // pipe from Right socket to Left socket
        private System.DateTime timeCreated;

        public bool IsWaiting {
            get {
                return pipeL2R.IsWaiting;
            }
            set {
                pipeL2R.IsWaiting = value;
                pipeR2L.IsWaiting = value;
            }
        }

        private long BytesTransferred {
            get {
                long result = pipeL2R.BytesTransferred + pipeR2L.BytesTransferred;
                return result;
            }
        }

        public double Speed () {
            double seconds = (System.DateTime.Now - timeCreated).TotalSeconds;
            double speed = BytesTransferred / seconds;
            return speed;
        }

        public string UserL {
            get {
                return pipeL2R.UserFrom;
            }
            set {
                pipeL2R.UserFrom = value;
            }
        }

        public string UserFrom {
            get {
                return pipeR2L.UserFrom;
            }
            set {
                pipeR2L.UserFrom = value;
            }
        }

        public Socket SocketL {
            get {
                return pipeL2R.SocketFrom;
            }
            set {
                pipeL2R.SocketFrom = value;
                pipeR2L.SocketTo = value;
            }
        }

        public Socket SocketR

        {
            get {
                return pipeL2R.SocketTo;
            }
            set {
                pipeL2R.SocketTo = value;
                pipeR2L.SocketFrom = value;
            }
        }

        public bool EncryptL {
            get {
                return pipeL2R.EncryptFrom;
            }
            set {
                pipeL2R.EncryptFrom = value;
                pipeR2L.EncryptTo = value;
            }
        }

        public bool EncryptR {
            get {
                return pipeL2R.EncryptTo;
            }
            set {
                pipeL2R.EncryptTo = value;
                pipeR2L.EncryptFrom = value;
            }
        }

        public bool IsWorking {
            get {
                bool result = false;
                if (SocketL != null) {
                    result = SocketL.Connected;
                }
                if (SocketR != null) {
                    result = SocketR.Connected || result;
                }
                return result;
            }
        }

        public Tunnel (Socket socketl = null, Socket socketr = null) {
            pipeL2R = new Pipe (socketl, socketr);
            pipeR2L = new Pipe (socketr, socketl);
            timeCreated = System.DateTime.Now;
        }

        public void Flow () {
            pipeL2R.Flow ();
            pipeR2L.Flow ();
        }

        public void Close () {
            pipeL2R.Close ();
            pipeR2L.Close ();
        }

        public string ReadStringL () {
            return pipeL2R.ReadString ();
        }

        public string ReadStringR () {
            return pipeR2L.ReadString ();
        }

        public bool WriteL (string msg) {
            return pipeR2L.Write (msg);
        }

        public bool WriteR (string msg) {
            return pipeL2R.Write (msg);
        }

        public byte[] ReadL () {
            return pipeL2R.ReadByte ();
        }

        public byte[] ReadR () {
            return pipeR2L.ReadByte ();
        }
    }
}