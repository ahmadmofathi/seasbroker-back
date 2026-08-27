using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence;

public class SeasbrokerDbContext : IdentityDbContext<User, Role, Guid>
{
    public SeasbrokerDbContext(DbContextOptions<SeasbrokerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<RequestedQuote> RequestedQuotes => Set<RequestedQuote>();

    public DbSet<Chat> Chats => Set<Chat>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<ChatToken> ChatTokens => Set<ChatToken>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Vessel> Vessels => Set<Vessel>();

    public DbSet<VesselAvailability> VesselAvailabilities => Set<VesselAvailability>();

    public DbSet<CargoListing> CargoListings => Set<CargoListing>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<MatchingRule> MatchingRules => Set<MatchingRule>();

    public DbSet<VesselReservation> VesselReservations => Set<VesselReservation>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<Faq> Faqs => Set<Faq>();

    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();

    public DbSet<FormVersion> FormVersions => Set<FormVersion>();

    public DbSet<FormSection> FormSections => Set<FormSection>();

    public DbSet<FormField> FormFields => Set<FormField>();

    public DbSet<FormFieldOption> FormFieldOptions => Set<FormFieldOption>();

    public DbSet<FormFieldCondition> FormFieldConditions => Set<FormFieldCondition>();

    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();

    public DbSet<FormSubmissionValue> FormSubmissionValues => Set<FormSubmissionValue>();

    public DbSet<FormSubmissionFile> FormSubmissionFiles => Set<FormSubmissionFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SeasbrokerDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid();
                    }

                    entry.Entity.Created = utcNow;
                    entry.Entity.Updated = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.Updated = utcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<User>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid();
                    }

                    entry.Entity.Created = utcNow;
                    entry.Entity.Updated = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.Updated = utcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Role>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid();
                    }

                    entry.Entity.Created = utcNow;
                    entry.Entity.Updated = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.Updated = utcNow;
                    break;
            }
        }
    }
}
