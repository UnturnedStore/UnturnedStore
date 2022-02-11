using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;
using Website.Components.Alerts;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.ProductPage.Components.UploadVersion.Inputs
{
    public partial class VersionZipFileInput
    {
        [Parameter]
        public MVersion Version { get; set; }
        [Parameter]
        public EventCallback<MVersion> VersionChanged { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; }

        [Inject]
        public AlertService AlertService { get; set; }

        private async Task OnFileChange(InputFileChangeEventArgs e)
        {
            System.Console.WriteLine(e.File.ContentType);
            if (e.File.ContentType != "application/zip" && e.File.ContentType != "application/x-zip-compressed")
            {
                return;
            }

            Version.Content = new byte[e.File.Size];
            Version.ContentType = e.File.ContentType;
            Version.FileName = e.File.Name;
            await e.File.OpenReadStream(30 * 1024 * 1024).ReadAsync(Version.Content);
            StateHasChanged();
        }
    }
}
