using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Robot.ED.FacebookConnector.Service.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenToRobot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "robot",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "robot");
        }
    }
}
