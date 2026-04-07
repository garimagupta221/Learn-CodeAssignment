using GeoCodingApp.Domain;
using GeoCodingApp.Application.Interfaces;
using GeoCodingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoCodingApp.Infrastructure.External
{
    public class GeoCodingProvider : IGeocodingProvider
    {

        public GeoCodingProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GeoCodingApp/1.0");
        }

        public async Task<List<Location>> GeocodeAsync(string location)
        {
            var encodedLocation = Uri.EscapeDataString(location);
            var url = $"https://nominatim.openstreetmap.org/search?q={encodedLocation}&format=json";
            var json = await _httpClient.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<List<GeoResponse>>(json);

            if (data == null || data.Count == 0)
            {
                return new List<Location>();
            }

            return data.Select(response => new Location
            {
                Name = response.DisplayName,
                Latitude = double.Parse(response.Latitude),
                Longitude = double.Parse(response.Longitude)
            }).ToList();
        }

        private readonly HttpClient _httpClient;
    }
}
