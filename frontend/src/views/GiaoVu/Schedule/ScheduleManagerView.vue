<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  CalendarRange, Search, Plus, CheckCircle, AlertTriangle, BookOpen, X, Loader2, Pencil,
} from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { scheduleApi } from '@/services/scheduleApi'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

// ── Catalog (UI config, not API data) ─────────────────────────────
const caHocCatalog = [
  { id: 'ca1', maCaHoc: 1, tenCa: 'Ca 1', buoi: 'Sáng', gioBatDau: '07:30', gioKetThuc: '09:00', thuTu: 1 },
  { id: 'ca2', maCaHoc: 2, tenCa: 'Ca 2', buoi: 'Sáng', gioBatDau: '09:10', gioKetThuc: '10:40', thuTu: 2 },
  { id: 'ca3', maCaHoc: 3, tenCa: 'Ca 3', buoi: 'Sáng', gioBatDau: '10:50', gioKetThuc: '12:20', thuTu: 3 },
  { id: 'ca4', maCaHoc: 4, tenCa: 'Ca 4', buoi: 'Chiều', gioBatDau: '13:00', gioKetThuc: '14:30', thuTu: 4 },
  { id: 'ca5', maCaHoc: 5, tenCa: 'Ca 5', buoi: 'Chiều', gioBatDau: '14:40', gioKetThuc: '16:10', thuTu: 5 },
  { id: 'ca6', maCaHoc: 6, tenCa: 'Ca 6', buoi: 'Tối', gioBatDau: '18:00', gioKetThuc: '19:30', thuTu: 6 },
]
const thuTrongTuanOptions = [
  { value: 2, label: 'Thứ 2' }, { value: 3, label: 'Thứ 3' },
  { value: 4, label: 'Thứ 4' }, { value: 5, label: 'Thứ 5' },
  { value: 6, label: 'Thứ 6' }, { value: 7, label: 'Thứ 7' },
]

// ── State ─────────────────────────────────────────────────────────
const rows = ref([])
const loading = ref(false)
const selectedRow = ref(null)
const showFormModal = ref(false)
const formMode = ref('create')
const confirmAction = ref(null)
const searchQuery = ref('')
const filterHocKy = ref('')
const filterTrangThai = ref('')
const submitting = ref(false)
const conflictPreview = ref([])
const checkingConflict = ref(false)

// drag state
const draggingRow = ref(null)
const dragOverCell = ref(null)

const days = thuTrongTuanOptions
const shifts = caHocCatalog

// ── Lookup ────────────────────────────────────────────────────────
const hocKyOptions = computed(() => {
  const set = new Set(rows.value.map(r => r.hocKy?.ten).filter(Boolean))
  return [...set]
})

const emptyForm = () => ({
  hocKy: { ma: '', ten: '' },
  lop: { ma: '', ten: '' },
  monHoc: { ma: '', ten: '' },
  giaoVien: { ma: '', ten: '' },
  thuTrongTuan: 2,
  caHoc: caHocCatalog[0],
  phongHoc: { ma: '', ten: '' },
  ngayBatDau: '',
  ngayKetThuc: '',
  trangThai: 'nhap',
})
const form = ref(emptyForm())
const editingId = ref(null)

// ── Data loading ──────────────────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const data = await scheduleApi.list({
      TrangThai: filterTrangThai.value || undefined,
      MaHocKy: filterHocKy.value || undefined,
    })
    rows.value = (Array.isArray(data) ? data : data?.items || data?.data || []).map(beToView)
  } catch {
    popupStore.error('Lỗi tải dữ liệu', 'Không thể tải danh sách thời khóa biểu.')
  } finally {
    loading.value = false
  }
}

