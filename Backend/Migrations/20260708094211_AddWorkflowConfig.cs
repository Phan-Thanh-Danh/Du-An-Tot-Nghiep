using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuyTrinhDonTu",
                columns: table => new
                {
                    MaQuyTrinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiDon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenQuyTrinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SlaKhoangThoiGian = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyTrinhDonTu", x => x.MaQuyTrinh);
                });

            migrationBuilder.CreateTable(
                name: "BuocQuyTrinh",
                columns: table => new
                {
                    MaBuoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaQuyTrinh = table.Column<int>(type: "int", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TenBuoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTroXuLy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KieuBuoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SlaKhoangThoiGian = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuocQuyTrinh", x => x.MaBuoc);
                    table.ForeignKey(
                        name: "FK_BuocQuyTrinh_QuyTrinhDonTu_MaQuyTrinh",
                        column: x => x.MaQuyTrinh,
                        principalTable: "QuyTrinhDonTu",
                        principalColumn: "MaQuyTrinh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuocQuyTrinh_MaQuyTrinh",
                table: "BuocQuyTrinh",
                column: "MaQuyTrinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuocQuyTrinh");

            migrationBuilder.DropTable(
                name: "QuyTrinhDonTu");
        }
    }
}
