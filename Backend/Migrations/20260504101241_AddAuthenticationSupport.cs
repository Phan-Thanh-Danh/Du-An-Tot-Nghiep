using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung",
                sql: "[vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh', N'sieu_quan_tri', N'quan_tri_co_so', N'quan_tri_co_so_con')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NguoiDung_vai_tro_chinh_1",
                schema: "dbo",
                table: "NguoiDung",
                sql: "[vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh')");
        }
    }
}