function beToView(be) {
  const caEntry = caHocCatalog.find(c => c.maCaHoc === (typeof be.maCaHoc === 'number' ? be.maCaHoc : Number(be.maCaHoc)))
  return {
    id: be.maTkb?.toString() || String(be.maTkb),
    maTkb: be.maTkb?.toString() || String(be.maTkb),
    maKhoaHoc: be.maKhoaHoc,
    tieuDeKhoaHoc: be.tieuDeKhoaHoc,
    hocKy: { ma: be.maHocKy, ten: be.tenHocKy },
    lop: { ma: be.maCodeLop || be.maLop, ten: be.tenLop },
    monHoc: { ma: be.maCodeMonHoc || be.maMonHoc, ten: be.tenMonHoc },
    giaoVien: { ma: be.maGiaoVien, ten: be.tenGiaoVien, email: be.emailGiaoVien },
    thuTrongTuan: be.thuTrongTuan,
    caHoc: caEntry ? { ...caEntry } : { id: be.maCaHoc ? `ca${be.maCaHoc}` : '', tenCa: be.tenCa, gioBatDau: be.gioBatDau, gioKetThuc: be.gioKetThuc },
    phongHoc: { ma: be.maCodePhong || be.maPhong, ten: be.tenPhong },
    ngayBatDau: be.ngayBatDau?.split?.('T')[0] || be.ngayBatDau,
    ngayKetThuc: be.ngayKetThuc?.split?.('T')[0] || be.ngayKetThuc,
    trangThai: be.trangThai || 'nhap',
  }
}

function viewToBe(view) {
  return {
    maKhoaHoc: view.maKhoaHoc,
    tieuDeKhoaHoc: view.tieuDeKhoaHoc || view.monHoc?.ten || view.lop?.ma,
    maHocKy: view.hocKy?.ma,
    tenHocKy: view.hocKy?.ten,
    maLop: view.lop?.ma,
    tenLop: view.lop?.ten,
    maCodeLop: view.lop?.ma,
    maMonHoc: view.monHoc?.ma,
    maCodeMonHoc: view.monHoc?.ma,
    tenMonHoc: view.monHoc?.ten,
    maGiaoVien: view.giaoVien?.ma,
    tenGiaoVien: view.giaoVien?.ten,
    thuTrongTuan: view.thuTrongTuan,
    maCaHoc: view.caHoc?.maCaHoc || Number(view.caHoc?.id?.replace('ca', '')) || 0,
    tenCa: view.caHoc?.tenCa,
    gioBatDau: view.caHoc?.gioBatDau,
    gioKetThuc: view.caHoc?.gioKetThuc,
    maPhong: view.phongHoc?.ma,
    maCodePhong: view.phongHoc?.ma,
    tenPhong: view.phongHoc?.ten,
    ngayBatDau: view.ngayBatDau,
    ngayKetThuc: view.ngayKetThuc,
    trangThai: view.trangThai || 'nhap',
  }
}

onMounted(loadData)

// ── Filtered rows ─────────────────────────────────────────────────
const filteredRows = computed(() => {
  let list = rows.value
  if (filterHocKy.value) list = list.filter(r => r.hocKy?.ten === filterHocKy.value)
  if (filterTrangThai.value) list = list.filter(r => r.trangThai === filterTrangThai.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(r =>
      r.maTkb?.toLowerCase().includes(q) ||
      r.monHoc?.ten?.toLowerCase().includes(q) ||
      r.giaoVien?.ten?.toLowerCase().includes(q) ||
      r.lop?.ma?.toLowerCase().includes(q)
    )
  }
  return list
})

// ── Grid cell lookup ──────────────────────────────────────────────
function cellItems(thu, caId) {
  return filteredRows.value.filter(r => r.thuTrongTuan === thu && r.caHoc?.id === caId)
}

// ── Card colors ───────────────────────────────────────────────────
const cardColors = {
  nhap: { bg: 'bg-(--surface-input)', border: 'border-(--border-default)', text: 'text-(--text-body)', dot: 'bg-(--text-muted)' },
  da_xuat_ban: { bg: 'bg-(--color-success-bg)', border: 'border-(--border-default)', text: 'text-(--color-success-text)', dot: 'bg-emerald-500' },
  da_huy: { bg: 'bg-(--color-danger-bg)', border: 'border-(--border-default)', text: 'text-(--color-danger-text)', dot: 'bg-red-400' },
}
function getCardColor(status) { return cardColors[status] || cardColors.nhap }

// ── Summary cards ─────────────────────────────────────────────────
const summaryCards = computed(() => [
  { label: 'Bản nháp', value: rows.value.filter(r => r.trangThai === 'nhap').length, color: 'text-(--text-muted)', bg: 'bg-(--surface-input)' },
  { label: 'Đã xuất bản', value: rows.value.filter(r => r.trangThai === 'da_xuat_ban').length, color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg) border-(--border-default)' },
  { label: 'Tổng lịch', value: rows.value.length, color: 'text-(--lg-primary)', bg: 'bg-(--surface-input)' },
  { label: 'Đã hủy', value: rows.value.filter(r => r.trangThai === 'da_huy').length, color: 'text-(--color-danger-text)', bg: 'bg-(--color-danger-bg) border-(--border-default)' },
])

