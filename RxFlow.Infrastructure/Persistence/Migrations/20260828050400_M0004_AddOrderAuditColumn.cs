using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RxFlow.Infrastructure.Persistence.Migrations;

public partial class M0004_AddOrderAuditColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "audit_note", table: "orders", type: "text", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "audit_note", table: "orders");
    }
}