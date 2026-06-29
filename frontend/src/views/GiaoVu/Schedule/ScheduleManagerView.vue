<script setup>
import { ref, computed } from 'vue'
import {
  CalendarRange, Search, Plus, CheckCircle, AlertTriangle, BookOpen, X,
} from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import {
  staffScheduleRows, caHocCatalog, thuTrongTuanOptions,
} from '@/mocks/scheduleAttendanceMockData'
import { getStatusLabel } from '@/utils/statusLabels'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

// ── State ──────────────────────────────────────────────────────
const rows = ref(staffScheduleRows.map(r => ({ ...r })))
const selectedRow = ref(null)      // clicked card detail
const showCreatePanel = ref(false)
const confirmAction = ref(null)
const searchQuery = ref('')
const filterHocKy = ref('')
const filterTrangThai = ref('')

// drag state
const draggingRow = ref(null)
const dragOverCell = ref(null)

// ── Grid dimensions ────────────────────────────────────────────
const days = thuTrongTuanOptions  // [{value: 2, label:'Thứ 2'}, ...]
const shifts = caHocCatalog       // [{id:'ca1', tenCa:'Ca 1', ...}, ...]

// ── Lookup ─────────────────────────────────────────────────────
const hocKyOptions = ['Spring 2026', 'Summer 2026', 'Fall 2025']

const emptyForm = () => ({
  hocKy: { ma: 'SP2026', ten: 'Spring 2026' },
  lop: { ma: '', ten: '' },
  monHoc: { ma: '', ten: '' },
  giaoVien: { ma: '', ten: '' },
  thuTrongTuan: 2,
  caHoc: caHocCatalog[0],
  phongHoc: { ma: '', ten: '' },
  ngayBatDau: '2026-01-06',
  ngayKetThuc: '2026-05-30',
  ghiChu: '',
  trangThai: 'nhap',
})
const form = ref(emptyForm())
const conflictPreview = ref([])

// ── Filtered rows ───────────────────────────────────────────────
const filteredRows = computed(() => {
  let list = rows.value
  if (filterHocKy.value) list = list.filter(r => r.hocKy.ten === filterHocKy.value)
  if (filterTrangThai.value) list = list.filter(r => r.trangThai === filterTrangThai.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(r =>
      r.maTkb.toLowerCase().includes(q) ||
      r.monHoc.ten.toLowerCase().includes(q) ||
      r.giaoVien.ten.toLowerCase().includes(q) ||
      r.lop.ma.toLowerCase().includes(q)
    )
  }
  return list
})

// ── Grid cell lookup ───────────────────────────────────────────
function cellItems(thu, caId) {
  return filteredRows.value.filter(r => r.thuTrongTuan === thu && r.caHoc?.id === caId)
}

// ── Card color by status ───────────────────────────────────────
const cardColors = {
  nhap: { bg: 'bg-(--surface-input)', border: 'border-(--border-default)', text: 'text-(--text-body)', dot: 'bg-slate-400', accent: 'border-l-slate-400' },
  da_xuat_ban: { bg: 'bg-emerald-50/80 dark:bg-emerald-950/40', border: 'border-emerald-200 dark:border-emerald-800', text: 'text-emerald-800 dark:text-emerald-300', dot: 'bg-emerald-500', accent: 'border-l-emerald-500' },
  da_huy: { bg: 'bg-red-50/80 dark:bg-red-950/30', border: 'border-red-200 dark:border-red-800', text: 'text-red-700 dark:text-red-400', dot: 'bg-red-400', accent: 'border-l-red-400' },
}
function getCardColor(status) {
  return cardColors[status] || cardColors.nhap
}

