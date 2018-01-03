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
                byte[] buffer = new byte[1024];
                int count;
                while(true)
                {
                    count = From.Read(buffer, 0, 1024);
                    To.Write(buffer, 0, count);
                }
            }
            catch
            {
                
            }
        }
    }
}