export const rewardDisciplineMockData = {
  studentRewards: [
    {
      id: 'rw-01',
      maKhenThuong: 'KT-2026-001',
      tieuDe: 'Sinh viên xuất sắc Học kỳ Fall 2025',
      loaiKhenThuong: 'Học tập',
      hocKy: 'Fall 2025',
      ngayCap: '2026-01-15T08:00:00Z',
      trangThai: 'issued',
      diemThanhTich: 100,
      xepHang: 'Xuất sắc',
      moTa: 'Tuyên dương thành tích học tập xuất sắc với GPA 4.0.',
      certificateUrl: '/mocks/cert-001.pdf',
      certificateStatus: 'generated',
      timeline: [
        { loaiSuKien: 'TAO_MOI', tieuDe: 'Đề cử khen thưởng', thoiGian: '2025-12-20T08:00:00Z' },
        { loaiSuKien: 'DUYET', tieuDe: 'BGH Duyệt', thoiGian: '2026-01-10T08:00:00Z' },
        { loaiSuKien: 'CAP_BANG', tieuDe: 'Cấp bằng khen', thoiGian: '2026-01-15T08:00:00Z' }
      ]
    },
    {
      id: 'rw-02',
      maKhenThuong: 'KT-2026-045',
      tieuDe: 'Thanh niên tiên tiến làm theo lời Bác',
      loaiKhenThuong: 'Phong trào',
      hocKy: 'Spring 2026',
      ngayCap: null,
      trangThai: 'approved',
      diemThanhTich: 50,
      xepHang: 'Giỏi',
      moTa: 'Tích cực tham gia các hoạt động Đoàn - Hội.',
      certificateUrl: null,
      certificateStatus: 'pending',
      timeline: [
        { loaiSuKien: 'TAO_MOI', tieuDe: 'Đề cử khen thưởng', thoiGian: '2026-05-10T08:00:00Z' },
        { loaiSuKien: 'DUYET', tieuDe: 'BGH Duyệt', thoiGian: '2026-05-20T08:00:00Z' }
      ]
    }
  ],
  studentDiscipline: [
    {
      id: 'dl-01',
      maKyLuat: 'KL-2025-012',
      tieuDe: 'Vi phạm quy chế thi',
      mucDoKyLuat: 'Cảnh cáo',
      hinhThucXuLy: 'Hủy kết quả môn học',
      ngayViPham: '2025-11-05T08:00:00Z',
      trangThai: 'active',
      moTaCongKhai: 'Mang tài liệu vào phòng thi môn PRJ301.',
      thoiHanHieuLuc: '2026-05-05T08:00:00Z',
      coTheKhieuNai: true,
      appealStatus: 'pending',
      timeline: [
        { loaiSuKien: 'TAO_MOI', tieuDe: 'Lập biên bản', thoiGian: '2025-11-05T10:00:00Z' },
        { loaiSuKien: 'DUYET', tieuDe: 'Ra quyết định kỷ luật', thoiGian: '2025-11-10T08:00:00Z' }
      ]
    }
  ],
  studentAppeals: [
    {
      id: 'ap-01',
      maKhieuNai: 'KN-2025-001',
      maHoSo: 'KL-2025-012',
      tieuDe: 'Khiếu nại KL-2025-012',
      lyDo: 'Em không mang tài liệu, đó là giấy nháp giám thị phát.',
      trangThai: 'pending',
      ngayGui: '2025-11-12T08:00:00Z'
    }
  ],
  rewardCampaigns: [
    {
      id: 'cmp-01',
      maDot: 'DOT-KT-2026-01',
      tenDot: 'Khen thưởng học kỳ Fall 2025',
      hocKy: 'Fall 2025',
      donVi: 'Toàn trường',
      loaiDot: 'Học tập',
      trangThai: 'approved',
      tongUngVien: 1250,
      daDuyet: 1200,
      biLoai: 50,
      choDuyet: 0,
      certificateGenerated: 1200,
      certificateFailed: 0,
      ngayBatDau: '2025-12-01T08:00:00Z',
      ngayKetThuc: '2025-12-31T08:00:00Z'
    },
    {
      id: 'cmp-02',
      maDot: 'DOT-KT-2026-02',
      tenDot: 'Khen thưởng phong trào Spring 2026',
      hocKy: 'Spring 2026',
      donVi: 'Phòng CTSV',
      loaiDot: 'Phong trào',
      trangThai: 'evaluating',
      tongUngVien: 300,
      daDuyet: 250,
      biLoai: 10,
      choDuyet: 40,
      certificateGenerated: 0,
      certificateFailed: 0,
      ngayBatDau: '2026-05-01T08:00:00Z',
      ngayKetThuc: '2026-05-30T08:00:00Z'
    }
  ],
  candidates: [
    {
      id: 'cand-01',
      campaignId: 'cmp-02',
      maUngVien: 'UV-001',
      hoTen: 'Nguyễn Văn A',
      maSinhVien: 'SE150001',
      lop: 'SE1501',
      diemTrungBinh: 8.5,
      diemRenLuyen: 90,
      soTinChiDat: 15,
      xepHang: 'Giỏi',
      trangThai: 'pending',
      lyDoLoai: null,
      coKyLuatDangHieuLuc: false
    },
    {
      id: 'cand-02',
      campaignId: 'cmp-02',
      maUngVien: 'UV-002',
      hoTen: 'Trần Thị B',
      maSinhVien: 'SE150002',
      lop: 'SE1501',
      diemTrungBinh: 9.2,
      diemRenLuyen: 95,
      soTinChiDat: 15,
      xepHang: 'Xuất sắc',
      trangThai: 'approved',
      lyDoLoai: null,
      coKyLuatDangHieuLuc: false
    },
    {
      id: 'cand-03',
      campaignId: 'cmp-02',
      maUngVien: 'UV-003',
      hoTen: 'Lê Văn C',
      maSinhVien: 'SE150003',
      lop: 'SE1502',
      diemTrungBinh: 6.5,
      diemRenLuyen: 60,
      soTinChiDat: 10,
      xepHang: 'Trung bình',
      trangThai: 'excluded',
      lyDoLoai: 'Không đủ điểm rèn luyện',
      coKyLuatDangHieuLuc: true
    }
  ],
  disciplineRecords: [
    {
      id: 'dl-01',
      maHoSo: 'KL-2025-012',
      hoTen: 'Nguyễn Văn D',
      maSinhVien: 'SE150004',
      lop: 'SE1502',
      tieuDe: 'Vi phạm quy chế thi',
      mucDoKyLuat: 'Cảnh cáo',
      hinhThucXuLy: 'Hủy kết quả thi',
      ngayViPham: '2025-11-05T08:00:00Z',
      trangThai: 'active',
      nguoiTao: 'Giáo vụ thi',
      ngayTao: '2025-11-05T10:00:00Z',
      appealCount: 1,
      moTa: 'Mang tài liệu trái phép vào phòng thi.',
      canCuXuLy: 'Khoản 2 Điều 15 Quy chế thi',
      timeline: []
    },
    {
      id: 'dl-02',
      maHoSo: 'KL-2026-001',
      hoTen: 'Phạm Thị E',
      maSinhVien: 'SE150005',
      lop: 'SE1503',
      tieuDe: 'Đánh nhau trong khuôn viên trường',
      mucDoKyLuat: 'Đình chỉ học tập',
      hinhThucXuLy: 'Đình chỉ 1 học kỳ',
      ngayViPham: '2026-02-10T08:00:00Z',
      trangThai: 'pending_approval',
      nguoiTao: 'Ban bảo vệ',
      ngayTao: '2026-02-10T14:00:00Z',
      appealCount: 0,
      moTa: 'Gây rối trật tự công cộng.',
      canCuXuLy: 'Quy chế CTSV',
      timeline: []
    }
  ],
  adminAppeals: [
    {
      id: 'ap-01',
      maKhieuNai: 'KN-2025-001',
      maHoSo: 'KL-2025-012',
      hoTen: 'Nguyễn Văn D',
      maSinhVien: 'SE150004',
      lyDo: 'Em không mang tài liệu, đó là giấy nháp giám thị phát.',
      trangThai: 'pending',
      ngayGui: '2025-11-12T08:00:00Z',
      nguoiXuLy: null,
      ngayXuLy: null,
      ketQua: null
    }
  ],
  reports: {
    totalRewards: 3250,
    totalDiscipline: 145,
    pendingAppeals: 12,
    certificatesGenerated: 3200,
    certificateFailureRate: 1.5,
    topRewardStudents: [
      { name: 'Nguyễn Văn A', studentId: 'SE150001', score: 100 },
      { name: 'Trần Thị B', studentId: 'SE150002', score: 95 }
    ],
    campaignBreakdown: { hocTap: 2000, phongTrao: 1250 },
    disciplineBreakdown: { khienTrach: 100, canhCao: 35, dinhChi: 10 }
  }
}
