using Microsoft.EntityFrameworkCore.Migrations;

namespace SmtpMailDam.Common.Data.Migrations
{
    public partial class Imap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ImapEnabled",
                table: "Mailbox",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImapHost",
                table: "Mailbox",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImapPassword",
                table: "Mailbox",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImapPort",
                table: "Mailbox",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ImapSSLEnabled",
                table: "Mailbox",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImapUsername",
                table: "Mailbox",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImapEnabled",
                table: "Mailbox");

            migrationBuilder.DropColumn(
                name: "ImapHost",
                table: "Mailbox");

            migrationBuilder.DropColumn(
                name: "ImapPassword",
                table: "Mailbox");

            migrationBuilder.DropColumn(
                name: "ImapPort",
                table: "Mailbox");

            migrationBuilder.DropColumn(
                name: "ImapSSLEnabled",
                table: "Mailbox");

            migrationBuilder.DropColumn(
                name: "ImapUsername",
                table: "Mailbox");
        }
    }
}
