using System.ComponentModel;

namespace Blink_ChatBot.Services
{
    internal class EmailGetService
    {
        [Description("This method generates an email address based on the provided name.")]
        public string GetEmail( 
           [Description("The name for which to generate an email address.")]
            string name) => $"{name}@gmail.com";
    }
}
