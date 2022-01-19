namespace Website.Server.Services
{
    public interface IBaseUrl
    {
        string Get(string relativeUrl, params object[] args);
    }
}