using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Pages.Prerender;
using Website.Server.Pages.Prerender.PrerenderedPages;
using Website.Shared.Models.Database;

namespace Website.Server.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class HostModel : PageModel
    {
        public PrerenderedMetaTags MetaTags { get; set; }
        public IPrerenderedPage[] PrerenderedPages { get; }

        public HostModel(ProductsRepository productsRepository)
        {
            PrerenderedPages = new IPrerenderedPage[]
            {
                new PrerenderedProductPage(productsRepository)
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Request.Headers.ContainsKey("UsePrerender") && Request.Path.HasValue)
            {
                foreach (IPrerenderedPage prerenderedPage in PrerenderedPages)
                    if (prerenderedPage.IsPage(Request))
                    {
                        MetaTags = await prerenderedPage.GetMetaTags();
                        return Page();
                    }
            }

            return File("index.html", "text/html");
        }
    }
}
