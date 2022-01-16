using System;
using System.Reflection;
using LineTen.DataAccess.EntityFramework.Database;
using LineTen.DataAccess.EntityFramework.Interfaces;
using LineTen.Analytics.Data.Database;
using LineTen.Analytics.Data.Options;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LineTen.Analytics.Data.Extensions
{

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method that establishes the ExampleDbContext with the specified configuration and uses MySql as the provider
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="loggerFactory">Represents a type used to configure the logging system</param>
        /// <param name="dbOptions">Represents strongly typed environment configuration</param>
        /// <param name="changeTrackerType">We can specify a customized change tracker</param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationDbContext(this IServiceCollection services,
            ILoggerFactory loggerFactory,
            DbOptions dbOptions, Type changeTrackerType = null)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = dbOptions.DbServer,
                Password = dbOptions.DbPassword,
                UserID = dbOptions.DbUsername,
                InitialCatalog = dbOptions.DbDatabase
            };
            services.AddDbContext<ApplicationDbContext>(x =>
            {
                x.UseLazyLoadingProxies();
                x.UseSqlServer(connectionStringBuilder.ToString(), y =>
                {
                    y.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                });
                x.EnableSensitiveDataLogging(); // Ensures we get the full SQL output - disable in production
                x.UseLoggerFactory(loggerFactory);
            });

            while (true) //To ensure database server is available before continuing
            {
                try
                {
                    using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
                    using var command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "select 1";
                    if (command.ExecuteScalar() != DBNull.Value)
                        break;
                }
                catch (SqlException e)
                {
                    if (e.Number == 4060) //Unknown database - this is ok, we'll scaffold it.
                    {
                        break;
                    }
                }
            }

            services.AddScoped(typeof(ISaveChangesShim), changeTrackerType ?? typeof(DateSaveChangesShim));
            return services;
        }
    }
}

