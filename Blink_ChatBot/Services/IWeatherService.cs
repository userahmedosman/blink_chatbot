using System;
using System.Collections.Generic;
using System.Text;

namespace Blink_ChatBot.Services
{
    internal interface IWeatherService
    {
        Task<string> GetWeatherAsync(string city);
    }
}
