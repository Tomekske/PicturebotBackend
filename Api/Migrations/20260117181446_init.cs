using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hierarchies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    parent_id = table.Column<int>(type: "INTEGER", nullable: true),
                    type = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    uuid = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hierarchies_hierarchies_parent_id",
                        column: x => x.parent_id,
                        principalTable: "hierarchies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    theme_mode = table.Column<string>(type: "TEXT", nullable: false),
                    library_path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sub_folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    location = table.Column<string>(type: "TEXT", nullable: false),
                    hierarchy_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sub_folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sub_folders_hierarchies_hierarchy_id",
                        column: x => x.hierarchy_id,
                        principalTable: "hierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "picture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    Index = table.Column<string>(type: "TEXT", nullable: true),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    CurationStatus = table.Column<string>(type: "TEXT", nullable: false),
                    Sharpness = table.Column<int>(type: "INTEGER", nullable: false),
                    PHash = table.Column<long>(type: "INTEGER", nullable: false),
                    sub_folder_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_picture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_picture_sub_folders_sub_folder_id",
                        column: x => x.sub_folder_id,
                        principalTable: "sub_folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "settings",
                columns: new[] { "Id", "library_path", "theme_mode" },
                values: new object[] { 1, "", "system" });

            migrationBuilder.CreateIndex(
                name: "IX_hierarchies_parent_id",
                table: "hierarchies",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_picture_CurationStatus",
                table: "picture",
                column: "CurationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_picture_PHash",
                table: "picture",
                column: "PHash");

            migrationBuilder.CreateIndex(
                name: "IX_picture_sub_folder_id",
                table: "picture",
                column: "sub_folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_picture_Type",
                table: "picture",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_sub_folders_hierarchy_id",
                table: "sub_folders",
                column: "hierarchy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "picture");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "sub_folders");

            migrationBuilder.DropTable(
                name: "hierarchies");
        }
    }
}
