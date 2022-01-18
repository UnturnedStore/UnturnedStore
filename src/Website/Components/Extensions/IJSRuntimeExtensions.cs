using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Website.Components.Extensions
{
    public static class IJSRuntimeExtensions
    {
        public static async Task ShowModalAsync(this IJSRuntime jsRuntime, string modalId)
        {
            await jsRuntime.InvokeVoidAsync("ShowModal", modalId);
        }

        public static async Task ShowModalStaticAsync(this IJSRuntime jsRuntime, string modalId)
        {
            await jsRuntime.InvokeVoidAsync("ShowModalStatic", modalId);
        }

        public static async Task HideModalAsync(this IJSRuntime jsRuntime, string modalId)
        {
            await jsRuntime.InvokeVoidAsync("HideModal", modalId);
        }

        public static async Task AddClassAsync(this IJSRuntime jsRuntime, ElementReference element, string cssClass)
        {
            await jsRuntime.InvokeVoidAsync("AddClass", element, cssClass);
        }
    }
}
