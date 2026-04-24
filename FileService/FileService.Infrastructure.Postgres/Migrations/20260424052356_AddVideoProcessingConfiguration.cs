using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoProcessingConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "key",
                table: "media_assets",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddColumn<string>(
                name: "UploadId",
                table: "media_assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "raw_key",
                table: "media_assets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "video_processing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    progress_percentage = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    IsCriticalError = table.Column<bool>(type: "boolean", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_processing", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processing_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_type = table.Column<string>(type: "text", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    result_data = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    video_processing_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_processing_steps_video_processing_video_processing_id",
                        column: x => x.video_processing_id,
                        principalTable: "video_processing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_processing_steps_status",
                table: "processing_steps",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_processing_steps_step_type",
                table: "processing_steps",
                column: "step_type");

            migrationBuilder.CreateIndex(
                name: "IX_processing_steps_video_processing_id",
                table: "processing_steps",
                column: "video_processing_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_processing_status",
                table: "video_processing",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_video_processing_status_started_at",
                table: "video_processing",
                columns: new[] { "status", "started_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processing_steps");

            migrationBuilder.DropTable(
                name: "video_processing");

            migrationBuilder.DropColumn(
                name: "UploadId",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "raw_key",
                table: "media_assets");

            migrationBuilder.AlterColumn<string>(
                name: "key",
                table: "media_assets",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
