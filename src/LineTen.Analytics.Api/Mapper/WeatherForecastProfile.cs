using AutoMapper;
using LineTen.Analytics.Api.Models;
using LineTen.Analytics.Domain.Entities;

namespace LineTen.Analytics.Api.Mapper
{
    /// <summary>
    /// The AutoMapper profile between the domain entity and the web api model
    /// </summary>
    public class WeatherForecastProfile :Profile
    {
        /// <inheritdoc />
        public WeatherForecastProfile()
        {
            CreateMap<WeatherForecastModel, WeatherForecast>()
                .ForMember(x=> x.CreatedDate, map=> map.Ignore())
                .ForMember(x=> x.UpdatedDate, map=> map.Ignore());

            CreateMap<WeatherForecast, WeatherForecastModel>();
        }
    }
}
