using LineTen.DataAccess.EntityFramework.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LineTen.Analytics.Data.Database
{
    /// <summary>
    /// Used by the code first migration tools to connect to the database
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <inheritdoc />
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(new SqlConnectionStringBuilder
                {
                    DataSource = "127.0.0.1",
                    InitialCatalog = "LineTen.Analytics",
                    UserID = "sa",
                    Password = "P@ssw0rd!"
                }.ToString())
                .Options, new DateSaveChangesShim());
        }
    }
}
