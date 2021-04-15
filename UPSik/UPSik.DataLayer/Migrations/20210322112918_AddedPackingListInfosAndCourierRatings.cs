using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UPSik.DataLayer.Migrations
{
    public partial class AddedPackingListInfosAndCourierRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CourierRating",
                table: "Users",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "CourierRatingForDelivery",
                table: "Packages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EtaToReceiver",
                table: "Packages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EtaToSender",
                table: "Packages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "ReceiverDistanceFromPreviousPoint",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SenderDistanceFromPreviousPoint",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "PackingListsInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourierId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackagesCount = table.Column<int>(type: "int", nullable: false),
                    ManuallyManaged = table.Column<bool>(type: "bit", nullable: false),
                    IsManaged = table.Column<bool>(type: "bit", nullable: false),
                    CourierRating = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingListsInfo", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackingListsInfo");

            migrationBuilder.DropColumn(
                name: "CourierRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CourierRatingForDelivery",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "EtaToReceiver",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "EtaToSender",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ReceiverDistanceFromPreviousPoint",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "SenderDistanceFromPreviousPoint",
                table: "Packages");
        }
    }
}
