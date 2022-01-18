using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Website.Components.Navs
{
    public partial class NavItem
    {
        [CascadingParameter(Name = "NavList")]
        public NavList NavList { get; set; }

        [Parameter]
        public string Name { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                NavList.AddNavItem(this);
        }
    }
}
