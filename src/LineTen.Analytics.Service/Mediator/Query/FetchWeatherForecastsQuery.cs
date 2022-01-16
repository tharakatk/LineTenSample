using System.Linq;
using LineTen.Analytics.Domain.Entities;
using MediatR;

namespace LineTen.Analytics.Service.Mediator.Query
{
    /// <summary>
    /// Represents a query
    /// </summary>
    public class FetchWeatherForecastsQuery :IRequest<IQueryable<WeatherForecast>>
    {
    }
}
