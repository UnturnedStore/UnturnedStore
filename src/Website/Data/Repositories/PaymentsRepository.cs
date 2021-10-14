using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Data.Repositories
{
    public class PaymentsRepository
    {
        private readonly SqlConnection connection;

        public PaymentsRepository(SqlConnection connection)
        {
            this.connection = connection;
        }
    }
}
