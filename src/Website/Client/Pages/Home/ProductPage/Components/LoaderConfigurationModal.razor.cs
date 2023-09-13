using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Client.Helpers;
using Website.Client.Models;
using Website.Shared.Constants;
using Website.Shared.Models.Database;
using Website.Shared.Models.Product;

namespace Website.Client.Pages.Home.ProductPage.Components
{
    public partial class LoaderConfigurationModal
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public MProduct Product { get; set; }
        [Parameter]
        public MBranch Branch { get; set; }
        [Parameter]
        public ProductLicense ProductLicense { get; set; }

        public LoaderConfigurationExample LoaderConfigurationExample { get; set; }
        public string ConfigurationStringXml { get; set; }
        public string ConfigurationStringYaml { get; set; }

        public async Task ShowModalAsync()
        {
            LoaderConfigurationExample = new()
            {
                Name = Product.Name,
                Branch = Branch?.Name ?? "",
                Version = "latest",
                License = ProductLicense?.LicenseKey ?? Guid.Empty,
                Enabled = true
            };

            if (Product.Category == ProductCategoryConstants.RocketPlugin)
            {
                ConfigurationStringXml = SerializationHelper.SerializeObjectToXML(LoaderConfigurationExample);
                StateHasChanged();
            }

            if (Product.Category == ProductCategoryConstants.OpenModPlugin)
            {
                ConfigurationStringYaml = SerializationHelper.SerializeObjectToYaml(LoaderConfigurationExample);
                StateHasChanged();
            }

            await JSRuntime.ShowModalAsync(nameof(LoaderConfigurationModal));
        }
    }
}
