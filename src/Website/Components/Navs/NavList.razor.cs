using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Components.Navs
{
    public partial class NavList
    {
        [Parameter]
        public string Title { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        [Parameter]
        public NavType Type { get; set; } = NavType.Pills;

        private List<NavItem> items = new();

        public IEnumerable<NavItem> Items => items; 

        public void AddNavItem(NavItem item)
        {
            items.Add(item);
            if (CurrentItem == null)
                ChangeNavItem(item);

            StateHasChanged();
        }

        public NavItem CurrentItem { get; private set; }

        public void ChangeNavItem(NavItem item)
        {
            CurrentItem = item;
        }

        public string GetNavItemClass(NavItem item)
        {
            if (CurrentItem == item)
            {
                return "active";
            }
            return string.Empty;
        }
    }
}
