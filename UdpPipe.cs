using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class UdpPipe
    {
        private string ClientIP { get; set;}
        private int ClientPort { get; set;}
        private string BondIP { get; set;}
        private int BondPort { get; set;}
        public bool Running { get; set;}
        private IPEndPoint BondIpe;
        public bool EncryptFrom { get; set;} = false;
        public bool EncryptTo { get; set;} = false;
        private static byte EncryptionKey = 0x22;

        public UdpPipe(string clientIp, int clientPort, string bondIp, int bondPort)
        {
            ClientIP = clientIp;
            ClientPort = clientPort;
            BondIP = bondIp;
            BondPort = bondPort;
            IPAddress.TryParse(bondIp, out IPAddress ipa);
            BondIpe = new IPEndPoint(ipa, BondPort);
        }

        public byte[] Read()
        {
            UdpClient client = new UdpClient();
            byte[] buffer = client.Receive(ref BondIpe);
            if(EncryptFrom)
            {
                buffer = Decrypt(buffer);
            }
            return buffer;
        }

        private void Write(byte[] buffer, int count, string ip, int port)
        {
            byte[] tmp = new byte[count];
            Array.Copy(buffer, tmp, count);
            IPAddress.TryParse(ip, out IPAddress ipa);
            IPEndPoint ipep = new IPEndPoint(ipa, port);
            Write(tmp, ipep);
        }

        private void Write(byte[] buffer, IPEndPoint ipep)
        {
            UdpClient client = new UdpClient();
            if(EncryptTo)
            {
                buffer = Encryption(buffer);
            }
            client.Send(buffer, buffer.Length, ipep);
        }

        public void Flow()
        {
            Thread flowThread = new Thread(_Flow);
            flowThread.IsBackground = true;
            flowThread.Start();
        }

        private void _Flow()
        {
            try
            {
                while(Running)
                {
                    byte[] buffer = Read();
                    if(buffer[0] == 5)
                    {
                        byte[] request = buffer;
                        string ip = SocksServer.GetIP(request);
                        int port = SocksServer.GetPort(request);
                        byte[] data = SocksServer.GetUDPData(request);
                        Write(data, data.Length, ip, port);
                    }
                    else
                    {
                        Write(buffer, buffer.Length, ClientIP, ClientPort);
                    }
                }
            }
            catch {}
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