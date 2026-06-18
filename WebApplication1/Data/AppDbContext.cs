using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using WebApplication1.entites;
using WebApplication1.entites.Audits;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(
    ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClientCompanyBranchAccess>()
            .HasOne(x => x.Company)
            .WithMany(x =>
                x.ClientCompanyBranchAccesses)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ClientCompanyBranchAccess>()
            .HasOne(x => x.Branch)
            .WithMany(x =>
                x.ClientCompanyBranchAccesses)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ClientCompanyBranchAccess>()
            .HasOne(x => x.Client)
            .WithMany(x =>
                x.ClientCompanyBranchAccesses)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Client>()
    .ToTable("Clients");

        modelBuilder.Entity<ClientCompanyBranchAccess>()
            .ToTable("ClientCompanyBranchAccesses");

        //indexes 
        modelBuilder.Entity<AdminUser>()
            .HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Client>().HasIndex(x => x.Email).IsUnique();

        modelBuilder.Entity<Company>()
            .HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<States>()
            .HasIndex(x => x.StateCode).IsUnique();
        modelBuilder.Entity<County>()
            .HasIndex(x => x.CountyCode).IsUnique();
        modelBuilder.Entity<CompanyStateCountyAccess>()
            .HasIndex(x => new
            {
                x.CompanyId,
                x.CountyId
            });

        
    }

    // Tables
    public DbSet<AdminUser> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<States> States { get; set; }

    public DbSet<County> Counties { get; set; }
    public DbSet<CompanyStateCountyAccess>CompanyStateCountyAccesses{ get; set; }
    public DbSet<Branch>Branches { get; set; }
    public DbSet<BranchStateCountyAccess>BranchStateCountyAccesses { get; set; }

    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientCompanyBranchAccess> ClientCompanyBranchAccesses { get; set; }

    public DbSet<AuditLogs> AuditLogs { get; set; }
    // Later:
    // public DbSet<Branch> Branches { get; set; }
    // public DbSet<Client> Clients { get; set; }
}