// ── Summary cards ───────────────────────────────────────────────
const summaryCards = computed(() => [
  { label: 'Bản nháp', value: rows.value.filter(r => r.trangThai === 'nhap').length, color: 'text-(--text-muted)', bg: 'bg-(--surface-input)' },
  { label: 'Đã xuất bản', value: rows.value.filter(r => r.trangThai === 'da_xuat_ban').length, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-50/80 dark:bg-emerald-950/40 border-emerald-200 dark:border-emerald-800' },
  { label: 'Tổng lịch', value: rows.value.length, color: 'text-(--lg-primary)', bg: 'bg-(--surface-input)' },
  { label: 'Xung đột', value: 1, color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-50/80 dark:bg-amber-950/30 border-amber-200 dark:border-amber-800' },
])

// ── Drag & Drop ────────────────────────────────────────────────
function onDragStart(row, event) {
  draggingRow.value = row
  event.dataTransfer.effectAllowed = 'move'
}

function onDragOver(thu, caId, event) {
  event.preventDefault()
  event.dataTransfer.dropEffect = 'move'
  dragOverCell.value = `${thu}-${caId}`
}

function onDragLeave() {
  dragOverCell.value = null
}

function onDrop(thu, caId, event) {
  event.preventDefault()
  dragOverCell.value = null
  if (!draggingRow.value) return
  const row = draggingRow.value
  draggingRow.value = null
  if (row.thuTrongTuan === thu && row.caHoc?.id === caId) return

  const thuLabel = thuTrongTuanOptions.find(t => t.value === thu)?.label || `Thứ ${thu}`
  const caObj = caHocCatalog.find(c => c.id === caId)
  if (!caObj) return

  confirmAction.value = {
    title: 'Di chuyển lịch học?',
    message: `Chuyển "${row.monHoc.ten}" (${row.lop.ma}) sang ${thuLabel} · ${caObj.tenCa} (${caObj.gioBatDau}–${caObj.gioKetThuc})?`,
    label: 'Di chuyển',
    variant: 'primary',
    run: () => {
      const idx = rows.value.findIndex(r => r.id === row.id)
      if (idx !== -1) {
        rows.value[idx].thuTrongTuan = thu
        rows.value[idx].caHoc = { ...caObj }
      }
      confirmAction.value = null
      popupStore.success('Đã di chuyển', `Lịch "${row.monHoc.ten}" đã được chuyển sang ${thuLabel} · ${caObj.tenCa}.`)
    }
  }
}

function onDragEnd() {
  draggingRow.value = null
  dragOverCell.value = null
}

// ── Detail panel ───────────────────────────────────────────────
function openDetail(row) {
  selectedRow.value = row
  showCreatePanel.value = false
}

function closeDetail() {
  selectedRow.value = null
}

// ── Create panel ───────────────────────────────────────────────
function openCreate(thu = 2, caId = 'ca1') {
  form.value = emptyForm()
  form.value.thuTrongTuan = thu
  form.value.caHoc = caHocCatalog.find(c => c.id === caId) || caHocCatalog[0]
  conflictPreview.value = []
  selectedRow.value = null
  showCreatePanel.value = true
}

function closeCreatePanel() { showCreatePanel.value = false }

// ── Conflict check ─────────────────────────────────────────────
function checkConflicts() {
  conflictPreview.value = []
  const { thuTrongTuan, caHoc, giaoVien, lop, phongHoc } = form.value
  if (!thuTrongTuan || !caHoc) return
  const existing = rows.value.filter(r =>
    r.thuTrongTuan === thuTrongTuan && r.caHoc?.id === caHoc.id && r.trangThai !== 'da_huy'
  )
  existing.forEach(r => {
    if (giaoVien.ma && r.giaoVien.ma === giaoVien.ma)
      conflictPreview.value.push({ loai: 'GV', text: `${giaoVien.ten} đã có lịch ${r.maTkb}` })
    if (lop.ma && r.lop.ma === lop.ma)
      conflictPreview.value.push({ loai: 'Lớp', text: `Lớp ${lop.ma} đã có lịch ${r.maTkb}` })
    if (phongHoc.ma && r.phongHoc.ma === phongHoc.ma)
      conflictPreview.value.push({ loai: 'Phòng', text: `${phongHoc.ten} đã được dùng bởi ${r.maTkb}` })
  })
  if (conflictPreview.value.length === 0)
    popupStore.success('Không xung đột', 'Lịch này không xung đột với dữ liệu hiện tại.')
}

// ── Save ───────────────────────────────────────────────────────
function saveDraft() {
  if (!form.value.monHoc.ten || !form.value.lop.ma) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập Lớp và Môn học.')
    return
  }
  const newId = `TKB-${String(rows.value.length + 1).padStart(3, '0')}`
  rows.value.push({ ...form.value, id: newId, maTkb: newId, trangThai: 'nhap' })
  showCreatePanel.value = false
  popupStore.success('Đã lưu nháp', `Lịch ${newId} đã được tạo.`)
}

function publishDraft() {
  if (!form.value.monHoc.ten || !form.value.lop.ma) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập Lớp và Môn học.')
    return
  }
  if (conflictPreview.value.length > 0) {
    popupStore.warning('Có xung đột', 'Giải quyết xung đột trước khi xuất bản.')
    return
  }
  confirmAction.value = {
    title: 'Xuất bản lịch học?',
    message: `Lịch "${form.value.monHoc.ten}" (${form.value.lop.ma}) sẽ được công bố.`,
    label: 'Xuất bản',
    variant: 'primary',
    run: () => {
      const newId = `TKB-${String(rows.value.length + 1).padStart(3, '0')}`
      rows.value.push({ ...form.value, id: newId, maTkb: newId, trangThai: 'da_xuat_ban' })
      confirmAction.value = null
      showCreatePanel.value = false
      popupStore.success('Đã xuất bản', `Lịch ${newId} đã được công bố.`)
    }
  }
}

