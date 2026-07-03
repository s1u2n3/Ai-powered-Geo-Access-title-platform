using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using WebApplication1.entites.AwsBackgroundJobs;
using WebApplication1.entites.StarterEntities;
using WebApplication1.GisEntities;

public class PostgresDbContext
    : DbContext
{
    public PostgresDbContext(
        DbContextOptions<PostgresDbContext> options)
        : base(options)
    {
    }
    public DbSet<Parcels> Parcels { get; set; }

    public DbSet<DocumentChunk>
        DocumentChunks
    { get; set; }
    public DbSet<StarterPackage>StarterPackage { get; set; }
    public DbSet<StarterIssue> StarterIssue { get; set; }
    public DbSet<StarterDocument> StarterDocument { get; set; }
    //documentJobs//

    public DbSet<DocumentProcessingJob> DocumentProcessingJob { get; set; }
    public DbSet<DocumentProcessingItem> DocumentProcessingItem { get; set; }
  

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.Entity<DocumentChunk>()
            .Property(x => x.Embedding)
            .HasColumnType("vector(3072)");
        modelBuilder.Entity<DocumentProcessingJob>()
    .HasMany(x => x.Items)
    .WithOne(x => x.Job)
    .HasForeignKey(
        x => x.DocumentProcessingJobId);


        modelBuilder.Entity<DocumentProcessingJob>()
    .HasIndex(x => x.ParcelId);

        modelBuilder.Entity<DocumentProcessingJob>()
            .HasIndex(x => x.Status);

        modelBuilder.Entity<DocumentProcessingItem>()
            .HasIndex(x => x.DocumentProcessingJobId);
    }

}