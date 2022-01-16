using System.Threading;
using System.Threading.Tasks;
using LineTen.DataAccess.EntityFramework.Repository;
using LineTen.Analytics.Domain.Entities;
using MediatR;

namespace LineTen.Analytics.Service.Mediator.Query
{
    /// <summary>
    /// Handles getting weather forecasts
    /// </summary>
    public class GetWeatherForecastsHandler : IRequestHandler<GetWeatherForecastQuery, WeatherForecast>
    {
        private readonly IRepository<WeatherForecast> _repository;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="repository"></param>
        public GetWeatherForecastsHandler(IRepository<WeatherForecast> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public Task<WeatherForecast> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
        {
            var result = _repository.Find(request.Id);
            return Task.FromResult(result);
        }
    }
}
