using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Website.Shared.Models.Database;
using Website.Shared.Models;
using Website.Components.Alerts;
using System.Net;
using System.Collections.Generic;
using Website.Shared.Constants;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class WorkshopItemsTab
    {
        [Parameter]
        public SellerProduct Product { get; set; }

        [Parameter]
        public EventCallback<SellerProduct> ProductChanged { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        public WorkshopItemModal WorkshopItemModal { get; set; }

        private async Task HandleAddWorkshopItem()
        {
            await WorkshopItemModal.ShowAddAsync(Product.Id);
        }

        private async Task AddWorkshopItem(MProductWorkshopItem workshopItem)
        {
            Product.WorkshopItems.Add(workshopItem);

            await ProductChanged.InvokeAsync(Product);
        }

        private async Task HandleEditWorkshopItem(MProductWorkshopItem workshopItem)
        {
            await WorkshopItemModal.ShowEditAsync(workshopItem);
        }

        private async Task EditWorkshopItem(MProductWorkshopItem workshopItem)
        {
            for (int i = 0; i < Product.WorkshopItems.Count; i++)
            {
                if (Product.WorkshopItems[i].Id == workshopItem.Id)
                {
                    Product.WorkshopItems[i] = workshopItem;
                    break;
                }
            }

            await ProductChanged.InvokeAsync(Product);
        }

        private bool isLoading;
        private MProductWorkshopItem removingWorkshopItem;
        private async Task RemoveWorkshopItem(MProductWorkshopItem workshopItem)
        {
            removingWorkshopItem = workshopItem;
            isLoading = true;
            HttpResponseMessage response = await HttpClient.DeleteAsync($"api/products/workshop/{workshopItem.Id}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Product.WorkshopItems.Remove(workshopItem);
                await ProductChanged.InvokeAsync(Product);
            }
            else
            {
                AlertService.ShowAlert("product-workshop", "There was an error when trying to remove this workshop item!", AlertType.Danger);
            }
            isLoading = false;
            removingWorkshopItem = null;
        }
    }
}