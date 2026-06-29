<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { scheduleAttendanceMockService } from '@/mocks/scheduleAttendanceMockService'
import ScheduleMockBanner from './ScheduleMockBanner.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import AttendanceStatusBadge from './AttendanceStatusBadge.vue'
import { ArrowLeft, Check, X, Clock, UserCheck, Lock } from 'lucide-vue-next'
import dayjs from 'dayjs'

const route = useRoute()
const router = useRouter()
const sessionId = route.params.id

const session = ref(null)
const students = ref([])
const loading = ref(false)
const submitting = ref(false)

const loadData = async () => {
  loading.value = true
  try {
    const res = await scheduleAttendanceMockService.getAttendanceSession(sessionId)
    session.value = res.session
    students.value = res.attendance
  } finally {
    loading.value = false
  }
}

onMounted(() => loadData())

const setStatus = (student, status) => {
  if (session.value?.daKhoaDiemDanh) return
  student.trangThai = status
}

const markAll = (status) => {
  if (session.value?.daKhoaDiemDanh) return
  students.value.forEach(s => setStatus(s, status))
}

const submitAttendance = async () => {
  submitting.value = true
  try {
    await scheduleAttendanceMockService.submitAttendance(sessionId)
    alert('Điểm danh thành công!')
    router.back()
  } finally {
    submitting.value = false
  }
}

const reqUnlock = async () => {
  await scheduleAttendanceMockService.requestAttendanceUnlock(sessionId)
  alert('Đã gửi yêu cầu mở khóa (Mock).')
}

