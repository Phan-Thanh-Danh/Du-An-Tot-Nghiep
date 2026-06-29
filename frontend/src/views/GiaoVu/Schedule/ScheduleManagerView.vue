<script setup>
import { ref, computed } from 'vue'
import {
  CalendarRange, Search, Plus, CheckCircle, AlertTriangle, BookOpen, X,
} from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import {
  staffScheduleRows, caHocCatalog, thuTrongTuanOptions,
} from '@/mocks/scheduleAttendanceMockData'
import { getStatusLabel, getStatusVariant } from '@/utils/statusLabels'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

// ── State ──────────────────────────────────────────────────────
const rows = ref(staffScheduleRows.map(r => ({ ...r })))
const showPanel = ref(false)
const panelMode = ref('create') // create | edit
const editingRow = ref(null)
const confirmAction = ref(null)
const searchQuery = ref('')
const filterHocKy = ref('')
const filterTrangThai = ref('')
const selectedRow = ref(null)
const conflictPreview = ref([])

const hocKyOptions = ['Spring 2026', 'Summer 2026', 'Fall 2025']

const emptyForm = () => ({
  maTkb: '',
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

// ── Computed ───────────────────────────────────────────────────
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

const summaryCards = computed(() => [
  { label: 'Lịch nháp', value: rows.value.filter(r => r.trangThai === 'nhap').length, border: 'border-[var(--border-default)]' },
  { label: 'Đã xuất bản', value: rows.value.filter(r => r.trangThai === 'da_xuat_ban').length, border: 'border-emerald-500' },
  { label: 'Buổi học tuần này', value: rows.value.filter(r => r.trangThai === 'da_xuat_ban').length * 3, border: 'border-blue-500' },
  { label: 'Xung đột cần xử lý', value: 1, border: 'border-amber-500' },
])

const thuLabel = (thu) => {
  const found = thuTrongTuanOptions.find(t => t.value === thu)
  return found ? found.label : `Thứ ${thu}`
}

// ── Panel ──────────────────────────────────────────────────────
function openCreate() {
  form.value = emptyForm()
  panelMode.value = 'create'
  editingRow.value = null
  conflictPreview.value = []
  showPanel.value = true
}

function openEdit(row) {
  form.value = { ...row, caHoc: { ...row.caHoc }, hocKy: { ...row.hocKy }, lop: { ...row.lop }, monHoc: { ...row.monHoc }, giaoVien: { ...row.giaoVien }, phongHoc: { ...row.phongHoc } }
  panelMode.value = 'edit'
  editingRow.value = row
  conflictPreview.value = []
  showPanel.value = true
}

function closePanel() { showPanel.value = false }

// ── Conflict check ─────────────────────────────────────────────
function checkConflicts() {
  conflictPreview.value = []
  const { thuTrongTuan, caHoc, giaoVien, lop, phongHoc } = form.value
  if (!thuTrongTuan || !caHoc) return

  const existing = rows.value.filter(r => {
    if (editingRow.value && r.id === editingRow.value.id) return false
    return r.thuTrongTuan === thuTrongTuan && r.caHoc?.id === caHoc.id && r.trangThai !== 'da_huy'
  })

  existing.forEach(r => {
    if (giaoVien.ma && r.giaoVien.ma === giaoVien.ma) {
      conflictPreview.value.push({ loai: 'Giảng viên', mo_ta: `${giaoVien.ten} đã có lịch ${r.maTkb} · ${thuLabel(r.thuTrongTuan)} ${r.caHoc.tenCa}`, de_xuat: 'Đổi sang ca khác hoặc chọn giảng viên khác' })
    }
    if (lop.ma && r.lop.ma === lop.ma) {
      conflictPreview.value.push({ loai: 'Lớp học', mo_ta: `Lớp ${lop.ma} đã có lịch ${r.maTkb} · ${thuLabel(r.thuTrongTuan)} ${r.caHoc.tenCa}`, de_xuat: 'Đổi sang thứ hoặc ca khác' })
    }
    if (phongHoc.ma && r.phongHoc.ma === phongHoc.ma) {
      conflictPreview.value.push({ loai: 'Phòng học', mo_ta: `Phòng ${phongHoc.ten} đã được dùng bởi ${r.maTkb} · ${thuLabel(r.thuTrongTuan)} ${r.caHoc.tenCa}`, de_xuat: 'Đổi sang phòng khác còn trống' })
    }
  })

  if (conflictPreview.value.length === 0) {
    popupStore.success('Không có xung đột', 'Lịch này không xung đột với bất kỳ lịch hiện tại nào.')
  }
}

// ── Save ───────────────────────────────────────────────────────
function saveDraft() {
  if (!form.value.monHoc.ten || !form.value.lop.ma) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập đầy đủ Lớp và Môn học.')
    return
  }
  form.value.trangThai = 'nhap'
  if (panelMode.value === 'create') {
    const newId = `TKB-${String(rows.value.length + 1).padStart(3, '0')}`
    rows.value.push({ ...form.value, id: newId, maTkb: newId })
  } else {
    const idx = rows.value.findIndex(r => r.id === editingRow.value.id)
    if (idx !== -1) rows.value[idx] = { ...form.value, id: editingRow.value.id }
  }
  showPanel.value = false
  popupStore.success('Đã lưu', 'Lịch đã được lưu dưới dạng bản nháp.')
}

function confirmPublish() {
  if (conflictPreview.value.length > 0) {
    popupStore.warning('Có xung đột', 'Vui lòng giải quyết xung đột trước khi xuất bản.')
    return
  }
  confirmAction.value = {
    title: 'Xuất bản lịch học?',
    message: `Lịch "${form.value.monHoc.ten}" cho lớp ${form.value.lop.ma} sẽ được công bố đến sinh viên và giảng viên.`,
    label: 'Xuất bản',
    variant: 'primary',
    run: () => {
      form.value.trangThai = 'da_xuat_ban'
      saveDraft()
      confirmAction.value = null
      popupStore.success('Đã xuất bản', 'Lịch học đã được công bố.')
    }
  }
}

function confirmDelete(row) {
  confirmAction.value = {
    title: 'Xóa lịch học?',
    message: `Lịch ${row.maTkb} · ${row.monHoc.ten} (${row.lop.ma}) sẽ bị xóa vĩnh viễn.`,
    label: 'Xóa',
    variant: 'danger',
    run: () => {
      const idx = rows.value.findIndex(r => r.id === row.id)
      if (idx !== -1) rows.value.splice(idx, 1)
      confirmAction.value = null
      if (selectedRow.value?.id === row.id) selectedRow.value = null
      popupStore.success('Đã xóa', 'Lịch học đã được xóa.')
    }
  }
}

function confirmGenerateSessions(row) {
  const weeks = Math.round((new Date(row.ngayKetThuc) - new Date(row.ngayBatDau)) / (7 * 86400000))
  confirmAction.value = {
    title: 'Sinh buổi học?',
    message: `Sẽ sinh khoảng ${weeks} buổi học cho lịch ${row.maTkb} (${row.ngayBatDau} → ${row.ngayKetThuc}).`,
    label: 'Sinh buổi học',
    variant: 'primary',
    run: () => {
      confirmAction.value = null
      popupStore.success('Thành công', `Đã tạo ${weeks} buổi học cho ${row.maTkb}.`)
    }
  }
}
</script>

<template>
  <div class="staff-schedule max-w-7xl mx-auto space-y-5">
    <!-- Header -->
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-1">
        <CalendarRange class="text-(--lg-primary)" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Quản lý Thời khóa biểu</h1>
      </div>
      <p class="text-(--text-body)">Lập lịch học theo học kỳ, kiểm tra xung đột và xuất bản cho sinh viên.</p>
    </GlassPanel>

    <!-- Summary cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <GlassPanel
        v-for="card in summaryCards" :key="card.label"
        variant="flat" density="compact"
        class="flex flex-col justify-center min-h-[88px]"
        :class="`border-l-4 ${card.border}`"
      >
        <p class="text-sm font-medium text-(--text-muted) mb-1">{{ card.label }}</p>
        <strong class="text-2xl text-(--text-heading)">{{ card.value }}</strong>
      </GlassPanel>
    </div>

    <!-- Main area -->
    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <!-- Toolbar -->
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 items-center">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-10 rounded-lg border border-(--border-input) flex-1 min-w-[200px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
          <Search :size="16" class="text-(--text-muted) shrink-0" />
          <input v-model="searchQuery" type="text" placeholder="Tìm mã TKB, môn, lớp, GV..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        <select v-model="filterHocKy" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả học kỳ</option>
          <option v-for="hk in hocKyOptions" :key="hk">{{ hk }}</option>
        </select>
        <select v-model="filterTrangThai" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả trạng thái</option>
          <option value="nhap">Bản nháp</option>
          <option value="da_xuat_ban">Đã xuất bản</option>
          <option value="da_huy">Đã hủy</option>
        </select>
        <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate">
          <Plus :size="16" class="mr-1" />Tạo lịch
        </GlassButton>
      </div>

      <!-- Split layout -->
      <div class="grid grid-cols-1" :class="showPanel ? 'lg:grid-cols-5' : ''">
        <!-- Table -->
        <div :class="showPanel ? 'lg:col-span-3' : ''" class="overflow-x-auto border-r border-(--border-default)">
          <TableShell v-if="filteredRows.length > 0">
            <table>
              <thead>
                <tr>
                  <th class="whitespace-nowrap">Mã TKB</th>
                  <th>Lớp</th>
                  <th>Môn học</th>
                  <th>Giảng viên</th>
                  <th class="whitespace-nowrap">Thứ / Ca</th>
                  <th>Phòng</th>
                  <th>Trạng thái</th>
                  <th class="text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="row in filteredRows" :key="row.id"
                  class="cursor-pointer transition-colors"
                  :class="selectedRow?.id === row.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                  @click="selectedRow = row"
                >
                  <td class="font-mono text-sm text-(--text-muted) whitespace-nowrap">{{ row.maTkb }}</td>
                  <td class="text-sm font-medium">{{ row.lop.ma }}</td>
                  <td class="max-w-[180px]">
                    <div class="text-sm font-medium line-clamp-1" :title="row.monHoc.ten">{{ row.monHoc.ten }}</div>
                    <div class="text-xs text-(--text-muted)">{{ row.monHoc.ma }}</div>
                  </td>
                  <td class="text-sm line-clamp-1 max-w-[160px]" :title="row.giaoVien.ten">{{ row.giaoVien.ten }}</td>
                  <td class="whitespace-nowrap text-sm">
                    <div>{{ thuLabel(row.thuTrongTuan) }}</div>
                    <div class="text-xs text-(--text-muted)">{{ row.caHoc?.tenCa }} · {{ row.caHoc?.gioBatDau }}–{{ row.caHoc?.gioKetThuc }}</div>
                  </td>
                  <td class="text-sm font-medium">{{ row.phongHoc?.ten }}</td>
                  <td>
                    <GlassBadge :variant="getStatusVariant('timetable', row.trangThai)" size="sm">
                      {{ getStatusLabel('timetable', row.trangThai) }}
                    </GlassBadge>
                  </td>
                  <td class="text-right">
                    <div class="flex justify-end gap-1">
                      <GlassButton variant="ghost" size="xs" @click.stop="openEdit(row)">Sửa</GlassButton>
                      <GlassButton v-if="row.trangThai === 'da_xuat_ban'" variant="ghost" size="xs" @click.stop="confirmGenerateSessions(row)">Sinh buổi</GlassButton>
                      <GlassButton variant="ghost" size="xs" class="text-red-500 hover:bg-red-500/10" @click.stop="confirmDelete(row)">Xóa</GlassButton>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState title="Không có lịch học" description="Chưa có lịch nào phù hợp với bộ lọc. Hãy tạo lịch mới." />
          </div>
        </div>

        <!-- Side panel -->
        <div v-if="showPanel" class="lg:col-span-2 bg-(--surface-card) flex flex-col max-h-[calc(100vh-200px)] overflow-y-auto">
          <div class="p-4 border-b border-(--border-default) flex items-center justify-between shrink-0">
            <h3 class="font-bold text-(--text-heading)">{{ panelMode === 'create' ? 'Tạo lịch mới' : 'Chỉnh sửa lịch' }}</h3>
            <button @click="closePanel" class="text-(--text-muted) hover:text-(--text-heading) p-1 rounded"><X :size="18" /></button>
          </div>

          <div class="p-4 space-y-4 flex-1">
            <!-- Học kỳ -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Học kỳ *</label>
              <select v-model="form.hocKy.ten" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                <option v-for="hk in hocKyOptions" :key="hk" :value="hk">{{ hk }}</option>
              </select>
            </div>

            <!-- Lớp -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã lớp *</label>
              <input v-model="form.lop.ma" type="text" placeholder="VD: SE1601" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>

            <!-- Môn học -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Môn học *</label>
              <input v-model="form.monHoc.ten" type="text" placeholder="Tên môn học" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã môn</label>
              <input v-model="form.monHoc.ma" type="text" placeholder="VD: PRO192" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>

            <!-- Giảng viên -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Giảng viên *</label>
              <input v-model="form.giaoVien.ten" type="text" placeholder="Tên giảng viên" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>

            <!-- Thứ -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Thứ trong tuần *</label>
              <select v-model="form.thuTrongTuan" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                <option v-for="thu in thuTrongTuanOptions" :key="thu.value" :value="thu.value">{{ thu.label }}</option>
              </select>
            </div>

            <!-- Ca học -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ca học *</label>
              <select v-model="form.caHoc" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                <option v-for="ca in caHocCatalog" :key="ca.id" :value="ca">{{ ca.tenCa }} · {{ ca.gioBatDau }}–{{ ca.gioKetThuc }}</option>
              </select>
            </div>

            <!-- Phòng học -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Phòng học *</label>
              <input v-model="form.phongHoc.ten" type="text" placeholder="VD: P.302, Lab 2" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
            </div>

            <!-- Ngày -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày bắt đầu *</label>
                <input v-model="form.ngayBatDau" type="date" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày kết thúc *</label>
                <input v-model="form.ngayKetThuc" type="date" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
            </div>

            <!-- Ghi chú -->
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ghi chú</label>
              <textarea v-model="form.ghiChu" rows="2" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) resize-none" placeholder="Ghi chú thêm (không bắt buộc)"></textarea>
            </div>

            <!-- Conflict preview -->
            <div>
              <GlassButton variant="secondary" class="w-full h-10 justify-center" @click="checkConflicts">
                <AlertTriangle :size="16" class="mr-1" /> Kiểm tra xung đột
              </GlassButton>
              <div v-if="conflictPreview.length > 0" class="mt-3 space-y-2">
                <div v-for="(c, i) in conflictPreview" :key="i" class="bg-(--color-warning-bg) border border-amber-300/40 rounded-lg p-3 text-sm">
                  <div class="font-semibold text-(--color-warning-text) mb-1">⚠ Xung đột {{ c.loai }}</div>
                  <div class="text-(--text-body) mb-1">{{ c.mo_ta }}</div>
                  <div class="text-(--text-muted) text-xs italic">Đề xuất: {{ c.de_xuat }}</div>
                </div>
              </div>
              <div v-else-if="conflictPreview.length === 0 && form.monHoc.ten" class="mt-2 text-xs text-(--text-muted) italic text-center">Nhấn để kiểm tra xung đột trước khi lưu.</div>
            </div>
          </div>

          <!-- Actions -->
          <div class="p-4 border-t border-(--border-default) shrink-0 bg-(--surface-modal)">
            <div class="flex flex-col gap-2">
              <GlassButton variant="secondary" class="w-full h-10 justify-center" @click="saveDraft">
                <BookOpen :size="16" class="mr-1" /> Lưu nháp
              </GlassButton>
              <GlassButton variant="primary" class="w-full h-10 justify-center" @click="confirmPublish">
                <CheckCircle :size="16" class="mr-1" /> Xuất bản lịch
              </GlassButton>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>

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
