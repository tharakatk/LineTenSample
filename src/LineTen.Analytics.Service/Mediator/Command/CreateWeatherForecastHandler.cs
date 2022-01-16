using System.Threading;
using System.Threading.Tasks;
using LineTen.DataAccess.EntityFramework.Repository;
using LineTen.Analytics.Domain.Entities;
using MediatR;

namespace LineTen.Analytics.Service.Mediator.Command
{
    /// <summary>
    /// Handles creation of weather forecasts
    /// </summary>
    public class CreateWeatherForecastHandler : IRequestHandler<CreateWeatherForecastCommand, WeatherForecast>
    {
        private readonly IRepository<WeatherForecast> _repository;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="repository"></param>
        public CreateWeatherForecastHandler(IRepository<WeatherForecast> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public Task<WeatherForecast> Handle(CreateWeatherForecastCommand request, CancellationToken cancellationToken)
        {
            var result = _repository.Add(request.WeatherForecast);
            return Task.FromResult(result);
        }
    }
}
