using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNganhChuyenNganhToDanhMucMonHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucMonHoc_ma_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_nganh");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhMucMonHoc_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_chuyen_nganh",
                principalSchema: "dbo",
                principalTable: "ChuyenNganh",
                principalColumn: "ma_chuyen_nganh");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhMucMonHoc_ma_nganh__NganhDaoTao",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_nganh",
                principalSchema: "dbo",
                principalTable: "NganhDaoTao",
                principalColumn: "ma_nganh");

            BackfillSubjectMajors(migrationBuilder);
        }

        private static void BackfillSubjectMajors(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Phát triển phần mềm'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Phát triển phần mềm')
                WHERE ma_code_mon_hoc IN ('CTDL101','COM101','COM102','COM103','DBI101','PRO101','BE101','DEV201','SEC101','CLOUD101','CAP101','INT101');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Lập trình Web'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Lập trình Web')
                WHERE ma_code_mon_hoc IN ('WEB101','WEB102','FE101');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Ứng dụng phần mềm'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Ứng dụng phần mềm')
                WHERE ma_code_mon_hoc IN ('API101','MOB101');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Thiết kế nhận diện thương hiệu'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Thiết kế nhận diện thương hiệu')
                WHERE ma_code_mon_hoc IN ('DES101','DES102','DES103','DES104','DES105','DES107','DES110','DES114','DES115','DES116');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Thiết kế UI/UX'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Thiết kế UI/UX')
                WHERE ma_code_mon_hoc IN ('DES106','DES109','DES111','DES112');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Thiết kế 3D / Motion Graphic'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Thiết kế 3D / Motion Graphic')
                WHERE ma_code_mon_hoc IN ('DES108','DES113');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Digital Marketing'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Digital Marketing')
                WHERE ma_code_mon_hoc IN ('MKT101','MKT103','MKT105','MKT106','MKT107','MKT110','MKT111','MKT115','MKT116');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Content Marketing'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Content Marketing')
                WHERE ma_code_mon_hoc IN ('MKT104','MKT109','MKT114');

                UPDATE dbo.DanhMucMonHoc
                SET ma_nganh = (SELECT TOP 1 cn.ma_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Marketing & Sales'),
                    ma_chuyen_nganh = (SELECT TOP 1 cn.ma_chuyen_nganh FROM dbo.ChuyenNganh cn WHERE cn.ten_chuyen_nganh = N'Marketing & Sales')
                WHERE ma_code_mon_hoc IN ('MKT102','MKT108','MKT112','MKT113');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhMucMonHoc_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhMucMonHoc_ma_nganh__NganhDaoTao",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucMonHoc_ma_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropColumn(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropColumn(
                name: "ma_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc");
        }
    }
}
