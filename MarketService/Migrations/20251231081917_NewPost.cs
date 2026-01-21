using Microsoft.EntityFrameworkCore.Migrations;




#nullable disable

namespace MarketService.Migrations
{
    /// <inheritdoc />
    public partial class NewPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SharesCount",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PostId1",
                table: "PostLikes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostId1",
                table: "PostComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostImages_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostId1",
                table: "PostLikes",
                column: "PostId1");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostId1",
                table: "PostComments",
                column: "PostId1");

            migrationBuilder.CreateIndex(
                name: "IX_PostImages_PostId",
                table: "PostImages",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostId1",
                table: "PostComments",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Posts_PostId1",
                table: "PostLikes",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostId1",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Posts_PostId1",
                table: "PostLikes");

            migrationBuilder.DropTable(
                name: "PostImages");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_PostId1",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_PostId1",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "SharesCount",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "PostComments");
        }
    }
}
