using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Website.Server.Pages.Prerender
{
    public interface IPrerenderedPage
    {
        bool IsPage(HttpRequest Request);
        Task<PrerenderedMetaTags> GetMetaTags();
    }
}