// ── Drag & Drop ───────────────────────────────────────────────────
function onDragStart(row, event) {
  draggingRow.value = row
  event.dataTransfer.effectAllowed = 'move'
}

function onDragOver(thu, caId, event) {
  event.preventDefault()
  event.dataTransfer.dropEffect = 'move'
  dragOverCell.value = `${thu}-${caId}`
}

function onDragLeave() { dragOverCell.value = null }

async function onDrop(thu, caId, event) {
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
    message: `Chuyển "${row.monHoc?.ten}" (${row.lop?.ma}) sang ${thuLabel} · ${caObj.tenCa} (${caObj.gioBatDau}–${caObj.gioKetThuc})?`,
    label: 'Di chuyển',
    variant: 'primary',
    run: async () => {
      confirmAction.value = null
      const updateData = viewToBe(row)
      updateData.thuTrongTuan = thu
      updateData.maCaHoc = caObj.maCaHoc
      updateData.tenCa = caObj.tenCa
      updateData.gioBatDau = caObj.gioBatDau
      updateData.gioKetThuc = caObj.gioKetThuc
      try {
        await scheduleApi.update(row.id, updateData)
        const idx = rows.value.findIndex(r => r.id === row.id)
        if (idx !== -1) {
          rows.value[idx].thuTrongTuan = thu
          rows.value[idx].caHoc = { ...caObj }
          if (selectedRow.value?.id === row.id) selectedRow.value = { ...rows.value[idx] }
        }
        popupStore.success('Đã di chuyển', `Lịch "${row.monHoc?.ten}" đã được chuyển sang ${thuLabel} · ${caObj.tenCa}.`)
      } catch {
        popupStore.error('Lỗi', 'Không thể di chuyển lịch.')
      }
    },
  }
}

function onDragEnd() {
  draggingRow.value = null
  dragOverCell.value = null
}

// ── Detail panel ──────────────────────────────────────────────────
function openDetail(row) {
  selectedRow.value = row
  showFormModal.value = false
}

function closeDetail() { selectedRow.value = null }

// ── Create / Edit modal ───────────────────────────────────────────
function openCreate(thu, caId) {
  formMode.value = 'create'
  editingId.value = null
  form.value = emptyForm()
  if (thu) form.value.thuTrongTuan = thu
  if (caId) form.value.caHoc = caHocCatalog.find(c => c.id === caId) || caHocCatalog[0]
  conflictPreview.value = []
  selectedRow.value = null
  showFormModal.value = true
}

function openEdit(row) {
  formMode.value = 'edit'
  editingId.value = row.id
  form.value = {
    maKhoaHoc: row.maKhoaHoc,
    tieuDeKhoaHoc: row.tieuDeKhoaHoc,
    hocKy: { ...row.hocKy },
    lop: { ...row.lop },
    monHoc: { ...row.monHoc },
    giaoVien: { ...row.giaoVien },
    thuTrongTuan: row.thuTrongTuan,
    caHoc: row.caHoc ? { ...row.caHoc } : caHocCatalog[0],
    phongHoc: { ...row.phongHoc },
    ngayBatDau: row.ngayBatDau,
    ngayKetThuc: row.ngayKetThuc,
    trangThai: row.trangThai,
  }
  conflictPreview.value = []
  selectedRow.value = null
  showFormModal.value = true
}

function closeFormModal() {
  showFormModal.value = false
  form.value = emptyForm()
  editingId.value = null
  conflictPreview.value = []
}

