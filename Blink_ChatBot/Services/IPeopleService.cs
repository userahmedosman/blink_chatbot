using System.ComponentModel;

namespace Blink_ChatBot.Services
{
    [Description("Service interface for managing people-related operations.")]
    public interface IPeopleService
    {
        [Description("Asynchronously retrieves all people from the data source.")]
        Task<IEnumerable<Entities.Person>> GetAllPeopleAsync();
    }
}
