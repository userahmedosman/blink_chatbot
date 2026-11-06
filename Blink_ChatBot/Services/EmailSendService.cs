using System.ComponentModel;

namespace Blink_ChatBot.Services
{
    internal class EmailSendService
    {
        [Description("Sends an email to the specified recipient.")]
        public Task SendEmail(
            [Description("The email address of the recipient.")]
            string to,
            [Description("The subject of the email.")]
            string subject,
            [Description("The body of the email.")]
            string body)
        {
            // Simulate sending an email
            

            Console.WriteLine($"Sending email to: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");

            Console.WriteLine("Email sent successfully!");

            return Task.CompletedTask;
        }
    }
}
