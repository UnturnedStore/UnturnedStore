using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Website.Client.Services
{
    public class StorageService
    {
        private readonly IJSRuntime jsRuntime;

        public StorageService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task SetItemAsync(string key, object obj)
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, JsonSerializer.Serialize(obj));
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            string json = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);
            if (json != null)
                return JsonSerializer.Deserialize<T>(json);

            return default;
        }

        public async Task<T> GetAndRemoveItemAsync<T>(string key)
        {
            var obj = await GetItemAsync<T>(key);
            if (obj != null)
                await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
            return obj;
        }

        public async Task RemoveItemAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
        }
    }
}
