using Microsoft.EntityFrameworkCore;

using ShepherdsInn.API.Configuration;
using ShepherdsInn.API.Models;

namespace ShepherdsInn.API.Data
{
    public class ShepherdsInnDbContext : DbContext
    {
        public ShepherdsInnDbContext(DbContextOptions<ShepherdsInnDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.ToTable("ContactMessages");

                entity.Property(x => x.Name).HasMaxLength(ContactFormLimits.MaxNameLength).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(ContactFormLimits.MaxPhoneLength);
                entity.Property(x => x.Email).HasMaxLength(ContactFormLimits.MaxEmailLength);
                entity.Property(x => x.PreferredContact).HasMaxLength(ContactFormLimits.MaxPreferredContactLength);
                entity.Property(x => x.Subject).HasMaxLength(ContactFormLimits.MaxSubjectLength);
                entity.Property(x => x.Message).HasMaxLength(ContactFormLimits.MaxMessageLength).IsRequired();
                entity.Property(x => x.UserAgent).HasMaxLength(ContactFormLimits.MaxUserAgentLength);
                entity.Property(x => x.IpAddress).HasMaxLength(ContactFormLimits.MaxIpAddressLength);

                entity.HasIndex(x => x.Submitted);
                entity.HasIndex(x => x.IsRead);
            });
        }
    }
}
