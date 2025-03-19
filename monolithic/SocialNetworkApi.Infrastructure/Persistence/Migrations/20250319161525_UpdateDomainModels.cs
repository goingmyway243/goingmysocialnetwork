using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetworkApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ChatRooms_ChatroomEntityId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ChatroomEntityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ChatroomEntityId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "ChatroomParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChatroomId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatroomParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatroomParticipants_ChatRooms_ChatroomId",
                        column: x => x.ChatroomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatroomParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChatroomParticipants_ChatroomId",
                table: "ChatroomParticipants",
                column: "ChatroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatroomParticipants_UserId",
                table: "ChatroomParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatroomParticipants");

            migrationBuilder.AddColumn<Guid>(
                name: "ChatroomEntityId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ChatroomEntityId",
                table: "Users",
                column: "ChatroomEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ChatRooms_ChatroomEntityId",
                table: "Users",
                column: "ChatroomEntityId",
                principalTable: "ChatRooms",
                principalColumn: "Id");
        }
    }
}
