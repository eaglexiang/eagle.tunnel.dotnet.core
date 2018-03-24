using System;
using System.Net;
using System.Net.Sockets;

namespace eagle.tunnel.dotnet.core
{
    public class SocksClient : Authen_ClientRelay
    {
        private static Random rand = new Random();

        public SocksClient(IPEndPoint[] remoteIPEPs, IPEndPoint localIPEP) : base(remoteIPEPs, localIPEP) { }

        enum CMDType
        {
            Null,
            Connect,
            Bind,
            Udp
        }

        protected override bool WorkFlow(Pipe pipeClient2Server, Pipe pipeServer2Client)
        {
            bool result = false;
            byte[] request = pipeClient2Server.ReadByte();
            if(request != null)
            {
                int version = request[0];
                // check if is socks version 5
                if(version == '\u0005')
                {
                    string reply = "\u0005\u0000";
                    pipeServer2Client.Write(reply);

                    request = pipeClient2Server.ReadByte();
                    if(request != null)
                    {
                        CMDType cmdType = (CMDType)request[1];
                        if(cmdType == CMDType.Connect)
                        {
                            result = HandleTCPReq(request, pipeServer2Client, pipeClient2Server);
                        }
                        else if(cmdType == CMDType.Udp)
                        {
                            // result = HandleUDPReq(request, pipeServer2Client, pipeClient2Server);
                        }
                    }
                }
            }
            return result;
        }

        protected override void PrintServerInfo(IPEndPoint localIPEP)
        {
            Console.WriteLine ("Socks Relay started: " + serverIPEP.ToString ());
        }

        // private bool HandleUDPReq(byte[] request, Pipe pipeServer2Client, Pipe pipeClient2Server)
        // {

        // }

        private bool HandleTCPReq(byte[] request, Pipe pipeServer2Client, Pipe pipeClient2Server)
        {
            string ip = GetIP(request);
            int port = GetPort(request);
            if(ip != null && port != 0)
            {
                if (IPAddress.TryParse(ip, out IPAddress ipa))
                {
                    IPEndPoint reqIPEP = new IPEndPoint(ipa, port);
                    string reply;
                    bool reqReply = SendReqEndPoint(pipeClient2Server.SocketTo, reqIPEP);
                    if (reqReply)
                    {
                        reply = "\u0005\u0000\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                    }
                    else
                    {
                        reply = "\u0005\u0001\u0000\u0001\u0000\u0000\u0000\u0000\u0000\u0000";
                    }
                    pipeServer2Client.Write(reply);
                    return reqReply;
                }
            }
            return false;
        }

        public static string GetIP(byte[] request)
        {
            try
            {
                int destype = request[3];
                string ip;
                switch (destype)
                {
                case 1:
                    ip = request[4].ToString();
                    ip += '.' + request[5].ToString();
                    ip += '.' + request[6].ToString();
                    ip += '.' + request[7].ToString();
                    break;
                case 3:
                    int len = request[4];
                    char[] hostChars = new char[len];
                    for(int i = 0; i<len; ++i)
                    {
                        hostChars[i] = (char)request[5 + i];
                    }
                    string host = new string(hostChars);
                    // if host is real ip but not domain name
                    if(IPAddress.TryParse(host, out IPAddress ipa))
                    {
                        ip = host;
                    }
                    else
                    {
                        IPHostEntry iphe = Dns.GetHostEntry(host);
                        ip = null;
                        foreach(IPAddress tmp in iphe.AddressList)
                        {
                            if(tmp.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ip = tmp.ToString();
                            }
                        }
                    }
                    break;
                default:
                    ip = null;
                    break;
                }
                return ip;
            }
            catch
            {
                return null;
            }
        }

        public static int GetPort(byte[] request)
        {
            try
            {
                int destype = request[3];
                int port;
                int high;
                int low;
                switch (destype)
                {
                case 1:
                    high = request[8];
                    low = request[9];
                    port = high * 0x100 + low;
                    break;
                case 3:
                    int len = request[4];
                    high = request[5 + len];
                    low = request[6 + len];
                    port = high * 0x100 + low;
                    break;
                default:
                    port = 0;
                    break;
                }
                return port;
            }
            catch
            {
                return 0;
            }
        }

        public static byte[] GetUDPData(byte[] request)
        {
            try
            {
                int destype = request[3];
                byte[] data;
                switch (destype)
                {
                case 1:
                    data = new byte[request.Length - 10];
                    request.CopyTo(data, 10);
                    break;
                case 3:
                    int len = request[4];
                    data = new byte[request.Length - 7 - len];
                    request.CopyTo(data, 7 + len);
                    break;
                default:
                    data = null;
                    break;
                }
                return data;
            }
            catch
            {
                return null;
            }
        }
    }
}