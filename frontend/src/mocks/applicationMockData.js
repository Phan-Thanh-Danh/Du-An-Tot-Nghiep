import dayjs from 'dayjs'

export const applicationTypes = [
  { value: 'xac_nhan_sinh_vien', label: 'Xin xác nhận sinh viên' },
  { value: 'nghi_phep', label: 'Đơn xin nghỉ phép' },
  { value: 'phuc_khao', label: 'Đơn phúc khảo điểm' },
  { value: 'thi_lai', label: 'Đăng ký thi lại' },
]

const baseDate = dayjs().startOf('day')

function date(days, hour = 9) {
  return baseDate.add(days, 'day').hour(hour).minute(0).second(0).toDate()
}

function timeline(...items) {
  return items.map((item, index) => ({
    id: `tl-${index + 1}`,
    at: item.at,
    title: item.title,
    description: item.description,
    actor: item.actor || 'Hệ thống LMS',
  }))
}

export const studentApplications = [
  {
    id: 'APP-2406-001',
    tieuDe: 'Xin xác nhận sinh viên để bổ sung hồ sơ thực tập',
    loaiDon: 'xac_nhan_sinh_vien',
    trangThai: 'dang_xem_xet',
    ngayTao: date(-7, 8),
    ngayNop: date(-6, 10),
    hanXuLy: date(1, 17),
    capNhatLanCuoi: date(-1, 15),
    moTaNgan: 'Cần giấy xác nhận sinh viên đang theo học tại trường để nộp cho doanh nghiệp thực tập.',
    nguoiXuLy: 'Trần Thị Giáo Vụ',
    noiDungYeuCauBoSung: '',
    lyDoTuChoi: '',
    formData: [
      { label: 'Mục đích sử dụng', value: 'Nộp hồ sơ thực tập doanh nghiệp' },
      { label: 'Số bản cần cấp', value: '02 bản' },
      { label: 'Ngôn ngữ', value: 'Tiếng Việt' },
    ],
    evidence: [
      { id: 'ev-1', name: 'thu-moi-thuc-tap.pdf', size: '420 KB', uploadedAt: date(-6, 9) },
    ],
    timeline: timeline(
      { at: date(-7, 8), title: 'Tạo bản nháp', description: 'Sinh viên tạo đơn.' },
      { at: date(-6, 10), title: 'Nộp đơn', description: 'Đơn đã được gửi đến giáo vụ.' },
      { at: date(-1, 15), title: 'Đang xem xét', description: 'Giáo vụ đang kiểm tra thông tin.', actor: 'Trần Thị Giáo Vụ' },
    ),
  },
  {
    id: 'APP-2406-002',
    tieuDe: 'Đơn xin nghỉ phép buổi học Cơ sở dữ liệu',
    loaiDon: 'nghi_phep',
    trangThai: 'yeu_cau_bo_sung',
    ngayTao: date(-4, 11),
    ngayNop: date(-4, 11),
    hanXuLy: date(2, 17),
    capNhatLanCuoi: date(-2, 14),
    moTaNgan: 'Xin phép nghỉ một buổi học vì lịch khám sức khỏe bắt buộc.',
    nguoiXuLy: 'Lê Hoàng Nam',
    noiDungYeuCauBoSung: 'Vui lòng bổ sung giấy hẹn khám hoặc minh chứng y tế có ngày rõ ràng.',
    lyDoTuChoi: '',
    formData: [
      { label: 'Ngày nghỉ', value: '27/06/2026' },
      { label: 'Môn học', value: 'Cơ sở dữ liệu' },
      { label: 'Lý do', value: 'Khám sức khỏe theo lịch bệnh viện' },
    ],
    evidence: [],
    timeline: timeline(
      { at: date(-4, 11), title: 'Nộp đơn', description: 'Đơn nghỉ phép đã được gửi.' },
      { at: date(-2, 14), title: 'Yêu cầu bổ sung', description: 'Cần bổ sung minh chứng y tế.', actor: 'Lê Hoàng Nam' },
    ),
  },
  {
    id: 'APP-2406-003',
    tieuDe: 'Phúc khảo điểm giữa kỳ môn Lập trình Java',
    loaiDon: 'phuc_khao',
    trangThai: 'nhap',
    ngayTao: date(-1, 20),
    ngayNop: null,
    hanXuLy: null,
    capNhatLanCuoi: date(-1, 20),
    moTaNgan: 'Bản nháp đơn phúc khảo, chưa nộp cho phòng đào tạo.',
    nguoiXuLy: '',
    noiDungYeuCauBoSung: '',
    lyDoTuChoi: '',
    formData: [
      { label: 'Môn học', value: 'Lập trình Java' },
      { label: 'Cột điểm', value: 'Giữa kỳ' },
      { label: 'Điểm hiện tại', value: '6.5' },
    ],
    evidence: [],
    timeline: timeline(
      { at: date(-1, 20), title: 'Tạo bản nháp', description: 'Sinh viên lưu thông tin ban đầu.' },
    ),
  },
  {
    id: 'APP-2405-014',
    tieuDe: 'Đăng ký thi lại môn Toán rời rạc',
    loaiDon: 'thi_lai',
    trangThai: 'da_duyet',
    ngayTao: date(-18, 8),
    ngayNop: date(-18, 9),
    hanXuLy: date(-10, 17),
    capNhatLanCuoi: date(-12, 10),
    moTaNgan: 'Đơn đã được duyệt, sinh viên theo dõi lịch thi lại trong cổng học vụ.',
    nguoiXuLy: 'Nguyễn Minh Khoa',
    noiDungYeuCauBoSung: '',
    lyDoTuChoi: '',
    formData: [
      { label: 'Môn học', value: 'Toán rời rạc' },
      { label: 'Học kỳ', value: 'Summer 2026' },
      { label: 'Lý do', value: 'Chưa đạt điều kiện qua môn' },
    ],
    evidence: [
      { id: 'ev-2', name: 'bang-diem.pdf', size: '188 KB', uploadedAt: date(-18, 8) },
    ],
    timeline: timeline(
      { at: date(-18, 9), title: 'Nộp đơn', description: 'Đơn đăng ký thi lại đã gửi.' },
      { at: date(-12, 10), title: 'Đã duyệt', description: 'Đơn được phê duyệt.', actor: 'Nguyễn Minh Khoa' },
    ),
  },
]

