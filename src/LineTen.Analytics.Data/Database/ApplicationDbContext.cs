using LineTen.DataAccess.EntityFramework.Database;
using LineTen.DataAccess.EntityFramework.Interfaces;
using LineTen.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LineTen.Analytics.Data.Database
{
    /// <summary>
    /// Represents the database
    /// </summary>
    public class ApplicationDbContext : ChangeTrackerDbContext<ApplicationDbContext>
    {
        /// <inheritdoc />
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ISaveChangesShim changeTracker) : base(options, changeTracker)
        {
        }
        /// <summary>
        /// The weather forecasts
        /// </summary>
        public virtual DbSet<WeatherForecast> WeatherForecasts { get; set; }
    }
}
