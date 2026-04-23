using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoingMy.User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestsAndSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "UserProfiles",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IsVerified",
                table: "UserProfiles",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Location",
                table: "UserProfiles",
                column: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_IsVerified",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Location",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "UserProfiles");
        }
    }
}
