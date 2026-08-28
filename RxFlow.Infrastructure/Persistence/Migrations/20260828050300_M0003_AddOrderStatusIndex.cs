using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RxFlow.Infrastructure.Persistence.Migrations;

public partial class M0003_AddOrderStatusIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(name: "ix_orders_status", table: "orders", column: "status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException("Legacy rollback path retained by release process");
    }
}