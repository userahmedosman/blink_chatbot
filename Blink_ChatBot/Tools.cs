using Blink_ChatBot.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blink_ChatBot
{
    internal static class Tools
    {
        internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
        {
            var weatherService = sp.GetRequiredService<IWeatherService>();

            yield return AIFunctionFactory.Create(
                weatherService.GetWeatherAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "get_weather",
                    Description = "Gets the current weather conditions for a specified city.",
                    
                });

            var evaluationService = sp.GetRequiredService<EvaluateWeatherConditions>();

            yield return AIFunctionFactory.Create(
                evaluationService.Evaluate,
                new AIFunctionFactoryOptions
                {
                    Name = "evaluate_weather",
                    Description = "Evaluates the weather conditions and provides recommendations based on the current weather.",
                });

            var emailGetService = sp.GetRequiredService<EmailGetService>();
            yield return AIFunctionFactory.Create(emailGetService.GetEmail);

            var emailSendService = sp.GetRequiredService<EmailSendService>();

            var  sendEmailTool = AIFunctionFactory.Create(emailSendService.SendEmail);

            yield return new ApprovalRequiredAIFunction(sendEmailTool);

            var peopleService = sp.GetRequiredService<IPeopleService>();
            yield return AIFunctionFactory.Create(peopleService.GetAllPeopleAsync);

        }
    }
}
