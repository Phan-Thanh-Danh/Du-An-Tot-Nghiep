<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import {
  Search,
  Filter,
  XCircle,
  RefreshCw,
  History,
  Bell,
  X,
  CheckCircle2,
  Clock,
  User,
  MapPin,
  BookOpen,
  AlertTriangle,
  Send,
  Calendar,
  SlidersHorizontal,
  FileText,
  Undo2,
  Info,
  RotateCcw,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const publishedSchedules = ref([
  { id: 1, subject: 'Lập trình Java', class: 'SE1601', teacher: 'Nguyễn Văn A', room: 'P.302', time: '07:30 - 10:30', day: 'Thứ 2', date: '02/06/2026', status: 'published' },
  { id: 2, subject: 'Cấu trúc dữ liệu', class: 'SE1602', teacher: 'Trần Thị B', room: 'P.105', time: '13:30 - 15:30', day: 'Thứ 3', date: '03/06/2026', status: 'published' },
  { id: 3, subject: 'Hệ quản trị CSDL', class: 'SE1603', teacher: 'Lê Văn C', room: 'Lab 2', time: '08:30 - 11:30', day: 'Thứ 4', date: '04/06/2026', status: 'published' },
  { id: 4, subject: 'Mạng máy tính', class: 'SE1604', teacher: 'Phạm Minh Tuấn', room: 'P.401', time: '13:30 - 16:30', day: 'Thứ 5', date: '05/06/2026', status: 'published' },
  { id: 5, subject: 'Lập trình Web', class: 'SE1605', teacher: 'Nguyễn Văn A', room: 'Lab 1', time: '07:30 - 10:30', day: 'Thứ 6', date: '06/06/2026', status: 'published' },
  { id: 6, subject: 'Cơ sở dữ liệu', class: 'SE1601', teacher: 'Trần Thị B', room: 'P.302', time: '09:30 - 11:00', day: 'Thứ 7', date: '07/06/2026', status: 'published' },
  { id: 7, subject: 'Toán rời rạc', class: 'SE1602', teacher: 'Lê Văn C', room: 'P.201', time: '14:00 - 16:30', day: 'Thứ 2', date: '09/06/2026', status: 'published' },
  { id: 8, subject: 'Kỹ thuật phần mềm', class: 'SE1603', teacher: 'Nguyễn Văn A', room: 'P.201', time: '10:00 - 11:30', day: 'Thứ 3', date: '10/06/2026', status: 'published' },
])

// ── Search & Filter ──────────────────────────────────────────
const searchQuery = ref('')
const showFilterPanel = ref(false)
const filterDay = ref('all')
const filterTeacher = ref('all')

const TEACHERS = computed(() => {
  const set = new Set(publishedSchedules.value.map(s => s.teacher))
  return [...set]
})

const DAYS = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật']

const filteredSchedules = computed(() => {
  return publishedSchedules.value.filter(item => {
    const q = searchQuery.value.toLowerCase().trim()
    const matchSearch = !q
      || item.subject.toLowerCase().includes(q)
      || item.class.toLowerCase().includes(q)
      || item.teacher.toLowerCase().includes(q)
      || item.room.toLowerCase().includes(q)
    const matchDay = filterDay.value === 'all' || item.day === filterDay.value
    const matchTeacher = filterTeacher.value === 'all' || item.teacher === filterTeacher.value
    return matchSearch && matchDay && matchTeacher
  })
})

const activeFilterCount = computed(() => {
  let count = 0
  if (filterDay.value !== 'all') count++
  if (filterTeacher.value !== 'all') count++
  return count
})

function clearFilters() {
  filterDay.value = 'all'
  filterTeacher.value = 'all'
  searchQuery.value = ''
}

// ── Stats ────────────────────────────────────────────────────
const totalPublished = computed(() => publishedSchedules.value.length)

// ── Cancel/Change Modal ──────────────────────────────────────
const showCancelModal = ref(false)
const cancelTarget = ref(null)
const cancelReason = ref('')
const isCancelling = ref(false)
useBodyScrollLock(showCancelModal)

function openCancel(item) {
  cancelTarget.value = item
  cancelReason.value = ''
  showCancelModal.value = true
}

function closeCancel() {
  showCancelModal.value = false
  cancelTarget.value = null
  cancelReason.value = ''
}

async function confirmCancel() {
  if (!cancelTarget.value || !cancelReason.value.trim()) return
  isCancelling.value = true
  await new Promise(r => setTimeout(r, 1000))
  const idx = publishedSchedules.value.findIndex(s => s.id === cancelTarget.value.id)
  if (idx !== -1) publishedSchedules.value.splice(idx, 1)
  isCancelling.value = false
  closeCancel()
}

// ── Make-up Class Modal ─────────────────────────────────────
const showMakeupModal = ref(false)
const makeupTarget = ref(null)
const makeupDate = ref('')
const makeupTime = ref('')
const makeupRoom = ref('')
const isMakingUp = ref(false)
useBodyScrollLock(showMakeupModal)

function openMakeup(item) {
  makeupTarget.value = item
  makeupDate.value = ''
  makeupTime.value = ''
  makeupRoom.value = ''
  showMakeupModal.value = true
}

function closeMakeup() {
  showMakeupModal.value = false
  makeupTarget.value = null
}

async function confirmMakeup() {
  if (!makeupTarget.value || !makeupDate.value || !makeupTime.value) return
  isMakingUp.value = true
  await new Promise(r => setTimeout(r, 1000))
  publishedSchedules.value.push({
    id: Date.now(),
    subject: makeupTarget.value.subject + ' (Học bù)',
    class: makeupTarget.value.class,
    teacher: makeupTarget.value.teacher,
    room: makeupRoom.value || makeupTarget.value.room,
    time: makeupTime.value,
    day: dayjs(makeupDate.value, 'YYYY-MM-DD').format('dddd').replace('Monday', 'Thứ 2').replace('Tuesday', 'Thứ 3').replace('Wednesday', 'Thứ 4').replace('Thursday', 'Thứ 5').replace('Friday', 'Thứ 6').replace('Saturday', 'Thứ 7').replace('Sunday', 'Chủ nhật'),
    date: dayjs(makeupDate.value).format('DD/MM/YYYY'),
    status: 'published',
  })
  isMakingUp.value = false
  closeMakeup()
}

// ── History Modal ────────────────────────────────────────────
const showHistoryModal = ref(false)
const historyTarget = ref(null)
useBodyScrollLock(showHistoryModal)

const historyLogs = {
  1: [
    { action: 'Tạo lịch', by: 'Trần Thị Giáo Vụ', at: '20/05/2026 08:30', detail: 'Tạo lịch học Java SE1601' },
    { action: 'Công bố', by: 'Hệ thống', at: '25/05/2026 14:00', detail: 'Lịch đã được publish' },
  ],
  2: [
    { action: 'Tạo lịch', by: 'Trần Thị Giáo Vụ', at: '20/05/2026 09:00', detail: 'Tạo lịch CTDL SE1602' },
    { action: 'Điều chỉnh phòng', by: 'Lê Văn Giáo Vụ', at: '22/05/2026 10:15', detail: 'Đổi phòng từ P.103 sang P.105' },
    { action: 'Công bố', by: 'Hệ thống', at: '25/05/2026 14:00', detail: 'Lịch đã được publish' },
  ],
}

function openHistory(item) {
  historyTarget.value = item
  showHistoryModal.value = true
}

function closeHistory() {
  showHistoryModal.value = false
  historyTarget.value = null
}

// ── Send Notification Modal ──────────────────────────────────
const showNotifModal = ref(false)
const notifTitle = ref('')
const notifContent = ref('')
const isSendingNotif = ref(false)
useBodyScrollLock(showNotifModal)

function openNotif() {
  notifTitle.value = 'Thông báo thay đổi lịch học'
  notifContent.value = ''
  showNotifModal.value = true
}

function closeNotif() {
  showNotifModal.value = false
}

async function confirmSendNotif() {
  isSendingNotif.value = true
  await new Promise(r => setTimeout(r, 1200))
  isSendingNotif.value = false
  closeNotif()
}

// ── Helpers ──────────────────────────────────────────────────
function getDayClass(day) {
  const map = {
    'Thứ 2': 'day-chip day-primary',
    'Thứ 3': 'day-chip day-success',
    'Thứ 4': 'day-chip day-info',
    'Thứ 5': 'day-chip day-warning',
    'Thứ 6': 'day-chip day-danger',
    'Thứ 7': 'day-chip day-info',
    'Chủ nhật': 'day-chip day-neutral',
  }
  return map[day] || 'day-chip day-neutral'
}

function getInitial(name) {
  return name.split(' ').pop().charAt(0)
}

import dayjs from 'dayjs'
</script>

<template>
  <PageContainer 
    title="Lịch đã publish" 
    subtitle="Thời khóa biểu chính thức đã công bố. Mọi chỉnh sửa sẽ được lưu audit log và gửi thông báo cho GV/SV."
  >
    <div class="space-y-4">

      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] space-y-3">
        <div class="flex flex-wrap items-center gap-3">
          <div class="flex-1 min-w-[260px] relative">
            <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo môn, lớp, giảng viên..."
              class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium transition-all"
            />
            <button
              v-if="searchQuery"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label"
              @click="searchQuery = ''"
            >
              <X :size="14" />
            </button>
          </div>

          <button
            :class="[
              'flex items-center gap-2 px-4 py-2.5 text-sm font-bold rounded-xl transition-all',
              showFilterPanel ? 'lg-button-primary' : 'lg-button-secondary text-body'
            ]"
            @click.stop="showFilterPanel = !showFilterPanel"
          >
            <SlidersHorizontal :size="16" />
            Bộ lọc nâng cao
            <span
              v-if="activeFilterCount > 0"
              class="inline-flex items-center justify-center h-4 w-4 rounded-full bg-[var(--lg-success)] text-white text-[10px] font-semibold"
            >{{ activeFilterCount }}</span>
          </button>

          <button
            v-if="activeFilterCount > 0 || searchQuery"
            class="text-xs font-bold text-placeholder hover:text-[var(--lg-danger)] transition-colors"
            @click="clearFilters"
          >Xóa bộ lọc</button>
        </div>

        <!-- ── Advanced Filter Panel ── -->
        <Transition name="slide-down">
          <div v-if="showFilterPanel" class="pt-3 border-t border-default flex flex-wrap gap-4">
            <div>
              <p class="text-[10px] font-semibold uppercase tracking-widest text-placeholder mb-2">Thứ</p>
              <div class="flex gap-2 flex-wrap">
                <button
                  v-for="d in ['all', ...DAYS]" :key="d"
                  :class="[
                    'px-3 py-1.5 rounded-xl text-xs font-bold transition-all',
                    filterDay === d ? 'lg-button-primary' : 'lg-button-secondary text-body'
                  ]"
                  @click="filterDay = d"
                >
                  {{ d === 'all' ? 'Tất cả' : d }}
                </button>
              </div>
            </div>
            <div>
              <p class="text-[10px] font-semibold uppercase tracking-widest text-placeholder mb-2">Giảng viên</p>
              <div class="flex gap-2 flex-wrap">
                <button
                  v-for="t in ['all', ...TEACHERS]" :key="t"
                  :class="[
                    'px-3 py-1.5 rounded-xl text-xs font-bold transition-all',
                    filterTeacher === t ? 'lg-button-primary' : 'lg-button-secondary text-body'
                  ]"
                  @click="filterTeacher = t"
                >
                  {{ t === 'all' ? 'Tất cả' : t }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <!-- ── Result count ── -->
      <div class="flex items-center justify-between px-1">
        <p class="text-sm text-label font-semibold">
          Hiển thị <span class="font-semibold text-heading">{{ filteredSchedules.length }}</span> / {{ publishedSchedules.length }} buổi học
        </p>
      </div>

      <!-- ── Published Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-[var(--surface-input)]">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thời gian</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Môn & Lớp</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Giảng viên</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Phòng</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y border-default">
            <tr v-for="item in filteredSchedules" :key="item.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                <span :class="['inline-block px-2 py-0.5 rounded-lg text-[10px] font-semibold border mb-1', getDayClass(item.day)]">
                  {{ item.day }}
                </span>
                <p class="text-xs font-bold text-[var(--lg-primary)]">{{ item.time }}</p>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading leading-tight">{{ item.subject }}</p>
                <p class="text-[11px] font-bold text-placeholder mt-0.5 uppercase tracking-tighter">{{ item.class }}</p>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-full bg-[var(--color-info-bg)] flex items-center justify-center text-[10px] font-semibold text-[var(--color-info-text)] border border-[var(--color-info-bg)]">
                    {{ getInitial(item.teacher) }}
                  </div>
                  <span class="text-sm font-bold text-label">{{ item.teacher }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span class="text-sm font-semibold text-heading">{{ item.room }}</span>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-[var(--color-warning-bg)] hover:text-[var(--color-warning-text)] rounded-lg text-placeholder transition-all" title="Hủy buổi / Thay đổi" @click="openCancel(item)">
                    <XCircle :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--color-info-text)] rounded-lg text-placeholder transition-all" title="Lịch học bù" @click="openMakeup(item)">
                    <RefreshCw :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-placeholder transition-all" title="Lịch sử thay đổi" @click="openHistory(item)">
                    <History :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- ── Empty state ── -->
        <div
          v-if="filteredSchedules.length === 0"
          class="flex flex-col items-center justify-center py-16 text-center"
        >
          <div class="h-14 w-14 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
            <FileText :size="24" class="text-placeholder" />
          </div>
          <p class="text-base font-semibold text-heading">Không tìm thấy buổi học nào</p>
          <p class="text-sm text-label mt-1">Thử thay đổi từ khóa hoặc điều chỉnh bộ lọc.</p>
          <button class="mt-3 text-sm font-bold text-[var(--lg-primary)] hover:underline" @click="clearFilters">Xóa tất cả bộ lọc</button>
        </div>
      </div>

      <!-- ── Summary Info ── -->
      <div class="flex flex-col md:flex-row items-center justify-between gap-4 px-4 py-2">
        <div class="flex items-center gap-4">
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-[var(--lg-success)] shadow-sm shadow-[var(--lg-success)]/20"></span>
            <span class="text-xs font-bold text-label">{{ totalPublished }} Buổi học đã publish</span>
          </div>
          <div class="flex items-center gap-2 text-xs font-bold text-placeholder">
            <History :size="14" />
            Lần cuối cập nhật: 10:30 Hôm nay
          </div>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-xs font-semibold uppercase tracking-widest flex items-center gap-2" @click="openNotif">
          <Bell :size="14" /> Gửi thông báo toàn bộ
        </button>
      </div>
    </div>
  </PageContainer>

  <!-- ════════════════════════════════════════════════════════════
       Cancel / Change Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showCancelModal && cancelTarget"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        @click.self="closeCancel"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-md surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input flex items-center justify-center">
                  <XCircle :size="20" class="text-link" />
                </div>
                <div>
                  <h2 class="text-lg font-semibold text-heading">Hủy buổi học</h2>
                  <p class="text-xs text-label mt-0.5">Thao tác này sẽ gửi thông báo đến GV & SV</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="closeCancel"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div class="surface-input rounded-2xl p-4 border border-default">
              <div class="flex items-center gap-3 mb-3">
                <BookOpen :size="16" class="text-placeholder" />
                <div>
                  <p class="text-sm font-semibold text-heading">{{ cancelTarget.subject }}</p>
                  <p class="text-xs text-label">{{ cancelTarget.class }} · {{ cancelTarget.day }}, {{ cancelTarget.time }} · {{ cancelTarget.room }}</p>
                </div>
              </div>
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Lý do hủy <span class="text-[var(--lg-danger)]">*</span></label>
              <textarea
                v-model="cancelReason"
                rows="3"
                class="lg-input w-full px-4 py-2.5 text-sm font-medium resize-none"
                placeholder="Nhập lý do hủy buổi học..."
              ></textarea>
            </div>

            <div class="flex items-start gap-2 p-3 rounded-xl bg-[var(--color-warning-bg)]">
              <AlertTriangle :size="14" class="text-[var(--color-warning-text)] shrink-0 mt-0.5" />
              <p class="text-xs font-medium text-[var(--color-warning-text)]">
                Hủy buổi học sẽ gửi thông báo đến giảng viên và sinh viên. Thao tác này không thể hoàn tác.
              </p>
            </div>
          </div>

          <div class="px-6 pb-6 flex items-center justify-end gap-3">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeCancel"
            >Hủy</button>
            <button
              :class="[
                'px-6 py-2.5 rounded-[18px] text-sm font-bold text-white transition-all flex items-center gap-2',
                !cancelReason.trim() || isCancelling
                  ? 'bg-[var(--border-default)] cursor-not-allowed'
                  : 'bg-[var(--lg-danger)] hover:opacity-90 shadow-lg shadow-[var(--lg-danger)]/20'
              ]"
              :disabled="!cancelReason.trim() || isCancelling"
              @click="confirmCancel"
            >
              <span v-if="isCancelling" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <XCircle v-else :size="16" />
              {{ isCancelling ? 'Đang hủy...' : 'Xác nhận hủy buổi' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       Make-up Class Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showMakeupModal && makeupTarget"
        class="fixed inset-0 z-[110] flex items-center justify-center p-4"
        @click.self="closeMakeup"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-md surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input flex items-center justify-center">
                  <RefreshCw :size="20" class="text-link" />
                </div>
                <div>
                  <h2 class="text-lg font-semibold text-heading">Tạo lịch học bù</h2>
                  <p class="text-xs text-label mt-0.5">Lên lịch học bù cho buổi đã hủy</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="closeMakeup"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div class="surface-input rounded-2xl p-4 border border-default">
              <div class="flex items-center gap-3">
                <BookOpen :size="16" class="text-placeholder" />
                <div>
                  <p class="text-sm font-semibold text-heading">{{ makeupTarget.subject }}</p>
                  <p class="text-xs text-label">{{ makeupTarget.class }} · GV: {{ makeupTarget.teacher }}</p>
                </div>
              </div>
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Ngày học bù <span class="text-[var(--lg-danger)]">*</span></label>
              <input
                v-model="makeupDate"
                type="date"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Giờ học bù <span class="text-[var(--lg-danger)]">*</span></label>
              <input
                v-model="makeupTime"
                type="text"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                placeholder="VD: 07:30 - 10:30"
              />
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Phòng học</label>
              <input
                v-model="makeupRoom"
                type="text"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                :placeholder="makeupTarget.room"
              />
            </div>
          </div>

          <div class="px-6 pb-6 flex items-center justify-end gap-3">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeMakeup"
            >Hủy</button>
            <button
              :class="[
                'lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2',
                (!makeupDate || !makeupTime || isMakingUp) ? 'opacity-45 pointer-events-none' : ''
              ]"
              :disabled="!makeupDate || !makeupTime || isMakingUp"
              @click="confirmMakeup"
            >
              <span v-if="isMakingUp" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <RefreshCw v-else :size="16" />
              {{ isMakingUp ? 'Đang tạo...' : 'Tạo lịch học bù' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       History Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showHistoryModal && historyTarget"
        class="fixed inset-0 z-[120] flex items-center justify-center p-4"
        @click.self="closeHistory"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[80vh] overflow-y-auto border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input flex items-center justify-center">
                  <History :size="20" class="text-link" />
                </div>
                <div>
                  <h2 class="text-lg font-semibold text-heading">Lịch sử thay đổi</h2>
                  <p class="text-xs text-label mt-0.5">{{ historyTarget.subject }} · {{ historyTarget.class }}</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="closeHistory"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6">
            <div v-if="historyLogs[historyTarget.id] && historyLogs[historyTarget.id].length > 0" class="space-y-0">
              <div
                v-for="(log, idx) in historyLogs[historyTarget.id]"
                :key="idx"
                class="relative flex items-start gap-4 pb-6 last:pb-0"
              >
                <!-- Timeline dot + line -->
                <div class="flex flex-col items-center shrink-0">
                  <div class="h-3 w-3 rounded-full border-2 border-[var(--lg-primary)] bg-[var(--surface-card)] z-10"></div>
                  <div v-if="idx < historyLogs[historyTarget.id].length - 1" class="w-px flex-1 bg-[var(--border-default)] -mt-1"></div>
                </div>
                <!-- Content -->
                <div class="flex-1 -mt-0.5">
                  <div class="flex items-center justify-between">
                    <p class="text-sm font-semibold text-heading">{{ log.action }}</p>
                    <p class="text-[10px] font-bold text-placeholder">{{ log.at }}</p>
                  </div>
                  <p class="text-xs font-medium text-label mt-0.5">{{ log.detail }}</p>
                  <p class="text-[10px] font-bold text-placeholder mt-0.5">Bởi: {{ log.by }}</p>
                </div>
              </div>
            </div>

            <div v-else class="py-8 text-center">
              <div class="h-12 w-12 rounded-2xl surface-input border border-default flex items-center justify-center mx-auto mb-3">
                <Info :size="20" class="text-placeholder" />
              </div>
              <p class="text-sm font-semibold text-heading">Chưa có lịch sử thay đổi</p>
              <p class="text-xs text-label mt-1">Buổi học này chưa có thay đổi nào được ghi nhận.</p>
            </div>
          </div>

          <div class="px-6 pb-6 flex items-center justify-end">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeHistory"
            >Đóng</button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       Send Notification Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showNotifModal"
        class="fixed inset-0 z-[130] flex items-center justify-center p-4"
        @click.self="closeNotif"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-md surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input flex items-center justify-center">
                  <Bell :size="20" class="text-link" />
                </div>
                <div>
                  <h2 class="text-lg font-semibold text-heading">Gửi thông báo</h2>
                  <p class="text-xs text-label mt-0.5">Thông báo đến toàn bộ GV & SV liên quan</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="closeNotif"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Tiêu đề</label>
              <input
                v-model="notifTitle"
                type="text"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Nội dung <span class="text-[var(--lg-danger)]">*</span></label>
              <textarea
                v-model="notifContent"
                rows="4"
                class="lg-input w-full px-4 py-2.5 text-sm font-medium resize-none"
                placeholder="Nhập nội dung thông báo..."
              ></textarea>
            </div>

            <div class="surface-input rounded-2xl p-4 border border-default">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)]">
                  <Send :size="18" />
                </div>
                <div>
                  <p class="text-sm font-semibold text-heading">{{ totalPublished }} buổi học</p>
                  <p class="text-xs text-label mt-0.5">Sẽ gửi thông báo đến tất cả giảng viên và sinh viên có liên quan.</p>
                </div>
              </div>
            </div>
          </div>

          <div class="px-6 pb-6 flex items-center justify-end gap-3">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeNotif"
            >Hủy</button>
            <button
              :class="[
                'lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2',
                (!notifContent.trim() || isSendingNotif) ? 'opacity-45 pointer-events-none' : ''
              ]"
              :disabled="!notifContent.trim() || isSendingNotif"
              @click="confirmSendNotif"
            >
              <span v-if="isSendingNotif" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Bell v-else :size="16" />
              {{ isSendingNotif ? 'Đang gửi...' : 'Gửi thông báo' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-header {
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-input);
}

.day-chip {
  background: var(--surface-input);
}

.day-primary {
  border-color: var(--color-info-bg);
  color: var(--color-info-text);
}

.day-success {
  border-color: var(--color-success-bg);
  color: var(--color-success-text);
}

.day-warning {
  border-color: var(--color-warning-bg);
  color: var(--color-warning-text);
}

.day-danger {
  border-color: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.day-info,
.day-neutral {
  border-color: var(--border-card);
  color: var(--text-body);
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.25s ease;
}
.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from .relative.w-full,
.modal-leave-to .relative.w-full {
  transform: scale(0.9) translateY(20px);
}
</style>
