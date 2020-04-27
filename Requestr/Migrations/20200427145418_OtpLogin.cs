using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Requestr.Migrations
{
    public partial class OtpLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "OtpPaymentRequest",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "OtpLogin",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpLogin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpLogin_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtpLogin_UserId",
                table: "OtpLogin",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpLogin");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OtpPaymentRequest",
                newName: "id");
        }
    }
}
