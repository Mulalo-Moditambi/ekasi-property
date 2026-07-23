using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Subscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrialStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrialEndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GracePeriodStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GracePeriodEndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Day1ReminderSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Day4ReminderSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Day6ReminderSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPaymentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayFastToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PayFastMerchantPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PfPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AmountGross = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "dbo",
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_PfPaymentId",
                schema: "dbo",
                table: "SubscriptionPayments",
                column: "PfPaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_SubscriptionId",
                schema: "dbo",
                table: "SubscriptionPayments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OwnerId",
                schema: "dbo",
                table: "Subscriptions",
                column: "OwnerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPayments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Subscriptions",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "dbo",
                table: "Users");
        }
    }
}
