using GeoCodingApp.Domain;
using GeoCodingApp.Application.Interfaces;
using GeoCodingApp.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoCodingApp.Application.Services
{
    public class GeocodingService
    {
        public GeocodingService(IGeocodingProvider provider)
        {
            _provider = provider;
        }

        public async Task<List<Location>> GetLocationDataAsync(string input)
        {
            if (!InputValidator.IsValid(input))
            {
                throw new ArgumentException("Invalid input");
            }

            var results = await _provider.GeocodeAsync(input);

            if (results == null || results.Count == 0)
            {
                throw new Exception("No results found");
            }

            return results;
        }

        private readonly IGeocodingProvider _provider;
    }
}
