using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutiCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceScreeningResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIResults");

            migrationBuilder.DropTable(
                name: "ParentAnswers");

            migrationBuilder.DropTable(
                name: "AIQuestions");

            migrationBuilder.DropTable(
                name: "ParentTest");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.AddColumn<string>(
                name: "AnswersJson",
                table: "PredictionResults",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswersJson",
                table: "PredictionResults");

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.TestId);
                });

            migrationBuilder.CreateTable(
                name: "AIQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TestId = table.Column<int>(type: "integer", nullable: false),
                    QuestionOrder = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_AIQuestions_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentTest",
                columns: table => new
                {
                    ParentTestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChildId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false),
                    TestId = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentTest", x => x.ParentTestId);
                    table.ForeignKey(
                        name: "FK_ParentTest_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentTest_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "ParentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentTest_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIResults",
                columns: table => new
                {
                    AIResultId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentTestId = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Recommendation = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    StatusLevel = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResults", x => x.AIResultId);
                    table.ForeignKey(
                        name: "FK_AIResults_ParentTest_ParentTestId",
                        column: x => x.ParentTestId,
                        principalTable: "ParentTest",
                        principalColumn: "ParentTestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentAnswers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentTestId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    AnswerDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: true),
                    AnswerValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_ParentAnswers_AIQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "AIQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentAnswers_ParentTest_ParentTestId",
                        column: x => x.ParentTestId,
                        principalTable: "ParentTest",
                        principalColumn: "ParentTestId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIQuestions_TestId",
                table: "AIQuestions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResults_ParentTestId",
                table: "AIResults",
                column: "ParentTestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentAnswers_ParentTestId_QuestionId",
                table: "ParentAnswers",
                columns: new[] { "ParentTestId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentAnswers_QuestionId",
                table: "ParentAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentTest_ChildId",
                table: "ParentTest",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentTest_ParentId",
                table: "ParentTest",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentTest_TestId",
                table: "ParentTest",
                column: "TestId");
        }
    }
}
