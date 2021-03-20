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

        public async Task SetSessionItemAsync(string key, object obj)
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, JsonSerializer.Serialize(obj));
        }

        public async Task<T> GetSessionItemAsync<T>(string key)
        {
            string json = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);
            if (json != null)
                return JsonSerializer.Deserialize<T>(json);

            return default;
        }

        public async Task<T> GetAndRemoveSessionItemAsync<T>(string key)
        {
            var obj = await GetSessionItemAsync<T>(key);
            if (obj != null)
                await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
            return obj;
        }

        public async Task RemoveSessionItemAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
        }

        public async Task SetLocalItemAsync(string key, object obj)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(obj));
        }

        public async Task<T> GetLocalItemAsync<T>(string key)
        {
            string json = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (json != null)
                return JsonSerializer.Deserialize<T>(json);

            return default;
        }

        public async Task<T> GetAndRemoveLocalItemAsync<T>(string key)
        {
            var obj = await GetSessionItemAsync<T>(key);
            if (obj != null)
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            return obj;
        }

        public async Task RemoveLocalItemAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
