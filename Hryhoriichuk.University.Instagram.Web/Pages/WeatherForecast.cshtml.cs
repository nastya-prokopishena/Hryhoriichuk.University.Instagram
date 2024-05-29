using Microsoft.AspNetCore.Mvc.RazorPages;
using Hryhoriichuk.University.Instagram.Core.Interfaces;
using Hryhoriichuk.University.Instagram.Models.Weather;

namespace Hryhoriichuk.University.Instagram.Web.Pages
{
    public class WeatherForecastModel : PageModel
    {
        public IList<WeatherForecast> Forecasts { get; set; }

        private readonly IWeatherForecastService _weatherForecastService;

        public WeatherForecastModel(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        public void OnGet()
        {
            Forecasts = _weatherForecastService.GetRandomForecast().ToList();
        }
    }
}