const summary = computed(() => {
  let coMat = 0, diMuon = 0, coPhep = 0, vang = 0, total = students.value.length
  students.value.forEach(s => {
    if(s.trangThai === 'co_mat') coMat++
    else if(s.trangThai === 'di_muon') diMuon++
    else if(s.trangThai === 'co_phep') coPhep++
    else if(s.trangThai === 'vang') vang++
  })
  return { coMat, diMuon, coPhep, vang, total }
})
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar flex flex-col">
    <ScheduleMockBanner />
    <div class="flex items-center gap-4 mb-6">
      <button @click="router.back()" class="p-2 hover:bg-[var(--surface-hover)] rounded-full text-[var(--text-muted)]">
        <ArrowLeft class="w-5 h-5" />
      </button>
      <h1 class="text-2xl font-bold text-[var(--text-heading)]">Điểm danh buổi học</h1>
    </div>

    <div v-if="loading" class="animate-pulse bg-[var(--surface-card)] h-64 rounded-xl"></div>
    <div v-else-if="session" class="flex-1 flex flex-col">
      
      <!-- Session Info -->
      <GlassPanel padding="normal" class="mb-6 flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h2 class="text-lg font-bold text-[var(--text-heading)]">{{ session.tenMon }} ({{ session.lop }})</h2>
          <p class="text-sm text-[var(--text-muted)] mt-1">{{ dayjs(session.ngayHoc).format('DD/MM/YYYY') }} | Ca: {{ session.caHoc }} | Phòng: {{ session.phong }}</p>
        </div>
        <div class="flex gap-4">
          <div class="text-center px-4 border-r border-[var(--border-card)]">
            <div class="text-xs text-[var(--text-muted)]">Tổng</div>
            <div class="text-xl font-bold text-[var(--text-heading)]">{{ summary.total }}</div>
          </div>
          <div class="text-center px-4 border-r border-[var(--border-card)]">
            <div class="text-xs text-[var(--text-muted)]">Có mặt</div>
            <div class="text-xl font-bold text-[var(--color-success-text)]">{{ summary.coMat }}</div>
          </div>
          <div class="text-center px-4">
            <div class="text-xs text-[var(--text-muted)]">Vắng</div>
            <div class="text-xl font-bold text-[var(--color-danger-text)]">{{ summary.vang }}</div>
          </div>
        </div>
      </GlassPanel>

      <!-- Lock Banner -->
      <div v-if="session.daKhoaDiemDanh" class="bg-[var(--color-danger-bg)] border border-[var(--color-danger-border)] p-4 rounded-lg flex items-center justify-between mb-6">
        <div class="flex items-center gap-2 text-[var(--color-danger-text)]">
          <Lock class="w-5 h-5" />
          <span class="font-medium">Buổi học này đã bị khóa điểm danh (quá hạn).</span>
        </div>
        <GlassButton variant="primary" size="sm" @click="reqUnlock">Yêu cầu mở khóa (Mock)</GlassButton>
      </div>

      <!-- Toolbar -->
      <div class="flex justify-between items-center mb-4" v-if="!session.daKhoaDiemDanh">
        <div class="flex gap-2">
          <GlassButton variant="success" size="sm" @click="markAll('co_mat')">Đánh dấu tất cả có mặt</GlassButton>
          <GlassButton variant="danger" size="sm" @click="markAll('vang')">Đánh dấu tất cả vắng</GlassButton>
        </div>
      </div>

      <!-- Grid -->
      <GlassPanel padding="none" class="flex-1 overflow-hidden mb-6 flex flex-col">
        <div class="overflow-y-auto custom-scrollbar flex-1">
          <table class="w-full text-sm text-left">
            <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs sticky top-0 z-10">
              <tr>
                <th class="px-4 py-3">MSSV</th>
                <th class="px-4 py-3">Họ Tên</th>
                <th class="px-4 py-3">Trạng thái</th>
                <th class="px-4 py-3 text-right">Ghi chú</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-[var(--border-card)]">
              <tr v-for="stu in students" :key="stu.maSinhVien" class="hover:bg-[var(--surface-hover)]">
                <td class="px-4 py-3 font-medium">{{ stu.maSinhVien }}</td>
                <td class="px-4 py-3 font-medium text-[var(--text-heading)]">{{ stu.hoTen }}</td>
                <td class="px-4 py-3">
                  <div class="flex gap-2" v-if="!session.daKhoaDiemDanh">
                    <button @click="setStatus(stu, 'co_mat')" class="px-3 py-1 rounded border transition-colors" :class="stu.trangThai === 'co_mat' ? 'bg-[var(--color-success-bg)] border-[var(--color-success-border)] text-[var(--color-success-text)]' : 'border-[var(--border-card)] text-[var(--text-muted)]'">C</button>
                    <button @click="setStatus(stu, 'di_muon')" class="px-3 py-1 rounded border transition-colors" :class="stu.trangThai === 'di_muon' ? 'bg-[var(--color-warning-bg)] border-[var(--color-warning-border)] text-[var(--color-warning-text)]' : 'border-[var(--border-card)] text-[var(--text-muted)]'">M</button>
                    <button @click="setStatus(stu, 'co_phep')" class="px-3 py-1 rounded border transition-colors" :class="stu.trangThai === 'co_phep' ? 'bg-blue-100 border-blue-200 text-blue-700 dark:bg-blue-900/30 dark:border-blue-800' : 'border-[var(--border-card)] text-[var(--text-muted)]'">P</button>
                    <button @click="setStatus(stu, 'vang')" class="px-3 py-1 rounded border transition-colors" :class="stu.trangThai === 'vang' ? 'bg-[var(--color-danger-bg)] border-[var(--color-danger-border)] text-[var(--color-danger-text)]' : 'border-[var(--border-card)] text-[var(--text-muted)]'">V</button>
                  </div>
                  <AttendanceStatusBadge v-else :status="stu.trangThai" />
                </td>
                <td class="px-4 py-3 text-right">
                  <input v-if="!session.daKhoaDiemDanh" v-model="stu.ghiChu" type="text" class="w-full bg-[var(--surface-input)] border border-[var(--border-input)] rounded px-2 py-1 outline-none text-[var(--text-body)]" placeholder="Ghi chú...">
                  <span v-else class="text-[var(--text-muted)]">{{ stu.ghiChu || '--' }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </GlassPanel>

      <!-- Submit -->
      <div class="flex justify-end gap-3" v-if="!session.daKhoaDiemDanh">
        <GlassButton variant="secondary" @click="router.back()">Hủy</GlassButton>
        <GlassButton variant="primary" :loading="submitting" @click="submitAttendance">Lưu điểm danh</GlassButton>
      </div>

    </div>
  </div>
</template>
