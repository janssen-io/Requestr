using Microsoft.EntityFrameworkCore;
using System;

namespace Requestr.Data
{
    public class RequestrContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<OneTimePasswordForLogin> OneTimePasswordForLogin { get; set; }

        public RequestrContext(DbContextOptions<RequestrContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().ToTable("Users");
            builder.Entity<OneTimePasswordForLogin>().ToTable("OtpLogin");
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }

    public class OneTimePasswordForLogin
    {
        public Guid Id { get; set; }
        public string Password { get; set; }
        public User User { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
