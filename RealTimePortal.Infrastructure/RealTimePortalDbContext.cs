using Microsoft.EntityFrameworkCore;
using RealTimePortal.Domain;

namespace RealTimePortal.Infrastructure;

public class RealTimePortalDbContext : DbContext
{
    public RealTimePortalDbContext(
        DbContextOptions<RealTimePortalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Process> Processes => Set<Process>();

    public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();

    public DbSet<Conversation> Conversations { get; set; } = null!;

    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;

    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Process>(entity =>
        {
            entity.ToTable("Processes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ClientId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ExecutionId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ApplicationId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ProcessType)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ReferenceNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ErrorMessage)
                .HasMaxLength(4000);

            entity.Property(x => x.RequestData)
                .HasColumnType("nvarchar(max)");

            entity.HasIndex(x => new
            {
                x.ClientId,
                x.ApplicationId,
                x.ProcessType
            });

            entity.HasIndex(x => new
            {
                x.Status,
                x.CreatedAt
            });

            entity.HasIndex(x => x.ExecutionId).IsUnique();
            entity.HasMany(x => x.Steps)
                .WithOne(x => x.Process)
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);


        });

        modelBuilder.Entity<ProcessStep>(entity =>
        {
            entity.ToTable("ProcessSteps");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.StepName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ErrorMessage)
                .HasMaxLength(4000);

            entity.Property(x => x.InputData)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.OutputData)
                .HasColumnType("nvarchar(max)");

            entity.HasIndex(x => new
            {
                x.ProcessId,
                x.StepNumber
            });
        });


        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ConversationType)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasMany(x => x.Participants)
                .WithOne(x => x.Conversation)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Messages)
                .WithOne()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.ToTable("ConversationParticipants");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x =>
                new
                {
                    x.ConversationId,
                    x.UserId
                })
                .IsUnique();
        });


        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Message)
                .HasMaxLength(4000)
                .IsRequired();

            entity.HasIndex(x =>
                new
                {
                    x.ConversationId,
                    x.SentAt
                });
        });
    }
}