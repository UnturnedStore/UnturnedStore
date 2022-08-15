using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Shared.Models.Database;
using Website.Components.Alerts;
using System.Net;
using Website.Shared.Results;
using System.Collections.Generic;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class WorkshopItemModal
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        [Parameter]
        public EventCallback<MProductWorkshopItem> OnWorkshopItemAdded { get; set; }
        [Parameter]
        public EventCallback<MProductWorkshopItem> OnWorkshopItemEdited { get; set; }
        [Parameter]
        public string SellerSteamId { get; set; }

        public MProductWorkshopItem Model { get; set; }
        public Publishedfiledetail SuccessWorkshopItem { get; set; }

        private bool isEditing { get; set; }
        private bool isVerifed { get; set; }
        private bool noContent => string.IsNullOrEmpty(WorkshopFileId);

        public async Task ShowAddAsync(int productId)
        {
            Model = new MProductWorkshopItem() { ProductId = productId };
            SuccessWorkshopItem = null;
            isEditing = false;
            isVerifed = isEditing;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(WorkshopItemModal));
        }

        public async Task ShowEditAsync(MProductWorkshopItem model)
        {
            Model = new MProductWorkshopItem(model);
            SuccessWorkshopItem = null;
            isEditing = true;
            isVerifed = isEditing;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(WorkshopItemModal));
            await VerifyAsync();
        }

        private string WorkshopFileId
        {
            get
            {
                if (Model.WorkshopFileId == default) return string.Empty; 
                return Model.WorkshopFileId.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Model.WorkshopFileId = 0;
                    isVerifed = false;
                }
                else if (ulong.TryParse(value, out ulong fileId))
                {
                    Model.WorkshopFileId = fileId;
                    isVerifed = false;
                }
            }
        }

        private bool isLoading2 = false;
        public async Task VerifyAsync()
        {
            isLoading2 = true;

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products/workshop/verify", new List<MProductWorkshopItem>() { Model });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WorkshopItemResult workshopResult = await response.Content.ReadFromJsonAsync<WorkshopItemResult>();
                SuccessWorkshopItem = workshopResult.GetSuccessItem(Model.WorkshopFileId);
                isVerifed = SuccessWorkshopItem != null;
                if (!isVerifed)
                {
                    AlertService.ShowAlert("product-workshop-modal", $"Invalid Workshop item, only public Unturned published workshop items are allowed", AlertType.Danger);
                }
            }
            else
            {
                AlertService.ShowAlert("product-workshop-modal", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
            }

            isLoading2 = false;
        }

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            if (!isVerifed) return;

            isLoading = true;

            if (!isEditing)
            {
                HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products/workshop", Model);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        AlertService.ShowAlert("product-workshop", $"There's already a workshop item with this file id", AlertType.Danger);
                    }
                    else
                    {
                        AlertService.ShowAlert("product-workshop", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                    }
                }
                else
                {
                    MProductWorkshopItem workshopItem = await response.Content.ReadFromJsonAsync<MProductWorkshopItem>();
                    await OnWorkshopItemAdded.InvokeAsync(workshopItem);

                    AlertService.ShowAlert("product-workshop", $"Successfully created new workshop item with file id <strong>{workshopItem.WorkshopFileId}</strong>!", AlertType.Success);

                    await JsRuntime.HideModalAsync(nameof(WorkshopItemModal));
                    Model = new MProductWorkshopItem();
                    StateHasChanged();
                }
            }
            else
            {
                HttpResponseMessage response = await HttpClient.PutAsJsonAsync("api/products/workshop", Model);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        AlertService.ShowAlert("product-workshop", $"There's already a workshop item with this file id", AlertType.Danger);
                    }
                    else
                    {
                        AlertService.ShowAlert("product-workshop", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                    }
                }
                else
                {
                    await OnWorkshopItemEdited.InvokeAsync(Model);

                    AlertService.ShowAlert("product-workshop", $"Successfully edited workshop item with file id <strong>{Model.WorkshopFileId}</strong>!", AlertType.Success);

                    await JsRuntime.HideModalAsync(nameof(WorkshopItemModal));
                    Model = new MProductWorkshopItem();
                    StateHasChanged();
                }
            }

            isLoading = false;

            await JsRuntime.HideModalAsync(nameof(WorkshopItemModal));
        }
    }
}