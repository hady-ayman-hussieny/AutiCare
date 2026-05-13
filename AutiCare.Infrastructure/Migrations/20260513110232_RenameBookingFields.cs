using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutiCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingTime",
                table: "Bookings",
                newName: "PreferredTime");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "Bookings",
                newName: "PreferredDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredTime",
                table: "Bookings",
                newName: "BookingTime");

            migrationBuilder.RenameColumn(
                name: "PreferredDate",
                table: "Bookings",
                newName: "BookingDate");
        }
    }
}
