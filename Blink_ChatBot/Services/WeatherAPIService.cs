using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Blink_ChatBot.Services
{
    internal class WeatherAPIService(HttpClient httpClient, IConfiguration configuration): IWeatherService
    {
        public async Task<string> GetWeatherAsync(string city)
        {
            var apiKey = configuration["WEATHER_API_KEY"];
            var cityURL = Uri.EscapeDataString(city);

            var url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={cityURL}&aqi=no";

            var weatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(url);
            
            return weatherResponse?.Current?.Condition?.Text ?? "Weather information not available.";
        }

        public class WeatherResponse
        {
            [JsonPropertyName("current")]
            public Current Current { get; set; } = default!;
        }

        public class Current {
            [JsonPropertyName("condition")]
            public Condition Condition { get; set; } = default!;
        }

        public class Condition
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = default!;
        }
    }
}
