using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core
{
    public class Pipe
    {
        private TcpClient clientFrom;
        private TcpClient clientTo;
        public TcpClient ClientFrom
        {
            set
            {
                try
                {
                    clientFrom = value;
                    if(StreamFrom != null)
                    {
                        StreamFrom.Close();
                        StreamFrom = null;
                    }
                    if(clientFrom != null)
                    {
                        StreamFrom = clientFrom.GetStream();
                        // BufferSize = clientFrom.ReceiveBufferSize;
                        BufferSize = 204800;
                        bufferRead = new byte[BufferSize];
                    }
                }
                catch {}
            }
            get
            {
                return clientFrom;
            }
        }
        public TcpClient ClientTo
        {
            set
            {
                try
                {
                    clientTo = value;
                    if(StreamTo != null)
                    {
                        StreamTo.Close();
                        StreamTo = null;
                    }
                    if(clientTo != null)
                    {
                        StreamTo = clientTo.GetStream();
                    }
                }
                catch {}
            }
            get
            {
                return clientTo;
            }
        }
        private Stream StreamFrom { get; set;}
        private Stream StreamTo { get; set;}
        public bool EncryptFrom { get; set;}
        public bool EncryptTo { get; set;}
        private Thread flowThread;
        private static byte EncryptionKey = 0x22;
        private byte[] bufferRead;
        private int BufferSize { get; set;}

        public Pipe(TcpClient from, TcpClient to)
        {
            ClientFrom = from;
            ClientTo = to;
            EncryptFrom = false;
            EncryptTo = false;

            flowThread = new Thread(_Flow);
            flowThread.IsBackground = true;
        }

        public void Flow()
        {
            flowThread.Start();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if(ClientTo != null)
            {
                byte[] buffer1 = new byte[count];
                Array.Copy(buffer, buffer1, count);
                if(EncryptTo)
                {
                    buffer1 = Encryption(buffer1);
                }
                StreamTo.Write(buffer1, 0, count);
            }
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public void Write(string msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            Write(buffer);
        }

        public byte[] Read()
        {
            byte[] buffer;
            try
            {
                int count = StreamFrom.Read(bufferRead, 0, bufferRead.Length);
                if(count == 0)
                {
                    return null;
                }
                buffer = new byte[count];
                Array.Copy(bufferRead, buffer, count);
                if(EncryptFrom)
                {
                    buffer = Decrypt(buffer);
                }
            }
            catch
            {
                return null;
            }
            return buffer;
        }

        private void _Flow()
        {
            try
            {
                do
                {
                    byte[] buffer = Read();
                    if(buffer == null)
                    {
                        break;
                    }
                    Write(buffer);
                }while(true);
            }
            catch
            {
                StreamFrom.Close();
                StreamTo.Close();
                ClientFrom.Close();
                ClientTo.Close();
                return;
            }
            StreamFrom.Close();
            StreamTo.Close();
            ClientFrom.Close();
            ClientTo.Close();
        }

        public static byte[] Encryption(byte[] src)
        {
            byte[] des = new byte[src.Length];
            for(int i = 0; i < src.Length; ++i)
            {
                des[i] = (byte)(src[i] ^ EncryptionKey);
            }
            return des;
        }

        public static byte[] Decrypt(byte[] src)
        {
            byte[] des = new byte[src.Length];
            for(int i = 0; i < src.Length; ++i)
            {
                des[i] = (byte)(src[i] ^ EncryptionKey);
            }
            return des;
        }

        public void Close()
        {
            ClientFrom = null;
            ClientTo = null;
        }

        public string ReadString()
        {
            byte[] buffer = Read();
            if(buffer == null)
            {
                return null;
            }
            string str = Encoding.UTF8.GetString(buffer);
            return str;
        }
    }
}