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

            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Entries>().ToTable("Entries");
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<EventOrganizer>().ToTable("EventOrganizer");
            modelBuilder.Entity<Participant>().ToTable("Participant");
            modelBuilder.Entity<ParticipantResults>().ToTable("ParticipantResults");
            modelBuilder.Entity<UserRole>().ToTable("UserRole");
        }
    }
}
