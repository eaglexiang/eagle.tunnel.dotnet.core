using System;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Client
    {
        private static Hashtable certificateErrors;
        private TcpListener localServer;
        private string ServerHost { get; set;}
        private int ServerPort { get; set;}
        private string CertHost { get; set;}

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Cert error: " + sslPolicyErrors);
                return false;
            }
        }

        public Client()
        {
            certificateErrors = new Hashtable();
        }

        private SslStream Connect2Server(string serverhost, int port, string certhost)
        {
            TcpClient client = new TcpClient(serverhost, port);
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback (ValidateServerCertificate),
                null
            );

            try
            {
                sslStream.AuthenticateAsClient(certhost);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine ("Authentication failed - closing the connection.");
                client.Close();
                return null;
            }
            return sslStream;
        }

        public bool Start(
            string serverhost,int serverport, string certhost,
            string localhost, int localport, int localbacklog
        )
        {
            ServerHost = serverhost;
            ServerPort = serverport;
            CertHost = certhost;

            IPAddress ipa = IPAddress.Parse(localhost);
            localServer = new TcpListener(ipa, localport);
            localServer.Start(localbacklog);

            while(true)
            {
                TcpClient client = localServer.AcceptTcpClient();
                NetworkStream stream2Client = client.GetStream();
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(stream2Client);
            }
        }

        private void HandleClient(object streamObj)
        {
            SslStream stream2Server = Connect2Server(ServerHost, ServerPort, CertHost);
            if(stream2Server == null)
            {
                return;
            }
            NetworkStream stream2Client = streamObj as NetworkStream;

            Pipe pipe0 = new Pipe(stream2Client, stream2Server);
            Pipe pipe1 = new Pipe(stream2Server, stream2Client);

            pipe0.Flow();
            pipe1.Flow();
        }
    }
}