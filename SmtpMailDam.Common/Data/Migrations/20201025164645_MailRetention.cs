using Microsoft.EntityFrameworkCore.Migrations;

namespace SmtpMailDam.Common.Data.Migrations
{
    public partial class MailRetention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MailRetention",
                table: "Mailbox",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailRetention",
                table: "Mailbox");
        }
    }
}
