using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core
{
    public class Tunnel
    {
        public Pipe[] pipes;
        public Socket SocketL
        {
            get
            {
                return pipes[0].SocketFrom;
            }
            set
            {
                pipes[0].SocketFrom = value;
                pipes[1].SocketTo = value;
            }
        }

        public Socket SocketR

        {
            get
            {
                return pipes[0].SocketTo;
            }
            set
            {
                pipes[0].SocketTo = value;
                pipes[1].SocketFrom = value;
            }
        }

        public bool EncryptL
        {
            get
            {
                return pipes[0].EncryptFrom;
            }
            set
            {
                pipes[0].EncryptFrom = value;
                pipes[1].EncryptTo = value;
            }
        }

        public bool EncryptR
        {
            get
            {
                return pipes[1].EncryptFrom;
            }
            set
            {
                pipes[1].EncryptFrom = value;
                pipes[0].EncryptTo = value;
            }
        }

        public bool IsWorking
        {
            get
            {
                bool result = false;
                if (SocketL != null)
                {
                    result = SocketL.Connected;
                }
                if (SocketR != null)
                {
                    result = SocketR.Connected || result;
                }
                return result;
            }
        }

        public Tunnel(Socket socketl = null, Socket socketr = null)
        {
            pipes = new Pipe[2] { new Pipe(socketl, socketr), new Pipe(socketr, socketl)};
        }

        public void Flow()
        {
            pipes[0].Flow();
            pipes[1].Flow();
        }

        public void Close()
        {
            pipes[0].Close();
            pipes[1].Close();
        }

        public string ReadStringL()
        {
            return pipes[0].ReadString();
        }

        public string ReadStringR()
        {
            return pipes[1].ReadString();
        }

        public bool WriteL(string msg)
        {
            return pipes[1].Write(msg);
        }

        public bool WriteR(string msg)
        {
            return pipes[0].Write(msg);
        }

        public byte[] ReadL()
        {
            return pipes[0].ReadByte();
        }

        public byte[] ReadR()
        {
            return pipes[1].ReadByte();
        }
    }
}