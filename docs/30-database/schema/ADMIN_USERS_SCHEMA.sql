-- Schema tham khảo cho module Admin Users.
-- Repo hiện đã có các bảng/model tương ứng nên không cần migration mới nếu database đang theo ApplicationDbContext hiện tại.

CREATE TABLE dbo.NguoiDung (
    ma_nguoi_dung INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ma_don_vi INT NOT NULL,
    email NVARCHAR(255) NOT NULL UNIQUE,
    ho_ten NVARCHAR(255) NOT NULL,
    vai_tro_chinh NVARCHAR(50) NOT NULL,
    ma_lop INT NULL,
    so_dien_thoai NVARCHAR(15) NULL,
    trang_thai NVARCHAR(20) NOT NULL DEFAULT N'dang_nhap_lan_dau',
    nam_nhap_hoc INT NULL,
    mat_khau_hash NVARCHAR(MAX) NULL,
    ngay_tao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    lan_dang_nhap_cuoi DATETIME2 NULL,
    so_lan_sai_mat_khau INT NOT NULL DEFAULT 0,
    dang_nhap_lan_dau BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_NguoiDung_trang_thai_admin_users
        CHECK (trang_thai IN (N'hoat_dong', N'bi_khoa', N'dang_nhap_lan_dau'))
);

CREATE TABLE dbo.VaiTro (
    ma_vai_tro INT NOT NULL PRIMARY KEY,
    ma_code_vai_tro NVARCHAR(50) NOT NULL UNIQUE,
    ten_vai_tro NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.PhanQuyenNguoiDung (
    ma_nguoi_dung INT NOT NULL,
    ma_vai_tro INT NOT NULL,
    ngay_gan DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PhanQuyenNguoiDung_admin_users PRIMARY KEY (ma_nguoi_dung, ma_vai_tro)
);

CREATE TABLE dbo.NhatKyKiemToan (
    ma_kiem_toan INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ma_don_vi INT NOT NULL,
    loai_doi_tuong NVARCHAR(100) NOT NULL,
    ma_doi_tuong INT NOT NULL,
    hanh_dong NVARCHAR(50) NOT NULL,
    gia_tri_cu NVARCHAR(MAX) NULL,
    gia_tri_moi NVARCHAR(MAX) NULL,
    nguoi_thay_doi INT NULL,
    thoi_diem_thay_doi DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
