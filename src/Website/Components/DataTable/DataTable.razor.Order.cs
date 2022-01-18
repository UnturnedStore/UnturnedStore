using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Website.Components.DataTable
{
    public partial class DataTable<TItem>
    {
        public OrderByModel CurrentOrder { get; set; } = new OrderByModel();

        public class OrderByModel
        {
            public DataTableColumn<TItem> Column { get; set; }
            public bool Asc { get; set; }
        }

        public void OrderBy(DataTableColumn<TItem> column)
        {
            searchTimer.Stop();
            isLoading = true;

            if (CurrentOrder.Column == column)
            {
                CurrentOrder.Asc = !CurrentOrder.Asc;
            } else
            {
                CurrentOrder.Column = column;
                CurrentOrder.Asc = true;
            }
            
            StateHasChanged();
            searchTimer.Start();      
        }

        public void ApplyOrder(ref IEnumerable<TItem> data)
        {
            if (CurrentOrder.Column == null)
            {
                return;
            }
            
            if (CurrentOrder.Asc)
            {
                data = data.OrderBy(x => CurrentOrder.Column.GetValue(x));
                return;
            }

            data = data.OrderByDescending(x => CurrentOrder.Column.GetValue(x));
        }
    }
}
