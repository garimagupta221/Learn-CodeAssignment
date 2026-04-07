using GeoCodingApp.Application.Interfaces;
using GeoCodingApp.Application.Services;
using GeoCodingApp.Infrastructure.External;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GeoCodingApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<HttpClient>();
            services.AddTransient<IGeocodingProvider, GeoCodingProvider>();
            services.AddTransient<GeocodingService>();
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<GeocodingService>();

            Console.Write("Enter location: ");
            var input = Console.ReadLine();

            try
            {
                var results = await service.GetLocationDataAsync(input);
                foreach (var location in results)
                {
                    Console.WriteLine($"Location: {location.Name}");
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