function publishRow(row) {
  confirmAction.value = {
    title: 'Xuất bản lịch này?',
    message: `"${row.monHoc.ten}" (${row.lop.ma}) sẽ được công bố tới sinh viên và giảng viên.`,
    label: 'Xuất bản',
    variant: 'primary',
    run: () => {
      const idx = rows.value.findIndex(r => r.id === row.id)
      if (idx !== -1) rows.value[idx].trangThai = 'da_xuat_ban'
      if (selectedRow.value?.id === row.id) selectedRow.value = { ...rows.value.find(r => r.id === row.id) }
      confirmAction.value = null
      popupStore.success('Đã xuất bản', `Lịch "${row.monHoc.ten}" đã được công bố.`)
    }
  }
}

function deleteRow(row) {
  confirmAction.value = {
    title: 'Xóa lịch này?',
    message: `Lịch ${row.maTkb} · "${row.monHoc.ten}" (${row.lop.ma}) sẽ bị xóa vĩnh viễn.`,
    label: 'Xóa',
    variant: 'danger',
    run: () => {
      const idx = rows.value.findIndex(r => r.id === row.id)
      if (idx !== -1) rows.value.splice(idx, 1)
      if (selectedRow.value?.id === row.id) selectedRow.value = null
      confirmAction.value = null
      popupStore.success('Đã xóa', `Lịch ${row.maTkb} đã được xóa.`)
    }
  }
}

function thuLabel(thu) {
  return thuTrongTuanOptions.find(t => t.value === thu)?.label || `Thứ ${thu}`
}
</script>

