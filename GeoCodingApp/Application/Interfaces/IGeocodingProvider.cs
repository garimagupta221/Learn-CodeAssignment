using GeoCodingApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoCodingApp.Application.Interfaces
{
    public interface IGeocodingProvider
    {
        Task<List<Location>> GeocodeAsync(string location);
    }
}
