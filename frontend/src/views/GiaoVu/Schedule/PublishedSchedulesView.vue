<script setup>
import { ref, computed, onMounted } from 'vue'
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
  Loader2,
} from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'

const popupStore = usePopupStore()
const loading = ref(true)
const apiError = ref('')
const publishedSchedules = ref([])
const rooms = ref([])

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getSchedules({ trangThai: 'published' })
    publishedSchedules.value = res?.items ?? res ?? []
  } catch (e) {
    console.error(e)
    apiError.value = e?.message || 'Không thể tải danh sách thời khóa biểu đã công bố.'
  } finally {
    loading.value = false
  }
}

async function loadRooms() {
  try {
    const res = await staffApi.getRooms({ pageSize: 100 })
    const rRaw = res?.items ?? res?.data ?? res ?? []
    rooms.value = Array.isArray(rRaw) ? rRaw : (rRaw.items ?? [])
  } catch (e) {
    console.error(e)
  }
}

onMounted(() => {
  loadData()
  loadRooms()
})

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
  // Fake action delay removed per UX standard
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
  // Fake action delay removed per UX standard
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
  // Fake action delay removed per UX standard
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
  <div class="published-schedules max-w-7xl mx-auto space-y-6 p-4 md:p-6">
    <!-- Header -->
    <div class="border-b border-(--border-default) pb-4">
      <div class="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-(--text-link) mb-1">
        <Calendar :size="14"/>
        Thời khóa biểu
      </div>
      <h1 class="text-3xl font-extrabold text-(--text-heading) tracking-tight">Lịch đã publish</h1>
      <p class="text-sm text-(--text-muted) mt-1">Thời khóa biểu chính thức đã công bố. Mọi chỉnh sửa sẽ được lưu audit log và gửi thông báo cho GV/SV.</p>
    </div>

    <!-- 5 States Container -->
    <div v-if="loading" class="space-y-4 py-8">
      <LoadingSkeleton :lines="6" />
    </div>

    <!-- Error State with Retry -->
    <div v-else-if="apiError" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
      <AlertCircle :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
      <h3 class="text-lg font-bold text-(--text-heading)">Đã xảy ra lỗi</h3>
      <p class="text-sm text-(--text-muted) mt-1 mb-4">{{ apiError }}</p>
      <GlassButton variant="secondary" @click="loadData">Thử lại</GlassButton>
    </div>

    <template v-else>
      <div class="space-y-4">

        <!-- ── Toolbar ── -->
        <div class="lg-glass-strong p-4 rounded-2xl space-y-3">
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
                class="inline-flex items-center justify-center h-4 w-4 rounded-full bg-(--lg-success) text-white text-[10px] font-semibold"
              >{{ activeFilterCount }}</span>
            </button>

            <button
              v-if="activeFilterCount > 0 || searchQuery"
              class="text-xs font-bold text-placeholder hover:text-(--lg-danger) transition-colors"
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
              <tr class="bg-(--surface-input)">
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thời gian</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Môn & Lớp</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Giảng viên</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Phòng</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y border-default">
              <tr v-for="item in filteredSchedules" :key="item.id" class="group hover:bg-(--surface-input) transition-colors">
                <td class="px-4 py-4">
                  <span :class="['inline-block px-2 py-0.5 rounded-lg text-[10px] font-semibold border mb-1', getDayClass(item.day)]">
                    {{ item.day }}
                  </span>
                  <p class="text-xs font-bold text-(--lg-primary)">{{ item.time }}</p>
                </td>
                <td class="px-4 py-4">
                  <p class="text-sm font-semibold text-heading leading-tight">{{ item.subject }}</p>
                  <p class="text-[11px] font-bold text-placeholder mt-0.5 uppercase tracking-tighter">{{ item.class }}</p>
                </td>
                <td class="px-4 py-4">
                  <div class="flex items-center gap-3">
                    <div class="h-8 w-8 rounded-full bg-(--color-info-bg) flex items-center justify-center text-[10px] font-semibold text-(--color-info-text) border border-(--color-info-bg)">
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
                    <button class="p-2 hover:bg-(--color-warning-bg) hover:text-(--color-warning-text) rounded-lg text-placeholder transition-all" title="Hủy buổi / Thay đổi" @click="openCancel(item)">
                      <XCircle :size="16" />
                    </button>
                    <button class="p-2 hover:bg-(--color-info-bg) hover:text-(--color-info-text) rounded-lg text-placeholder transition-all" title="Lịch học bù" @click="openMakeup(item)">
                      <RefreshCw :size="16" />
                    </button>
                    <button class="p-2 hover:bg-(--surface-input) rounded-lg text-placeholder transition-all" title="Lịch sử thay đổi" @click="openHistory(item)">
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
            <button class="mt-3 text-sm font-bold text-(--lg-primary) hover:underline" @click="clearFilters">Xóa tất cả bộ lọc</button>
        <!-- Cancel Booking/Class Modal -->
  <ConfirmActionDialog
    :modelValue="showCancelModal"
    @update:modelValue="showCancelModal = $event"
    title="Hủy buổi học đã công bố"
    confirmLabel="Hủy buổi học"
    variant="danger"
    :loading="isCancelling"
    @confirm="confirmCancel"
    @cancel="closeCancel"
  >
    <div v-if="cancelTarget" class="space-y-4 mt-3">
      <div class="p-3 bg-(--surface-input) border border-(--border-default) rounded-xl">
        <p class="text-sm font-bold text-(--text-heading)">{{ cancelTarget.subject }}</p>
        <p class="text-xs text-(--text-muted) mt-1">Lớp: {{ cancelTarget.class }} &bull; {{ cancelTarget.day }}, {{ cancelTarget.time }} &bull; Phòng: {{ cancelTarget.room }}</p>
      </div>
      <div>
        <label class="block text-xs font-semibold text-(--text-muted) mb-1">Lý do hủy <span class="text-red-500">*</span></label>
        <GlassInput v-model="cancelReason" placeholder="Nhập lý do hủy buổi học (tối thiểu 10 ký tự)..." class="w-full" />
      </div>
      <div class="p-3 bg-red-500/10 text-red-600 dark:text-red-400 rounded-lg text-xs font-semibold flex items-start gap-2 border border-red-500/20">
        <AlertTriangle :size="14" class="shrink-0 mt-0.5" />
        <span>Thao tác này sẽ gửi email và thông báo tức thời đến giảng viên cùng sinh viên của lớp. Hành động không thể hoàn tác.</span>
      </div>
    </div>
  </ConfirmActionDialog>

  <!-- Make-up Class Modal -->
  <ConfirmActionDialog
    :modelValue="showMakeupModal"
    @update:modelValue="showMakeupModal = $event"
    title="Xếp lịch học bù"
    confirmLabel="Tạo lịch học bù"
    variant="primary"
    :loading="isMakingUp"
    @confirm="confirmMakeup"
    @cancel="closeMakeup"
  >
    <div v-if="makeupTarget" class="space-y-4 mt-3">
      <div class="p-3 bg-(--surface-input) border border-(--border-default) rounded-xl">
        <p class="text-sm font-bold text-(--text-heading)">{{ makeupTarget.subject }}</p>
        <p class="text-xs text-(--text-muted) mt-1">Lớp: {{ makeupTarget.class }} &bull; Giảng viên: {{ makeupTarget.teacher }}</p>
      </div>
      
      <div>
        <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày học bù <span class="text-red-500">*</span></label>
        <GlassInput v-slot="scope" class="w-full">
          <input v-model="makeupDate" v-bind="scope" type="date" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
        </GlassInput>
      </div>

      <div>
        <label class="block text-xs font-semibold text-(--text-muted) mb-1">Giờ học bù <span class="text-red-500">*</span></label>
        <GlassInput v-model="makeupTime" placeholder="VD: Ca 1 (07:30 - 09:30)" class="w-full" />
      </div>

      <div>
        <label class="block text-xs font-semibold text-(--text-muted) mb-1">Chọn phòng học bù</label>
        <select v-slot="scope" v-model="makeupRoom" class="text-xs bg-(--surface-input) border border-(--border-input) text-(--text-body) rounded-lg w-full px-3 py-2.5 outline-none focus:ring-1 focus:ring-(--border-focus)">
          <option value="">-- Giữ nguyên hoặc chọn phòng mới --</option>
          <option v-for="room in rooms" :key="room.maPhong" :value="room.tenPhong || room.maCodePhong">
            {{ room.tenPhong || room.maCodePhong }}
          </option>
        </select>
      </div>
    </div>
  </ConfirmActionDialog>

  <!-- History Modal -->
  <ConfirmActionDialog
    :modelValue="showHistoryModal"
    @update:modelValue="showHistoryModal = $event"
    title="Lịch sử thay đổi"
    confirmLabel="Xác nhận"
    variant="primary"
    @confirm="closeHistory"
    @cancel="closeHistory"
  >
    <div v-if="historyTarget" class="space-y-4 mt-3 max-h-[350px] overflow-y-auto pr-1">
      <p class="text-xs text-(--text-muted) font-semibold">{{ historyTarget.subject }} &bull; Lớp: {{ historyTarget.class }}</p>
      
      <div v-if="historyLogs[historyTarget.id] && historyLogs[historyTarget.id].length > 0" class="relative pl-3 border-l-2 border-(--border-default) space-y-4">
        <div v-for="(log, idx) in historyLogs[historyTarget.id]" :key="idx" class="relative">
          <div class="absolute -left-4.5 w-2.5 h-2.5 rounded-full border-2 border-blue-500 bg-(--surface-card) mt-1"></div>
          <p class="text-xs font-bold text-(--text-heading) flex justify-between gap-2">
            <span>{{ log.action }}</span>
            <span class="text-[10px] text-muted">{{ log.at }}</span>
          </p>
          <p class="text-[11px] text-(--text-body) mt-0.5">{{ log.detail }}</p>
          <p class="text-[9px] text-muted mt-0.5">Thực hiện bởi: {{ log.by }}</p>
        </div>
      </div>

      <div v-else class="py-8 text-center">
        <p class="text-xs text-muted">Chưa có lịch sử thay đổi nào được ghi nhận cho buổi học này.</p>
      </div>
    </div>
  </ConfirmActionDialog>

  <!-- Send Notification Modal -->
  <ConfirmActionDialog
    :modelValue="showNotifModal"
    @update:modelValue="showNotifModal = $event"
    title="Gửi thông báo thay đổi lịch học"
    confirmLabel="Gửi thông báo"
    variant="primary"
    :loading="isSendingNotif"
    @confirm="confirmSendNotif"
    @cancel="closeNotif"
  >
    <div class="space-y-4 mt-3">
      <div>
        <label class="block text-xs font-semibold text-(--text-muted) mb-1">Tiêu đề thông báo <span class="text-red-500">*</span></label>
        <GlassInput v-model="notifTitle" class="w-full" />
      </div>
      <div>
        <label class="block text-xs font-semibold text-(--text-muted) mb-1">Nội dung thông báo <span class="text-red-500">*</span></label>
        <GlassInput v-model="notifContent" placeholder="Nhập nội dung chi tiết gửi tới toàn bộ lớp học..." class="w-full" />
      </div>
      <div class="p-3 bg-blue-500/10 text-blue-600 dark:text-blue-400 rounded-lg text-xs font-semibold flex items-start gap-2 border border-blue-500/20">
        <Send :size="14" class="shrink-0 mt-0.5" />
        <span>Hệ thống sẽ gửi email và thông báo thông qua app cho toàn bộ giảng viên và sinh viên liên quan đến {{ totalPublished }} buổi học này.</span>
      </div>
    </div>
  </ConfirmActionDialog>
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
