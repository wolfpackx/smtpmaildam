using Microsoft.EntityFrameworkCore.Migrations;

namespace SmtpMailDam.Common.Data.Migrations
{
    public partial class MessagePassthrough : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Passthrough",
                table: "Mailbox",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Passthrough",
                table: "Mailbox");
        }
    }
}
