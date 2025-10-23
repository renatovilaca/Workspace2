using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Robot.ED.FacebookConnector.Dashboard.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "robot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Available = table.Column<bool>(type: "boolean", nullable: false),
                    LastAllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_robot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "token",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    TokenValue = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    TokenValue = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "queue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AiConfig = table.Column<string>(type: "text", nullable: false),
                    TrackId = table.Column<string>(type: "text", nullable: false),
                    BridgeKey = table.Column<string>(type: "text", nullable: false),
                    OriginType = table.Column<string>(type: "text", nullable: false),
                    MediaId = table.Column<string>(type: "text", nullable: false),
                    Customer = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    Phrase = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllocatedRobotId = table.Column<int>(type: "integer", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    HasError = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_robot_AllocatedRobotId",
                        column: x => x.AllocatedRobotId,
                        principalTable: "robot",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "queue_data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_data_queue_QueueId",
                        column: x => x.QueueId,
                        principalTable: "queue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "queue_result",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    ProcessedByRobotId = table.Column<int>(type: "integer", nullable: true),
                    HasError = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    TrackId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    MediaId = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_result_queue_QueueId",
                        column: x => x.QueueId,
                        principalTable: "queue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_queue_result_robot_ProcessedByRobotId",
                        column: x => x.ProcessedByRobotId,
                        principalTable: "robot",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "queue_tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_tag_queue_QueueId",
                        column: x => x.QueueId,
                        principalTable: "queue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "queue_result_attachment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueResultId = table.Column<int>(type: "integer", nullable: false),
                    AttachmentId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_result_attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_result_attachment_queue_result_QueueResultId",
                        column: x => x.QueueResultId,
                        principalTable: "queue_result",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "queue_result_message",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueResultId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_result_message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_result_message_queue_result_QueueResultId",
                        column: x => x.QueueResultId,
                        principalTable: "queue_result",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_queue_AllocatedRobotId",
                table: "queue",
                column: "AllocatedRobotId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_CreatedAt",
                table: "queue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_queue_IsProcessed",
                table: "queue",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_queue_TrackId",
                table: "queue",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_UniqueId",
                table: "queue",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_queue_data_QueueId",
                table: "queue_data",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_result_ProcessedByRobotId",
                table: "queue_result",
                column: "ProcessedByRobotId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_result_QueueId",
                table: "queue_result",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_result_ReceivedAt",
                table: "queue_result",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_queue_result_TrackId",
                table: "queue_result",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_result_attachment_QueueResultId",
                table: "queue_result_attachment",
                column: "QueueResultId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_result_message_QueueResultId",
                table: "queue_result_message",
                column: "QueueResultId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_tag_QueueId",
                table: "queue_tag",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_robot_Available",
                table: "robot",
                column: "Available");

            migrationBuilder.CreateIndex(
                name: "IX_token_TokenValue",
                table: "token",
                column: "TokenValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_TokenValue",
                table: "user",
                column: "TokenValue",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "queue_data");

            migrationBuilder.DropTable(
                name: "queue_result_attachment");

            migrationBuilder.DropTable(
                name: "queue_result_message");

            migrationBuilder.DropTable(
                name: "queue_tag");

            migrationBuilder.DropTable(
                name: "token");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "queue_result");

            migrationBuilder.DropTable(
                name: "queue");

            migrationBuilder.DropTable(
                name: "robot");
        }
    }
}
