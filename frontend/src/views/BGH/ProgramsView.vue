<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h1 class="text-xl font-bold text-heading">Ngành & Chuyên ngành</h1>
        <p class="text-xs text-muted mt-1">Chương trình đào tạo theo chuyên ngành và khóa tuyển sinh</p>
      </div>
      <div class="flex gap-2">
        <select v-model="statusFilter" class="px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)]">
          <option value="">Tất cả trạng thái</option>
          <option value="active">Đang hoạt động</option>
          <option value="draft">Bản thảo</option>
          <option value="pending_approval">Chờ duyệt</option>
          <option value="approved">Đã duyệt</option>
          <option value="archived">Đã lưu trữ</option>
        </select>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-4">
      <div v-for="prog in filteredPrograms" :key="prog.maChuongTrinh" class="surface-card border border-card rounded-2xl p-5 shadow-sm hover:shadow-md transition-all">
        <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
          <div class="flex items-start gap-4">
            <div class="h-12 w-12 rounded-2xl bg-gradient-to-br from-indigo-500 to-indigo-400 flex items-center justify-center text-white font-bold text-sm shrink-0">
              {{ prog.maCodeChuongTrinh }}
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <h3 class="text-base font-bold text-heading truncate">{{ prog.tenChuongTrinh }}</h3>
                <span :class="statusBadge(prog.trangThai)" class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider">
                  <component :is="statusIcon(prog.trangThai)" :size="12" />
                  {{ statusLabel(prog.trangThai) }}
                </span>
              </div>
              <div class="flex flex-wrap gap-x-4 gap-y-1 mt-2 text-xs text-muted">
                <span class="flex items-center gap-1"><BookOpen :size="13" /> {{ prog.tenChuyenNganh }}</span>
                <span class="flex items-center gap-1"><Users :size="13" /> {{ prog.tenKhoa }}</span>
                <span class="flex items-center gap-1"><Layers :size="13" /> {{ prog.soHocKy }} học kỳ</span>
                <span class="flex items-center gap-1"><BookMarked :size="13" /> {{ prog.tongTinChiYeuCau }} tín chỉ</span>
                <span class="flex items-center gap-1"><Clock :size="13" /> {{ prog.thoiGianDaoTaoThang }} tháng</span>
              </div>
            </div>
          </div>
          <div class="flex items-center gap-2 shrink-0">
            <button @click="toggleExpand(prog.maChuongTrinh)" class="flex items-center gap-1 px-3 py-1.5 border border-input rounded-lg text-xs font-bold text-body hover:bg-[var(--surface-input)] transition-colors">
              <ChevronDown v-if="expandedId === prog.maChuongTrinh" :size="14" />
              <ChevronRight v-else :size="14" />
              Chi tiết
            </button>
          </div>
        </div>

        <Transition enter-active-class="transition-all duration-200" enter-from-class="max-h-0 opacity-0" enter-to-class="max-h-[500px] opacity-100" leave-active-class="transition-all duration-200" leave-from-class="max-h-[500px] opacity-100" leave-to-class="max-h-0 opacity-0">
          <div v-if="expandedId === prog.maChuongTrinh" class="mt-4 pt-4 border-t border-default overflow-hidden">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
              <div class="space-y-2">
                <div class="flex justify-between"><span class="text-muted">Mã chương trình:</span><span class="font-bold text-heading">{{ prog.maCodeChuongTrinh }}</span></div>
                <div class="flex justify-between"><span class="text-muted">Phiên bản:</span><span class="font-bold text-heading">{{ prog.version }}</span></div>
                <div class="flex justify-between"><span class="text-muted">Ngày hiệu lực:</span><span class="font-bold text-heading">{{ prog.ngayHieuLuc || '—' }}</span></div>
                <div class="flex justify-between"><span class="text-muted">Ngày hết hiệu lực:</span><span class="font-bold text-heading">{{ prog.ngayHetHieuLuc || '—' }}</span></div>
              </div>
              <div class="space-y-2">
                <div class="flex justify-between"><span class="text-muted">Người gửi duyệt:</span><span class="font-bold text-heading">{{ prog.nguoiGuiDuyet || '—' }}</span></div>
                <div class="flex justify-between"><span class="text-muted">Người duyệt:</span><span class="font-bold text-heading">{{ prog.nguoiDuyet || '—' }}</span></div>
                <div class="flex justify-between"><span class="text-muted">Ngày tạo:</span><span class="font-bold text-heading">{{ prog.ngayTao }}</span></div>
                <div class="flex justify-between"><span class="text-muted">Mô tả:</span><span class="font-bold text-heading text-right max-w-[200px]">{{ prog.moTa || '—' }}</span></div>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <div v-if="filteredPrograms.length === 0" class="text-center py-12 text-muted">
        <BookOpen :size="40" class="mx-auto mb-3 opacity-50" />
        <p>Không tìm thấy chương trình đào tạo nào.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import {
  BookOpen, Users, Layers, BookMarked, Clock,
  ChevronDown, ChevronRight, FileText, CheckCircle2,
  AlertCircle, Archive, Eye
} from 'lucide-vue-next'

const statusFilter = ref('')
const expandedId = ref(null)

function toggleExpand(id) {
  expandedId.value = expandedId.value === id ? null : id
}

