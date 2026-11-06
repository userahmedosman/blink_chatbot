using System;
using System.Collections.Generic;
using System.Text;

namespace Blink_ChatBot.Services
{
    internal class EvaluateWeatherConditions
    {
        public string Evaluate(string weatherCondition)
        {
            if (string.IsNullOrWhiteSpace(weatherCondition))
            {
                return "Weather information is not available.";
            }
            var condition = weatherCondition.ToLower();
            if (condition.Contains("rain") || condition.Contains("storm"))
            {
                return "It's rainy or stormy. Don't forget your umbrella!";
            }
            else if (condition.Contains("snow"))
            {
                return "It's snowy. Dress warmly!";
            }
            else if (condition.Contains("sunny") || condition.Contains("clear"))
            {
                return "It's sunny and clear. Enjoy the nice weather!";
            } else if (condition.Contains("cloudy") || condition.Contains("overcast"))
            {
                return "It's cloudy. You might want to carry a light jacket.";
            } else if(condition.Contains("hot") || condition.Contains("warm"))
            {
                return "It's hot or warm. Stay hydrated!";
            }
            else if (condition.Contains("cold") || condition.Contains("chilly"))
            {
                return "It's cold or chilly. Dress warmly!";
            }
            else
            {
                return $"The current weather condition is: {weatherCondition}.";
            }
        }
    }
}
