using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpress_backend_V2.Migrations
{
    /// <inheritdoc />
    public partial class JsonbDocumentAndIsBillable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. First add the new columns
            migrationBuilder.AddColumn<List<string>>(
                name: "AccomodationDocumentPath",
                table: "TravelRequests",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "InsuranceDocumentPath",
                table: "TravelRequests",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                table: "TravelRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // 2. Convert existing TicketDocumentPath in place
            migrationBuilder.Sql(@"
                ALTER TABLE ""TravelRequests"" 
                ALTER COLUMN ""TicketDocumentPath"" TYPE text 
                USING ""TicketDocumentPath""::text;

                UPDATE ""TravelRequests""
                SET ""TicketDocumentPath"" = 
                    CASE 
                        WHEN ""TicketDocumentPath"" IS NULL THEN NULL
                        WHEN ""TicketDocumentPath"" = '' THEN '[]'
                        WHEN ""TicketDocumentPath"" NOT LIKE '[%' THEN 
                            '[""' || replace(""TicketDocumentPath"", '""', '\""') || '""]'
                        ELSE ""TicketDocumentPath""
                    END;

                ALTER TABLE ""TravelRequests""
                ALTER COLUMN ""TicketDocumentPath"" TYPE jsonb
                USING ""TicketDocumentPath""::jsonb;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert jsonb back to text
            migrationBuilder.Sql(@"
                ALTER TABLE ""TravelRequests""
                ALTER COLUMN ""TicketDocumentPath"" TYPE text
                USING ""TicketDocumentPath""::text;
            ");

            migrationBuilder.DropColumn(
                name: "AccomodationDocumentPath",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "InsuranceDocumentPath",
                table: "TravelRequests");

            migrationBuilder.DropColumn(
                name: "IsBillable",
                table: "TravelRequests");
        }
    }
}
