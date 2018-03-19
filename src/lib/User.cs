namespace eagle.tunnel.dotnet.core
{
    public class TunnelUser
    {
        public string ID { get; }
        public string Password { get; set;}

        public TunnelUser(string id, string password)
        {
            ID = id;
            Password = password;
        }
    }
}