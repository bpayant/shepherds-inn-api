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

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<ScheduleVisitRequest> ScheduleVisitRequests => Set<ScheduleVisitRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region ContactMessage Entity Configuration

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

            #endregion

            #region ScheduleVisitRequest Entity Configuration

            modelBuilder.Entity<ScheduleVisitRequest>(entity =>
            {
                entity.ToTable("ScheduleVisitRequest");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(254);
                entity.Property(x => x.InquiryFor).HasMaxLength(30).IsRequired();
                entity.Property(x => x.TourReadiness).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Timeline).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Submitted).IsRequired();
                entity.Property(x => x.UserAgent).HasMaxLength(500);
                entity.Property(x => x.IpAddress).HasMaxLength(45);

                entity.HasIndex(x => x.Submitted);
                entity.HasIndex(x => x.IsRead);
            });

            #endregion
        }
    }
}