// ── Conflict check via API ────────────────────────────────────────
async function checkConflicts() {
  conflictPreview.value = []
  const { thuTrongTuan, caHoc, giaoVien, lop, phongHoc } = form.value
  if (!thuTrongTuan || !caHoc) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn Thứ và Ca học trước khi kiểm tra.')
    return
  }
  checkingConflict.value = true
  try {
    const payload = {
      thuTrongTuan,
      maCaHoc: caHoc.maCaHoc || Number(caHoc.id?.replace('ca', '')),
      maGiaoVien: giaoVien.ma || undefined,
      maLop: lop.ma || undefined,
      maPhong: phongHoc.ma || undefined,
      excludeMaTkb: formMode.value === 'edit' ? Number(editingId.value) : undefined,
    }
    const result = await scheduleApi.checkConflicts(payload)
    if (result.hasConflict) {
      conflictPreview.value = (result.conflicts || []).map(c => ({ loai: c.type, text: c.message }))
    } else {
      popupStore.success('Không xung đột', 'Lịch này không xung đột với dữ liệu hiện tại.')
    }
  } catch {
    popupStore.error('Lỗi', 'Không thể kiểm tra xung đột.')
  } finally {
    checkingConflict.value = false
  }
}

// ── Save ──────────────────────────────────────────────────────────
function validateForm() {
  if (!form.value.lop?.ma?.trim()) { popupStore.warning('Thiếu thông tin', 'Vui lòng nhập Mã lớp.'); return false }
  if (!form.value.monHoc?.ten?.trim()) { popupStore.warning('Thiếu thông tin', 'Vui lòng nhập Tên môn học.'); return false }
  if (!form.value.giaoVien?.ten?.trim()) { popupStore.warning('Thiếu thông tin', 'Vui lòng nhập Giảng viên.'); return false }
  if (!form.value.phongHoc?.ten?.trim()) { popupStore.warning('Thiếu thông tin', 'Vui lòng nhập Phòng học.'); return false }
  if (!form.value.ngayBatDau) { popupStore.warning('Thiếu thông tin', 'Vui lòng chọn Ngày bắt đầu.'); return false }
  if (!form.value.ngayKetThuc) { popupStore.warning('Thiếu thông tin', 'Vui lòng chọn Ngày kết thúc.'); return false }
  if (form.value.ngayBatDau > form.value.ngayKetThuc) { popupStore.warning('Ngày không hợp lệ', 'Ngày bắt đầu phải trước ngày kết thúc.'); return false }
  return true
}

async function saveSchedule(publishNow = false) {
  if (!validateForm()) return
  submitting.value = true
  try {
    const payload = viewToBe(form.value)
    if (publishNow) payload.trangThai = 'da_xuat_ban'

    if (formMode.value === 'edit') {
      await scheduleApi.update(editingId.value, payload)
      await loadData()
      closeFormModal()
      popupStore.success('Đã cập nhật', 'Lịch học đã được cập nhật.')
    } else {
      await scheduleApi.create(payload)
      await loadData()
      closeFormModal()
      popupStore.success('Đã tạo', 'Lịch học mới đã được tạo.')
    }
  } catch {
    popupStore.error('Lỗi', 'Không thể lưu lịch học.')
  } finally {
    submitting.value = false
  }
}

function saveDraft() { saveSchedule(false) }
function publishDraft() { saveSchedule(true) }

// ── Publish existing row ──────────────────────────────────────────
function publishRow(row) {
  confirmAction.value = {
    title: 'Xuất bản lịch này?',
    message: `"${row.monHoc?.ten}" (${row.lop?.ma}) sẽ được công bố tới sinh viên và giảng viên.`,
    label: 'Xuất bản',
    variant: 'primary',
    run: async () => {
      confirmAction.value = null
      try {
        await scheduleApi.update(row.id, { ...viewToBe(row), trangThai: 'da_xuat_ban' })
        const idx = rows.value.findIndex(r => r.id === row.id)
        if (idx !== -1) rows.value[idx].trangThai = 'da_xuat_ban'
        if (selectedRow.value?.id === row.id) selectedRow.value = { ...rows.value.find(r => r.id === row.id) }
        popupStore.success('Đã xuất bản', `Lịch "${row.monHoc?.ten}" đã được công bố.`)
      } catch { popupStore.error('Lỗi', 'Không thể xuất bản lịch.') }
    },
  }
}

