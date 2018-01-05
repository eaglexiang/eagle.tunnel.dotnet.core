using System;
using System.IO;
using System.Threading;
using System.Text;

namespace eagle.tunnel.dotnet.core
{
    public class Pipe
    {
        public Stream From { get; set;}
        public Stream To { get; set;}
        public bool EncryptFrom { get; set;}
        public bool EncryptTo { get; set;}
        private Thread flowThread;
        private static byte EncryptionKey = 0x22;
        public Pipe(Stream from, Stream to)
        {
            From = from;
            To = to;
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
            byte[] buffer1 = new byte[count];
            Array.Copy(buffer, buffer1, count);
            if(EncryptTo)
            {
                buffer1 = Encryption(buffer1);
            }
            To.Write(buffer1, 0, count);
        }

        public void Write(byte[] buffer)
        {
            byte[] buffer1;
            if(EncryptTo)
            {
                buffer1 = Encryption(buffer);
            }
            else
            {
                buffer1 = buffer;
            }
            To.Write(buffer1, 0, buffer1.Length);
        }

        public byte[] Read()
        {
            byte[] buffer0 = new byte[102400];
            int count = From.Read(buffer0, 0, 102400);
            byte[] buffer1 = new byte[count];
            Array.Copy(buffer0, buffer1, count);
            
            if(EncryptFrom)
            {
                buffer1 = Decrypt(buffer1);
            }

            return buffer1;
        }

        private void _Flow()
        {
            try
            {
                byte[] buffer;
                do
                {
                    buffer = Read();
                    Write(buffer);
                }while(buffer.Length > 0);
            }
            catch
            {
                From.Close();
                To.Close();
                return;
            }
            finally
            {
                From.Close();
                To.Close();
            }
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
    }
}