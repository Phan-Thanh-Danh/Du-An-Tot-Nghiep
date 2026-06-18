<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Quản lý Người Dùng</h2>
        <p class="text-xs text-muted mt-1">Danh sách tất cả tài khoản trong hệ thống</p>
      </div>
      <button @click="openCreateModal" class="flex items-center gap-2 px-4 py-2 bg-[var(--lg-primary)] hover:bg-[var(--lg-primary-dark)] text-white text-sm font-bold rounded-xl transition-all shadow-sm">
        <Plus :size="18" /> <span>Thêm người dùng</span>
      </button>
    </div>

    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-wrap gap-4 items-end">
      <div class="flex-1 min-w-[200px]">
        <label class="block text-xs font-bold text-heading mb-1.5">Tìm kiếm</label>
        <div class="relative">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
          <input v-model="keyword" @keyup.enter="handleFilter" type="text" placeholder="Tên, Email, SĐT..." class="w-full pl-9 pr-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)]" />
        </div>
      </div>
      <div class="w-full sm:w-48">
        <label class="block text-xs font-bold text-heading mb-1.5">Vai trò</label>
        <select v-model="roleFilter" @change="handleFilter" class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)]">
          <option value="">Tất cả vai trò</option>
          <option v-for="r in rolesList" :key="r.maCodeVaiTro" :value="r.maCodeVaiTro">{{ r.tenVaiTro }}</option>
        </select>
      </div>
      <div class="w-full sm:w-40">
        <label class="block text-xs font-bold text-heading mb-1.5">Trạng thái</label>
        <select v-model="statusFilter" @change="handleFilter" class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)]">
          <option value="">Tất cả trạng thái</option>
          <option value="hoat_dong">Hoạt động</option>
          <option value="bi_khoa">Bị khóa</option>
        </select>
      </div>
      <button @click="handleFilter" class="px-4 py-2 bg-[var(--surface-input)] border border-input hover:bg-[var(--surface-input-hover)] text-heading text-sm font-bold rounded-lg transition-colors h-10">Lọc dữ liệu</button>
    </div>

    <div class="flex-1 surface-card border border-card rounded-2xl shadow-sm flex flex-col overflow-hidden">
      <div class="flex-1 overflow-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="sticky top-0 bg-[var(--surface-card)] border-b border-default z-10">
            <tr>
              <th class="px-4 py-3 font-bold text-heading">Mã / ID</th>
              <th class="px-4 py-3 font-bold text-heading">Họ tên</th>
              <th class="px-4 py-3 font-bold text-heading">Email</th>
              <th class="px-4 py-3 font-bold text-heading">Vai trò</th>
              <th class="px-4 py-3 font-bold text-heading">Đơn vị</th>
              <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
              <th class="px-4 py-3 font-bold text-heading text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredUsers.length === 0" class="bg-transparent">
              <td colspan="7" class="py-12 text-center text-muted"><p>Không tìm thấy người dùng nào.</p></td>
            </tr>
            <tr v-for="user in pagedUsers" :key="user.maNguoiDung" class="hover:bg-[var(--surface-input)]/50 transition-colors">
              <td class="px-4 py-3 font-medium">{{ user.maNguoiDung }}</td>
              <td class="px-4 py-3 font-bold text-heading">{{ user.hoTen }}</td>
              <td class="px-4 py-3">{{ user.email }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-[var(--surface-input)] text-heading border border-default">{{ user.tenVaiTro }}</span>
              </td>
              <td class="px-4 py-3 text-xs">{{ user.tenDonVi || 'N/A' }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center gap-1 px-2 py-1 rounded-md text-[10px] font-bold uppercase tracking-wider" :class="user.trangThai === 'hoat_dong' ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]' : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]'">
                  <CheckCircle2 v-if="user.trangThai === 'hoat_dong'" :size="12" />
                  <Lock v-else :size="12" />
                  {{ user.trangThai === 'hoat_dong' ? 'Hoạt động' : 'Bị khóa' }}
                </span>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEditModal(user)" class="p-1.5 text-muted hover:text-[var(--lg-primary)] hover:bg-[var(--lg-primary)]/10 rounded-lg transition-colors" title="Chỉnh sửa"><Edit2 :size="16" /></button>
                  <button v-if="user.trangThai === 'hoat_dong'" @click="handleToggleLock(user)" class="p-1.5 text-muted hover:text-[var(--color-danger-text)] hover:bg-[var(--color-danger-bg)] rounded-lg transition-colors" title="Khóa tài khoản"><Lock :size="16" /></button>
                  <button v-else @click="handleToggleLock(user)" class="p-1.5 text-[var(--color-danger-text)] hover:text-[var(--color-success-text)] hover:bg-[var(--color-success-bg)] rounded-lg transition-colors" title="Mở khóa tài khoản"><Unlock :size="16" /></button>
                  <button @click="handleResetPassword(user)" class="p-1.5 text-muted hover:text-[var(--color-warning-text)] hover:bg-[var(--color-warning-bg)] rounded-lg transition-colors" title="Đặt lại mật khẩu"><Key :size="16" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="p-4 border-t border-default bg-[var(--surface-card)] flex items-center justify-between text-sm">
        <span class="text-muted">Hiển thị {{ pagedUsers.length }} / {{ filteredUsers.length }} người dùng</span>
        <div class="flex items-center gap-2">
          <button @click="prevPage" :disabled="currentPage === 1" class="px-3 py-1.5 rounded-lg border border-default hover:bg-[var(--surface-input)] disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang trước</button>
          <span class="px-2 font-bold text-heading">Trang {{ currentPage }} / {{ totalPages }}</span>
          <button @click="nextPage" :disabled="currentPage >= totalPages" class="px-3 py-1.5 rounded-lg border border-default hover:bg-[var(--surface-input)] disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang sau</button>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-lg surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col max-h-full">
        <div class="p-4 border-b border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-heading">{{ modalMode === 'create' ? 'Thêm Người Dùng Mới' : 'Chỉnh Sửa Người Dùng' }}</h3>
          <button @click="closeModal" class="p-1 hover:bg-[var(--surface-input)] rounded-lg text-muted"><X :size="20" /></button>
        </div>
        <form @submit.prevent="submitForm" class="p-6 overflow-y-auto space-y-4">
          <div v-if="apiError" class="p-3 bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] text-xs rounded-lg flex gap-2 items-start">
            <AlertTriangle :size="16" class="shrink-0 mt-0.5" /><span>{{ apiError }}</span>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Họ và tên <span class="text-[var(--color-danger-text)]">*</span></label>
            <input v-model="formData.hoTen" type="text" required class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm focus:border-[var(--lg-primary)] outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Email <span class="text-[var(--color-danger-text)]">*</span></label>
            <input v-model="formData.email" type="email" required class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm focus:border-[var(--lg-primary)] outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Số điện thoại</label>
            <input v-model="formData.soDienThoai" type="text" class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm focus:border-[var(--lg-primary)] outline-none" />
          </div>
          <div v-if="modalMode === 'create'">
            <label class="block text-xs font-bold text-heading mb-1.5">Mật khẩu <span class="text-[var(--color-danger-text)]">*</span></label>
            <input v-model="formData.matKhau" type="password" required minlength="8" class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm focus:border-[var(--lg-primary)] outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Vai trò <span class="text-[var(--color-danger-text)]">*</span></label>
            <select v-model="formData.maCodeVaiTro" required class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm focus:border-[var(--lg-primary)] outline-none">
              <option value="" disabled>-- Chọn vai trò --</option>
              <option v-for="r in rolesList" :key="r.maCodeVaiTro" :value="r.maCodeVaiTro">{{ r.tenVaiTro }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Đơn vị <span class="text-[var(--color-danger-text)]">*</span></label>
            <select v-model="formData.maDonVi" required class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm focus:border-[var(--lg-primary)] outline-none">
              <option value="" disabled>-- Chọn đơn vị --</option>
              <option v-for="org in orgsList" :key="org.maDonVi" :value="org.maDonVi">{{ org.tenDonVi }} ({{ org.capDonVi }})</option>
            </select>
          </div>
        </form>
        <div class="p-4 border-t border-default bg-[var(--surface-card)] flex justify-end gap-3">
          <button @click="closeModal" type="button" class="px-4 py-2 text-sm font-bold border border-input rounded-lg hover:bg-[var(--surface-input)] transition-colors">Hủy</button>
          <button @click="submitForm" class="flex items-center justify-center gap-2 px-6 py-2 bg-[var(--lg-primary)] text-white text-sm font-bold rounded-lg hover:bg-[var(--lg-primary-dark)] transition-colors min-w-[100px]">Lưu lại</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Search, Plus, Edit2, Lock, Unlock, Key, CheckCircle2, AlertTriangle, X } from 'lucide-vue-next'

const keyword = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = 15

const showModal = ref(false)
const modalMode = ref('create')
const apiError = ref('')
const formData = ref({ maNguoiDung: null, hoTen: '', email: '', soDienThoai: '', matKhau: '', maCodeVaiTro: '', maDonVi: '' })

const rolesList = [
  { maVaiTro: 1, maCodeVaiTro: 'sieu_quan_tri', tenVaiTro: 'Siêu quản trị' },
  { maVaiTro: 2, maCodeVaiTro: 'quan_tri', tenVaiTro: 'Quản trị hệ thống' },
  { maVaiTro: 3, maCodeVaiTro: 'quan_tri_co_so', tenVaiTro: 'Quản trị cơ sở' },
  { maVaiTro: 4, maCodeVaiTro: 'nhan_vien', tenVaiTro: 'Giáo vụ' },
  { maVaiTro: 5, maCodeVaiTro: 'hieu_truong', tenVaiTro: 'Ban Giám Hiệu' },
  { maVaiTro: 6, maCodeVaiTro: 'giao_vien', tenVaiTro: 'Giảng viên' },
  { maVaiTro: 7, maCodeVaiTro: 'hoc_sinh', tenVaiTro: 'Sinh viên' },
  { maVaiTro: 8, maCodeVaiTro: 'chu_tich', tenVaiTro: 'Chủ tịch hệ thống' },
  { maVaiTro: 9, maCodeVaiTro: 'admin_tai_chinh', tenVaiTro: 'Admin tài chính' },
  { maVaiTro: 10, maCodeVaiTro: 'ke_toan_co_so', tenVaiTro: 'Kế toán cơ sở' },
]

const orgsList = [
  { maDonVi: 1, tenDonVi: 'FPT Polytechnic Hồ Chí Minh', capDonVi: 'Cơ sở' },
  { maDonVi: 2, tenDonVi: 'FPT Polytechnic Đà Nẵng', capDonVi: 'Cơ sở' },
  { maDonVi: 3, tenDonVi: 'FPT Polytechnic Cần Thơ', capDonVi: 'Cơ sở' },
  { maDonVi: 4, tenDonVi: 'Khoa Công nghệ thông tin', capDonVi: 'Khoa' },
  { maDonVi: 5, tenDonVi: 'Khoa Kinh tế', capDonVi: 'Khoa' },
  { maDonVi: 6, tenDonVi: 'Khoa Thiết kế', capDonVi: 'Khoa' },
  { maDonVi: 7, tenDonVi: 'Phòng Đào tạo', capDonVi: 'Phòng ban' },
  { maDonVi: 8, tenDonVi: 'Phòng Công tác sinh viên', capDonVi: 'Phòng ban' },
]

const roleCodeToName = {
  'sieu_quan_tri': 'Siêu quản trị', 'quan_tri': 'Quản trị', 'quan_tri_co_so': 'Quản trị CS',
  'nhan_vien': 'Giáo vụ', 'hieu_truong': 'Ban Giám Hiệu', 'giao_vien': 'Giảng viên',
  'hoc_sinh': 'Sinh viên', 'chu_tich': 'Chủ tịch', 'admin_tai_chinh': 'Admin TC',
  'ke_toan_co_so': 'Kế toán CS',
}

const mockUsers = [
  { maNguoiDung: 1, maDonVi: 1, email: 'hieutruong@lms.edu.vn', hoTen: 'Nguyễn Văn Hiệu Trưởng', vaiTroChinh: 'hieu_truong', tenVaiTro: 'Ban Giám Hiệu', tenDonVi: 'FPT Polytechnic Hồ Chí Minh', soDienThoai: '0909 123 456', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2020', lanDangNhapCuoi: '13/06/2026 08:30' },
  { maNguoiDung: 2, maDonVi: 1, email: 'admin@lms.edu.vn', hoTen: 'Super Admin', vaiTroChinh: 'sieu_quan_tri', tenVaiTro: 'Siêu quản trị', tenDonVi: 'FPT Polytechnic Hồ Chí Minh', soDienThoai: '0909 000 001', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2020', lanDangNhapCuoi: '13/06/2026 10:00' },
  { maNguoiDung: 3, maDonVi: 1, email: 'nguyenvana@lms.edu.vn', hoTen: 'Nguyễn Văn An', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 001', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '15/08/2021', lanDangNhapCuoi: '12/06/2026 14:00' },
  { maNguoiDung: 4, maDonVi: 1, email: 'tranthib@lms.edu.vn', hoTen: 'Trần Thị Bích', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Kinh tế', soDienThoai: '0909 123 002', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/09/2022', lanDangNhapCuoi: '11/06/2026 09:30' },
  { maNguoiDung: 5, maDonVi: 1, email: 'levanc@lms.edu.vn', hoTen: 'Lê Văn Cường', vaiTroChinh: 'nhan_vien', tenVaiTro: 'Giáo vụ', tenDonVi: 'Phòng Đào tạo', soDienThoai: '0909 123 003', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/03/2022', lanDangNhapCuoi: '13/06/2026 07:45' },
  { maNguoiDung: 6, maDonVi: 1, email: 'phamthid@lms.edu.vn', hoTen: 'Phạm Thị Dung', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Thiết kế', soDienThoai: '0909 123 004', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/06/2023', lanDangNhapCuoi: '10/06/2026 11:00' },
  { maNguoiDung: 7, maDonVi: 1, email: 'hoangminhduc@lms.edu.vn', hoTen: 'Hoàng Minh Đức', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 005', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2023', lanDangNhapCuoi: '09/06/2026 15:30' },
  { maNguoiDung: 8, maDonVi: 1, email: 'nguyenthihoa@lms.edu.vn', hoTen: 'Nguyễn Thị Hoa', vaiTroChinh: 'quan_tri_co_so', tenVaiTro: 'Quản trị cơ sở', tenDonVi: 'FPT Polytechnic Hồ Chí Minh', soDienThoai: '0909 123 006', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2021', lanDangNhapCuoi: '13/06/2026 09:00' },
  { maNguoiDung: 9, maDonVi: 1, email: 'vovanhung@lms.edu.vn', hoTen: 'Võ Văn Hùng', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 007', trangThai: 'bi_khoa', namNhapHoc: null, ngayTao: '01/09/2020', lanDangNhapCuoi: '01/06/2026 08:00' },
  { maNguoiDung: 10, maDonVi: 1, email: 'dangthikim@lms.edu.vn', hoTen: 'Đặng Thị Kim', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Thiết kế', soDienThoai: '0909 123 008', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/03/2022', lanDangNhapCuoi: '08/06/2026 10:15' },
  { maNguoiDung: 11, maDonVi: 1, email: 'buiquanglinh@lms.edu.vn', hoTen: 'Bùi Quang Linh', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Kinh tế', soDienThoai: '0909 123 009', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2023', lanDangNhapCuoi: '07/06/2026 14:00' },
  { maNguoiDung: 12, maDonVi: 2, email: 'ngothimai@lms.edu.vn', hoTen: 'Ngô Thị Mai', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'FPT Polytechnic Đà Nẵng', soDienThoai: '0909 123 010', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/06/2022', lanDangNhapCuoi: '06/06/2026 09:00' },
  { maNguoiDung: 13, maDonVi: 1, email: 'tranvannam@lms.edu.vn', hoTen: 'Trần Văn Nam', vaiTroChinh: 'hoc_sinh', tenVaiTro: 'Sinh viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 011', trangThai: 'hoat_dong', namNhapHoc: 2024, ngayTao: '01/09/2024', lanDangNhapCuoi: '13/06/2026 07:00' },
  { maNguoiDung: 14, maDonVi: 1, email: 'lehoangphat@lms.edu.vn', hoTen: 'Lê Hoàng Phát', vaiTroChinh: 'hoc_sinh', tenVaiTro: 'Sinh viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 012', trangThai: 'hoat_dong', namNhapHoc: 2024, ngayTao: '01/09/2024', lanDangNhapCuoi: '12/06/2026 20:30' },
  { maNguoiDung: 15, maDonVi: 1, email: 'tranthuy@lms.edu.vn', hoTen: 'Trần Bích Thủy', vaiTroChinh: 'hoc_sinh', tenVaiTro: 'Sinh viên', tenDonVi: 'Khoa Kinh tế', soDienThoai: '0909 123 013', trangThai: 'hoat_dong', namNhapHoc: 2024, ngayTao: '01/09/2024', lanDangNhapCuoi: '11/06/2026 18:00' },
  { maNguoiDung: 16, maDonVi: 1, email: 'phamminh@lms.edu.vn', hoTen: 'Phạm Văn Minh', vaiTroChinh: 'hoc_sinh', tenVaiTro: 'Sinh viên', tenDonVi: 'Khoa Thiết kế', soDienThoai: '0909 123 014', trangThai: 'bi_khoa', namNhapHoc: 2023, ngayTao: '01/09/2023', lanDangNhapCuoi: '01/04/2026 10:00' },
  { maNguoiDung: 17, maDonVi: 2, email: 'nguyenvanhieu@dn.lms.edu.vn', hoTen: 'Nguyễn Văn Hiếu', vaiTroChinh: 'quan_tri_co_so', tenVaiTro: 'Quản trị cơ sở', tenDonVi: 'FPT Polytechnic Đà Nẵng', soDienThoai: '0909 123 015', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2022', lanDangNhapCuoi: '13/06/2026 08:00' },
  { maNguoiDung: 18, maDonVi: 1, email: 'nguyenkhanh@lms.edu.vn', hoTen: 'TS. Nguyễn Khắc Anh', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 016', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '15/08/2019', lanDangNhapCuoi: '13/06/2026 11:00' },
  { maNguoiDung: 19, maDonVi: 1, email: 'levanbinh@lms.edu.vn', hoTen: 'Lê Văn Bình', vaiTroChinh: 'giao_vien', tenVaiTro: 'Giảng viên', tenDonVi: 'Khoa Công nghệ thông tin', soDienThoai: '0909 123 017', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2024', lanDangNhapCuoi: '10/06/2026 09:30' },
  { maNguoiDung: 20, maDonVi: 1, email: 'hoangthianh@lms.edu.vn', hoTen: 'Hoàng Thị Ánh', vaiTroChinh: 'ke_toan_co_so', tenVaiTro: 'Kế toán cơ sở', tenDonVi: 'FPT Polytechnic Hồ Chí Minh', soDienThoai: '0909 123 018', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/06/2023', lanDangNhapCuoi: '12/06/2026 16:00' },
  { maNguoiDung: 21, maDonVi: 1, email: 'tranhoang@lms.edu.vn', hoTen: 'Trần Hoàng', vaiTroChinh: 'chu_tich', tenVaiTro: 'Chủ tịch hệ thống', tenDonVi: 'FPT Polytechnic Hồ Chí Minh', soDienThoai: '0909 123 019', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2019', lanDangNhapCuoi: '12/06/2026 13:00' },
  { maNguoiDung: 22, maDonVi: 3, email: 'adminct@lms.edu.vn', hoTen: 'Admin Cần Thơ', vaiTroChinh: 'quan_tri_co_so', tenVaiTro: 'Quản trị cơ sở', tenDonVi: 'FPT Polytechnic Cần Thơ', soDienThoai: '0909 123 020', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/01/2023', lanDangNhapCuoi: '11/06/2026 08:30' },
  { maNguoiDung: 23, maDonVi: 1, email: 'nguyenhong@lms.edu.vn', hoTen: 'Nguyễn Hồng', vaiTroChinh: 'admin_tai_chinh', tenVaiTro: 'Admin tài chính', tenDonVi: 'FPT Polytechnic Hồ Chí Minh', soDienThoai: '0909 123 021', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/03/2022', lanDangNhapCuoi: '10/06/2026 14:30' },
  { maNguoiDung: 24, maDonVi: 1, email: 'buithanh@lms.edu.vn', hoTen: 'Bùi Thanh', vaiTroChinh: 'nhan_vien', tenVaiTro: 'Giáo vụ', tenDonVi: 'Phòng Đào tạo', soDienThoai: '0909 123 022', trangThai: 'hoat_dong', namNhapHoc: null, ngayTao: '01/06/2023', lanDangNhapCuoi: '13/06/2026 07:30' },
]

const filteredUsers = computed(() => {
  return mockUsers.filter(u => {
    if (keyword.value) {
      const kw = keyword.value.toLowerCase()
      if (!u.hoTen.toLowerCase().includes(kw) && !u.email.toLowerCase().includes(kw) && !(u.soDienThoai || '').includes(kw)) return false
    }
    if (roleFilter.value && u.vaiTroChinh !== roleFilter.value) return false
    if (statusFilter.value && u.trangThai !== statusFilter.value) return false
    return true
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredUsers.value.length / pageSize)))

const pagedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredUsers.value.slice(start, start + pageSize)
})

function handleFilter() { currentPage.value = 1 }
function prevPage() { if (currentPage.value > 1) currentPage.value-- }
function nextPage() { if (currentPage.value < totalPages.value) currentPage.value++ }

function openCreateModal() {
  modalMode.value = 'create'
  formData.value = { maNguoiDung: null, hoTen: '', email: '', soDienThoai: '', matKhau: '', maCodeVaiTro: '', maDonVi: '' }
  apiError.value = ''
  showModal.value = true
}

function openEditModal(user) {
  modalMode.value = 'edit'
  apiError.value = ''
  formData.value = {
    maNguoiDung: user.maNguoiDung,
    hoTen: user.hoTen,
    email: user.email,
    soDienThoai: user.soDienThoai || '',
    matKhau: '',
    maCodeVaiTro: user.vaiTroChinh,
    maDonVi: user.maDonVi,
  }
  showModal.value = true
}

function closeModal() { showModal.value = false }

function submitForm() {
  if (!formData.value.hoTen || !formData.value.email || !formData.value.maCodeVaiTro || !formData.value.maDonVi) {
    apiError.value = 'Vui lòng điền đầy đủ các trường bắt buộc (*).'
    return
  }
  if (modalMode.value === 'create' && !formData.value.matKhau) {
    apiError.value = 'Vui lòng nhập mật khẩu.'
    return
  }
  apiError.value = ''
  if (modalMode.value === 'edit') {
    const user = mockUsers.find(u => u.maNguoiDung === formData.value.maNguoiDung)
    if (user) {
      user.hoTen = formData.value.hoTen
      user.email = formData.value.email
      user.soDienThoai = formData.value.soDienThoai
      user.vaiTroChinh = formData.value.maCodeVaiTro
      user.tenVaiTro = roleCodeToName[formData.value.maCodeVaiTro] || formData.value.maCodeVaiTro
      user.maDonVi = parseInt(formData.value.maDonVi)
      user.tenDonVi = orgsList.find(o => o.maDonVi === parseInt(formData.value.maDonVi))?.tenDonVi || ''
    }
  }
  closeModal()
}

function handleToggleLock(user) {
  const isLocking = user.trangThai === 'hoat_dong'
  if (!confirm(`Bạn có chắc chắn muốn ${isLocking ? 'khóa' : 'mở khóa'} tài khoản ${user.email}?`)) return
  user.trangThai = isLocking ? 'bi_khoa' : 'hoat_dong'
}

function handleResetPassword(user) {
  const newPassword = prompt(`Nhập mật khẩu mới cho ${user.email} (tối thiểu 8 ký tự):`)
  if (!newPassword || newPassword.length < 8) return
  alert('Đặt lại mật khẩu thành công!')
}
</script>
