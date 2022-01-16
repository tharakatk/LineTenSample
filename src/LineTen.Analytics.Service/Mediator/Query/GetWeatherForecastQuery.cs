using System;
using LineTen.Analytics.Domain.Entities;
using MediatR;

namespace LineTen.Analytics.Service.Mediator.Query
{
    /// <summary>
    /// Gets a specific forecast
    /// </summary>
    public class GetWeatherForecastQuery :IRequest<WeatherForecast>
    {
        /// <summary>
        /// The Id of the forecast
        /// </summary>
        public Guid Id { get; set; }
    }
}
