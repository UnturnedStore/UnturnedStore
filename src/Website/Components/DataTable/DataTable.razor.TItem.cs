using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Website.Components.DataTable
{
    public partial class DataTable<TItem>
    {
        public Type ItemType => typeof(TItem);
        public PropertyInfo[] GetProperties() => ItemType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

    }
}
