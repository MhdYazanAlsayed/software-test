using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObserveTool.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "monitoring");

            migrationBuilder.CreateTable(
                name: "Logs",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LevelText = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Method = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Request = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    Response = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    IpAddress = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Level",
                schema: "monitoring",
                table: "Logs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Timestamp",
                schema: "monitoring",
                table: "Logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_TraceId",
                schema: "monitoring",
                table: "Logs",
                column: "TraceId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_Method",
                schema: "monitoring",
                table: "RequestLogs",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_Timestamp",
                schema: "monitoring",
                table: "RequestLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "RequestLogs",
                schema: "monitoring");
        }
    }
}
