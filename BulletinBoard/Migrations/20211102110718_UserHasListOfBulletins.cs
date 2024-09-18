using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BulletinBoard.Migrations
{
    public partial class UserHasListOfBulletins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Bulletins_BulletinID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BulletinID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BulletinID",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "AuthorID",
                table: "Bulletins",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Bulletins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Bulletins_AuthorID",
                table: "Bulletins",
                column: "AuthorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bulletins_Users_AuthorID",
                table: "Bulletins",
                column: "AuthorID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bulletins_Users_AuthorID",
                table: "Bulletins");

            migrationBuilder.DropIndex(
                name: "IX_Bulletins_AuthorID",
                table: "Bulletins");

            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "Bulletins");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "Bulletins");

            migrationBuilder.AddColumn<int>(
                name: "BulletinID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BulletinID",
                table: "Users",
                column: "BulletinID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Bulletins_BulletinID",
                table: "Users",
                column: "BulletinID",
                principalTable: "Bulletins",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
