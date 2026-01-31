using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace FoodAdviser.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "food_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                Unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_food_items", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "receipts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_receipts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "recipes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                DishType = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_recipes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "receipt_line_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                Unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ReceiptId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_receipt_line_items", x => x.Id);
                table.ForeignKey(
                    name: "FK_receipt_line_items_receipts_ReceiptId",
                    column: x => x.ReceiptId,
                    principalTable: "receipts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_ingredients",
            columns: table => new
            {
                RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                Unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_recipe_ingredients", x => new { x.RecipeId, x.Name });
                table.ForeignKey(
                    name: "FK_recipe_ingredients_recipes_RecipeId",
                    column: x => x.RecipeId,
                    principalTable: "recipes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_food_items_Name",
            table: "food_items",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_receipt_line_items_ReceiptId",
            table: "receipt_line_items",
            column: "ReceiptId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "food_items");
        migrationBuilder.DropTable(name: "receipt_line_items");
        migrationBuilder.DropTable(name: "recipe_ingredients");
        migrationBuilder.DropTable(name: "receipts");
        migrationBuilder.DropTable(name: "recipes");
    }
}
