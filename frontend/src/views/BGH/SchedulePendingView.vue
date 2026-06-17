<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h1 class="text-xl font-bold text-heading">Duyệt Thời khóa biểu</h1>
        <p class="text-xs text-muted mt-1">Phê duyệt thời khóa biểu trước khi công bố</p>
      </div>
      <div class="flex gap-2">
        <select v-model="campusFilter" class="px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)]">
          <option value="">Tất cả cơ sở</option>
          <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
        </select>
      </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4 flex-1">
      <div class="xl:col-span-2 space-y-4 overflow-y-auto">
        <div v-for="item in filteredSchedules" :key="item.id" class="surface-card border border-card rounded-2xl p-5 shadow-sm hover:shadow-md transition-all">
          <div class="flex items-start justify-between gap-4">
            <div class="flex items-start gap-3 flex-1 min-w-0">
              <div class="h-10 w-10 rounded-xl bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-white font-bold text-sm shrink-0">{{ item.initials }}</div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <h3 class="text-base font-bold text-heading">{{ item.tenHocKy }}</h3>
                  <span :class="urgencyBadge(item.doUrgent)" class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider">
                    {{ item.doUrgent === 'cao' ? 'Khẩn' : item.doUrgent === 'trung_binh' ? 'Thường' : 'Thấp' }}
                  </span>
                </div>
                <p class="text-sm text-muted mt-0.5">{{ item.tenDonVi }} · {{ item.ngayTao }}</p>
                <div class="flex flex-wrap gap-3 mt-2 text-xs text-muted">
                  <span class="flex items-center gap-1"><BookOpen :size="13" /> {{ item.soLop }} lớp</span>
                  <span class="flex items-center gap-1"><Users :size="13" /> {{ item.soGiaoVien }} GV</span>
                  <span class="flex items-center gap-1"><Clock :size="13" /> {{ item.tongSoTiet }} tiết/tuần</span>
                </div>
              </div>
            </div>
            <div class="flex items-center gap-2 shrink-0">
              <button @click="approve(item)" class="flex items-center gap-1 px-3 py-1.5 bg-[var(--color-success-bg)] text-[var(--color-success-text)] rounded-lg text-xs font-bold hover:brightness-90 transition-all">
                <CheckCircle2 :size="14" /> Duyệt
              </button>
              <button @click="reject(item)" class="flex items-center gap-1 px-3 py-1.5 bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] rounded-lg text-xs font-bold hover:brightness-90 transition-all">
                <XCircle :size="14" /> Từ chối
              </button>
            </div>
          </div>

          <div class="mt-4 pt-3 border-t border-default">
            <div class="flex items-center justify-between mb-2">
              <span class="text-xs font-bold text-heading">Xung đột phát hiện</span>
              <span v-if="item.xungDot > 0" class="text-xs text-[var(--color-danger-text)] bg-[var(--color-danger-bg)] px-2 py-0.5 rounded-full font-bold">{{ item.xungDot }} xung đột</span>
              <span v-else class="text-xs text-[var(--color-success-text)] bg-[var(--color-success-bg)] px-2 py-0.5 rounded-full font-bold">Không có xung đột</span>
            </div>
            <div v-if="item.conflicts && item.conflicts.length > 0" class="space-y-1.5">
              <div v-for="(conf, i) in item.conflicts" :key="i" class="flex items-center gap-2 text-xs text-[var(--color-danger-text)] bg-[var(--color-danger-bg)]/30 p-2 rounded-lg">
                <AlertTriangle :size="13" class="shrink-0" />
                <span>{{ conf }}</span>
              </div>
            </div>
            <div v-else class="text-xs text-muted italic">Không phát hiện xung đột.</div>
          </div>
        </div>

        <div v-if="filteredSchedules.length === 0" class="text-center py-12 text-muted">
          <CalendarCheck :size="40" class="mx-auto mb-3 opacity-50" />
          <p>Không có thời khóa biểu nào chờ duyệt.</p>
        </div>
      </div>

      <div class="space-y-4">
        <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
          <h3 class="text-base font-bold text-heading mb-3 flex items-center gap-2">
            <BarChart3 :size="18" class="text-[var(--lg-primary)]" />
            Thống kê
          </h3>
          <div class="space-y-3">
            <div class="flex justify-between text-sm">
              <span class="text-muted">Chờ duyệt</span>
              <span class="font-bold text-heading">{{ pendingCount }}</span>
            </div>
            <div class="flex justify-between text-sm">
              <span class="text-muted">Đã duyệt hôm nay</span>
              <span class="font-bold text-[var(--color-success-text)]">3</span>
            </div>
            <div class="flex justify-between text-sm">
              <span class="text-muted">Từ chối hôm nay</span>
              <span class="font-bold text-[var(--color-danger-text)]">1</span>
            </div>
            <div class="flex justify-between text-sm pt-2 border-t border-default">
              <span class="text-muted">Tổng số lớp trong kỳ</span>
              <span class="font-bold text-heading">286</span>
            </div>
          </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
          <h3 class="text-base font-bold text-heading mb-3">Lịch sử duyệt gần đây</h3>
          <div class="space-y-3">
            <div v-for="log in approvalHistory" :key="log.time" class="flex gap-2 text-xs">
              <div class="flex flex-col items-center">
                <div :class="log.action === 'approved' ? 'bg-[var(--color-success-text)]' : 'bg-[var(--color-danger-text)]'" class="h-2 w-2 rounded-full mt-1" />
                <div class="flex-1 w-px bg-default" />
              </div>
              <div>
                <p class="font-bold text-heading">{{ log.tenHocKy }}</p>
                <p class="text-muted">{{ log.time }} bởi {{ log.nguoiDuyet }}</p>
                <span :class="log.action === 'approved' ? 'text-[var(--color-success-text)]' : 'text-[var(--color-danger-text)]'" class="font-bold">
                  {{ log.action === 'approved' ? 'Đã duyệt' : 'Đã từ chối' }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="actionModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" @click.self="actionModal = false">
      <div class="w-full max-w-md surface-card rounded-2xl shadow-2xl border border-default overflow-hidden">
        <div class="p-4 border-b border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-heading">{{ actionType === 'approve' ? 'Xác nhận duyệt' : 'Từ chối' }} TKB</h3>
          <button @click="actionModal = false" class="p-1 hover:bg-[var(--surface-input)] rounded-lg text-muted">
            <X :size="20" />
          </button>
        </div>
        <div class="p-6 space-y-4">
          <p class="text-sm text-body">{{ actionType === 'approve' ? 'Bạn có chắc muốn duyệt thời khóa biểu' : 'Nhập lý do từ chối thời khóa biểu' }} <strong class="text-heading">{{ actionTarget?.tenHocKy }}</strong>?</p>
          <div v-if="actionType === 'reject'" class="space-y-1">
            <label class="block text-xs font-bold text-heading">Lý do từ chối <span class="text-[var(--color-danger-text)]">*</span></label>
            <textarea v-model="rejectReason" rows="3" placeholder="Nhập lý do từ chối..." class="w-full px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)] resize-none"></textarea>
          </div>
        </div>
        <div class="p-4 border-t border-default flex justify-end gap-3">
          <button @click="actionModal = false" class="px-4 py-2 border border-input rounded-lg text-sm font-bold text-body hover:bg-[var(--surface-input)] transition-colors">Hủy</button>
          <button @click="confirmAction" :class="actionType === 'approve' ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] hover:brightness-90' : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] hover:brightness-90'" class="px-4 py-2 text-sm font-bold rounded-lg transition-all">
            {{ actionType === 'approve' ? 'Xác nhận duyệt' : 'Từ chối' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import {
  BookOpen, Users, Clock, CheckCircle2, XCircle,
  AlertTriangle, CalendarCheck, BarChart3, X
} from 'lucide-vue-next'

const campusFilter = ref('')
const actionModal = ref(false)
const actionType = ref('approve')
const actionTarget = ref(null)
const rejectReason = ref('')

const campuses = ['FPT Polytechnic Hồ Chí Minh', 'FPT Polytechnic Đà Nẵng']

const pendingSchedules = [
  {
    id: 1, initials: 'CN', tenDonVi: 'FPT Polytechnic Hồ Chí Minh',
    tenHocKy: 'Spring 2026 - Khoa CNTT', ngayTao: 'Gửi: 12/06/2026 bởi Nguyễn Văn A (Giáo vụ)',
    soLop: 42, soGiaoVien: 18, tongSoTiet: 210, doUrgent: 'cao',
    xungDot: 2,
    conflicts: ['Phòng PTO.101 trùng lịch: PRO101 (T2 - 7h30) vs WEB101 (T2 - 7h30)', 'GV Nguyễn Văn B dạy 2 lớp cùng giờ (T3 - 9h30)'],
  },
  {
    id: 2, initials: 'KT', tenDonVi: 'FPT Polytechnic Hồ Chí Minh',
    tenHocKy: 'Spring 2026 - Khoa Kinh Tế', ngayTao: 'Gửi: 11/06/2026 bởi Trần Thị C (Giáo vụ)',
    soLop: 28, soGiaoVien: 12, tongSoTiet: 140, doUrgent: 'trung_binh',
    xungDot: 0, conflicts: [],
  },
  {
    id: 3, initials: 'TK', tenDonVi: 'FPT Polytechnic Hồ Chí Minh',
    tenHocKy: 'Spring 2026 - Khoa Thiết Kế', ngayTao: 'Gửi: 11/06/2026 bởi Lê Văn D (Giáo vụ)',
    soLop: 18, soGiaoVien: 8, tongSoTiet: 90, doUrgent: 'thap',
    xungDot: 1,
    conflicts: ['Phòng Lab PTO.203 trùng lịch: Đồ họa 2D (T5 - 13h30) vs Đồ họa 3D (T5 - 13h30)'],
  },
  {
    id: 4, initials: 'ĐN', tenDonVi: 'FPT Polytechnic Đà Nẵng',
    tenHocKy: 'Spring 2026 - Đà Nẵng', ngayTao: 'Gửi: 10/06/2026 bởi Phạm Văn E (Giáo vụ)',
    soLop: 22, soGiaoVien: 10, tongSoTiet: 110, doUrgent: 'cao',
    xungDot: 0, conflicts: [],
  },
]

const approvalHistory = [
  { tenHocKy: 'Summer 2025 - Khoa CNTT', time: '15/05/2025 10:30', nguoiDuyet: 'Nguyễn Văn Hiệu Trưởng', action: 'approved' },
  { tenHocKy: 'Fall 2025 - Khoa CNTT', time: '20/08/2025 14:00', nguoiDuyet: 'Nguyễn Văn Hiệu Trưởng', action: 'approved' },
  { tenHocKy: 'Fall 2025 - Khoa Kinh Tế', time: '21/08/2025 09:15', nguoiDuyet: 'Nguyễn Văn Hiệu Trưởng', action: 'rejected' },
  { tenHocKy: 'Spring 2026 - Đà Nẵng', time: '05/01/2026 08:00', nguoiDuyet: 'Nguyễn Văn Hiệu Trưởng', action: 'approved' },
]

const filteredSchedules = computed(() => {
  if (!campusFilter.value) return pendingSchedules
  return pendingSchedules.filter(s => s.tenDonVi === campusFilter.value)
})

const pendingCount = computed(() => filteredSchedules.value.length)

function urgencyBadge(level) {
  switch (level) {
    case 'cao': return 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]'
    case 'trung_binh': return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]'
    default: return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)]'
  }
}

function approve(item) {
  actionType.value = 'approve'
  actionTarget.value = item
  actionModal.value = true
}

function reject(item) {
  actionType.value = 'reject'
  actionTarget.value = item
  rejectReason.value = ''
  actionModal.value = true
}

function confirmAction() {
  if (actionType.value === 'reject' && !rejectReason.value.trim()) return
  actionModal.value = false
}
</script>
