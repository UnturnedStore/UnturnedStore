using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace Website.Components.DataTable
{
    public partial class DataTable<TItem>
    {
        [Parameter]
        public IEnumerable<TItem> Data { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }        

        protected override void OnInitialized()
        {
            searchTimer = new(420)
            {
                AutoReset = false
            };
            searchTimer.Elapsed += OnSearchTimeElapsed;
        }

        protected override void OnParametersSet()
        {
            if (Data != null)
                RefreshItems();
        }

        private List<DataTableColumn<TItem>> Columns { get; set; } = new List<DataTableColumn<TItem>>();

        public void AddColumn(DataTableColumn<TItem> column)
        {
            Columns.Add(column);
            if (CurrentOrder.Column == null)
                CurrentOrder.Column = column;
        }

        public DataTableTitle<TItem> Title { get; private set; }
        public void UpdateTitle(DataTableTitle<TItem> title)
        {
            Title = title;
            StateHasChanged();
            Console.WriteLine("updated title hurrah!");
        }
    }
}
