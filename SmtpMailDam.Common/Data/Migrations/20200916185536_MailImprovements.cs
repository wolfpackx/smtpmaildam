using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmtpMailDam.Common.Data.Migrations
{
    public partial class MailImprovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "RawEmail",
                table: "Mail");

            migrationBuilder.AddColumn<string>(
                name: "RawEmail",
                table: "Mail",
                type: "varbinary(max)",
                nullable: true);

            /*migrationBuilder.AlterColumn<byte[]>(
                name: "RawEmail",
                table: "Mail",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);*/

            migrationBuilder.AddColumn<string>(
                name: "HtmlBody",
                table: "Mail",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextBody",
                table: "Mail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlBody",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "TextBody",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "RawEmail",
                table: "Mail");

            migrationBuilder.AddColumn<string>(
                name: "RawEmail",
                table: "Mail",
                type: "nvarchar(max)",
                nullable: true);

            /*migrationBuilder.AlterColumn<string>(
                name: "RawEmail",
                table: "Mail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);*/

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "Mail",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
