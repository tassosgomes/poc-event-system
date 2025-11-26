using Microsoft.EntityFrameworkCore;
using VideoEnricher.Domain;

namespace VideoEnricher.Data
{
    public class VideoEnricherDbContext : DbContext
    {
        public VideoEnricherDbContext(DbContextOptions<VideoEnricherDbContext> options) : base(options)
        {
        }

        public DbSet<VideoMetadata> VideoMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<VideoMetadata>()
                .HasIndex(v => v.SongId);
        }
    }
}
