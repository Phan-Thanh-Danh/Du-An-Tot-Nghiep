using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUtf8DatabaseEncoding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Chuẩn hóa nhan_xet_tu_do và ai_chu_de trong bảng DanhGiaGiaoVien
            migrationBuilder.Sql(@"
                UPDATE [DanhGiaGiaoVien]
                SET [ai_chu_de] = N'[""Phương pháp giảng dạy""]'
                WHERE [ai_chu_de] LIKE N'%Ph%' OR [ai_chu_de] LIKE N'%giang%' OR [ai_chu_de] LIKE N'%gi%ng%' OR [ai_chu_de] LIKE N'%Æ%';

                UPDATE [DanhGiaGiaoVien]
                SET [nhan_xet_tu_do] = N'Giảng viên chuẩn bị bài đầy đủ và giải thích dễ hiểu.'
                WHERE [nhan_xet_tu_do] LIKE N'%chu%' OR [nhan_xet_tu_do] LIKE N'%d% hi%' OR [nhan_xet_tu_do] LIKE N'%bĂ i%' OR [nhan_xet_tu_do] LIKE N'%áº%';

                UPDATE [DanhGiaGiaoVien] SET [nhan_xet_tu_do] = REPLACE([nhan_xet_tu_do], N'Giáº£ng viĂªn', N'Giảng viên') WHERE [nhan_xet_tu_do] LIKE N'%Giáº£ng%';
                UPDATE [DanhGiaGiaoVien] SET [nhan_xet_tu_do] = REPLACE([nhan_xet_tu_do], N'truyá»n Ä‘áº¡t', N'truyền đạt') WHERE [nhan_xet_tu_do] LIKE N'%truyá»n%';
                UPDATE [DanhGiaGiaoVien] SET [nhan_xet_tu_do] = REPLACE([nhan_xet_tu_do], N'kiáº¿n thá»©c', N'kiến thức') WHERE [nhan_xet_tu_do] LIKE N'%kiáº¿n%';
            ");

            // 2. Chuẩn hóa Nội dung câu hỏi trong bảng CauHoiDanhGia
            migrationBuilder.Sql(@"
                UPDATE [CauHoiDanhGia] SET [noi_dung_cau_hoi] = N'Giảng viên truyền đạt kiến thức rõ ràng' WHERE [ma_cau_hoi_dg] = 1 OR [noi_dung_cau_hoi] LIKE N'%Giá%ng%' OR [noi_dung_cau_hoi] LIKE N'%truy%n%';
                UPDATE [CauHoiDanhGia] SET [noi_dung_cau_hoi] = N'Giảng viên chuẩn bị bài đầy đủ' WHERE [ma_cau_hoi_dg] = 2 OR [noi_dung_cau_hoi] LIKE N'%chu%n%';
                UPDATE [CauHoiDanhGia] SET [noi_dung_cau_hoi] = N'Giảng viên sẵn sàng hỗ trợ ngoài giờ' WHERE [ma_cau_hoi_dg] = 3;
                UPDATE [CauHoiDanhGia] SET [noi_dung_cau_hoi] = N'Tài liệu giảng dạy phong phú' WHERE [ma_cau_hoi_dg] = 4;
            ");

            // 3. Chuẩn hóa Tên người dùng trong bảng NguoiDung
            migrationBuilder.Sql(@"
                UPDATE [NguoiDung] SET [ho_ten] = REPLACE([ho_ten], N'Giáº£ng viĂªn', N'Giảng viên') WHERE [ho_ten] LIKE N'%Giáº£ng%';
                UPDATE [NguoiDung] SET [ho_ten] = REPLACE([ho_ten], N'Sinh viĂªn', N'Sinh viên') WHERE [ho_ten] LIKE N'%Sinh%';
                UPDATE [NguoiDung] SET [ho_ten] = REPLACE([ho_ten], N'Quáº£n trá»‹', N'Quản trị') WHERE [ho_ten] LIKE N'%Quáº£n%';
            ");

            // 4. Chuẩn hóa Tên đơn vị / Cơ sở trong bảng DonVi
            migrationBuilder.Sql(@"
                UPDATE [DonVi] SET [ten_don_vi] = N'Trường Đại học / Cao đẳng FPT Polytechnic' WHERE [ma_don_vi] = 1 OR [ten_don_vi] LIKE N'%TrÆ°á» ng%' OR [ten_don_vi] LIKE N'%Polytechnic%';
                UPDATE [DonVi] SET [ten_don_vi] = N'Cơ sở FPT Polytechnic Hồ Chí Minh' WHERE [ma_don_vi] = 2 OR [ten_don_vi] LIKE N'%H%Ch%Minh%';
            ");

            // 5. Chuẩn hóa Tên môn học trong bảng DanhMucMonHoc
            migrationBuilder.Sql(@"
                UPDATE [DanhMucMonHoc] SET [ten_mon_hoc] = N'Cơ sở dữ liệu SQL Server' WHERE [ma_code_mon_hoc] = 'COM102';
                UPDATE [DanhMucMonHoc] SET [ten_mon_hoc] = N'Lập trình Java cơ bản' WHERE [ma_code_mon_hoc] = 'COM108';
                UPDATE [DanhMucMonHoc] SET [ten_mon_hoc] = N'Thiết kế giao diện Web HTML5/CSS3' WHERE [ma_code_mon_hoc] = 'WEB101';
                UPDATE [DanhMucMonHoc] SET [ten_mon_hoc] = N'Lập trình JavaScript nâng cao' WHERE [ma_code_mon_hoc] = 'WEB102';
            ");

            // 6. Chuẩn hóa Lý do rớt môn trong bảng DiemSo
            migrationBuilder.Sql(@"
                UPDATE [DiemSo] SET [ly_do_rot] = N'[""Điểm tổng kết dưới ngưỡng đạt""]' WHERE [ly_do_rot] IS NOT NULL AND [ly_do_rot] <> '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
