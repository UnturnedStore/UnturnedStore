using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Website.Client.Extensions;

namespace Website.Client.Shared
{
    public partial class MainLayout
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await JsRuntime.ClearModalBackdropAsync();
        }
    }
}
