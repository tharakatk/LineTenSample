using LineTen.Analytics.Domain.Entities;
using MediatR;

namespace LineTen.Analytics.Service.Mediator.Command
{
    /// <summary>
    /// Creates a weather forecast
    /// </summary>
    public class CreateWeatherForecastCommand :IRequest<WeatherForecast>{
        /// <summary>
        /// The weather forecast to create
        /// </summary>
        public WeatherForecast WeatherForecast { get; set; }
    }
}
