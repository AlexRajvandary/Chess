using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessWPF.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalFenToGameRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalFen",
                table: "GameRecords",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalFen",
                table: "GameRecords");
        }
    }
}
