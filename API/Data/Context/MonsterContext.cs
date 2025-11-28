using API_Pokemon.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Pokemon.Data.Context
{
    public class MonsterContext : DbContext
    {
        public DbSet<Monster> Monster { get; set; }
        public DbSet<Tuile> Tuiles { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<User>Users { get; set; }
        public DbSet<InstanceMonstre> InstanceMonstres { get; set; }

        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }

        public bool isExistingEmail(string email) 
        {
            if (Users.Any(e => e.Email == email)) 
            {
                return true;
            }

            return false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clé primaire composite sur PositionX + PositionY
            modelBuilder.Entity<Tuile>()
                .HasKey(t => new { t.PositionX, t.PositionY });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<InstanceMonstre>()
                .HasKey(t => new { t.PositionX, t.PositionY });
        }
    }
}
