using ChatWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<AuthToken> AuthToken { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.AdminUser)
                .WithMany()
                .HasForeignKey(c => c.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.ParticipantUser)
                .WithMany()
                .HasForeignKey(c => c.ParticipantUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
    
