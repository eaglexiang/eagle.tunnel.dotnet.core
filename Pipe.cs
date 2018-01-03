using System;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Pipe
    {
        public Stream From;
        public Stream To;
        public Pipe(Stream from, Stream to)
        {
            From = from;
            To = to;
        }

        public void Flow()
        {
            Thread thread = new Thread(_Flow);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            To.Write(buffer, 0, count);
        }

        private void _Flow()
        {
            try
            {
                byte[] buffer = new byte[102400];
                int count;
                while(true)
                {
                    count = From.Read(buffer, 0, 102400);
                    To.Write(buffer, 0, count);
                }
            }
            catch
            {
                From.Close();
                To.Close();
                return;
            }
        }
    }
}