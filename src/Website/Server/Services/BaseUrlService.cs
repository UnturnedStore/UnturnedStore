using Microsoft.Extensions.Configuration;

namespace Website.Server.Services
{
    public class BaseUrlService : IBaseUrl
    {
        public BaseUrlService(IConfiguration configuration)
        {
            BaseUrl = configuration["BaseUrl"].TrimEnd('/');
        }

        public string BaseUrl { get; }

        public string Get(string relativeUrl, params object[] args)
        {
            relativeUrl = string.Format(relativeUrl, args);
            relativeUrl = relativeUrl.TrimStart('/');
            return BaseUrl + "/" + relativeUrl;
        }
    }
}
