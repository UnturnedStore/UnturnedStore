using Microsoft.AspNetCore.Components;

namespace Website.Components.Navs
{
    public abstract class NavTypeBase : ComponentBase
    {
        [Parameter]
        public NavList NavList { get; set; }

        protected string Title => NavList.Title;
        protected IEnumerable<NavItem> Items => NavList.Items;
        protected NavItem CurrentItem => NavList.CurrentItem;

        protected void ChangeNavItem(NavItem item)
        {
            NavList.ChangeNavItem(item);
        }

        protected string GetNavItemClass(NavItem item)
        {
            return NavList.GetNavItemClass(item);
        }
    }
}
