using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RxFlow.Infrastructure.Persistence.Migrations;

public partial class M0002_AddLabSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData("labs", new[] { "Id", "code", "capability", "static_priority", "queue_depth" }, new object[] { 1, "LAB-A", "polycarbonate", 1, 2 });
        migrationBuilder.InsertData("labs", new[] { "Id", "code", "capability", "static_priority", "queue_depth" }, new object[] { 2, "LAB-B", "polycarbonate", 2, 0 });
        migrationBuilder.InsertData("labs", new[] { "Id", "code", "capability", "static_priority", "queue_depth" }, new object[] { 3, "LAB-C", "high-index", 1, 1 });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("delete from labs where \"Id\" in (1,2,3);");
    }
}