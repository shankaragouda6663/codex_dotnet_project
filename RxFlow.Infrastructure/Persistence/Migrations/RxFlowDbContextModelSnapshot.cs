using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RxFlow.Infrastructure.Persistence;

#nullable disable

namespace RxFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RxFlowDbContext))]
partial class RxFlowDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("RxFlow.Domain.Labs.RxLab", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<string>("Capability").HasColumnName("capability");
            b.Property<string>("Code").HasColumnName("code");
            b.Property<int>("CurrentQueueDepth").HasColumnName("queue_depth");
            b.Property<int>("StaticPriority").HasColumnName("static_priority");
            b.HasKey("Id");
            b.ToTable("labs");
        });

        modelBuilder.Entity("RxFlow.Domain.Orders.LensOrder", b =>
        {
            b.Property<Guid>("Id");
            b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
            b.Property<string>("FrameCode").HasColumnName("frame_code");
            b.Property<string>("LensMaterial").HasColumnName("lens_material");
            b.Property<string>("PatientId").HasColumnName("patient_id");
            b.Property<decimal>("QuotedPrice").HasColumnName("quoted_price");
            b.Property<string>("RoutedLabCode").HasColumnName("routed_lab_code");
            b.Property<string>("Status").HasColumnName("status");
            b.HasKey("Id");
            b.HasIndex("PatientId").HasDatabaseName("ix_orders_patient_id");
            b.ToTable("orders");
        });

        modelBuilder.Entity("RxFlow.Domain.Orders.LensOrder", b =>
        {
            b.OwnsOne("RxFlow.Domain.Orders.Prescription", "Prescription", b1 =>
            {
                b1.Property<Guid>("LensOrderId");
                b1.Property<int>("Axis").HasColumnName("axis");
                b1.Property<decimal>("Cylinder").HasColumnName("cylinder");
                b1.Property<decimal>("Sphere").HasColumnName("sphere");
                b1.HasKey("LensOrderId");
                b1.ToTable("orders");
                b1.WithOwner().HasForeignKey("LensOrderId");
            });
            b.Navigation("Prescription").IsRequired();
        });
    }
}