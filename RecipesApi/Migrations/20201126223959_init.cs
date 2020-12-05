using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RecipesApi.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Category_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100) CHARACTER SET utf8mb4", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(300) CHARACTER SET utf8mb4", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Category_Id);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    Recipe_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TitleShort = table.Column<string>(type: "varchar(50) CHARACTER SET utf8mb4", maxLength: 50, nullable: false),
                    TitleLong = table.Column<string>(type: "varchar(150) CHARACTER SET utf8mb4", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "varchar(2000) CHARACTER SET utf8mb4", maxLength: 2000, nullable: false),
                    OriginalLink = table.Column<string>(type: "varchar(500) CHARACTER SET utf8mb4", maxLength: 500, nullable: true),
                    LastModifier = table.Column<string>(type: "varchar(500) CHARACTER SET utf8mb4", maxLength: 500, nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.Recipe_Id);
                });

            migrationBuilder.CreateTable(
                name: "timeinterval_labels",
                columns: table => new
                {
                    IntervalLabel_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    label = table.Column<string>(type: "varchar(100) CHARACTER SET utf8mb4", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeinterval_labels", x => x.IntervalLabel_Id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    Unit_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Symbol = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.Unit_Id);
                });

            migrationBuilder.CreateTable(
                name: "recipe_categories",
                columns: table => new
                {
                    RecipeCat_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Recipe_Id = table.Column<int>(type: "int", nullable: false),
                    Category_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_categories", x => x.RecipeCat_Id);
                    table.ForeignKey(
                        name: "FK_recipe_categories_categories_Category_Id",
                        column: x => x.Category_Id,
                        principalTable: "categories",
                        principalColumn: "Category_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recipe_categories_recipes_Recipe_Id",
                        column: x => x.Recipe_Id,
                        principalTable: "recipes",
                        principalColumn: "Recipe_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_media",
                columns: table => new
                {
                    RecipeMedia_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MediaPath = table.Column<string>(type: "varchar(200) CHARACTER SET utf8mb4", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "varchar(200) CHARACTER SET utf8mb4", maxLength: 200, nullable: false),
                    Tag = table.Column<string>(type: "varchar(50) CHARACTER SET utf8mb4", maxLength: 50, nullable: true),
                    Recipe_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_media", x => x.RecipeMedia_Id);
                    table.ForeignKey(
                        name: "FK_recipe_media_recipes_Recipe_Id",
                        column: x => x.Recipe_Id,
                        principalTable: "recipes",
                        principalColumn: "Recipe_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_timeintervals",
                columns: table => new
                {
                    TimeInterval_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IntervalLabel_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_timeintervals", x => x.TimeInterval_Id);
                    table.ForeignKey(
                        name: "FK_recipe_timeintervals_timeinterval_labels_IntervalLabel_Id",
                        column: x => x.IntervalLabel_Id,
                        principalTable: "timeinterval_labels",
                        principalColumn: "IntervalLabel_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                columns: table => new
                {
                    RecipeIng_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Quantity = table.Column<double>(type: "double", nullable: false),
                    Recipe_Id = table.Column<int>(type: "int", nullable: false),
                    Unit_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_ingredients", x => x.RecipeIng_Id);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_recipes_Recipe_Id",
                        column: x => x.Recipe_Id,
                        principalTable: "recipes",
                        principalColumn: "Recipe_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_units_Unit_Id",
                        column: x => x.Unit_Id,
                        principalTable: "units",
                        principalColumn: "Unit_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_instructions",
                columns: table => new
                {
                    RecipeInst_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StepNum = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500) CHARACTER SET utf8mb4", maxLength: 500, nullable: false),
                    Recipe_Id = table.Column<int>(type: "int", nullable: false),
                    RecipeMedia_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_instructions", x => x.RecipeInst_Id);
                    table.ForeignKey(
                        name: "FK_recipe_instructions_recipe_media_RecipeMedia_Id",
                        column: x => x.RecipeMedia_Id,
                        principalTable: "recipe_media",
                        principalColumn: "RecipeMedia_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipe_instructions_recipes_Recipe_Id",
                        column: x => x.Recipe_Id,
                        principalTable: "recipes",
                        principalColumn: "Recipe_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_timeintervalspans",
                columns: table => new
                {
                    IntervalSpan_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TimeValue = table.Column<int>(type: "int", nullable: false),
                    TimeUnit = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    TimeInterval_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_timeintervalspans", x => x.IntervalSpan_Id);
                    table.ForeignKey(
                        name: "FK_recipe_timeintervalspans_recipe_timeintervals_TimeInterval_Id",
                        column: x => x.TimeInterval_Id,
                        principalTable: "recipe_timeintervals",
                        principalColumn: "TimeInterval_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recipe_categories_Category_Id",
                table: "recipe_categories",
                column: "Category_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_categories_Recipe_Id",
                table: "recipe_categories",
                column: "Recipe_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_Recipe_Id",
                table: "recipe_ingredients",
                column: "Recipe_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_Unit_Id",
                table: "recipe_ingredients",
                column: "Unit_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_instructions_Recipe_Id",
                table: "recipe_instructions",
                column: "Recipe_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_instructions_RecipeMedia_Id",
                table: "recipe_instructions",
                column: "RecipeMedia_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_media_Recipe_Id",
                table: "recipe_media",
                column: "Recipe_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_timeintervals_IntervalLabel_Id",
                table: "recipe_timeintervals",
                column: "IntervalLabel_Id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_timeintervalspans_TimeInterval_Id",
                table: "recipe_timeintervalspans",
                column: "TimeInterval_Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipe_categories");

            migrationBuilder.DropTable(
                name: "recipe_ingredients");

            migrationBuilder.DropTable(
                name: "recipe_instructions");

            migrationBuilder.DropTable(
                name: "recipe_timeintervalspans");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "recipe_media");

            migrationBuilder.DropTable(
                name: "recipe_timeintervals");

            migrationBuilder.DropTable(
                name: "recipes");

            migrationBuilder.DropTable(
                name: "timeinterval_labels");
        }
    }
}
