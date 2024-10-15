using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services
{
    public class SqlLiteDatabaseCreator
    {
        private readonly SqlDataAccess sqlDataAccess;

        public SqlLiteDatabaseCreator(SqlDataAccess sqlDataAccess)
        {
            this.sqlDataAccess = sqlDataAccess;
        }

        public async Task CheckDatabaseIntegrity()
        {
            await Task.Delay(2000);

        }
    }
}
