using System;
using System.ComponentModel.DataAnnotations;
using LineTen.DataAccess.EntityFramework.Interfaces;

namespace LineTen.Analytics.Domain.Entities
{
    /// <summary>
    /// Represents a weather forecast
    /// </summary>
    public class WeatherForecast :IDateTracking
    {
        /// <summary>
        /// The Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Date of forecast
        /// </summary>
        public DateTimeOffset Date { get; set; }
        /// <summary>
        /// Temperature in celsius
        /// </summary>
        public int TemperatureC { get; set; }
        /// <summary>
        /// Summary of the weather
        /// </summary>
        [MaxLength(100)]
        public string Summary { get; set; }

        /// <inheritdoc />
        public DateTimeOffset CreatedDate { get; set; }

        /// <inheritdoc />
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
