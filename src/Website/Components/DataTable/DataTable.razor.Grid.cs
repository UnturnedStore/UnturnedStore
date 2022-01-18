using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Website.Components.DataTable
{
    public partial class DataTable<TItem>
    {
        public IEnumerable<string> GetColumns()
        {
            PropertyInfo[] properties = GetProperties();
            return properties.Select(x => x.Name);            
        }

        public IEnumerable<TItem> Items { get; private set; }

        public void RefreshItems()
        {
            isLoading = true;
            Items = GetItems();
            isLoading = false;
        }

        public class ColumnValue
        {
            public DataTableColumn<TItem> Column { get; set; }
            public TItem Value { get; set; }
        }

        public IEnumerable<TItem> GetItems()
        {
            //List<IEnumerable<string>> groups = new();

            IEnumerable<TItem> data = Data;
            
            ApplySearch(ref data);
            ApplyOrder(ref data);
            return data;

            //foreach (TItem item in data)
            //{
            //    List<string> values = new();
            //    foreach (DataTableColumn<TItem> column in Columns)
            //    {
            //        values.Add(column.GetValue(item).ToString());                    
            //    }
            //    groups.Add(values);
            //}

            //return groups;
        }
    }
}
