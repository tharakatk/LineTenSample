using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LineTen.Core;
using LineTen.Analytics.Api.Models;
using LineTen.Analytics.Domain.Entities;
using LineTen.Analytics.Service.Mediator.Command;
using LineTen.Analytics.Service.Mediator.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LineTen.Analytics.Api.Controllers
{
    /// <summary>
    /// Controls the weather!
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<WeatherForecastController> _logger;

        /// <inheritdoc />
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all weather forecasts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecastModel>>> GetAsync()
        {
            
            _logger.LogTrace("Handling GetAsync - UserId: {0} ",User.GetSubjectUserIdAsGuid());
            var weatherForecasts  = await _mediator.Send(new FetchWeatherForecastsQuery());
            var results = weatherForecasts.ProjectTo<WeatherForecastModel>(_mapper.ConfigurationProvider).ToList();
            return Ok(results);
        }

        /// <summary>
        /// Returns a single forecast
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<WeatherForecastModel>>> GetOneAsync(Guid id)
        {
            _logger.LogTrace("Handling GetOne - UserId: {0} ",User.GetSubjectUserIdAsGuid());
            var weatherForecasts  = await _mediator.Send(new GetWeatherForecastQuery{Id = id});
            if (weatherForecasts == null)
                return NotFound();
            return Ok(_mapper.Map<WeatherForecastModel>(weatherForecasts));
        }


        /// <summary>
        /// Creates a new weather forecast
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "lineten.analytics.write")]
        public async Task<ActionResult<WeatherForecastModel>> CreateAsync([FromBody] WeatherForecastModel model)
        {
            _logger.LogTrace("Handling CreateAsync - UserId: {0} ",User.GetSubjectUserIdAsGuid());
            var weatherForecast = await _mediator.Send(new CreateWeatherForecastCommand
            {
                WeatherForecast = _mapper.Map<WeatherForecast>(model)
            });
            return CreatedAtAction(nameof(GetOneAsync),new{id = weatherForecast.Id}, weatherForecast);
        }

        /// <summary>
        /// Deletes a weather forecast
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "lineten.analytics.write")]
        public Task<ActionResult<WeatherForecastModel>> DeleteAsync(Guid id)
        {
            _logger.LogTrace("Handling DeleteAsync - UserId: {0} ",User.GetSubjectUserIdAsGuid());
            throw new NotImplementedException();
        }
    }
}
