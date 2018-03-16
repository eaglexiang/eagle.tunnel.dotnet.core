using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace eagle.tunnel.dotnet.core
{
    public class AuthenticationClient : Client
    {
        public AuthenticationClient(IPEndPoint[] remoteIPEPs, IPEndPoint localIPEP) :
            base(remoteIPEPs, localIPEP) { }
            
        protected override void HandleClient(object clientObj)
        {
            TcpClient client2Client = clientObj as TcpClient;
            TcpClient client2Server = new TcpClient();
            try
            {
                int tmpIndex = indexOfRemoteAddresses++;
                if (tmpIndex >= remoteAddresses.Length)
                {
                    tmpIndex %= remoteAddresses.Length;
                }
                client2Server.Connect(remoteAddresses[tmpIndex]);
                if (!Authenticate(client2Server))
                {
                    client2Server.Close();
                    client2Client.Close();
                    return;
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
            

            Pipe pipe0 = new Pipe(
                client2Client,
                client2Server
            );
            pipe0.EncryptTo = true;
            Pipe pipe1 = new Pipe(
                client2Server,
                client2Client
                );
            pipe0.EncryptTo = true;
            pipe1.EncryptFrom = true;

            pipe0.Flow();
            pipe1.Flow();
        }

        private bool Authenticate(TcpClient client)
        {
            string id = Conf.Users.Keys.First();
            string pswd = Conf.Users.Values.First();
            WriteStr(client, id);
            WriteStr(client, pswd);
            string result = ReadStr(client);
            if (result == "valid")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}