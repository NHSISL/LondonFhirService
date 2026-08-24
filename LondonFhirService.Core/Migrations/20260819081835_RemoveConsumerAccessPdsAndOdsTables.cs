using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Types;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConsumerAccessPdsAndOdsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumerAccesses");

            migrationBuilder.DropTable(
                name: "OdsDatas");

            migrationBuilder.DropTable(
                name: "PdsDatas");

            migrationBuilder.DropTable(
                name: "Consumers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consumers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActiveTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OdsDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasChildren = table.Column<bool>(type: "bit", nullable: false),
                    OdsHierarchy = table.Column<SqlHierarchyId>(type: "hierarchyid", nullable: true),
                    OrganisationCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OrganisationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RelationshipWithParentEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RelationshipWithParentStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdsDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PdsDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhsNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrgCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OrganisationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipWithOrganisationEffectiveFromDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RelationshipWithOrganisationEffectiveToDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdsDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumerAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OrgCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumerAccesses_Consumers_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Consumers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerAccesses_ConsumerId",
                table: "ConsumerAccesses",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumers_Name",
                table: "Consumers",
                column: "Name",
                unique: true);
        }
    }
}
