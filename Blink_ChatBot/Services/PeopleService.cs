using Blink_ChatBot.Data;
using Microsoft.EntityFrameworkCore;

namespace Blink_ChatBot.Services
{
    public class PeopleService: IPeopleService
    {
        private readonly ApplicationDbContext _context;
        public PeopleService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Entities.Person>> GetAllPeopleAsync()
        {
            return await _context.People.ToListAsync();
        }
    
    }
}
