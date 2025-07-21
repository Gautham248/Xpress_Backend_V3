using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpress_backend_V2.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeEmail",
                table: "Users",
                column: "EmployeeEmail",
                unique: true);

            migrationBuilder.Sql(@"
            SELECT setval(
                pg_get_serial_sequence('""Users""', 'UserId'), 
                COALESCE((SELECT MAX(""UserId"") FROM ""Users""), 0) + 1, 
                false
            );
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmployeeEmail",
                table: "Users");
        }
    }
}