const trainingPrograms = [
  { maChuongTrinh: 1, maCodeChuongTrinh: 'CNTT-K19', tenChuongTrinh: 'Công nghệ thông tin - Chuyên ngành PT Ứng dụng', tenChuyenNganh: 'Công nghệ thông tin (Ứng dụng phần mềm)', tenKhoa: 'Khóa 19 (2024-2027)', version: '1.0', soHocKy: 7, thoiGianDaoTaoThang: 28, tongTinChiYeuCau: 135, trangThai: 'active', moTa: 'Chương trình đào tạo cử nhân Công nghệ thông tin chuyên ngành Ứng dụng phần mềm.', ngayHieuLuc: '01/09/2024', ngayHetHieuLuc: null, nguoiGuiDuyet: 'Nguyễn Văn A', nguoiDuyet: 'Trần Thị B', ngayTao: '15/05/2024', conHoatDong: true },
  { maChuongTrinh: 2, maCodeChuongTrinh: 'CNTT-K20', tenChuongTrinh: 'Công nghệ thông tin - Chuyên ngành PT Ứng dụng', tenChuyenNganh: 'Công nghệ thông tin (Ứng dụng phần mềm)', tenKhoa: 'Khóa 20 (2025-2028)', version: '1.1', soHocKy: 7, thoiGianDaoTaoThang: 28, tongTinChiYeuCau: 138, trangThai: 'pending_approval', moTa: 'Phiên bản cập nhật chương trình CNTT khóa 20 với điều chỉnh 3 tín chỉ.', ngayHieuLuc: null, ngayHetHieuLuc: null, nguoiGuiDuyet: 'Nguyễn Văn A', nguoiDuyet: null, ngayTao: '10/03/2025', conHoatDong: true },
  { maChuongTrinh: 3, maCodeChuongTrinh: 'KT-K19', tenChuongTrinh: 'Quản trị Kinh doanh - Chuyên ngành Kế toán', tenChuyenNganh: 'Quản trị Kinh doanh (Kế toán)', tenKhoa: 'Khóa 19 (2024-2027)', version: '1.0', soHocKy: 6, thoiGianDaoTaoThang: 24, tongTinChiYeuCau: 120, trangThai: 'active', moTa: 'Chương trình đào tạo cử nhân Quản trị Kinh doanh chuyên ngành Kế toán Doanh nghiệp.', ngayHieuLuc: '01/09/2024', ngayHetHieuLuc: null, nguoiGuiDuyet: 'Lê Thị C', nguoiDuyet: 'Trần Thị B', ngayTao: '20/05/2024', conHoatDong: true },
  { maChuongTrinh: 4, maCodeChuongTrinh: 'TK-K19', tenChuongTrinh: 'Thiết kế Đồ họa - Chuyên ngành Mỹ thuật', tenChuyenNganh: 'Thiết kế Đồ họa', tenKhoa: 'Khóa 19 (2024-2027)', version: '2.0', soHocKy: 6, thoiGianDaoTaoThang: 24, tongTinChiYeuCau: 126, trangThai: 'approved', moTa: 'Chương trình đào tạo cử nhân Thiết kế Đồ họa phiên bản 2.0.', ngayHieuLuc: '01/09/2025', ngayHetHieuLuc: null, nguoiGuiDuyet: 'Phạm Văn D', nguoiDuyet: 'Trần Thị B', ngayTao: '01/08/2024', conHoatDong: true },
  { maChuongTrinh: 5, maCodeChuongTrinh: 'CNTT-K18', tenChuongTrinh: 'Công nghệ thông tin - Chuyên ngành PT Ứng dụng', tenChuyenNganh: 'Công nghệ thông tin (Ứng dụng phần mềm)', tenKhoa: 'Khóa 18 (2023-2026)', version: '1.0', soHocKy: 7, thoiGianDaoTaoThang: 28, tongTinChiYeuCau: 135, trangThai: 'archived', moTa: 'Chương trình khóa 18 đã kết thúc và được lưu trữ.', ngayHieuLuc: '01/09/2023', ngayHetHieuLuc: '31/08/2026', nguoiGuiDuyet: 'Nguyễn Văn A', nguoiDuyet: 'Trần Thị B', ngayTao: '01/03/2023', conHoatDong: false },
  { maChuongTrinh: 6, maCodeChuongTrinh: 'QTKS-K19', tenChuongTrinh: 'Quản trị Khách sạn - Chuyên ngành Nhà hàng', tenChuyenNganh: 'Quản trị Khách sạn', tenKhoa: 'Khóa 19 (2024-2027)', version: '1.0', soHocKy: 6, thoiGianDaoTaoThang: 24, tongTinChiYeuCau: 122, trangThai: 'draft', moTa: 'Chương trình đang được xây dựng, chưa gửi duyệt.', ngayHieuLuc: null, ngayHetHieuLuc: null, nguoiGuiDuyet: null, nguoiDuyet: null, ngayTao: '05/06/2025', conHoatDong: true },
]

const filteredPrograms = computed(() => {
  if (!statusFilter.value) return trainingPrograms
  return trainingPrograms.filter(p => p.trangThai === statusFilter.value)
})

function statusBadge(status) {
  switch (status) {
    case 'active': return 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
    case 'draft': return 'bg-[var(--surface-input)] text-muted'
    case 'pending_approval': return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]'
    case 'approved': return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)]'
    case 'archived': return 'bg-[var(--color-danger-bg)]/50 text-muted'
    default: return 'bg-[var(--surface-input)] text-muted'
  }
}

function statusIcon(status) {
  switch (status) {
    case 'active': return CheckCircle2
    case 'draft': return FileText
    case 'pending_approval': return AlertCircle
    case 'approved': return Eye
    case 'archived': return Archive
    default: return FileText
  }
}

function statusLabel(status) {
  switch (status) {
    case 'active': return 'Đang áp dụng'
    case 'draft': return 'Bản thảo'
    case 'pending_approval': return 'Chờ duyệt'
    case 'approved': return 'Đã duyệt'
    case 'archived': return 'Lưu trữ'
    default: return status
  }
}
</script>
