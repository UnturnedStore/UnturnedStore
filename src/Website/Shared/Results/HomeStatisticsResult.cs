using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Shared.Results
{
    public class HomeStatisticsResult
    {
        public int SalesCount { get; set; }
        public int UsersCount { get; set; }
        public int ProductsCount { get; set; }
    }
}
