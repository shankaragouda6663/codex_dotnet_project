using Microsoft.EntityFrameworkCore;
using RxFlow.Domain.Labs;
using RxFlow.Domain.Orders;

namespace RxFlow.Infrastructure.Persistence;

public sealed class RxFlowDbContext : DbContext
{
    public RxFlowDbContext(DbContextOptions<RxFlowDbContext> options) : base(options)
    {
    }

    public DbSet<LensOrder> Orders => Set<LensOrder>();
    public DbSet<RxLab> Labs => Set<RxLab>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LensOrder>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PatientId).HasColumnName("patient_id").HasMaxLength(64);
            builder.Property(x => x.FrameCode).HasColumnName("frame_code").HasMaxLength(64);
            builder.Property(x => x.LensMaterial).HasColumnName("lens_material").HasMaxLength(64);
            builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            builder.Property(x => x.RoutedLabCode).HasColumnName("routed_lab_code").HasMaxLength(32);
            builder.Property(x => x.QuotedPrice).HasColumnName("quoted_price").HasPrecision(12, 2);
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            builder.OwnsOne(x => x.Prescription, owned =>
            {
                owned.Property(p => p.Sphere).HasColumnName("sphere").HasPrecision(5, 2);
                owned.Property(p => p.Cylinder).HasColumnName("cylinder").HasPrecision(5, 2);
                owned.Property(p => p.Axis).HasColumnName("axis");
            });
            builder.HasIndex(x => x.PatientId).HasDatabaseName("ix_orders_patient_id");
        });

        modelBuilder.Entity<RxLab>(builder =>
        {
            builder.ToTable("labs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(32);
            builder.Property(x => x.Capability).HasColumnName("capability").HasMaxLength(64);
            builder.Property(x => x.StaticPriority).HasColumnName("static_priority");
            builder.Property(x => x.CurrentQueueDepth).HasColumnName("queue_depth");
        });
    }
}