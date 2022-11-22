using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class PostComment1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_Posts_PostOwnerPostId",
                table: "PostComment");

            migrationBuilder.RenameColumn(
                name: "PostOwnerPostId",
                table: "PostComment",
                newName: "PostOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_PostComment_PostOwnerPostId",
                table: "PostComment",
                newName: "IX_PostComment_PostOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_Posts_PostOwnerId",
                table: "PostComment",
                column: "PostOwnerId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_Posts_PostOwnerId",
                table: "PostComment");

            migrationBuilder.RenameColumn(
                name: "PostOwnerId",
                table: "PostComment",
                newName: "PostOwnerPostId");

            migrationBuilder.RenameIndex(
                name: "IX_PostComment_PostOwnerId",
                table: "PostComment",
                newName: "IX_PostComment_PostOwnerPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_Posts_PostOwnerPostId",
                table: "PostComment",
                column: "PostOwnerPostId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
