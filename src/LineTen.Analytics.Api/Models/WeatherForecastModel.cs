using System;
using System.ComponentModel.DataAnnotations;

namespace LineTen.Analytics.Api.Models
{
    /// <summary>
    /// Represents a weather forecast
    /// </summary>
    public class WeatherForecastModel
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public Guid Id { get; internal set; }
        /// <summary>
        /// The date of forecast
        /// </summary>
        public DateTimeOffset Date { get; set; }
        /// <summary>
        /// The temperature in celsius
        /// </summary>
        [Required]
        public int? TemperatureC { get; set; }
        /// <summary>
        /// The temperature in fahrenheit
        /// </summary>
        public int? TemperatureF
        {
            get => TemperatureC.HasValue ? 32 + (int)(TemperatureC / 0.5556) : default(int?);
        }
        /// <summary>
        /// The summary of the forecast
        /// </summary>
        [Required]
        public string Summary { get; set; }
    }
}
