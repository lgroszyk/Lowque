using System;
using Microsoft.EntityFrameworkCore;
using Lowque.DataAccess.Entities;
using Lowque.DataAccess.Entities.Identity;

namespace Lowque.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<ApplicationDefinition> ApplicationDefinitions { get; set; }

        public DbSet<FlowDefinition> FlowDefinitions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            SeedDatabase(builder);
            base.OnModelCreating(builder);
        }

        private void SeedDatabase(ModelBuilder builder)
        {
            builder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Email = "admin@lowque.pl",
                EmailConfirmed = true,
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>().HashPassword(null, "Admin123!"),
            });
            builder.Entity<Role>().HasData(new Role
            {
                RoleId = 1,
                Name = "Admin",
            });
            builder.Entity<ApplicationDefinition>().HasData(new ApplicationDefinition
            {
                ApplicationDefinitionId = 1,
                Name = "HrApp_Template",
                IsTemplate = true,
                CreatedAt = new DateTime(2021, 10, 1, 12, 0, 0),
                IdentityModuleDefinition = "{}",
                DataAccessModuleDefinition = "[{\"Name\":\"Process\",\"Properties\":[{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"ProcessStatus\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"CreationDate\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"CandidateFirstName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"CandidateLastName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"CandidateFullName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":101,\"List\":false},{\"Name\":\"CandidateEmail\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"CandidatePhoneNumber\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":20,\"List\":false},{\"Name\":\"CandidateAge\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"UserId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"PreferredEmploymentDimension\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"PreferredTypeOfContract\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"PreferredSalary\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"PreferredStartDate\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"OfferedEmploymentDimension\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"OfferedTypeOfContract\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"OfferedSalary\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"OfferedStartDate\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"User\",\"Type\":\"User\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false},{\"Name\":\"Documents\",\"Type\":\"Document\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Meetings\",\"Type\":\"Meeting\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Comments\",\"Type\":\"Comment\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]},{\"Name\":\"Meeting\",\"Properties\":[{\"Name\":\"MeetingId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"MeetingTime\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"IsOnline\",\"Type\":\"Binary\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"OfficeId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Link\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":500,\"List\":false},{\"Name\":\"DidTakePlace\",\"Type\":\"Binary\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Process\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false},{\"Name\":\"Office\",\"Type\":\"Office\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false}]},{\"Name\":\"Document\",\"Properties\":[{\"Name\":\"DocumentId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Name\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"Type\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":1000,\"List\":false},{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Process\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false}]},{\"Name\":\"Office\",\"Properties\":[{\"Name\":\"OfficeId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Name\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"Meetings\",\"Type\":\"Meeting\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]},{\"Name\":\"Comment\",\"Properties\":[{\"Name\":\"CommentId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Content\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"UserId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"CreationTime\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Process\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false},{\"Name\":\"User\",\"Type\":\"User\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false}]},{\"Name\":\"User\",\"Properties\":[{\"Name\":\"UserId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Email\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"EmailConfirmed\",\"Type\":\"Binary\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"PasswordHash\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"FirstName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"LastName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"FullName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":101,\"List\":false},{\"Name\":\"Roles\",\"Type\":\"Role\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Comments\",\"Type\":\"Comment\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Processes\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]},{\"Name\":\"Role\",\"Properties\":[{\"Name\":\"RoleId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Name\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"Users\",\"Type\":\"User\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]}]"
            });            
        }
    }
}
