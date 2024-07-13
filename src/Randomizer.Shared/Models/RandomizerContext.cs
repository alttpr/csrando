using Microsoft.EntityFrameworkCore;
using Randomizer.Shared.Models;

namespace Randomizer.Shared.Models {

    public class RandomizerContext : DbContext {

        public RandomizerContext(DbContextOptions<RandomizerContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Seed>()
                .HasOne(s => s.BasePatch);
        }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionEvent> SessionEvents { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Seed> Seeds { get; set; }
        public DbSet<World> Worlds { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<BasePatch> BasePatches { get; set; }

    }

}
