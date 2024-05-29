﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hryhoriichuk.University.Instagram.Core.Interfaces;
using Hryhoriichuk.University.Instagram.Core.Services;
using Hryhoriichuk.University.Instagram.Models.Configuration;

namespace Hryhoriichuk.University.Instagram.Core
{
    public static class DIConfiguration
    {
        public static void RegisterCoreDependencies(this IServiceCollection services)
        {
            services.AddTransient<IWeatherForecastService, WeatherForecastService>();
        }

        public static void RegisterCoreConfiguration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<AppConfig>(configuration.GetSection("AppConfig"));
        }
    }
}