<template>
  <div class="schedule-calendar max-w-full space-y-4">

    <!-- ── Header ── -->
    <div class="flex items-start justify-between gap-4 flex-wrap">
      <div>
        <div class="flex items-center gap-2">
          <CalendarRange class="text-(--lg-primary)" :size="22" />
          <h1 class="text-xl font-bold text-(--text-heading)">Thời khóa biểu</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Quản lý lịch học theo học kỳ · Kéo thả để điều chỉnh · Click để xem chi tiết</p>
      </div>
      <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate()">
        <Plus :size="15" class="mr-1" /> Tạo lịch
      </GlassButton>
    </div>

    <!-- ── Summary pills ── -->
    <div class="flex flex-wrap gap-2">
      <div
        v-for="c in summaryCards" :key="c.label"
        class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) text-sm font-medium"
        :class="c.bg"
      >
        <span :class="c.color" class="font-bold text-base">{{ c.value }}</span>
        <span class="text-(--text-muted)">{{ c.label }}</span>
      </div>
    </div>

    <!-- ── Filters ── -->
    <div class="flex flex-wrap gap-2 items-center">
      <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-9 rounded-lg border border-(--border-input) min-w-[200px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
        <Search :size="15" class="text-(--text-muted) shrink-0" />
        <input v-model="searchQuery" type="text" placeholder="Tìm môn, lớp, GV..." class="bg-transparent border-none outline-none text-sm text-(--text-body) w-full" />
      </label>
      <select v-model="filterHocKy" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
        <option value="">Tất cả học kỳ</option>
        <option v-for="hk in hocKyOptions" :key="hk">{{ hk }}</option>
      </select>
      <select v-model="filterTrangThai" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
        <option value="">Tất cả trạng thái</option>
        <option value="nhap">Bản nháp</option>
        <option value="da_xuat_ban">Đã xuất bản</option>
        <option value="da_huy">Đã hủy</option>
      </select>
      <!-- Legend -->
      <div class="flex items-center gap-3 ml-auto text-xs text-(--text-muted)">
        <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded-full bg-slate-400 inline-block"></span>Bản nháp</span>
        <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded-full bg-emerald-500 inline-block"></span>Đã xuất bản</span>
        <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded-full bg-red-400 inline-block"></span>Đã hủy</span>
      </div>
    </div>

    <!-- ── Main calendar area ── -->
    <div class="flex gap-4 items-start">

      <!-- Calendar grid -->
      <div class="flex-1 min-w-0 overflow-x-auto">
        <div class="surface-card border border-(--border-card) rounded-2xl overflow-hidden shadow-sm" style="min-width: 680px">

          <!-- Day headers -->
          <div class="grid border-b border-(--border-default)" :style="`grid-template-columns: 72px repeat(${days.length}, 1fr)`">
            <div class="p-2 text-center text-xs font-semibold text-(--text-muted) bg-(--surface-solid) border-r border-(--border-default)">
              Ca / Thứ
            </div>
            <div
              v-for="day in days" :key="day.value"
              class="p-2.5 text-center text-xs font-bold text-(--text-heading) bg-(--surface-solid) border-r last:border-r-0 border-(--border-default)"
            >
              {{ day.label }}
            </div>
          </div>

          <!-- Shift rows -->
          <div v-for="shift in shifts" :key="shift.id" class="grid border-b last:border-b-0 border-(--border-default)" :style="`grid-template-columns: 72px repeat(${days.length}, 1fr)`">

            <!-- Shift label -->
            <div class="p-2 flex flex-col items-center justify-center text-center border-r border-(--border-default) bg-(--surface-solid) select-none">
              <span class="text-xs font-bold text-(--text-heading)">{{ shift.tenCa }}</span>
              <span class="text-[10px] text-(--text-muted) leading-tight mt-0.5">{{ shift.gioBatDau }}<br>{{ shift.gioKetThuc }}</span>
            </div>

            <!-- Day cells -->
            <div
              v-for="day in days" :key="day.value"
              class="border-r last:border-r-0 border-(--border-default) p-1 min-h-[90px] transition-colors relative"
              :class="[
                dragOverCell === `${day.value}-${shift.id}` ? 'bg-(--color-info-bg)' : 'hover:bg-(--surface-hover)',
              ]"
              @dragover="onDragOver(day.value, shift.id, $event)"
              @dragleave="onDragLeave"
              @drop="onDrop(day.value, shift.id, $event)"
              @click.self="openCreate(day.value, shift.id)"
            >
              <!-- Drop indicator -->
              <div
                v-if="dragOverCell === `${day.value}-${shift.id}`"
                class="absolute inset-1 border-2 border-dashed border-(--lg-primary) rounded-lg pointer-events-none z-0 opacity-60"
              ></div>

              <!-- TKB cards in this cell -->
              <div
                v-for="row in cellItems(day.value, shift.id)" :key="row.id"
                class="relative z-10 mb-1 last:mb-0 rounded-lg border px-2 py-1.5 cursor-grab active:cursor-grabbing select-none transition-all hover:shadow-md hover:-translate-y-0.5"
                :class="[
                  getCardColor(row.trangThai).bg,
                  getCardColor(row.trangThai).border,
                  selectedRow?.id === row.id ? 'ring-2 ring-(--lg-primary)' : '',
                  draggingRow?.id === row.id ? 'opacity-40' : '',
                ]"
                draggable="true"
                @dragstart="onDragStart(row, $event)"
                @dragend="onDragEnd"
                @click.stop="openDetail(row)"
              >
                <div class="flex items-start gap-1.5">
                  <span class="mt-1 shrink-0 w-1.5 h-1.5 rounded-full" :class="getCardColor(row.trangThai).dot"></span>
                  <div class="min-w-0">
                    <div class="text-xs font-bold truncate leading-tight" :class="getCardColor(row.trangThai).text" :title="row.monHoc.ten">
                      {{ row.monHoc.ten }}
                    </div>
                    <div class="text-[10px] truncate leading-tight opacity-75" :class="getCardColor(row.trangThai).text">
                      {{ row.lop.ma }} · {{ row.phongHoc?.ten }}
                    </div>
                  </div>
                </div>
              </div>

              <!-- Empty cell add hint -->
              <div
                v-if="cellItems(day.value, shift.id).length === 0"
                class="absolute inset-0 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity"
                @click.stop="openCreate(day.value, shift.id)"
              >
                <Plus :size="16" class="text-(--text-muted)" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Side panel ── -->
      <transition name="panel-slide">
        <!-- Detail panel -->
        <div
          v-if="selectedRow"
          class="w-80 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg flex flex-col overflow-hidden"
          style="max-height: calc(100vh - 200px)"
        >
          <!-- Panel header -->
          <div class="p-4 border-b border-(--border-default) flex items-center justify-between" :class="getCardColor(selectedRow.trangThai).bg">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full shrink-0" :class="getCardColor(selectedRow.trangThai).dot"></span>
              <span class="text-xs font-bold uppercase tracking-wide" :class="getCardColor(selectedRow.trangThai).text">
                {{ getStatusLabel('timetable', selectedRow.trangThai) }}
              </span>
            </div>
            <button @click="closeDetail" class="text-(--text-muted) hover:text-(--text-heading) p-1 rounded-lg hover:bg-(--surface-input) transition-colors">
              <X :size="16" />
            </button>
          </div>

          <!-- Content -->
          <div class="p-4 flex-1 overflow-y-auto space-y-4">
            <div>
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-0.5">Môn học</p>
              <p class="font-bold text-(--text-heading)">{{ selectedRow.monHoc.ten }}</p>
              <p class="text-xs text-(--text-muted)">{{ selectedRow.monHoc.ma }}</p>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
                <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-1">Lớp</p>
                <p class="text-sm font-bold text-(--text-heading)">{{ selectedRow.lop.ma }}</p>
              </div>
              <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
                <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-1">Phòng</p>
                <p class="text-sm font-bold text-(--text-heading)">{{ selectedRow.phongHoc?.ten }}</p>
              </div>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-1">Thời gian</p>
              <p class="text-sm font-bold text-(--text-heading)">{{ thuLabel(selectedRow.thuTrongTuan) }}</p>
              <p class="text-xs text-(--text-muted)">{{ selectedRow.caHoc?.tenCa }} · {{ selectedRow.caHoc?.gioBatDau }}–{{ selectedRow.caHoc?.gioKetThuc }}</p>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-1">Giảng viên</p>
              <p class="text-sm font-bold text-(--text-heading)">{{ selectedRow.giaoVien?.ten }}</p>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-1">Học kỳ · Thời gian</p>
              <p class="text-sm font-bold text-(--text-heading)">{{ selectedRow.hocKy?.ten }}</p>
              <p class="text-xs text-(--text-muted)">{{ selectedRow.ngayBatDau }} → {{ selectedRow.ngayKetThuc }}</p>
            </div>

            <div class="flex items-center justify-between">
              <span class="text-xs text-(--text-muted) font-mono">{{ selectedRow.maTkb }}</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="p-3 border-t border-(--border-default) bg-(--surface-modal) flex flex-col gap-2">
            <GlassButton
              v-if="selectedRow.trangThai === 'nhap'"
              variant="primary" class="w-full h-9 justify-center text-sm"
              @click="publishRow(selectedRow)"
            >
              <CheckCircle :size="15" class="mr-1" /> Xuất bản lịch
            </GlassButton>
            <GlassButton
              variant="secondary" class="w-full h-9 justify-center text-sm !text-red-500 !border-red-300 hover:!bg-red-50 dark:hover:!bg-red-900/20"
              @click="deleteRow(selectedRow)"
            >
              Xóa lịch này
            </GlassButton>
          </div>
        </div>

        <!-- Create panel -->
        <div
          v-else-if="showCreatePanel"
          class="w-80 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg flex flex-col overflow-hidden"
          style="max-height: calc(100vh - 200px)"
        >
          <div class="p-4 border-b border-(--border-default) flex items-center justify-between bg-(--surface-solid)">
            <h3 class="font-bold text-(--text-heading) text-sm">Tạo lịch mới</h3>
            <button @click="closeCreatePanel" class="text-(--text-muted) hover:text-(--text-heading) p-1 rounded-lg hover:bg-(--surface-input) transition-colors">
              <X :size="16" />
            </button>
          </div>

          <div class="p-4 flex-1 overflow-y-auto space-y-3">
            <div>
              <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Học kỳ *</label>
              <select v-model="form.hocKy.ten" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                <option v-for="hk in hocKyOptions" :key="hk" :value="hk">{{ hk }}</option>
              </select>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Mã lớp *</label>
              <input v-model="form.lop.ma" type="text" placeholder="VD: SE1601" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Môn học *</label>
              <input v-model="form.monHoc.ten" type="text" placeholder="Tên môn học" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Mã môn</label>
              <input v-model="form.monHoc.ma" type="text" placeholder="VD: PRO192" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Giảng viên *</label>
              <input v-model="form.giaoVien.ten" type="text" placeholder="Tên giảng viên" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>
            <div class="grid grid-cols-2 gap-2">
              <div>
                <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Thứ *</label>
                <select v-model="form.thuTrongTuan" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                  <option v-for="t in thuTrongTuanOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
                </select>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Ca học *</label>
                <select v-model="form.caHoc" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                  <option v-for="ca in caHocCatalog" :key="ca.id" :value="ca">{{ ca.tenCa }}</option>
                </select>
              </div>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Phòng học *</label>
              <input v-model="form.phongHoc.ten" type="text" placeholder="VD: P.302, Lab 2" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>
            <div class="grid grid-cols-2 gap-2">
              <div>
                <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Từ ngày</label>
                <input v-model="form.ngayBatDau" type="date" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Đến ngày</label>
                <input v-model="form.ngayKetThuc" type="date" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
            </div>

            <!-- Conflict check inline -->
            <button
              class="w-full h-9 text-xs font-semibold border border-amber-300 text-amber-700 dark:text-amber-400 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-900/20 flex items-center justify-center gap-1.5 transition-colors"
              @click="checkConflicts"
            >
              <AlertTriangle :size="14" /> Kiểm tra xung đột
            </button>
            <div v-if="conflictPreview.length > 0" class="space-y-1.5">
              <div v-for="(c, i) in conflictPreview" :key="i" class="bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-700 rounded-lg p-2 text-xs text-amber-700 dark:text-amber-300">
                <span class="font-semibold">[{{ c.loai }}]</span> {{ c.text }}
              </div>
            </div>
          </div>

          <div class="p-3 border-t border-(--border-default) bg-(--surface-modal) flex flex-col gap-2">
            <GlassButton variant="secondary" class="w-full h-9 justify-center text-sm" @click="saveDraft">
              <BookOpen :size="15" class="mr-1" /> Lưu nháp
            </GlassButton>
            <GlassButton variant="primary" class="w-full h-9 justify-center text-sm" @click="publishDraft">
              <CheckCircle :size="15" class="mr-1" /> Xuất bản ngay
            </GlassButton>
          </div>
        </div>
      </transition>
    </div>

    <ConfirmActionDialog
      v-if="confirmAction"
      :show="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
  </div>
</template>

<style scoped>
.panel-slide-enter-active,
.panel-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.panel-slide-enter-from,
.panel-slide-leave-to {
  opacity: 0;
  transform: translateX(16px);
}
</style>
