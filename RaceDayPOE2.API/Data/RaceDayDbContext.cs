using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(
            DbContextOptions<RaceDayDbContext> options)
            : base(options) 
        { 
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Entries> Entries { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventOrganizer> EventOrganizers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<ParticipantResults> ParticipantResults { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //category
            modelBuilder.Entity<Category>()
                .ToTable("Category")
                .HasKey(c => c.categoryID);
            modelBuilder.Entity<Category>()
                .Property(c => c.categoryID)
                .UseIdentityColumn();

            //entries
            modelBuilder.Entity<Entries>()
                .ToTable("Entries")
                .HasKey(e => e.entryID);
            modelBuilder.Entity<Entries>()
                .Property(e => e.entryID)
                .UseIdentityColumn();

            //event
            modelBuilder.Entity<Event>()
                .ToTable("Event")
                .HasKey(e => e.eventID);
            modelBuilder.Entity<Event>()
                .Property(e => e.eventID)
                .UseIdentityColumn();

            //EventOrganizer
            modelBuilder.Entity<EventOrganizer>()
                .ToTable("EventOrganizer")
                .HasKey(o => o.organizerID);
            modelBuilder.Entity<EventOrganizer>()
                .Property(o => o.organizerID)
                .UseIdentityColumn();

            //Participant
            modelBuilder.Entity<Participant>()
                .ToTable("Participant")
                .HasKey(p => p.participantID);
            modelBuilder.Entity<Participant>()
                .Property(p => p.participantID)
                .UseIdentityColumn();

            modelBuilder.Entity<ParticipantResults>()
                .ToTable("ParticipantResults")
                .HasKey(r => r.resultID);

            //UserRole
            modelBuilder.Entity<UserRole>()
                .ToTable("UserRole")
                .HasKey(r => r.roleID);
            modelBuilder.Entity<UserRole>()
                .Property(r => r.roleID)
                .UseIdentityColumn();
        }
    }
}
