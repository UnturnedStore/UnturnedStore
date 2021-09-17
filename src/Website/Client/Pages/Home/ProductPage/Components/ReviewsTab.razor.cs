using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Client.Shared.Components;
using Website.Shared.Models;

namespace Website.Client.Pages.Home.ProductPage.Components
{
    public partial class ReviewsTab
    {
        [Parameter]
        public MProduct Product { get; set; }
        [Parameter]
        public MProductReview UserReview { get; set; }

        [Parameter]
        public EventCallback<MProductReview> OnDeleteReview { get; set; }

        [Inject]
        public UserService UserService { get; set; }

        public ConfirmModal<MProductReview> DeleteReviewConfirm { get; set; }

        public async Task AskDeleteReviewAsync(MProductReview review)
        {
            await DeleteReviewConfirm.ShowAsync(review);
        }

        public async Task DeleteReviewAsync(MProductReview review)
        {
            await OnDeleteReview.InvokeAsync(review);
        }
    }
}
