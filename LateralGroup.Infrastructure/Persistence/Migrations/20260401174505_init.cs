using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LateralGroup.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LatestKnownVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    LatestPublishedVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    LatestPayloadJson = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDisabledByCms = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDisabledByAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastEventTimestampUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastEventType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedCmsEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentItemId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: true),
                    TimestampUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RawEventJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FailureReason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedCmsEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentItemId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    WasPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    WasUnpublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    ObservedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentVersions_ContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_IsDisabledByAdmin",
                table: "ContentItems",
                column: "IsDisabledByAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_IsDisabledByCms",
                table: "ContentItems",
                column: "IsDisabledByCms");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_IsPublished",
                table: "ContentItems",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_IsPublished_IsDisabledByCms_IsDisabledByAdmin",
                table: "ContentItems",
                columns: new[] { "IsPublished", "IsDisabledByCms", "IsDisabledByAdmin" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_LastEventTimestampUtc",
                table: "ContentItems",
                column: "LastEventTimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContentVersions_ContentItemId",
                table: "ContentVersions",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentVersions_ContentItemId_Version",
                table: "ContentVersions",
                columns: new[] { "ContentItemId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentVersions_ObservedAtUtc",
                table: "ContentVersions",
                column: "ObservedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedCmsEvents_ContentItemId",
                table: "ProcessedCmsEvents",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedCmsEvents_ContentItemId_TimestampUtc",
                table: "ProcessedCmsEvents",
                columns: new[] { "ContentItemId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedCmsEvents_Status",
                table: "ProcessedCmsEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedCmsEvents_TimestampUtc",
                table: "ProcessedCmsEvents",
                column: "TimestampUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentVersions");

            migrationBuilder.DropTable(
                name: "ProcessedCmsEvents");

            migrationBuilder.DropTable(
                name: "ContentItems");
        }
    }
}
