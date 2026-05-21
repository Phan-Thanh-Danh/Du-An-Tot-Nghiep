using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameCourseSyllabusToDeCuongMonHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_ma_don_vi__DonVi",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseSyllabus",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseSyllabus_hoc_ky_du_kien_1",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseSyllabus_trang_thai_1",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.RenameTable(
                name: "CourseSyllabus",
                schema: "dbo",
                newName: "DeCuongMonHoc",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "UQ_CourseSyllabus_1",
                schema: "dbo",
                table: "DeCuongMonHoc",
                newName: "UQ_DeCuongMonHoc_1");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSyllabus_ma_don_vi",
                schema: "dbo",
                table: "DeCuongMonHoc",
                newName: "IX_DeCuongMonHoc_ma_don_vi");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSyllabus_ma_chuyen_nganh",
                schema: "dbo",
                table: "DeCuongMonHoc",
                newName: "IX_DeCuongMonHoc_ma_chuyen_nganh");

            migrationBuilder.RenameIndex(
                name: "IX_CourseSyllabus_ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "DeCuongMonHoc",
                newName: "IX_DeCuongMonHoc_ma_chuong_trinh_mon_hoc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeCuongMonHoc",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_syllabus");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeCuongMonHoc_hoc_ky_du_kien_1",
                schema: "dbo",
                table: "DeCuongMonHoc",
                sql: "[hoc_ky_du_kien] IS NULL OR [hoc_ky_du_kien] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeCuongMonHoc_trang_thai_1",
                schema: "dbo",
                table: "DeCuongMonHoc",
                sql: "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'archived')");

            migrationBuilder.AddForeignKey(
                name: "FK_DeCuongMonHoc_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_chuong_trinh_mon_hoc",
                principalSchema: "dbo",
                principalTable: "MonHocTrongChuongTrinh",
                principalColumn: "ma_chuong_trinh_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_DeCuongMonHoc_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_chuyen_nganh",
                principalSchema: "dbo",
                principalTable: "ChuyenNganh",
                principalColumn: "ma_chuyen_nganh");

            migrationBuilder.AddForeignKey(
                name: "FK_DeCuongMonHoc_ma_don_vi__DonVi",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_DeCuongMonHoc_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "DeCuongMonHoc",
                column: "ma_mon_hoc",
                principalSchema: "dbo",
                principalTable: "DanhMucMonHoc",
                principalColumn: "ma_mon_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeCuongMonHoc_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_DeCuongMonHoc_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_DeCuongMonHoc_ma_don_vi__DonVi",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_DeCuongMonHoc_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeCuongMonHoc",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeCuongMonHoc_hoc_ky_du_kien_1",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeCuongMonHoc_trang_thai_1",
                schema: "dbo",
                table: "DeCuongMonHoc");

            migrationBuilder.RenameTable(
                name: "DeCuongMonHoc",
                schema: "dbo",
                newName: "CourseSyllabus",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "UQ_DeCuongMonHoc_1",
                schema: "dbo",
                table: "CourseSyllabus",
                newName: "UQ_CourseSyllabus_1");

            migrationBuilder.RenameIndex(
                name: "IX_DeCuongMonHoc_ma_don_vi",
                schema: "dbo",
                table: "CourseSyllabus",
                newName: "IX_CourseSyllabus_ma_don_vi");

            migrationBuilder.RenameIndex(
                name: "IX_DeCuongMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "CourseSyllabus",
                newName: "IX_CourseSyllabus_ma_chuyen_nganh");

            migrationBuilder.RenameIndex(
                name: "IX_DeCuongMonHoc_ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "CourseSyllabus",
                newName: "IX_CourseSyllabus_ma_chuong_trinh_mon_hoc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseSyllabus",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_syllabus");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseSyllabus_hoc_ky_du_kien_1",
                schema: "dbo",
                table: "CourseSyllabus",
                sql: "[hoc_ky_du_kien] IS NULL OR [hoc_ky_du_kien] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseSyllabus_trang_thai_1",
                schema: "dbo",
                table: "CourseSyllabus",
                sql: "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'archived')");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_chuong_trinh_mon_hoc",
                principalSchema: "dbo",
                principalTable: "MonHocTrongChuongTrinh",
                principalColumn: "ma_chuong_trinh_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_chuyen_nganh",
                principalSchema: "dbo",
                principalTable: "ChuyenNganh",
                principalColumn: "ma_chuyen_nganh");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_ma_don_vi__DonVi",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_mon_hoc",
                principalSchema: "dbo",
                principalTable: "DanhMucMonHoc",
                principalColumn: "ma_mon_hoc");
        }
    }
}
