using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace Website.Components.DataTable
{
    public partial class DataTableColumn<TItem>
    {
        [CascadingParameter(Name = "DataTable")]
        public DataTable<TItem> DataTable { get; set; }

        [Parameter]
        public string Name { get; set; }
        [Parameter]
        public Expression<Func<TItem, object>> Field { get; set; }
        [Parameter]
        public bool IsSearchable { get; set; }
        [Parameter]
        public ESearchType SearchType { get; set; }
        [Parameter]
        public RenderFragment<TItem> ChildContent { get; set; }
        [Parameter]
        public string Class { get; set; }

        public RenderFragment GetRenderFragment(TItem item)
        {
            if (ChildContent != null)
            {
                return ChildContent(item);
            }
            return null;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                DataTable.AddColumn(this);
        }

        public object GetValue(TItem item)
        {
            Func<TItem, object> func = Field.Compile();

            object value = func.Invoke(item);
            if (value == null)
                return string.Empty;

            return value;
        }

        public bool ValidateSearch(TItem item, string searchString)
        {
            string value = GetValue(item).ToString();
            if (SearchType == ESearchType.Contains)
            {
                return value.Contains(searchString, StringComparison.OrdinalIgnoreCase);
            } else
            {
                return value.Equals(searchString, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
