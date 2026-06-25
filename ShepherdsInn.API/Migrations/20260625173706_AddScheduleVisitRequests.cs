using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShepherdsInn.API.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleVisitRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleVisitRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    InquiryFor = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    TourReadiness = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Timeline = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Submitted = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleVisitRequest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleVisitRequest_IsRead",
                table: "ScheduleVisitRequest",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleVisitRequest_Submitted",
                table: "ScheduleVisitRequest",
                column: "Submitted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleVisitRequest");
        }
    }
}
