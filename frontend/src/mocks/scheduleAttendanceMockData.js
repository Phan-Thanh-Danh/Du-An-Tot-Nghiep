export const scheduleAttendanceMockData = {
  shifts: [
    { id: 'shift-1', maCaHoc: 'CA-01', tenCa: 'Ca 1', buoi: 'Sáng', gioBatDau: '07:00:00', gioKetThuc: '09:15:00', thuTu: 1, conHoatDong: true },
    { id: 'shift-2', maCaHoc: 'CA-02', tenCa: 'Ca 2', buoi: 'Sáng', gioBatDau: '09:30:00', gioKetThuc: '11:45:00', thuTu: 2, conHoatDong: true },
    { id: 'shift-3', maCaHoc: 'CA-03', tenCa: 'Ca 3', buoi: 'Chiều', gioBatDau: '13:00:00', gioKetThuc: '15:15:00', thuTu: 3, conHoatDong: true },
    { id: 'shift-4', maCaHoc: 'CA-04', tenCa: 'Ca 4', buoi: 'Chiều', gioBatDau: '15:30:00', gioKetThuc: '17:45:00', thuTu: 4, conHoatDong: true },
  ],
  schedules: [
    { id: 'sch-01', maTkb: 'TKB-001', tenMon: 'Cơ sở dữ liệu', maMon: 'DBI202', lop: 'SE1501', giaoVien: 'Nguyễn Văn T', hocKy: 'Fall 2025', thuTrongTuan: 2, caHoc: 'CA-01', phong: 'AL-101', trangThai: 'da_xuat_ban', ngayBatDau: '2025-09-01T00:00:00Z', ngayKetThuc: '2025-12-15T00:00:00Z' },
    { id: 'sch-02', maTkb: 'TKB-002', tenMon: 'Lập trình Java', maMon: 'PRO192', lop: 'SE1501', giaoVien: 'Lê Thị H', hocKy: 'Fall 2025', thuTrongTuan: 4, caHoc: 'CA-03', phong: 'AL-205', trangThai: 'da_xuat_ban', ngayBatDau: '2025-09-01T00:00:00Z', ngayKetThuc: '2025-12-15T00:00:00Z' }
  ],
  sessions: [
    { id: 'sess-01', maBuoiHoc: 'BH-001', maTkb: 'TKB-001', ngayHoc: '2025-10-15T00:00:00Z', tenMon: 'Cơ sở dữ liệu', lop: 'SE1501', giaoVien: 'Nguyễn Văn T', giaoVienDayThay: null, caHoc: 'CA-01', phong: 'AL-101', trangThaiBuoi: 'da_hoan_thanh', loaiThayDoi: 'none', lyDoThayDoi: null, daKhoaDiemDanh: true },
    { id: 'sess-02', maBuoiHoc: 'BH-002', maTkb: 'TKB-002', ngayHoc: '2025-10-17T00:00:00Z', tenMon: 'Lập trình Java', lop: 'SE1501', giaoVien: 'Lê Thị H', giaoVienDayThay: null, caHoc: 'CA-03', phong: 'AL-205', trangThaiBuoi: 'sap_dien_ra', loaiThayDoi: 'none', lyDoThayDoi: null, daKhoaDiemDanh: false },
    { id: 'sess-03', maBuoiHoc: 'BH-003', maTkb: 'TKB-001', ngayHoc: '2025-10-22T00:00:00Z', tenMon: 'Cơ sở dữ liệu', lop: 'SE1501', giaoVien: 'Nguyễn Văn T', giaoVienDayThay: 'Trần Văn X', caHoc: 'CA-01', phong: 'AL-102', trangThaiBuoi: 'sap_dien_ra', loaiThayDoi: 'doi_phong', lyDoThayDoi: 'Phòng AL-101 hỏng máy chiếu', daKhoaDiemDanh: false }
  ],
  attendances: [
    { 
      id: 'att-01', maBuoiHoc: 'BH-001', danhSachSinhVien: [
        { maSinhVien: 'SE150001', hoTen: 'Nguyễn Văn A', lop: 'SE1501', trangThai: 'co_mat', ghiChu: '', thoiGianDiemDanh: '2025-10-15T07:10:00Z' },
        { maSinhVien: 'SE150002', hoTen: 'Trần Thị B', lop: 'SE1501', trangThai: 'di_muon', ghiChu: 'Tắc đường', thoiGianDiemDanh: '2025-10-15T07:45:00Z' },
        { maSinhVien: 'SE150003', hoTen: 'Lê Văn C', lop: 'SE1501', trangThai: 'vang', ghiChu: '', thoiGianDiemDanh: '2025-10-15T08:00:00Z' }
      ]
    },
    { 
      id: 'att-02', maBuoiHoc: 'BH-002', danhSachSinhVien: [
        { maSinhVien: 'SE150001', hoTen: 'Nguyễn Văn A', lop: 'SE1501', trangThai: null, ghiChu: '', thoiGianDiemDanh: null },
        { maSinhVien: 'SE150002', hoTen: 'Trần Thị B', lop: 'SE1501', trangThai: null, ghiChu: '', thoiGianDiemDanh: null },
        { maSinhVien: 'SE150003', hoTen: 'Lê Văn C', lop: 'SE1501', trangThai: null, ghiChu: '', thoiGianDiemDanh: null }
      ]
    }
  ],
  unlockRequests: [
    { id: 'ul-01', maYeuCau: 'YC-UL-001', maBuoiHoc: 'BH-001', giaoVien: 'Nguyễn Văn T', lyDo: 'Sửa lại trạng thái cho sinh viên Lê Văn C do xin phép trực tiếp', trangThai: 'cho_duyet', ngayGui: '2025-10-16T08:00:00Z', nguoiDuyet: null }
  ]
}
