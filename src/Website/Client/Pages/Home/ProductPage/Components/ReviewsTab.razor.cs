using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Components.Basic;
using Website.Shared.Models.Database;
using System.Net.Http;

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
        [Parameter]
        public EventCallback<MProductReview> OnReviewReplyChanged { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AuthenticatedUserService UserService { get; set; }

        public ProductReviewReplyModal ReplyModal { get; set; }
        public ConfirmModal<MProductReview> DeleteReviewConfirm { get; set; }
        public ConfirmModal<MProductReview> DeleteReplyConfirm { get; set; }

        public async Task HandleEditReplyAsync(MProductReview review)
        {
            await ReplyModal.ShowModalAsync(review);
        }

        public async Task EditReplyAsync(MProductReview review)
        {
            await OnReviewReplyChanged.InvokeAsync(review);
        }

        public async Task HandleDeleteReplyAsync(MProductReview review)
        {
            await DeleteReplyConfirm.ShowAsync(review);
        }

        public async Task DeleteReplyAsync(MProductReview review)
        {
            await HttpClient.DeleteAsync("api/products/reviews/replies/" + review.Reply.Id);
            review.Reply = null;
            await OnReviewReplyChanged.InvokeAsync(review);
        }

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
