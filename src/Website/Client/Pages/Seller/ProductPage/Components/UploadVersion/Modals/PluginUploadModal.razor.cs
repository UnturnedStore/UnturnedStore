using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Website.Shared.Models.Database;
using Website.Client.Services;
using System.Linq;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Components.Alerts;

namespace Website.Client.Pages.Seller.ProductPage.Components.UploadVersion.Modals
{
    public partial class PluginUploadModal
    {
        [Parameter]
        public MVersion Version { get; set; }
        [Parameter]
        public EventCallback<MVersion> VersionChanged { get; set; }

        [Parameter]
        public EventCallback OnZipArchive { get; set; }

        [Inject]
        public ZIPService ZIPService { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        private IBrowserFile Plugin { get; set; }
        private List<IBrowserFile> Libraries { get; set; } = new List<IBrowserFile>();
        
        public async Task ShowAsync()
        {
            await JSRuntime.ShowModalStaticAsync(nameof(PluginUploadModal));
        }

        private async Task HideAsync()
        {
            await JSRuntime.HideModalAsync(nameof(PluginUploadModal));
        }

        private bool IsValidDll(IBrowserFile browserFile)
        {
            return browserFile.Name.EndsWith(".dll");
        }

        private void OnPluginFileChange(InputFileChangeEventArgs e)
        {
            if (!IsValidDll(e.File))
            {
                AlertService.ShowAlert("pluginuploadmodal-main", $"<strong>{e.File.Name}</strong> is not a valid .dll file", AlertType.Danger);
                Plugin = null;
                return;
            }   

            Plugin = e.File;
        }

        private void OnPluginLibraryFileChange(InputFileChangeEventArgs e)
        {
            IReadOnlyList<IBrowserFile> files = e.GetMultipleFiles(50);
            List<IBrowserFile> filesList = files.ToList();

            List<string> invalidLibraries = new List<string>();

            foreach (IBrowserFile file in files)
            {
                if (!IsValidDll(file))
                {
                    filesList.Remove(file);
                    invalidLibraries.Add(file.Name);
                }                
            }

            Libraries = filesList;

            if (invalidLibraries.Count > 0)
            {
                AlertService.ShowAlert("pluginuploadmodal-main", 
                    $"<strong>{string.Join(", ", invalidLibraries)}</strong> are not valid .dll files", AlertType.Warning);
            }
        }

        private bool isDisabled => Plugin == null;
        private bool isZipping = false;
        private async Task ZipPluginAsync()
        {
            if (Plugin == null)
                return;

            isZipping = true;
            
            Version.Content = await ZIPService.ZipAsync(new Dictionary<string, IEnumerable<IBrowserFile>>() 
            {
                { "Plugins",  new IBrowserFile[]{ Plugin } }, 
                { "Libraries", Libraries }
            });

            Version.FileName = Plugin.Name.Replace(".dll", ".zip", StringComparison.OrdinalIgnoreCase);
            Version.ContentType = "application/zip";

            Plugin = null;
            Libraries = new List<IBrowserFile>();

            await OnZipArchive.InvokeAsync();
            AlertService.ShowAlert("uploadversiontab-main", 
                $"Successfully zipped plugin & libraries into <strong>{Version.FileName}</strong>!", AlertType.Success);

            await HideAsync();
            isZipping = false;
        }
    }
}