export const adminApplications = [
  ...studentApplications,
  {
    id: 'APP-2406-009',
    tieuDe: 'Xin tạm hoãn học phần vì lý do cá nhân',
    loaiDon: 'nghi_phep',
    trangThai: 'da_nop',
    ngayTao: date(-2, 7),
    ngayNop: date(-2, 8),
    hanXuLy: date(0, 17),
    capNhatLanCuoi: date(-2, 8),
    moTaNgan: 'Sinh viên cần giáo vụ tiếp nhận để kiểm tra điều kiện tạm hoãn.',
    nguoiXuLy: '',
    sinhVien: 'Phạm Minh Danh',
    maSinhVien: 'SV16004',
    sla: 'sap_qua_han',
    formData: [
      { label: 'Học phần', value: 'Lập trình Web' },
      { label: 'Lý do', value: 'Lý do cá nhân cần xác minh' },
    ],
    evidence: [{ id: 'ev-3', name: 'don-giai-trinh.pdf', size: '310 KB', uploadedAt: date(-2, 8) }],
    timeline: timeline(
      { at: date(-2, 8), title: 'Nộp đơn', description: 'Sinh viên gửi đơn đến hàng đợi.' },
    ),
  },
].map((item, index) => ({
  sinhVien: item.sinhVien || ['Nguyễn Văn Anh', 'Trần Thị Bình', 'Lê Hoàng Cường', 'Đỗ Thùy Dương'][index % 4],
  maSinhVien: item.maSinhVien || `SV${16001 + index}`,
  nguoiXuLy: item.nguoiXuLy,
  sla: item.sla || (dayjs(item.hanXuLy).isBefore(dayjs()) && !['da_duyet', 'tu_choi', 'da_huy'].includes(item.trangThai) ? 'qua_han' : 'dung_han'),
  ...item,
}))

export const assignableUsers = [
  { id: 'staff-1', name: 'Trần Thị Giáo Vụ' },
  { id: 'staff-2', name: 'Lê Hoàng Nam' },
  { id: 'staff-3', name: 'Nguyễn Minh Khoa' },
]

export const applicationReportCards = [
  { label: 'Tổng tiếp nhận', value: 1245, status: 'da_nop' },
  { label: 'Đang xử lý', value: 84, status: 'dang_xem_xet' },
  { label: 'Đã hoàn thành', value: 1120, status: 'da_duyet' },
  { label: 'Quá hạn xử lý', value: 12, status: 'yeu_cau_bo_sung' },
]

export function getApplicationTypeLabel(type) {
  return applicationTypes.find((item) => item.value === type)?.label || 'Loại đơn khác'
}
