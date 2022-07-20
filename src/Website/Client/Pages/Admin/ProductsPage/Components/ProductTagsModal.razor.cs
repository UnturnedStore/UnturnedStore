using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Shared.Models.Database;
using Website.Shared.Constants;
using Website.Components.Alerts;
using System.Net;
using System.Collections.Generic;

namespace Website.Client.Pages.Admin.ProductsPage.Components
{
    public partial class ProductTagsModal
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        [Parameter]
        public EventCallback<MProductTag> OnTagAdded { get; set; }
        [Parameter]
        public EventCallback<MProductTag> OnTagEdited { get; set; }

        public MProductTag Model { get; set; } = new MProductTag();
        private bool IsEditing { get; set; } = false;

        public async Task ShowAddAsync()
        {
            Model = new MProductTag();
            IsEditing = false;
            await JSRuntime.ShowModalStaticAsync(nameof(ProductTagsModal));
        }

        public async Task ShowEditAsync(MProductTag Tag)
        {
            Model = new MProductTag(Tag);
            IsEditing = true;
            await JSRuntime.ShowModalStaticAsync(nameof(ProductTagsModal));
        }

        private bool isLoading = false;
        public async Task SubmitAddAsync()
        {
            isLoading = true;

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products/tags", Model);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    AlertService.ShowAlert("product-tags-modal-basicinfo", $"There's already a tag with this name", AlertType.Danger);
                } else
                {
                    AlertService.ShowAlert("product-tags-modal-basicinfo", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                }
            } else
            {
                MProductTag tag = await response.Content.ReadFromJsonAsync<MProductTag>();
                Model.Id = tag.Id;
                await OnTagAdded.InvokeAsync(Model);

                AlertService.ShowAlert("product-tags-modal-main", $"Successfully created new tag <strong>{tag.Title}</strong>!", AlertType.Success);
                
                await JSRuntime.HideModalAsync(nameof(ProductTagsModal));
                Model = new MProductTag();
                StateHasChanged();
            }

            isLoading = false;
        }
        public async Task SubmitEditAsync()
        {
            isLoading = true;

            HttpResponseMessage response = await HttpClient.PutAsJsonAsync("api/products/tags", Model);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    AlertService.ShowAlert("product-tags-modal-basicinfo", $"There's already a tag with this name", AlertType.Danger);
                }
                else
                {
                    AlertService.ShowAlert("product-tags-modal-basicinfo", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                }
            }
            else
            {
                await OnTagEdited.InvokeAsync(Model);

                AlertService.ShowAlert("product-tags-modal-main", $"Successfully edited tag <strong>{Model.Title}</strong>!", AlertType.Success);

                await JSRuntime.HideModalAsync(nameof(ProductTagsModal));
                Model = new MProductTag();
                StateHasChanged();
            }

            isLoading = false;
        }
    }
}