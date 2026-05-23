using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutiCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageTypeToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TreatmentId",
                table: "Sessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpecialistId",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ParentId",
                table: "Sessions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SpecialistId",
                table: "Sessions",
                column: "SpecialistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Parents_ParentId",
                table: "Sessions",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "ParentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Specialist_SpecialistId",
                table: "Sessions",
                column: "SpecialistId",
                principalTable: "Specialist",
                principalColumn: "SpecialistId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Parents_ParentId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Specialist_SpecialistId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_ParentId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SpecialistId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SpecialistId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "TreatmentId",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
