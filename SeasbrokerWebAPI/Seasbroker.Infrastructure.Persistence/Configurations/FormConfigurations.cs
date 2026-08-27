using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.ToTable("form_definitions");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.Key).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Description).HasMaxLength(1000);

        builder.HasIndex(f => f.Key).IsUnique().HasDatabaseName("IX_form_definitions_Key");

        builder.HasMany(f => f.Versions)
            .WithOne(v => v.FormDefinition)
            .HasForeignKey(v => v.FormDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormVersionConfiguration : IEntityTypeConfiguration<FormVersion>
{
    public void Configure(EntityTypeBuilder<FormVersion> builder)
    {
        builder.ToTable("form_versions");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(v => new { v.FormDefinitionId, v.VersionNumber })
            .IsUnique()
            .HasDatabaseName("IX_form_versions_FormDefinitionId_VersionNumber");

        builder.HasIndex(v => new { v.FormDefinitionId, v.Status })
            .HasDatabaseName("IX_form_versions_FormDefinitionId_Status");

        builder.HasMany(v => v.Sections)
            .WithOne(s => s.FormVersion)
            .HasForeignKey(s => s.FormVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormSectionConfiguration : IEntityTypeConfiguration<FormSection>
{
    public void Configure(EntityTypeBuilder<FormSection> builder)
    {
        builder.ToTable("form_sections");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Key).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Label).IsRequired().HasMaxLength(200);

        builder.HasIndex(s => new { s.FormVersionId, s.Key })
            .IsUnique()
            .HasDatabaseName("IX_form_sections_FormVersionId_Key");

        builder.HasMany(s => s.Fields)
            .WithOne(f => f.FormSection)
            .HasForeignKey(f => f.FormSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("form_fields");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.Key).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Label).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Type).IsRequired().HasMaxLength(30);
        builder.Property(f => f.Placeholder).HasMaxLength(300);
        builder.Property(f => f.HelpText).HasMaxLength(500);
        builder.Property(f => f.Width).IsRequired().HasMaxLength(10);
        builder.Property(f => f.SystemFieldKey).HasMaxLength(100);
        builder.Property(f => f.DefaultValue).HasMaxLength(1000);
        builder.Property(f => f.ValidationJson).HasMaxLength(2000);
        builder.Property(f => f.ConditionCombinator).HasMaxLength(5);

        builder.HasIndex(f => new { f.FormVersionId, f.Key })
            .IsUnique()
            .HasDatabaseName("IX_form_fields_FormVersionId_Key");

        builder.HasOne(f => f.FormVersion)
            .WithMany()
            .HasForeignKey(f => f.FormVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(f => f.Options)
            .WithOne(o => o.FormField)
            .HasForeignKey(o => o.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Conditions)
            .WithOne(c => c.FormField)
            .HasForeignKey(c => c.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormFieldOptionConfiguration : IEntityTypeConfiguration<FormFieldOption>
{
    public void Configure(EntityTypeBuilder<FormFieldOption> builder)
    {
        builder.ToTable("form_field_options");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Value).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Label).IsRequired().HasMaxLength(200);

        builder.HasIndex(o => o.FormFieldId).HasDatabaseName("IX_form_field_options_FormFieldId");
    }
}

public class FormFieldConditionConfiguration : IEntityTypeConfiguration<FormFieldCondition>
{
    public void Configure(EntityTypeBuilder<FormFieldCondition> builder)
    {
        builder.ToTable("form_field_conditions");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.SourceFieldKey).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Operator).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Value).HasMaxLength(2000);

        builder.HasIndex(c => c.FormFieldId).HasDatabaseName("IX_form_field_conditions_FormFieldId");
    }
}

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("form_submissions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.HasIndex(s => s.FormVersionId).HasDatabaseName("IX_form_submissions_FormVersionId");
        builder.HasIndex(s => s.CustomerId).HasDatabaseName("IX_form_submissions_CustomerId");

        builder.HasOne(s => s.FormVersion)
            .WithMany()
            .HasForeignKey(s => s.FormVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.RequestedQuote)
            .WithMany()
            .HasForeignKey(s => s.RequestedQuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Values)
            .WithOne(v => v.FormSubmission)
            .HasForeignKey(v => v.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Files)
            .WithOne(f => f.FormSubmission)
            .HasForeignKey(f => f.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormSubmissionValueConfiguration : IEntityTypeConfiguration<FormSubmissionValue>
{
    public void Configure(EntityTypeBuilder<FormSubmissionValue> builder)
    {
        builder.ToTable("form_submission_values");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.FieldKey).IsRequired().HasMaxLength(100);
        builder.Property(v => v.ValueText).HasColumnType("nvarchar(max)");

        builder.HasIndex(v => new { v.FormSubmissionId, v.FieldKey })
            .IsUnique()
            .HasDatabaseName("IX_form_submission_values_FormSubmissionId_FieldKey");
    }
}

public class FormSubmissionFileConfiguration : IEntityTypeConfiguration<FormSubmissionFile>
{
    public void Configure(EntityTypeBuilder<FormSubmissionFile> builder)
    {
        builder.ToTable("form_submission_files");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.FieldKey).IsRequired().HasMaxLength(100);
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(300);
        builder.Property(f => f.ContentType).IsRequired().HasMaxLength(150);
        builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(500);

        builder.HasIndex(f => f.FormSubmissionId).HasDatabaseName("IX_form_submission_files_FormSubmissionId");
    }
}
