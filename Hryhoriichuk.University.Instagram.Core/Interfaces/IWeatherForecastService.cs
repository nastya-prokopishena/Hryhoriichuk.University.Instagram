using Hryhoriichuk.University.Instagram.Models.Weather;

namespace Hryhoriichuk.University.Instagram.Core.Interfaces
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> GetRandomForecast();
    }
}
