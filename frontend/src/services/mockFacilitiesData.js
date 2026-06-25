export const mockBuildings = [
  { maToaNha: 1, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', diaChi: 'Khu A', soTang: 5, conHoatDong: true, ngayTao: '2024-01-15T00:00:00', ngayCapNhat: null },
  { maToaNha: 2, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', diaChi: 'Khu B', soTang: 3, conHoatDong: true, ngayTao: '2024-01-15T00:00:00', ngayCapNhat: null },
  { maToaNha: 3, maDonVi: 2, tenDonVi: 'Cơ sở phụ', maCodeToaNha: 'C', tenToaNha: 'Tòa nhà C', diaChi: 'Khu C', soTang: 2, conHoatDong: false, ngayTao: '2024-06-01T00:00:00', ngayCapNhat: null },
  { maToaNha: 4, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeToaNha: 'D', tenToaNha: 'Tòa nhà D (Ký túc xá)', diaChi: 'Khu D', soTang: 8, conHoatDong: true, ngayTao: '2024-02-10T00:00:00', ngayCapNhat: null },
]

export const mockFloors = [
  { maTang: 101, maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 1', thuTuTang: 1, moTa: 'Tầng trệt', conHoatDong: true },
  { maTang: 102, maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 2', thuTuTang: 2, moTa: null, conHoatDong: true },
  { maTang: 103, maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 3', thuTuTang: 3, moTa: null, conHoatDong: true },
  { maTang: 104, maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 4', thuTuTang: 4, moTa: 'Phòng máy tính', conHoatDong: true },
  { maTang: 105, maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 5', thuTuTang: 5, moTa: null, conHoatDong: false },
  { maTang: 201, maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 1', thuTuTang: 1, moTa: null, conHoatDong: true },
  { maTang: 202, maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 2', thuTuTang: 2, moTa: null, conHoatDong: true },
  { maTang: 203, maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maDonVi: 1, tenDonVi: 'Cơ sở chính', tenTang: 'Tầng 3', thuTuTang: 3, moTa: 'Phòng hội thảo', conHoatDong: true },
  { maTang: 301, maToaNha: 3, maCodeToaNha: 'C', tenToaNha: 'Tòa nhà C', maDonVi: 2, tenDonVi: 'Cơ sở phụ', tenTang: 'Tầng 1', thuTuTang: 1, moTa: null, conHoatDong: false },
]

export const mockRooms = [
  { maPhong: 1, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 101, tenTang: 'Tầng 1', thuTuTang: 1, maCodePhong: 'PH101', tenPhong: 'Phòng 101', sucChua: 40, loaiPhong: 'ly_thuyet', trangThaiPhong: 'hoat_dong', ghiChu: 'Phòng học lý thuyết tiêu chuẩn' },
  { maPhong: 2, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 101, tenTang: 'Tầng 1', thuTuTang: 1, maCodePhong: 'PH102', tenPhong: 'Phòng 102', sucChua: 35, loaiPhong: 'thuc_hanh', trangThaiPhong: 'hoat_dong', ghiChu: 'Trang bị máy tính' },
  { maPhong: 3, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 102, tenTang: 'Tầng 2', thuTuTang: 2, maCodePhong: 'PH201', tenPhong: 'Lab 201', sucChua: 30, loaiPhong: 'lab', trangThaiPhong: 'hoat_dong', ghiChu: 'Phòng thực hành CNTT' },
  { maPhong: 4, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 102, tenTang: 'Tầng 2', thuTuTang: 2, maCodePhong: 'PH202', tenPhong: 'Hội trường 202', sucChua: 150, loaiPhong: 'hoi_truong', trangThaiPhong: 'bao_tri', ghiChu: 'Đang sửa chữa' },
  { maPhong: 5, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 103, tenTang: 'Tầng 3', thuTuTang: 3, maCodePhong: 'PH301', tenPhong: 'Phòng 301', sucChua: 45, loaiPhong: 'ly_thuyet', trangThaiPhong: 'hoat_dong', ghiChu: null },
  { maPhong: 6, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 103, tenTang: 'Tầng 3', thuTuTang: 3, maCodePhong: 'PH302', tenPhong: 'Phòng 302', sucChua: 40, loaiPhong: 'ly_thuyet', trangThaiPhong: 'hoat_dong', ghiChu: null },
  { maPhong: 7, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 1, maCodeToaNha: 'A', tenToaNha: 'Tòa nhà A', maTang: 104, tenTang: 'Tầng 4', thuTuTang: 4, maCodePhong: 'LAB401', tenPhong: 'Phòng máy 401', sucChua: 30, loaiPhong: 'phong_thi_nghiem', trangThaiPhong: 'hoat_dong', ghiChu: '40 máy tính' },
  { maPhong: 8, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maTang: 201, tenTang: 'Tầng 1', thuTuTang: 1, maCodePhong: 'PH501', tenPhong: 'Phòng 501', sucChua: 50, loaiPhong: 'ly_thuyet', trangThaiPhong: 'hoat_dong', ghiChu: null },
  { maPhong: 9, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maTang: 201, tenTang: 'Tầng 1', thuTuTang: 1, maCodePhong: 'STU01', tenPhong: 'Studio 1', sucChua: 15, loaiPhong: 'khac', trangThaiPhong: 'ngung_hoat_dong', ghiChu: 'Phòng studio ngừng hoạt động' },
  { maPhong: 10, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maTang: 202, tenTang: 'Tầng 2', thuTuTang: 2, maCodePhong: 'PH601', tenPhong: 'Phòng 601', sucChua: 40, loaiPhong: 'ly_thuyet', trangThaiPhong: 'hoat_dong', ghiChu: null },
  { maPhong: 11, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maTang: 202, tenTang: 'Tầng 2', thuTuTang: 2, maCodePhong: 'LAB601', tenPhong: 'Lab 601', sucChua: 25, loaiPhong: 'phong_thi_nghiem', trangThaiPhong: 'hoat_dong', ghiChu: 'Phòng thí nghiệm hóa' },
  { maPhong: 12, maDonVi: 1, tenDonVi: 'Cơ sở chính', maToaNha: 2, maCodeToaNha: 'B', tenToaNha: 'Tòa nhà B', maTang: 203, tenTang: 'Tầng 3', thuTuTang: 3, maCodePhong: 'PH701', tenPhong: 'Phòng hội thảo', sucChua: 80, loaiPhong: 'hoi_truong', trangThaiPhong: 'hoat_dong', ghiChu: 'Trang bị âm thanh, máy chiếu' },
]
