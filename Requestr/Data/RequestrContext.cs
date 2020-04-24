using Microsoft.EntityFrameworkCore;
using System;

namespace Requestr.Data
{
    public class RequestrContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }

        public RequestrContext(DbContextOptions<RequestrContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().ToTable("Users");
            builder.Entity<PaymentRequest>().ToTable("PaymentRequests");
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }

    public class PaymentRequest
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string ToEmail { get; set; }
        public string? ToPhone { get; set; }
        public string Currency { get; set; } = "EUR";
        public Guid AttachmentId { get; set; }
        public User User { get; set; }
        public Uri Link { get; set; }
    }
}