function cancelRow(row) {
  confirmAction.value = {
    title: 'Hủy lịch này?',
    message: `Lịch ${row.maTkb} · "${row.monHoc?.ten}" (${row.lop?.ma}) sẽ bị hủy.`,
    label: 'Hủy lịch',
    variant: 'danger',
    run: async () => {
      confirmAction.value = null
      try {
        await scheduleApi.cancel(row.id)
        const idx = rows.value.findIndex(r => r.id === row.id)
        if (idx !== -1) rows.value[idx].trangThai = 'da_huy'
        if (selectedRow.value?.id === row.id) selectedRow.value = { ...rows.value.find(r => r.id === row.id) }
        popupStore.success('Đã hủy', `Lịch ${row.maTkb} đã được hủy.`)
      } catch { popupStore.error('Lỗi', 'Không thể hủy lịch.') }
    },
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
      <GlassButton variant="secondary" class="h-9 text-sm" @click="loadData">
        <Loader2 :size="14" class="mr-1" :class="{ 'animate-spin': loading }" /> Làm mới
      </GlassButton>
      <!-- Legend -->
      <div class="flex items-center gap-3 ml-auto text-xs text-(--text-muted)">
        <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded-full bg-(--text-muted) inline-block"></span>Bản nháp</span>
        <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded-full bg-emerald-500 inline-block"></span>Đã xuất bản</span>
        <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded-full bg-red-400 inline-block"></span>Đã hủy</span>
      </div>
    </div>

    <!-- ── Loading state ── -->
    <div v-if="loading && rows.length === 0" class="flex items-center justify-center py-16">
      <Loader2 :size="36" class="text-(--lg-primary) animate-spin" />
    </div>

    <!-- ── Empty state ── -->
    <div v-else-if="!loading && rows.length === 0" class="flex flex-col items-center justify-center py-16 text-(--text-muted)">
      <CalendarRange :size="48" class="mb-3 opacity-40" />
      <p class="text-lg font-semibold text-(--text-body)">Chưa có lịch học nào</p>
      <p class="text-sm mt-1">Nhấn "Tạo lịch" để thêm lịch học mới.</p>
    </div>

    <!-- ── Main calendar area ── -->
    <div v-else class="flex gap-4 items-start">

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
              <div
                v-if="dragOverCell === `${day.value}-${shift.id}`"
                class="absolute inset-1 border-2 border-dashed border-(--lg-primary) rounded-lg pointer-events-none z-0 opacity-60"
              ></div>

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
                    <div class="text-xs font-bold truncate leading-tight" :class="getCardColor(row.trangThai).text" :title="row.monHoc?.ten">
                      {{ row.monHoc?.ten }}
                    </div>
                    <div class="text-[10px] truncate leading-tight opacity-75" :class="getCardColor(row.trangThai).text">
                      {{ row.lop?.ma }} · {{ row.phongHoc?.ten }}
                    </div>
                  </div>
                </div>
              </div>

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

      <!-- ── Detail side panel ── -->
      <transition name="panel-slide">
        <div
          v-if="selectedRow"
          class="w-80 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg flex flex-col overflow-hidden"
          style="max-height: calc(100vh - 200px)"
        >
          <div class="p-4 border-b border-(--border-default) flex items-center justify-between" :class="getCardColor(selectedRow.trangThai).bg">
            <div class="flex items-center gap-2">
              <span class="w-2.5 h-2.5 rounded-full shrink-0" :class="getCardColor(selectedRow.trangThai).dot"></span>
              <span class="text-xs font-bold uppercase tracking-wide" :class="getCardColor(selectedRow.trangThai).text">
                {{ selectedRow.trangThai === 'nhap' ? 'Bản nháp' : selectedRow.trangThai === 'da_xuat_ban' ? 'Đã xuất bản' : 'Đã hủy' }}
              </span>
            </div>
            <div class="flex items-center gap-1">
              <button @click="openEdit(selectedRow)" class="text-(--text-muted) hover:text-(--lg-primary) p-1 rounded-lg hover:bg-(--surface-input) transition-colors">
                <Pencil :size="14" />
              </button>
              <button @click="closeDetail" class="text-(--text-muted) hover:text-(--text-heading) p-1 rounded-lg hover:bg-(--surface-input) transition-colors">
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-4 flex-1 overflow-y-auto space-y-4">
            <div>
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-0.5">Môn học</p>
              <p class="font-bold text-(--text-heading)">{{ selectedRow.monHoc?.ten }}</p>
              <p class="text-xs text-(--text-muted)">{{ selectedRow.monHoc?.ma }}</p>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
                <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wider mb-1">Lớp</p>
                <p class="text-sm font-bold text-(--text-heading)">{{ selectedRow.lop?.ma }}</p>
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

          <div class="p-3 border-t border-(--border-default) bg-(--surface-modal) flex flex-col gap-2">
            <GlassButton
              v-if="selectedRow.trangThai === 'nhap'"
              variant="primary" class="w-full h-9 justify-center text-sm"
              @click="publishRow(selectedRow)"
            >
              <CheckCircle :size="15" class="mr-1" /> Xuất bản lịch
            </GlassButton>
            <GlassButton
              v-if="selectedRow.trangThai !== 'da_huy'"
              variant="secondary" class="w-full h-9 justify-center text-sm !text-(--color-danger-text) border-(--border-default) hover:!bg-(--color-danger-bg)"
              @click="cancelRow(selectedRow)"
            >
              Hủy lịch này
            </GlassButton>
          </div>
        </div>
      </transition>
    </div>

    <!-- ════════════════════════════════════════════════════════════ -->
    <!-- ── Create / Edit Modal ── -->
    <!-- ════════════════════════════════════════════════════════════ -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div
          v-if="showFormModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="closeFormModal"
        >
          <div class="w-full max-w-xl lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <!-- Header -->
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">
                {{ formMode === 'create' ? 'Tạo lịch học mới' : 'Chỉnh sửa lịch học' }}
              </h3>
              <button @click="closeFormModal" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors">
                <X :size="18" />
              </button>
            </div>

            <!-- Body -->
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Học kỳ *</label>
                  <input v-model="form.hocKy.ten" type="text" placeholder="VD: Học kỳ 1 - 2025-2026" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã học kỳ</label>
                  <input v-model="form.hocKy.ma" type="text" placeholder="VD: HK1-2025" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã lớp *</label>
                  <input v-model="form.lop.ma" type="text" placeholder="VD: SE1601" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Tên lớp</label>
                  <input v-model="form.lop.ten" type="text" placeholder="Tên lớp" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Môn học *</label>
                  <input v-model="form.monHoc.ten" type="text" placeholder="Tên môn học" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã môn</label>
                  <input v-model="form.monHoc.ma" type="text" placeholder="VD: PRO192" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>

              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Giảng viên *</label>
                <input v-model="form.giaoVien.ten" type="text" placeholder="Tên giảng viên" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Thứ *</label>
                  <select v-model="form.thuTrongTuan" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                    <option v-for="t in thuTrongTuanOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ca học *</label>
                  <select v-model="form.caHoc" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                    <option v-for="ca in caHocCatalog" :key="ca.id" :value="ca">{{ ca.tenCa }} ({{ ca.gioBatDau }}–{{ ca.gioKetThuc }})</option>
                  </select>
                </div>
              </div>

              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Phòng học *</label>
                <input v-model="form.phongHoc.ten" type="text" placeholder="VD: P.302, Lab 2" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Từ ngày *</label>
                  <input v-model="form.ngayBatDau" type="date" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Đến ngày *</label>
                  <input v-model="form.ngayKetThuc" type="date" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>

              <!-- Conflict check -->
              <button
                :disabled="checkingConflict"
                class="w-full h-9 text-xs font-semibold border border-(--border-default) text-(--color-warning-text) rounded-lg hover:bg-(--color-warning-bg) flex items-center justify-center gap-1.5 transition-colors disabled:opacity-50"
                @click="checkConflicts"
              >
                <Loader2 v-if="checkingConflict" :size="14" class="animate-spin" />
                <AlertTriangle v-else :size="14" />
                Kiểm tra xung đột
              </button>
              <div v-if="conflictPreview.length > 0" class="space-y-1.5">
                <div v-for="(c, i) in conflictPreview" :key="i" class="bg-(--color-danger-bg) border border-(--border-default) rounded-lg p-2 text-xs text-(--color-danger-text)">
                  <span class="font-semibold">[{{ c.loai }}]</span> {{ c.text }}
                </div>
              </div>
            </div>

            <!-- Footer -->
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeFormModal">
                Hủy
              </GlassButton>
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" :disabled="submitting" @click="saveDraft">
                <BookOpen :size="15" class="mr-1.5" /> Lưu nháp
              </GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="publishDraft">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <CheckCircle v-else :size="15" class="mr-1.5" />
                {{ submitting ? 'Đang lưu...' : 'Xuất bản ngay' }}
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

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
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
