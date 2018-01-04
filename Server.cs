using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;

namespace eagle.tunnel.dotnet.core
{
    public class Server
    {
        public X509Certificate serverCertificate;
        public Server()
        {
            ;
        }

        public bool Start(string ip, int port, string cert)
        {
            try
            {
                IPAddress ipa = IPAddress.Parse(ip);
                TcpListener server = new TcpListener(ipa, port);
                server.Start();
                Thread handleServerThread = new Thread(HandleServer);
                handleServerThread.IsBackground = true;
                handleServerThread.Start(server);
                Console.WriteLine("server started: " + ip + ":" + port);
                serverCertificate = X509Certificate.CreateFromCertFile(cert);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void HandleServer(object serverObj)
        {
            TcpListener server = serverObj as TcpListener;
            while(true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("new client connected");
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(client);
            }
        }

        private void HandleClient(object clientObj)
        {
            TcpClient socket2Client = clientObj as TcpClient;
            NetworkStream stream2client = socket2Client.GetStream();
            SslStream sslStream2Client = new SslStream(
                stream2client, false);
            try
            {
                sslStream2Client.AuthenticateAsServer(
                    serverCertificate,
                    false,
                    SslProtocols.Tls,
                    true
                );

                sslStream2Client.ReadTimeout = 5000;
                sslStream2Client.ReadTimeout = 5000;
                HandleSocket2Client(sslStream2Client);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine ("Authentication failed - closing the connection.");
                sslStream2Client.Close();
                return;
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                sslStream2Client.Close();
            }
        }

        private void HandleSocket2Client(SslStream stream2client)
        {
            byte[] buffer = new byte[102400];
            try
            {
                int count = stream2client.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, count);
                Console.WriteLine("Request: " + request);
                string host = GetHost(request);
                int port = GetPort(request);
                if(host == "")
                {
                    return ;
                }
                IPAddress[] ipas = Dns.GetHostAddresses(host);
                string ip = ipas[0].ToString();
                Console.WriteLine("connect to " + host + ":" + port);
                
                TcpClient client2Server = new TcpClient(ip, port);
                NetworkStream stream2server = client2Server.GetStream();
                
                Pipe pipe0 = new Pipe(stream2client, stream2server);
                Pipe pipe1 = new Pipe(stream2server, stream2client);

                if(port == 443)
                {
                    string re443 = "HTTP/1.1 OK\r\n\r\n";
                    buffer = Encoding.UTF8.GetBytes(re443);
                    pipe1.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    pipe0.Write(buffer, 0, count);
                }

                pipe0.Flow();
                pipe1.Flow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetURI(string request)
        {
            StringReader reader = new StringReader(request);
            string line = reader.ReadLine();
            int ind0 = line.IndexOf(' ');
            int ind1 = line.LastIndexOf(' ');
            if(ind0 == ind1)
            {
                ind1 = line.Length;
            }
            string uri = request.Substring(ind0 + 1, ind1 - ind0 -1);
            return uri;
        }

        private bool IsNum(char c)
        {
            return '0' <= c && c <= '9';
        }

        private string GetHost(string request)
        {
            string uristr = GetURI(request);
            string host;
            if(
                uristr.Contains(":") &&
                IsNum(uristr[uristr.IndexOf(":") + 1]))
            {
                int ind = uristr.LastIndexOf(":");
                host = uristr.Substring(0, ind);
            }
            else
            {
                Uri uri = new Uri(uristr);
                host = uri.Host;
            }
            return host;
        }

        private int GetPort(string request)
        {
            string uristr = GetURI(request);
            int port;
            if(
                uristr.Contains(":") &&
                IsNum(uristr[uristr.IndexOf(":") + 1]))
            {
                int ind = uristr.IndexOf(":");
                string _port = uristr.Substring(ind + 1);
                port = int.Parse(_port);
            }
            else
            {
                Uri uri = new Uri(uristr);
                port = uri.Port;
            }
            return port;
        }
    }
}