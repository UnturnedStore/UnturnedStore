using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Components.Extensions;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Admin.UsersPage.Components
{
    public partial class UserModal
    {
        [Parameter]
        public EventCallback OnUpdate { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        private MUser User { get; set; }
        public MUser Model { get; set; }

        public const string ModalId = "UserModal";

        public async Task ShowUserAsync(MUser user)
        {
            if (User == null || User.Id != user.Id)
            {
                User = user;
                Model = MUser.FromUser(user);
            }            
            await JsRuntime.ShowModalStaticAsync(ModalId);
            StateHasChanged();
        }

        public async Task HideAsync()
        {
            await JsRuntime.HideModalAsync(ModalId);
        }

        private bool isLoading = false;

        private async Task SaveChangesAsync()
        {
            isLoading = true;
            await HttpClient.PutAsJsonAsync("api/admin/users", Model);
            User = Model;
            isLoading = false;            
            await OnUpdate.InvokeAsync();
            await HideAsync();
        }
    }
}
