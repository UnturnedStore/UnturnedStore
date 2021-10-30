using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Shared.Params;

namespace Website.Client.Pages.Home.CartPage.Components
{
    public partial class SellerCart
    {
        [Parameter]
        public OrderParams Cart { get; set; }

        [Parameter]
        public EventCallback<OrderItemParams> OnRemoveFromCart { get; set; }

        public async Task RemoveFromCartAsync(OrderItemParams item)
        {
            await OnRemoveFromCart.InvokeAsync(item);
        }
    }
}
