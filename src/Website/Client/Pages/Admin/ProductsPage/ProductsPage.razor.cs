using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Admin.ProductsPage.Components;
using Website.Components.Basic;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Admin.ProductsPage
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    public partial class ProductsPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public IEnumerable<MProduct> Products { get; set; }
        public List<MProductTag> ProductTags { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<MProduct[]>("api/admin/products");
            ProductTags = await HttpClient.GetFromJsonAsync<List<MProductTag>>("api/products/tags");
        }

        public ProductTagsModal Modal { get; set; }
        public async Task ShowModalAddTagAsync()
        {
            await Modal.ShowAddAsync();
        }

        public async Task ShowModalEditTagAsync(MProductTag Tag)
        {
            await Modal.ShowEditAsync(Tag);
        }

        public void AddTag(MProductTag Tag)
        {
            ProductTags.Add(Tag);
        }

        public void EditTag(MProductTag newTag)
        {
            for (int t = 0; t < ProductTags.Count; t++)
                if (ProductTags[t].Id == newTag.Id)
                {
                    ProductTags[t] = newTag;
                    break;
                }
        }

        public ConfirmModal<MProductTag> ConfirmDeleteTag { get; set; }
        public async Task ShowDeleteTagAsync(MProductTag Tag)
        {
            await ConfirmDeleteTag.ShowAsync(Tag);
        }

        public async Task DeleteTagAsync(MProductTag Tag)
        {
            await HttpClient.DeleteAsync("api/products/tags/" + Tag.Id);
            ProductTags.Remove(Tag);
        }
    }
}
