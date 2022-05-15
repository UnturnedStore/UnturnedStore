using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using Website.Shared.Constants;

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
            return string.Join('/',
                BaseUrl,
                relativeUrl);
        }
    }
}
