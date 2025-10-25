using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCrud.Rest.Example.Migrations
{
    /// <inheritdoc />
    public partial class addorderdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastOrderedDate",
                table: "Customers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastOrderedDate",
                table: "Customers");
        }
    }
}
