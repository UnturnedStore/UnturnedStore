using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;

namespace Website.Server.Services
{
    public class BaseUrlService : IBaseUrl
    {
        public BaseUrlService(IConfiguration configuration)
        {
            BaseUrl = configuration["BaseUrl"].TrimEnd(Path.AltDirectorySeparatorChar);
        }

        public string BaseUrl { get; }

        public string Get(string relativeUrl, params object[] args)
        {
            relativeUrl = string.Format(relativeUrl, args);
            relativeUrl = relativeUrl.TrimStart(Path.AltDirectorySeparatorChar);
            return new StringBuilder()
                .Append(BaseUrl)
                .Append(Path.AltDirectorySeparatorChar)
                .Append(relativeUrl)
                .ToString();
        }
    }
}
