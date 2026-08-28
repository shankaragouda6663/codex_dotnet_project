using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RxFlow.Infrastructure.Persistence.Migrations;

public partial class M0001_InitialOrderAndLabTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "labs",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                code = table.Column<string>(maxLength: 32, nullable: false),
                capability = table.Column<string>(maxLength: 64, nullable: false),
                static_priority = table.Column<int>(nullable: false),
                queue_depth = table.Column<int>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_labs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                patient_id = table.Column<string>(maxLength: 64, nullable: false),
                sphere = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                cylinder = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                axis = table.Column<int>(nullable: false),
                frame_code = table.Column<string>(maxLength: 64, nullable: false),
                lens_material = table.Column<string>(maxLength: 64, nullable: false),
                routed_lab_code = table.Column<string>(maxLength: 32, nullable: true),
                quoted_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                status = table.Column<string>(maxLength: 32, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_orders", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "ix_orders_patient_id",
            table: "orders",
            column: "patient_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "orders");
        migrationBuilder.DropTable(name: "labs");
    }
}