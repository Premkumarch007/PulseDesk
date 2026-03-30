using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PulseDesk.Application.Interfaces.Services
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public DbConnectionFactory(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        IDbConnection IDbConnectionFactory.CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
