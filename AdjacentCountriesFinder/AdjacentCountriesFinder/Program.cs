using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Countires
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "C:\\Users\\garima.gupta\\source\\repos\\AdjacentCountriesFinder\\AdjacentCountriesFinder\\Data\\countries.json";
            if(!File.Exists(filePath)){
                Console.WriteLine("Countries data file not found.");
                return;
            }
            string jsonData = File.ReadAllText(filePath);
            Dictionary<string, Country> content = JsonSerializer.Deserialize<Dictionary<string, Country>>(jsonData);

            if (content == null || content.Count == 0){
                Console.WriteLine("No country data available");
                return;
            }

            string countryCode = GetValidCountryCode(content);

            Country country = content[countryCode];
            Console.WriteLine($"Ajacent contries of {country.name} are: ");
            for (int index = 0; index < country.adjacentCountries.Count; index++)
            {
                Console.Write(country.adjacentCountries[index]);
                if (index != country.adjacentCountries.Count - 1)
                {
                    Console.Write(",");
                }
            }

        }
        static string GetValidCountryCode(Dictionary<string, Country> countries){
            while (true)
            {
                Console.Write("Enter Country Code (e.g. IN / US / NZ): ");
                string input = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(input)){
                    Console.WriteLine("Country code cannot be empty. Try again\n");
                    continue;
                }
                if (!countries.ContainsKey(input)){
                    Console.WriteLine("Invalid country code. Try again\n");
                    continue;
                }
                return input;
            }
        }
    }
}

class Country
{
    public string name { get; set; }
    public List<string> adjacentCountries { get; set; }
}
