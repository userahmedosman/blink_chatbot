using Blink_ChatBot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blink_ChatBot.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>().HasData( 
                new Person
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Salary = 50000,
                isActive = true
            },
                new Person 
                {
                    Id = 2,
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Salary = 60000,
                    isActive = true
                },

                new Person
                {
                    Id = 3,
                    Name = "Bob Johnson",
                    Email = "bobjohnson@example.com",
                    Salary = 45000,
                    isActive = false


                }
            );
        }
        public DbSet<Entities.Person> People { get; set; } = default!;
    }
}
