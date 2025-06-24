public class SocailMediaUser
{
    public string username;
    public string email;
    public string password;
    public string proxy;
    public UserProxy userProxy;

    public SocailMediaUser(string username, string email, string password)
    {
        this.username = username;
        this.email = email;
        this.password = password;
    }

    public SocailMediaUser(string username, string email, string password, UserProxy userProxy)
    {
        this.username = username;
        this.email = email;
        this.password = password;
        this.userProxy = userProxy;
    }
}

public class UserProxy
{
    public string proxyIP;
    public string proxyPort;
    public string username;
    public string password;

    public UserProxy(string proxyIP, string proxyPort, string username, string password)
    {
        this.proxyIP = proxyIP;
        this.proxyPort = proxyPort;
        this.username = username;
        this.password = password;
    }

    public string GetProxyString()
    {
        return $"{username}:{password}@{proxyIP}:{proxyPort}";
    }

    public string GetServerString()
    {
        return $"http://{proxyIP}:{proxyPort}";
    }
}