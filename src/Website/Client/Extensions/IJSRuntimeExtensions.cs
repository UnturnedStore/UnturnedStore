using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Client.Extensions
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

        public static async Task RefreshHighlightAsync(this IJSRuntime jsRuntime)
        {
            await jsRuntime.InvokeVoidAsync("HighlightAll");
        }
    }
}
