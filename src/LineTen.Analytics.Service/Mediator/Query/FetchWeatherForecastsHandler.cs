using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LineTen.DataAccess.EntityFramework.Repository;
using LineTen.Analytics.Domain.Entities;
using MediatR;

namespace LineTen.Analytics.Service.Mediator.Query
{
    /// <summary>
    /// Handles fetching of a weather forecast
    /// </summary>
    public class FetchWeatherForecastsHandler : IRequestHandler<FetchWeatherForecastsQuery, IQueryable<WeatherForecast>>
    {
        private readonly IRepository<WeatherForecast> _repository;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="repository"></param>
        public FetchWeatherForecastsHandler(IRepository<WeatherForecast> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public Task<IQueryable<WeatherForecast>> Handle(FetchWeatherForecastsQuery request, CancellationToken cancellationToken)
        {
            var result = _repository.Query();
            return Task.FromResult(result);
        }
    }
}
