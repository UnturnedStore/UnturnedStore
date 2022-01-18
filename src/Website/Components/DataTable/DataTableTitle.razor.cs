using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Website.Components.DataTable
{
    public partial class DataTableTitle<TItem>
    {
        [CascadingParameter(Name = "DataTable")]
        public DataTable<TItem> DataTable { get; set; }

        [Parameter]
        public RenderFragment<DataTable<TItem>> ChildContent { get; set; }
        [Parameter]
        public Expression<Func<DataTable<TItem>, object>> ValueFunc { get; set; }
        [Parameter]
        public string Value { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                DataTable.UpdateTitle(this);
        }

        public RenderFragment GetRenderFragment()
        {
            return ChildContent(DataTable);
        }

        public object GetValue()
        {
            if (Value != null)
                return Value;

            Func<DataTable<TItem>, object> func = ValueFunc.Compile();
            if (func == null)
                return string.Empty;

            return func.Invoke(DataTable);
        }
    }
}
