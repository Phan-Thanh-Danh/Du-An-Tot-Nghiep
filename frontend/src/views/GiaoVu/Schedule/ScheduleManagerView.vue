<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import {
  CalendarRange, Search, Plus, CheckCircle, AlertTriangle, BookOpen, X, Loader2, Pencil, Wand2,
  Clock, MapPin, Sparkles, GraduationCap, Users, ChevronDown
} from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import { scheduleApi } from '@/services/scheduleApi'
import { courseApi } from '@/services/courseApi'
import { staffApi } from '@/services/staffApi'
import { academicTermApi } from '@/services/academicTermApi'
import { usePopupStore } from '@/stores/popup'
import { useAcademicSchedulingContextStore } from '@/stores/academicSchedulingContext'

const popupStore = usePopupStore()
const schedulingContext = useAcademicSchedulingContextStore()

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
const generating = ref(false)
const isLoadingOptions = ref(false)
const activeCreateMode = ref('quick')

// course & room data
const courseOptions = ref([])
const roomOptions = ref([])
const shiftOptions = ref([])
const academicTermOptions = ref([])
const existingSchedules = ref([])
const courseSearchQuery = ref('')
const roomSearchQuery = ref('')
const showCourseDropdown = ref(false)
const showRoomDropdown = ref(false)
const suggestedSlots = ref([])
const suggestingSlots = ref(false)
const bulkSelectedCourseIds = ref([])
const bulkReviewRows = ref([])
const bulkCreating = ref(false)
const smartCourseScope = ref('unscheduled')
const smartCampusId = ref('')
const smartSelectedCourseIds = ref([])
const smartOptions = ref({ tongTheHe: 100, kichThuocQuanThe: 50, tyLeCheo: 0.5 })
const smartDraft = ref(null)

// drag state
const draggingRow = ref(null)
const dragOverCell = ref(null)

const days = thuTrongTuanOptions
const shifts = computed(() => shiftOptions.value)

// ── Lookup ────────────────────────────────────────────────────────
const hocKyOptions = computed(() => {
  const set = new Set(rows.value.map(r => r.hocKy?.ten).filter(Boolean))
  return [...set]
})

const termOptions = computed(() => {
  const map = new Map()
  courseOptions.value.forEach((c) => {
    if (c.maHocKy) map.set(Number(c.maHocKy), c.tenHocKy || `Học kỳ ${c.maHocKy}`)
  })
  academicTermOptions.value.forEach((t) => {
    if (t.maHocKy) map.set(Number(t.maHocKy), t.tenHocKy || `Học kỳ ${t.maHocKy}`)
  })
  rows.value.forEach((r) => {
    if (r.hocKy?.ma) map.set(Number(r.hocKy.ma), r.hocKy.ten || `Học kỳ ${r.hocKy.ma}`)
  })
  return [...map.entries()].map(([value, label]) => ({ value, label }))
})

const campusOptions = computed(() => {
  const map = new Map()
  courseOptions.value.forEach((c) => {
    if (c.maDonVi) map.set(Number(c.maDonVi), c.tenDonVi || `Cơ sở ${c.maDonVi}`)
  })
  rows.value.forEach((r) => {
    if (r.maDonVi) map.set(Number(r.maDonVi), r.tenDonVi || `Cơ sở ${r.maDonVi}`)
  })
  return [...map.entries()].map(([value, label]) => ({ value, label }))
})

const selectedCourse = computed(() =>
  courseOptions.value.find(c => Number(c.maKhoaHoc) === Number(form.value.maKhoaHoc)) || form.value.selectedCourse || null
)

const selectedCourseTermStart = computed(() =>
  toDateInput(selectedCourse.value?.ngayBatDauHocKy || selectedCourse.value?.hocKy?.ngayBatDau)
)

const selectedCourseTermEnd = computed(() =>
  toDateInput(selectedCourse.value?.ngayKetThucHocKy || selectedCourse.value?.hocKy?.ngayKetThuc)
)

const pendingDraftRoute = computed(() => {
  const draft = smartDraft.value || {}

  const draftId =
    draft.draftId ??
    draft.DraftId ??
    draft.id ??
    draft.Id

  const maDonVi =
    draft.maDonVi ??
    draft.MaDonVi ??
    smartCampusId.value

  const maHocKy =
    draft.maHocKy ??
    draft.MaHocKy ??
    schedulingContext.schedulableTerm?.maHocKy

  return {
    path: '/staff/schedule/pending',
    query: {
      maDonVi,
      maHocKy,
      draftId,
    },
  }
})

const unscheduledCourses = computed(() => {
  const scheduled = new Set(rows.value.filter(r => r.trangThai !== 'da_huy').map(r => Number(r.maKhoaHoc)))
  return courseOptions.value.filter(c => !scheduled.has(Number(c.maKhoaHoc)))
})

const bulkCandidateCourses = computed(() => {
  const source = unscheduledCourses.value.length ? unscheduledCourses.value : courseOptions.value
  return source.filter(c => !schedulingContext.schedulableTerm?.maHocKy || Number(c.maHocKy) === Number(schedulingContext.schedulableTerm?.maHocKy))
})

const filteredCourses = computed(() => {
  if (!courseSearchQuery.value) return courseOptions.value.slice(0, 50)
  const q = courseSearchQuery.value.toLowerCase()
  return courseOptions.value.filter(c =>
    c.tenMonHoc?.toLowerCase().includes(q) ||
    c.tenLop?.toLowerCase().includes(q) ||
    c.tenGiaoVien?.toLowerCase().includes(q) ||
    c.tenHocKy?.toLowerCase().includes(q)
  ).slice(0, 100)
})

const filteredRooms = computed(() => {
  let list = roomOptions.value
  const selectedCampusId = selectedCourse.value?.maDonVi
  if (selectedCampusId) {
    list = list.filter(r => Number(r.maDonVi) === Number(selectedCampusId))
  }
  if (!roomSearchQuery.value) return list
  const q = roomSearchQuery.value.toLowerCase()
  return list.filter(r =>
    r.tenPhong?.toLowerCase().includes(q) ||
    r.maCodePhong?.toLowerCase().includes(q) ||
    r.tenToaNha?.toLowerCase().includes(q)
  )
})

const emptyForm = () => ({
  maKhoaHoc: '',
  maPhong: '',
  selectedCourse: null,
  hocKy: { ma: '', ten: '' },
  lop: { ma: '', ten: '' },
  monHoc: { ma: '', ten: '' },
  giaoVien: { ma: '', ten: '' },
  thuTrongTuan: 2,
  caHoc: shiftOptions.value[0] || null,
  phongHoc: { ma: '', ten: '' },
  ngayBatDau: '',
  ngayKetThuc: '',
  trangThai: 'nhap',
  dateMode: 'whole_term',
})
const form = ref(emptyForm())
const editingId = ref(null)

// ── Data loading ──────────────────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const data = await scheduleApi.list({
      TrangThai: filterTrangThai.value || undefined,
      PageIndex: 1,
      PageSize: 100,
    })
    const raw = unwrapList(data)
    rows.value = Array.isArray(raw) ? raw.map(beToView) : []
    existingSchedules.value = rows.value.map(viewToScheduleRecord)
  } catch {
    popupStore.error('Lỗi tải dữ liệu', 'Không thể tải danh sách thời khóa biểu.')
  } finally {
    loading.value = false
  }
}

function beToView(be) {
  const caEntry = shiftOptions.value.find(c => Number(c.maCaHoc) === Number(be.maCaHoc))
  return {
    id: be.maTkb?.toString() || String(be.maTkb),
    maTkb: be.maTkb?.toString() || String(be.maTkb),
    maKhoaHoc: be.maKhoaHoc,
    maHocKy: be.maHocKy,
    maDonVi: be.maDonVi,
    tenDonVi: be.tenDonVi,
    maLop: be.maLop,
    maGiaoVien: be.maGiaoVien,
    maPhong: be.maPhong,
    maCaHoc: be.maCaHoc,
    tieuDeKhoaHoc: be.tieuDeKhoaHoc,
    hocKy: { ma: be.maHocKy, ten: be.tenHocKy },
    lop: { ma: be.maCodeLop || be.maLop, ten: be.tenLop },
    monHoc: { ma: be.maCodeMonHoc || be.maMonHoc, ten: be.tenMonHoc },
    giaoVien: { ma: be.maGiaoVien, ten: be.tenGiaoVien, email: be.emailGiaoVien },
    thuTrongTuan: be.thuTrongTuan,
    caHoc: caEntry ? { ...caEntry } : normalizeShift(be),
    phongHoc: { ma: be.maPhong, code: be.maCodePhong, ten: be.tenPhong },
    ngayBatDau: be.ngayBatDau?.split?.('T')[0] || be.ngayBatDau,
    ngayKetThuc: be.ngayKetThuc?.split?.('T')[0] || be.ngayKetThuc,
    trangThai: be.trangThai || 'nhap',
  }
}

function viewToBe(view) {
  return {
    maKhoaHoc: view.maKhoaHoc,
    thuTrongTuan: view.thuTrongTuan,
    maCaHoc: view.caHoc?.maCaHoc || Number(view.caHoc?.id?.replace('ca', '')) || 0,
    maPhong: view.phongHoc?.ma,
    ngayBatDau: view.ngayBatDau,
    ngayKetThuc: view.ngayKetThuc,
    trangThai: view.trangThai || 'nhap',
  }
}

function unwrapList(res) {
  const data = res?.data ?? res?.Data ?? res
  const list = data?.items ?? data?.Items ?? data?.data?.items ?? data?.Data?.Items ?? data?.data ?? data?.Data ?? data
  return Array.isArray(list) ? list : []
}

function normalizeCourse(c) {
  const termId = c.maHocKy ?? c.MaHocKy
  const term = academicTermOptions.value.find(t => Number(t.maHocKy) === Number(termId))
  return {
    ...c,
    maKhoaHoc: c.maKhoaHoc ?? c.MaKhoaHoc ?? c.id,
    tieuDe: c.tieuDe ?? c.TieuDe ?? c.tenKhoaHoc ?? c.title,
    maHocKy: termId,
    tenHocKy: c.tenHocKy ?? c.TenHocKy ?? term?.tenHocKy,
    ngayBatDauHocKy: c.ngayBatDauHocKy ?? c.NgayBatDauHocKy ?? c.hocKy?.ngayBatDau ?? term?.ngayBatDau,
    ngayKetThucHocKy: c.ngayKetThucHocKy ?? c.NgayKetThucHocKy ?? c.hocKy?.ngayKetThuc ?? term?.ngayKetThuc,
    maLop: c.maLop ?? c.MaLop,
    maCodeLop: c.maCodeLop ?? c.MaCodeLop,
    tenLop: c.tenLop ?? c.TenLop,
    maMonHoc: c.maMonHoc ?? c.MaMonHoc,
    tenMonHoc: c.tenMonHoc ?? c.TenMonHoc,
    maGiaoVien: c.maGiaoVien ?? c.MaGiaoVien,
    tenGiaoVien: c.tenGiaoVien ?? c.TenGiaoVien,
    maDonVi: c.maDonVi ?? c.MaDonVi,
    tenDonVi: c.tenDonVi ?? c.TenDonVi,
    siSo: c.siSo ?? c.SiSo ?? c.soSinhVien ?? c.SoSinhVien,
  }
}

function normalizeAcademicTerm(t) {
  return {
    ...t,
    maHocKy: t.maHocKy ?? t.MaHocKy ?? t.id,
    tenHocKy: t.tenHocKy ?? t.TenHocKy,
    ngayBatDau: t.ngayBatDau ?? t.NgayBatDau,
    ngayKetThuc: t.ngayKetThuc ?? t.NgayKetThuc,
  }
}

function normalizeRoom(r) {
  return {
    ...r,
    maPhong: r.maPhong ?? r.MaPhong ?? r.id,
    maCodePhong: r.maCodePhong ?? r.MaCodePhong,
    tenPhong: r.tenPhong ?? r.TenPhong ?? r.maCodePhong ?? r.MaCodePhong,
    tenToaNha: r.tenToaNha ?? r.TenToaNha,
    maDonVi: r.maDonVi ?? r.MaDonVi,
    sucChua: r.sucChua ?? r.SucChua,
    trangThaiPhong: r.trangThaiPhong ?? r.TrangThaiPhong,
    conHoatDong: r.conHoatDong ?? r.ConHoatDong,
  }
}

function normalizeShift(s) {
  return {
    ...s,
    id: `ca${s.maCaHoc ?? s.MaCaHoc ?? s.id}`,
    maCaHoc: s.maCaHoc ?? s.MaCaHoc ?? s.id,
    tenCa: s.tenCa ?? s.TenCa ?? s.tenShift ?? s.TenShift,
    buoi: s.buoi ?? s.Buoi,
    gioBatDau: s.gioBatDau ?? s.GioBatDau,
    gioKetThuc: s.gioKetThuc ?? s.GioKetThuc,
    thuTu: s.thuTu ?? s.ThuTu,
  }
}

function viewToScheduleRecord(row) {
  return {
    ...row,
    maHocKy: row.maHocKy ?? row.hocKy?.ma,
    maLop: row.maLop ?? row.lop?.ma,
    maGiaoVien: row.maGiaoVien ?? row.giaoVien?.ma,
    maPhong: row.maPhong ?? row.phongHoc?.ma,
    maCaHoc: row.maCaHoc ?? row.caHoc?.maCaHoc,
  }
}

async function loadScheduleOptions() {
  isLoadingOptions.value = true
  try {
    const [termRes, courseRes, roomRes, shiftRes, scheduleRes] = await Promise.all([
      academicTermApi.list({ PageIndex: 1, PageSize: 100 }),
      courseApi.getCourses({ PageIndex: 1, PageSize: 100 }),
      staffApi.getRooms({ pageIndex: 1, pageSize: 100 }),
      staffApi.getCaHoc(),
      scheduleApi.list({ PageIndex: 1, PageSize: 100 }),
    ])

    academicTermOptions.value = unwrapList(termRes).map(normalizeAcademicTerm)
    courseOptions.value = unwrapList(courseRes).map(normalizeCourse)
    roomOptions.value = unwrapList(roomRes).map(normalizeRoom)
    shiftOptions.value = unwrapList(shiftRes).map(normalizeShift).sort((a, b) => Number(a.thuTu || a.maCaHoc) - Number(b.thuTu || b.maCaHoc))
    existingSchedules.value = unwrapList(scheduleRes).map(beToView).map(viewToScheduleRecord)

    if (!form.value.caHoc && shiftOptions.value.length) form.value.caHoc = shiftOptions.value[0]
  } catch (e) {
    popupStore.error('Lỗi tải dữ liệu', e?.message || 'Không thể tải dữ liệu tạo lịch.')
  } finally {
    isLoadingOptions.value = false
  }
}

  onMounted(async () => {
    await schedulingContext.fetchContext()
    loadScheduleOptions().then(loadData)
  })

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
  const caObj = shiftOptions.value.find(c => c.id === caId)
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
  activeCreateMode.value = 'quick'
  formMode.value = 'create'
  editingId.value = null
  form.value = emptyForm()
  if (thu) form.value.thuTrongTuan = thu
  if (caId) form.value.caHoc = shiftOptions.value.find(c => c.id === caId) || shiftOptions.value[0] || null
  conflictPreview.value = []
  selectedRow.value = null
  showFormModal.value = true
}

function openSmartMode() {
  activeCreateMode.value = 'smart'
  formMode.value = 'create'
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
    caHoc: row.caHoc ? { ...row.caHoc } : shiftOptions.value[0] || null,
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
  showCourseDropdown.value = false
  showRoomDropdown.value = false
  courseSearchQuery.value = ''
  roomSearchQuery.value = ''
  suggestedSlots.value = []
  bulkReviewRows.value = []
  smartDraft.value = null
}

// ── Course / Room selection ──────────────────────────────────────
function toDateInput(value) {
  if (!value) return ''
  return String(value).split('T')[0]
}

function todayInput() {
  return new Date().toISOString().split('T')[0]
}

function maxDate(a, b) {
  return a > b ? a : b
}

function clampDatesToTerm() {
  const termStart = selectedCourseTermStart.value
  const termEnd = selectedCourseTermEnd.value

  if (!termStart || !termEnd) {
    popupStore.warning('Thiếu dữ liệu học kỳ', 'Khóa học chưa có ngày bắt đầu/kết thúc học kỳ để tạo lịch an toàn.')
    return false
  }
  if (!termStart || !termEnd) return
  if (form.value.ngayBatDau && form.value.ngayBatDau < termStart) form.value.ngayBatDau = termStart
  if (form.value.ngayBatDau && form.value.ngayBatDau > termEnd) form.value.ngayBatDau = termEnd
  if (form.value.ngayKetThuc && form.value.ngayKetThuc > termEnd) form.value.ngayKetThuc = termEnd
  if (form.value.ngayKetThuc && form.value.ngayKetThuc < termStart) form.value.ngayKetThuc = termStart
}

function applyDateMode() {
  const course = selectedCourse.value
  if (!course) return

  const termStart = selectedCourseTermStart.value
  const termEnd = selectedCourseTermEnd.value
  if (!termStart || !termEnd) return

  if (form.value.dateMode === 'whole_term') {
    form.value.ngayBatDau = termStart
    form.value.ngayKetThuc = termEnd
  }

  if (form.value.dateMode === 'from_today') {
    const today = todayInput()
    if (today > termEnd) {
      popupStore.warning(
        'Học kỳ đã kết thúc',
        'Khóa học này thuộc học kỳ đã kết thúc, không thể tạo lịch áp dụng từ hôm nay.'
      )
      form.value.ngayBatDau = termStart
      form.value.ngayKetThuc = termEnd
      return
    }
    form.value.ngayBatDau = maxDate(today, termStart)
    form.value.ngayKetThuc = termEnd
  }

  if (form.value.dateMode === 'custom') {
    if (!form.value.ngayBatDau) form.value.ngayBatDau = termStart
    if (!form.value.ngayKetThuc) form.value.ngayKetThuc = termEnd
    clampDatesToTerm()
  }
}

watch(() => form.value.dateMode, applyDateMode)

function selectCourse(course) {
  form.value.maKhoaHoc = course.maKhoaHoc
  form.value.selectedCourse = course
  form.value.hocKy = { ma: course.maHocKy || '', ten: course.tenHocKy || '' }
  form.value.lop = { ma: course.maLop || '', ten: course.tenLop || '' }
  form.value.monHoc = { ma: course.maMonHoc || '', ten: course.tenMonHoc || '' }
  form.value.giaoVien = { ma: course.maGiaoVien || '', ten: course.tenGiaoVien || '' }
  courseSearchQuery.value = course.tenMonHoc || course.tenLop || ''
  showCourseDropdown.value = false
  conflictPreview.value = []
  suggestedSlots.value = []
  applyDateMode()
}

function selectRoom(room) {
  form.value.maPhong = room.maPhong
  form.value.phongHoc = { ma: room.maPhong, ten: room.tenPhong }
  roomSearchQuery.value = room.tenPhong || room.maCodePhong || ''
  showRoomDropdown.value = false
  conflictPreview.value = []
}

// ── Smart schedule suggestion (FE algorithm) ────────────────────
function sameTermSchedule(s, course) {
  return Number(s.maHocKy) === Number(course.maHocKy)
}

function buildOccupiedMap(schedules, course) {
  const map = {
    teacher: new Set(),
    class: new Set(),
    room: new Set(),
    teacherDayLoad: new Map(),
    classDayLoad: new Map(),
  }

  for (const s of schedules.filter(x => sameTermSchedule(x, course) && x.trangThai !== 'da_huy')) {
    const thu = Number(s.thuTrongTuan)
    const ca = Number(s.maCaHoc ?? s.caHoc?.maCaHoc)
    const keyBase = `${thu}-${ca}`

    if (s.maGiaoVien) map.teacher.add(`${keyBase}-${s.maGiaoVien}`)
    if (s.maLop) map.class.add(`${keyBase}-${s.maLop}`)
    if (s.maPhong) map.room.add(`${keyBase}-${s.maPhong}`)

    if (s.maGiaoVien) {
      const k = `${thu}-${s.maGiaoVien}`
      map.teacherDayLoad.set(k, (map.teacherDayLoad.get(k) || 0) + 1)
    }

    if (s.maLop) {
      const k = `${thu}-${s.maLop}`
      map.classDayLoad.set(k, (map.classDayLoad.get(k) || 0) + 1)
    }
  }

  return map
}

function calculateSlotScore({ course, room, shift, thu, occupied }) {
  const reasons = []
  let score = 100

  const keyBase = `${thu}-${shift.maCaHoc}`

  if (occupied.teacher.has(`${keyBase}-${course.maGiaoVien}`)) return null
  if (occupied.class.has(`${keyBase}-${course.maLop}`)) return null
  if (occupied.room.has(`${keyBase}-${room.maPhong}`)) return null

  if (course.maDonVi && room.maDonVi && Number(room.maDonVi) !== Number(course.maDonVi)) return null
  if (room.trangThaiPhong && !['hoat_dong', 'active', 'san_sang'].includes(String(room.trangThaiPhong).toLowerCase())) return null
  if (room.conHoatDong === false) return null

  if (course.siSo && room.sucChua && Number(room.sucChua) < Number(course.siSo)) return null

  reasons.push('Không trùng giáo viên, lớp, phòng.')

  if (course.siSo && room.sucChua) {
    const ratio = Number(room.sucChua) / Number(course.siSo)
    if (ratio >= 1 && ratio <= 1.5) {
      score += 5
      reasons.push('Phòng có sức chứa phù hợp với sĩ số.')
    }
    if (ratio > 2) {
      score -= 5
      reasons.push('Phòng hơi dư sức chứa so với sĩ số.')
    }
  }

  const shiftText = String(`${shift.buoi || ''} ${shift.tenCa || ''}`).toLowerCase()
  if (shiftText.includes('tối')) {
    score -= 10
    reasons.push('Ca tối nên bị trừ điểm nhẹ.')
  } else {
    score += 5
    reasons.push('Ca sáng/chiều thuận tiện.')
  }

  if (Number(thu) === 7) {
    score -= 5
    reasons.push('Thứ 7 bị trừ điểm nhẹ.')
  }

  const teacherLoad = occupied.teacherDayLoad.get(`${thu}-${course.maGiaoVien}`) || 0
  if (teacherLoad >= 3) {
    score -= 15
    reasons.push('Giảng viên đã có nhiều ca trong ngày này.')
  }

  const classLoad = occupied.classDayLoad.get(`${thu}-${course.maLop}`) || 0
  if (classLoad >= 3) {
    score -= 15
    reasons.push('Lớp đã có nhiều ca trong ngày này.')
  }

  return {
    thuTrongTuan: thu,
    thu,
    thuLabel: thuLabel(thu),
    maCaHoc: shift.maCaHoc,
    maPhong: room.maPhong,
    ca: shift,
    room,
    tenCa: shift.tenCa,
    tenPhong: room.tenPhong,
    gioBatDau: shift.gioBatDau,
    gioKetThuc: shift.gioKetThuc,
    score: Math.max(0, Math.min(100, score)),
    reasons,
  }
}

function suggestSlotsForCourse(course, scheduleSource = null) {
  const source = scheduleSource || (existingSchedules.value.length ? existingSchedules.value : rows.value.map(viewToScheduleRecord))
  const occupied = buildOccupiedMap(source, course)
  const result = []
  for (const thu of [2, 3, 4, 5, 6, 7]) {
    for (const shift of shiftOptions.value) {
      for (const room of roomOptions.value) {
        const suggestion = calculateSlotScore({ course, room, shift, thu, occupied })
        if (suggestion) result.push(suggestion)
      }
    }
  }
  return result.sort((a, b) => b.score - a.score)
}

function validateDatesInTerm() {
  const course = selectedCourse.value
  if (!course) return false

  const termStart = selectedCourseTermStart.value
  const termEnd = selectedCourseTermEnd.value

  if (!termStart || !termEnd) {
    popupStore.warning(
      'Thiếu dữ liệu học kỳ',
      'Khóa học chưa có ngày bắt đầu/kết thúc học kỳ nên chưa thể tạo lịch an toàn.'
    )
    return false
  }

  if (!form.value.ngayBatDau || !form.value.ngayKetThuc) {
    popupStore.warning('Thiếu ngày áp dụng', 'Vui lòng chọn ngày bắt đầu và ngày kết thúc.')
    return false
  }

  if (form.value.ngayBatDau < termStart) {
    popupStore.warning('Ngày không hợp lệ', 'Ngày bắt đầu không được trước ngày bắt đầu học kỳ.')
    return false
  }

  if (form.value.ngayKetThuc > termEnd) {
    popupStore.warning('Ngày không hợp lệ', 'Ngày kết thúc không được sau ngày kết thúc học kỳ.')
    return false
  }

  if (form.value.ngayBatDau > form.value.ngayKetThuc) {
    popupStore.warning('Ngày không hợp lệ', 'Ngày bắt đầu không được sau ngày kết thúc.')
    return false
  }

  return true
}

function suggestSlots() {
  const course = selectedCourse.value
  if (!course) {
    popupStore.warning('Chưa chọn khóa học', 'Vui lòng chọn khóa học trước khi gợi ý lịch.')
    return
  }
  applyDateMode()
  if (!validateDatesInTerm()) return
  suggestingSlots.value = true
  try {
    suggestedSlots.value = suggestSlotsForCourse(course).slice(0, 5)
    if (!suggestedSlots.value.length) {
      popupStore.warning('Không tìm thấy slot phù hợp', 'Không có phòng/ca học nào thỏa điều kiện hiện tại.')
    }
  } finally {
    suggestingSlots.value = false
  }
}

function applySuggestion(slot) {
  form.value.thuTrongTuan = slot.thuTrongTuan
  form.value.caHoc = slot.ca
  form.value.maPhong = slot.maPhong
  form.value.phongHoc = { ma: slot.maPhong, ten: slot.tenPhong }
  roomSearchQuery.value = slot.tenPhong
  applyDateMode()
  suggestedSlots.value = []
  popupStore.success('Đã áp dụng gợi ý', `Đã chọn Thứ ${slot.thuTrongTuan}, ${slot.tenCa}, phòng ${slot.tenPhong}.`)
  checkConflicts()
}

// ── Conflict check via API ────────────────────────────────────────
async function checkConflicts() {
  conflictPreview.value = []
  const { thuTrongTuan, caHoc, maKhoaHoc, maPhong } = form.value
  if (!validateCreateScheduleForm()) return
  checkingConflict.value = true
  try {
    const payload = {
      maKhoaHoc: Number(maKhoaHoc),
      thuTrongTuan,
      maCaHoc: caHoc.maCaHoc || Number(caHoc.id?.replace('ca', '')),
      maPhong: maPhong ? Number(maPhong) : undefined,
      excludeMaTkb: formMode.value === 'edit' ? Number(editingId.value) : undefined,
    }
    const result = await scheduleApi.checkConflicts(payload)
    const data = result?.data ?? result?.Data ?? result
    const conflicts = data?.conflicts ?? data?.Conflicts ?? []
    if (data?.hasConflict || data?.HasConflict || conflicts.length) {
      conflictPreview.value = conflicts.map(c => ({ loai: c.type ?? c.Type ?? c.loai ?? 'conflict', text: c.message ?? c.Message ?? c.moTa ?? JSON.stringify(c) }))
    } else {
      popupStore.success('Không xung đột', 'Lịch này không xung đột với dữ liệu hiện tại.')
    }
  } catch (e) {
    popupStore.error('Lỗi', e?.message || 'Không thể kiểm tra xung đột.')
  } finally {
    checkingConflict.value = false
  }
}

// ── Save ──────────────────────────────────────────────────────────
function validateCreateScheduleForm() {
  if (!form.value.maKhoaHoc) { popupStore.warning('Thiếu thông tin', 'Vui lòng chọn khóa học.'); return false }
  if (!form.value.thuTrongTuan) { popupStore.warning('Thiếu thông tin', 'Vui lòng chọn thứ trong tuần.'); return false }
  if (!form.value.caHoc?.maCaHoc) { popupStore.warning('Thiếu thông tin', 'Vui lòng chọn ca học.'); return false }
  if (!form.value.maPhong) { popupStore.warning('Thiếu thông tin', 'Vui lòng chọn phòng học.'); return false }
  const courseCampusId = selectedCourse.value?.maDonVi
  const room = roomOptions.value.find(r => Number(r.maPhong) === Number(form.value.maPhong))
  if (courseCampusId && room?.maDonVi && Number(room.maDonVi) !== Number(courseCampusId)) {
    popupStore.warning('Phòng không cùng cơ sở', 'Vui lòng chọn phòng thuộc cùng cơ sở với khóa học.')
    return false
  }
  return validateDatesInTerm()
}

function buildCreatePayload(status = 'nhap') {
  return {
    maKhoaHoc: Number(form.value.maKhoaHoc),
    thuTrongTuan: Number(form.value.thuTrongTuan),
    maCaHoc: Number(form.value.caHoc.maCaHoc),
    maPhong: Number(form.value.maPhong),
    ngayBatDau: form.value.ngayBatDau || null,
    ngayKetThuc: form.value.ngayKetThuc || null,
    trangThai: status,
  }
}

async function saveSchedule(publishNow = false) {
  if (!validateCreateScheduleForm()) return
  submitting.value = true
  try {
    const payload = buildCreatePayload(publishNow ? 'da_xuat_ban' : (form.value.trangThai || 'nhap'))

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
  } catch (e) {
    popupStore.error('Lỗi', e?.message || 'Không thể lưu lịch học.')
  } finally {
    submitting.value = false
  }
}

function saveDraft() { saveSchedule(false) }
function publishDraft() { saveSchedule(true) }

async function suggestBulkCourses() {
  const selected = courseOptions.value.filter(c => bulkSelectedCourseIds.value.map(Number).includes(Number(c.maKhoaHoc)))
  if (!selected.length) {
    popupStore.warning('Chưa chọn khóa học', 'Vui lòng tick ít nhất một khóa học để gợi ý.')
    return
  }
  const baseSchedules = existingSchedules.value.length ? existingSchedules.value : rows.value.map(viewToScheduleRecord)
  const reservedSchedules = []
  bulkReviewRows.value = selected.map((course) => {
    const best = suggestSlotsForCourse(course, [...baseSchedules, ...reservedSchedules])[0] || null
    if (best) {
      reservedSchedules.push({
        maHocKy: course.maHocKy,
        maKhoaHoc: course.maKhoaHoc,
        maLop: course.maLop,
        maGiaoVien: course.maGiaoVien,
        maPhong: best.maPhong,
        maCaHoc: best.maCaHoc,
        thuTrongTuan: best.thuTrongTuan,
        trangThai: 'nhap',
      })
    }
    return {
      course,
      slot: best,
      status: best ? 'ready' : 'no_slot',
    }
  })
  if (bulkReviewRows.value.some(r => r.status === 'no_slot')) {
    popupStore.warning('Một số khóa chưa có slot', 'Kiểm tra bảng review trước khi tạo nháp hàng loạt.')
  }
}

async function createBulkDrafts() {
  const readyRows = bulkReviewRows.value.filter(r => r.slot)
  if (!readyRows.length) {
    popupStore.warning('Chưa có gợi ý hợp lệ', 'Vui lòng chạy gợi ý trước khi tạo nháp.')
    return
  }
  bulkCreating.value = true
  try {
    for (const row of readyRows) {
      const termStart = toDateInput(row.course.ngayBatDauHocKy || row.course.hocKy?.ngayBatDau)
      const termEnd = toDateInput(row.course.ngayKetThucHocKy || row.course.hocKy?.ngayKetThuc)
      await scheduleApi.create({
        maKhoaHoc: Number(row.course.maKhoaHoc),
        thuTrongTuan: Number(row.slot.thuTrongTuan),
        maCaHoc: Number(row.slot.maCaHoc),
        maPhong: Number(row.slot.maPhong),
        ngayBatDau: termStart || null,
        ngayKetThuc: termEnd || null,
        trangThai: 'nhap',
      })
    }
    popupStore.success('Đã tạo nháp hàng loạt', `Đã tạo ${readyRows.length} lịch nháp.`)
    await loadData()
    bulkSelectedCourseIds.value = []
    bulkReviewRows.value = []
  } catch (e) {
    popupStore.error('Lỗi tạo nháp hàng loạt', e?.message || 'Không thể tạo nháp hàng loạt.')
  } finally {
    bulkCreating.value = false
  }
}

async function generateSmartDraft() {
  if (!schedulingContext.schedulableTerm?.maHocKy || !smartCampusId.value) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn cơ sở và đảm bảo có học kỳ hợp lệ để xếp lịch.')
    return
  }
  const selectedIds =
    smartCourseScope.value === 'manual'
      ? smartSelectedCourseIds.value.map(Number)
      : unscheduledCourses.value
          .filter(c => Number(c.maHocKy) === Number(schedulingContext.schedulableTerm?.maHocKy) && Number(c.maDonVi) === Number(smartCampusId.value))
          .map(c => Number(c.maKhoaHoc))

  if (selectedIds.length === 0) {
    popupStore.warning('Không có khóa học phù hợp', 'Vui lòng chọn ít nhất một khóa học hoặc đổi cơ sở/học kỳ.')
    return
  }

  generating.value = true
  try {
    const res = await scheduleApi.generateDraft({
      maHocKy: Number(schedulingContext.schedulableTerm?.maHocKy),
      maDonVi: Number(smartCampusId.value),
      maKhoaHocFilter: selectedIds,
      tongTheHe: Number(smartOptions.value.tongTheHe || 100),
      kichThuocQuanThe: Number(smartOptions.value.kichThuocQuanThe || 50),
      tyLeCheo: Number(smartOptions.value.tyLeCheo || 0.5),
    })
    smartDraft.value = res?.data ?? res?.Data ?? res
    popupStore.success('Đã sinh bản nháp', 'Vui lòng kiểm tra bản nháp trước khi xuất bản.')
  } catch (e) {
    popupStore.error('Lỗi xếp lịch thông minh', e?.message || 'Không thể sinh bản nháp thời khóa biểu.')
  } finally {
    generating.value = false
  }
}

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
    <!-- P25 Banner -->
    <div v-if="schedulingContext.isContextLoaded && !schedulingContext.canPrepareSchedule" class="rounded-xl border border-(--color-danger-border, #f87171) bg-(--color-danger-bg) p-4 text-(--color-danger-text)">
      <div class="flex items-start gap-3">
        <AlertTriangle class="mt-0.5 shrink-0" :size="20" />
        <div>
          <h3 class="font-semibold">{{ schedulingContext.reasonMessage }}</h3>
          <ul v-if="schedulingContext.readiness?.blockingIssues?.length" class="mt-2 list-inside list-disc text-sm">
            <li v-for="issue in schedulingContext.readiness.blockingIssues" :key="issue.code">
              {{ issue.message }}
            </li>
          </ul>
        </div>
      </div>
    </div>
    <div v-else-if="schedulingContext.isContextLoaded && schedulingContext.canPrepareSchedule" class="rounded-xl border border-(--color-success-border, #6ee7b7) bg-(--color-success-bg) p-4 text-(--color-success-text)">
      <div class="flex items-center gap-3">
        <CheckCircle class="shrink-0" :size="20" />
        <h3 class="font-semibold">Đang thao tác xếp lịch cho Học kỳ: {{ schedulingContext.schedulableTerm?.tenHocKy }}</h3>
      </div>
    </div>

    <!-- ── Header ── -->
    <div class="flex items-start justify-between gap-4 flex-wrap">
      <div>
        <div class="flex items-center gap-2">
          <CalendarRange class="text-(--lg-primary)" :size="22" />
          <h1 class="text-xl font-bold text-(--text-heading)">Thời khóa biểu</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Quản lý lịch học theo học kỳ · Kéo thả để điều chỉnh · Click để xem chi tiết</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary" class="h-10 shrink-0 border-(--border-default)" @click="openSmartMode" :disabled="generating">
          <Loader2 v-if="generating" :size="15" class="mr-1 animate-spin" />
          <Wand2 v-else :size="15" class="mr-1 text-(--accent-violet)" />
          Xếp lịch thông minh
        </GlassButton>
        <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate()">
          <Plus :size="15" class="mr-1" /> Tạo lịch
        </GlassButton>
      </div>
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
    <div v-if="loading && rows.length === 0" class="p-4">
      <SkeletonTable :rows="6" :columns="7" />
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
              variant="secondary" class="w-full h-9 justify-center text-sm text-(--color-danger-text)! border-(--border-default) hover:bg-(--color-danger-bg)!"
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
          <div class="w-full max-w-4xl lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <!-- Header -->
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">
                {{ formMode === 'edit' ? 'Chỉnh sửa lịch học' : 'Tạo lịch thông minh' }}
              </h3>
              <button @click="closeFormModal" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors">
                <X :size="18" />
              </button>
            </div>

            <!-- Body -->
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <div v-if="formMode !== 'edit'" class="flex flex-wrap gap-2 rounded-xl border border-(--border-default) bg-(--surface-input) p-1">
                <button
                  v-for="mode in [
                    { value: 'quick', label: 'Tạo nhanh 1 khóa học' },
                    { value: 'bulk', label: 'Gợi ý nhiều khóa học' },
                    { value: 'smart', label: 'Xếp lịch thông minh toàn kỳ' },
                  ]"
                  :key="mode.value"
                  type="button"
                  class="rounded-lg px-3 py-2 text-xs font-bold transition-all"
                  :class="activeCreateMode === mode.value ? 'bg-(--surface-card) text-(--text-heading) shadow-sm' : 'text-(--text-muted)'"
                  @click="activeCreateMode = mode.value"
                >
                  {{ mode.label }}
                </button>
              </div>

              <div v-if="isLoadingOptions" class="rounded-xl border border-(--border-default) bg-(--surface-input) p-3 text-sm text-(--text-muted)">
                Đang tải khóa học, phòng học và ca học...
              </div>

              <template v-if="activeCreateMode === 'quick' || formMode === 'edit'">
              <!-- Course search -->
              <div class="relative">
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Khóa học *</label>
                <div class="relative">
                  <input
                    v-model="courseSearchQuery"
                    type="text"
                    placeholder="Tìm theo môn học, lớp, giảng viên..."
                    class="w-full h-9 pl-3 pr-8 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)"
                    @focus="showCourseDropdown = true"
                    @input="showCourseDropdown = true"
                  />
                  <ChevronDown :size="14" class="absolute right-2.5 top-1/2 -translate-y-1/2 text-(--text-muted)" />
                </div>
                <div v-if="form.selectedCourse" class="mt-1.5 flex flex-wrap gap-2 text-xs">
                  <span class="px-2 py-1 rounded-full bg-(--color-info-bg) text-(--color-info-text) flex items-center gap-1">
                    <GraduationCap :size="12" /> {{ form.monHoc?.ten }}
                  </span>
                  <span class="px-2 py-1 rounded-full bg-(--surface-input) text-(--text-muted) flex items-center gap-1">
                    {{ selectedCourseTermStart || '—' }} → {{ selectedCourseTermEnd || '—' }}
                  </span>
                  <span class="px-2 py-1 rounded-full bg-(--surface-input) text-(--text-body) flex items-center gap-1">
                    <Users :size="12" /> {{ form.lop?.ten || form.lop?.ma }}
                  </span>
                  <span class="px-2 py-1 rounded-full bg-(--surface-input) text-(--text-body) flex items-center gap-1">
                    {{ form.giaoVien?.ten }}
                  </span>
                  <span class="px-2 py-1 rounded-full bg-(--surface-input) text-(--text-muted) flex items-center gap-1">
                    <BookOpen :size="12" /> {{ form.hocKy?.ten }}
                  </span>
                </div>
                <div
                  v-if="showCourseDropdown && filteredCourses.length > 0"
                  class="absolute z-20 left-0 right-0 top-full mt-1 bg-(--surface-modal) border border-(--border-card) rounded-xl shadow-xl max-h-64 overflow-y-auto"
                  @click.outside="showCourseDropdown = false"
                >
                  <button
                    v-for="c in filteredCourses" :key="c.maKhoaHoc"
                    class="w-full px-3 py-2.5 text-left text-sm hover:bg-(--surface-hover) transition-colors border-b last:border-b-0 border-(--border-default) flex items-center gap-2"
                    @click="selectCourse(c)"
                  >
                    <GraduationCap :size="14" class="text-(--lg-primary) shrink-0" />
                    <div class="min-w-0 flex-1">
                      <span class="font-medium text-(--text-heading) block truncate">{{ c.tenMonHoc }}</span>
                      <span class="text-(--text-muted) text-xs block truncate">{{ c.tenLop }} · {{ c.tenGiaoVien }} · {{ c.tenHocKy }}</span>
                    </div>
                  </button>
                </div>
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
                    <option v-for="ca in shiftOptions" :key="ca.id" :value="ca">{{ ca.tenCa }} ({{ ca.gioBatDau }}–{{ ca.gioKetThuc }})</option>
                  </select>
                </div>
              </div>

              <div v-if="form.selectedCourse" class="grid gap-2 rounded-xl border border-(--border-default) bg-(--surface-input) p-3 text-xs md:grid-cols-2">
                <div><span class="text-(--text-muted)">Học kỳ:</span> <strong class="text-(--text-heading)">{{ form.hocKy?.ten }}</strong></div>
                <div><span class="text-(--text-muted)">Cơ sở:</span> <strong class="text-(--text-heading)">{{ selectedCourse?.tenDonVi || '—' }}</strong></div>
                <div><span class="text-(--text-muted)">Lớp:</span> <strong class="text-(--text-heading)">{{ form.lop?.ten || form.lop?.ma }}</strong></div>
                <div><span class="text-(--text-muted)">Môn:</span> <strong class="text-(--text-heading)">{{ form.monHoc?.ten }}</strong></div>
                <div><span class="text-(--text-muted)">Giảng viên:</span> <strong class="text-(--text-heading)">{{ form.giaoVien?.ten }}</strong></div>
                <div><span class="text-(--text-muted)">Sĩ số:</span> <strong class="text-(--text-heading)">{{ selectedCourse?.siSo || '—' }}</strong></div>
              </div>

              <!-- Room search -->
              <div class="relative">
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Phòng học *</label>
                <div class="relative">
                  <input
                    v-model="roomSearchQuery"
                    type="text"
                    placeholder="Tìm phòng theo tên, mã, tòa nhà..."
                    class="w-full h-9 pl-3 pr-8 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)"
                    @focus="showRoomDropdown = true"
                    @input="showRoomDropdown = true"
                  />
                  <MapPin :size="14" class="absolute right-2.5 top-1/2 -translate-y-1/2 text-(--text-muted)" />
                </div>
                <div
                  v-if="showRoomDropdown && filteredRooms.length > 0"
                  class="absolute z-20 left-0 right-0 top-full mt-1 bg-(--surface-modal) border border-(--border-card) rounded-xl shadow-xl max-h-56 overflow-y-auto"
                  @click.outside="showRoomDropdown = false"
                >
                  <button
                    v-for="r in filteredRooms" :key="r.maPhong"
                    class="w-full px-3 py-2.5 text-left text-sm hover:bg-(--surface-hover) transition-colors border-b last:border-b-0 border-(--border-default) flex items-center gap-2"
                    @click="selectRoom(r)"
                  >
                    <MapPin :size="14" class="text-(--lg-primary) shrink-0" />
                    <div class="min-w-0 flex-1">
                      <span class="font-medium text-(--text-heading) block truncate">{{ r.tenPhong }}</span>
                      <span class="text-(--text-muted) text-xs block truncate">{{ r.maCodePhong }} · {{ r.tenToaNha }} ({{ r.sucChua }} chỗ)</span>
                    </div>
                  </button>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div class="col-span-2">
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Phạm vi áp dụng ngày</label>
                  <div class="grid gap-2 md:grid-cols-3">
                    <label
                      v-for="mode in [
                        { value: 'whole_term', label: 'Áp dụng cả học kỳ' },
                        { value: 'from_today', label: 'Từ hôm nay đến hết kỳ' },
                        { value: 'custom', label: 'Tùy chỉnh trong học kỳ' },
                      ]"
                      :key="mode.value"
                      class="flex items-center gap-2 rounded-lg border border-(--border-default) bg-(--surface-input) px-3 py-2 text-xs font-semibold"
                    >
                      <input v-model="form.dateMode" type="radio" :value="mode.value" @change="applyDateMode" />
                      {{ mode.label }}
                    </label>
                  </div>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Từ ngày *</label>
                  <input v-model="form.ngayBatDau" type="date" :min="selectedCourseTermStart" :max="selectedCourseTermEnd" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" @change="clampDatesToTerm" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Đến ngày *</label>
                  <input v-model="form.ngayKetThuc" type="date" :min="selectedCourseTermStart" :max="selectedCourseTermEnd" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" @change="clampDatesToTerm" />
                </div>
              </div>

              <!-- Suggest slots + Conflict check -->
              <div class="flex gap-2">
                <button
                  :disabled="suggestingSlots || !form.maKhoaHoc"
                  class="flex-1 h-9 text-xs font-semibold border border-(--border-default) text-(--color-info-text) rounded-lg hover:bg-(--color-info-bg) flex items-center justify-center gap-1.5 transition-colors disabled:opacity-50"
                  @click="suggestSlots"
                >
                  <Sparkles :size="14" />
                  Gợi ý slot phù hợp
                </button>
                <button
                  :disabled="checkingConflict"
                  class="flex-1 h-9 text-xs font-semibold border border-(--border-default) text-(--color-warning-text) rounded-lg hover:bg-(--color-warning-bg) flex items-center justify-center gap-1.5 transition-colors disabled:opacity-50"
                  @click="checkConflicts"
                >
                  <Loader2 v-if="checkingConflict" :size="14" class="animate-spin" />
                  <AlertTriangle v-else :size="14" />
                  Kiểm tra xung đột
                </button>
              </div>

              <!-- Slot suggestions -->
              <div v-if="suggestedSlots.length > 0" class="space-y-1.5">
                <p class="text-xs font-semibold text-(--text-muted)">Các khung giờ đề xuất:</p>
                <button
                  v-for="(s, i) in suggestedSlots" :key="i"
                  class="w-full flex items-center justify-between px-3 py-2 rounded-lg border border-(--border-default) text-sm hover:bg-(--color-info-bg) transition-colors"
                  @click="applySuggestion(s)"
                >
                  <span class="flex items-center gap-2">
                    <Clock :size="14" class="text-(--lg-primary)" />
                    <span class="font-medium text-(--text-heading)">{{ s.thuLabel }} · {{ s.ca.tenCa }} · {{ s.tenPhong }}</span>
                  </span>
                  <span class="text-xs text-(--text-muted) bg-(--surface-input) px-2 py-0.5 rounded-full">Điểm: {{ s.score }}</span>
                  <span class="block w-full pl-6 text-left text-[11px] text-(--text-muted)">{{ s.reasons.join(' · ') }}</span>
                </button>
              </div>

              <!-- Conflict preview -->
              <div v-if="conflictPreview.length > 0" class="space-y-1.5">
                <div v-for="(c, i) in conflictPreview" :key="i" class="bg-(--color-danger-bg) border border-(--border-default) rounded-lg p-2 text-xs text-(--color-danger-text)">
                  <span class="font-semibold">[{{ c.loai }}]</span> {{ c.text }}
                </div>
              </div>
              </template>

              <template v-else-if="activeCreateMode === 'bulk'">
                <div class="space-y-3">
                  <div class="flex flex-wrap items-center gap-2">
                    <div class="h-9 flex items-center rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm font-medium text-(--text-muted)">
                      Học kỳ: {{ schedulingContext.schedulableTerm?.tenHocKy || '—' }}
                    </div>
                    <GlassButton variant="secondary" @click="suggestBulkCourses">Gợi ý lịch cho các khóa đã chọn</GlassButton>
                  </div>
                  <div class="max-h-56 overflow-y-auto rounded-xl border border-(--border-default)">
                    <label v-for="course in bulkCandidateCourses" :key="course.maKhoaHoc" class="flex items-center gap-3 border-b border-(--border-default) px-3 py-2 text-sm last:border-b-0">
                      <input v-model="bulkSelectedCourseIds" type="checkbox" :value="course.maKhoaHoc" />
                      <span class="min-w-0">
                        <strong class="block truncate text-(--text-heading)">{{ course.tenMonHoc || course.tieuDe }}</strong>
                        <small class="text-(--text-muted)">{{ course.tenLop }} · {{ course.tenGiaoVien }} · {{ course.tenHocKy }}</small>
                      </span>
                    </label>
                  </div>
                  <div v-if="bulkReviewRows.length" class="overflow-x-auto rounded-xl border border-(--border-default)">
                    <table class="w-full text-left text-xs">
                      <thead class="bg-(--surface-input) text-(--text-muted)">
                        <tr><th class="p-2">Khóa học</th><th class="p-2">Slot đề xuất</th><th class="p-2">Phòng</th><th class="p-2">Điểm</th><th class="p-2">Lý do</th><th class="p-2">Trạng thái</th></tr>
                      </thead>
                      <tbody>
                        <tr v-for="row in bulkReviewRows" :key="row.course.maKhoaHoc" class="border-t border-(--border-default)">
                          <td class="p-2"><strong>{{ row.course.tenMonHoc || row.course.tieuDe }}</strong><br><span class="text-(--text-muted)">{{ row.course.tenLop }} · {{ row.course.tenGiaoVien }}</span></td>
                          <td class="p-2">{{ row.slot ? `${row.slot.thuLabel} · ${row.slot.tenCa}` : '—' }}</td>
                          <td class="p-2">{{ row.slot?.tenPhong || '—' }}</td>
                          <td class="p-2">{{ row.slot?.score ?? '—' }}</td>
                          <td class="p-2">{{ row.slot?.reasons?.join(' · ') || 'Không có slot phù hợp' }}</td>
                          <td class="p-2"><GlassBadge :variant="row.slot ? 'success' : 'danger'" size="sm">{{ row.slot ? 'Sẵn sàng' : 'Không có slot' }}</GlassBadge></td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </template>

              <template v-else>
                <div class="grid gap-3 md:grid-cols-2">
                  <label class="text-xs font-semibold text-(--text-muted)">
                    Cơ sở
                    <select v-model="smartCampusId" class="mt-1 h-9 w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm">
                      <option value="">Chọn cơ sở</option>
                      <option v-for="campus in campusOptions" :key="campus.value" :value="campus.value">{{ campus.label }}</option>
                    </select>
                  </label>
                  <label class="text-xs font-semibold text-(--text-muted)">
                    Học kỳ
                    <div class="mt-1 h-9 flex items-center w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm">
                      {{ schedulingContext.schedulableTerm?.tenHocKy || '—' }}
                    </div>
                  </label>
                  <label class="text-xs font-semibold text-(--text-muted)">
                    Phạm vi khóa học
                    <select v-model="smartCourseScope" class="mt-1 h-9 w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm">
                      <option value="unscheduled">Tất cả khóa học chưa có lịch</option>
                      <option value="manual">Tick thủ công</option>
                    </select>
                  </label>
                  <label class="text-xs font-semibold text-(--text-muted)">Tổng thế hệ
                    <input v-model.number="smartOptions.tongTheHe" type="number" min="10" class="mt-1 h-9 w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm" />
                  </label>
                  <label class="text-xs font-semibold text-(--text-muted)">Kích thước quần thể
                    <input v-model.number="smartOptions.kichThuocQuanThe" type="number" min="10" class="mt-1 h-9 w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm" />
                  </label>
                  <label class="text-xs font-semibold text-(--text-muted)">Tỷ lệ chéo
                    <input v-model.number="smartOptions.tyLeCheo" type="number" min="0" max="1" step="0.1" class="mt-1 h-9 w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm" />
                  </label>
                </div>
                <div v-if="smartCourseScope === 'manual'" class="max-h-44 overflow-y-auto rounded-xl border border-(--border-default)">
                  <label v-for="course in bulkCandidateCourses" :key="course.maKhoaHoc" class="flex items-center gap-3 border-b border-(--border-default) px-3 py-2 text-sm last:border-b-0">
                    <input v-model="smartSelectedCourseIds" type="checkbox" :value="course.maKhoaHoc" />
                    <span>{{ course.tenMonHoc || course.tieuDe }} · {{ course.tenLop }}</span>
                  </label>
                </div>
                <div v-if="smartDraft" class="rounded-xl border border-(--border-default) bg-(--color-success-bg) p-3 text-sm text-(--color-success-text)">
                  Đã sinh bản nháp: {{ smartDraft.draftId || smartDraft.DraftId || smartDraft.id || 'xem trong lịch chờ duyệt' }}.
                  <RouterLink :to="pendingDraftRoute" class="font-bold underline">Đi tới lịch chờ duyệt</RouterLink>
                </div>
              </template>
            </div>

            <!-- Footer -->
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeFormModal">
                Hủy
              </GlassButton>
              <GlassButton v-if="activeCreateMode === 'quick' || formMode === 'edit'" variant="secondary" class="h-10 px-5 text-sm" :disabled="submitting" @click="saveDraft">
                <BookOpen :size="15" class="mr-1.5" /> Lưu nháp
              </GlassButton>
              <GlassButton v-if="activeCreateMode === 'quick' || formMode === 'edit'" variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="publishDraft">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <CheckCircle v-else :size="15" class="mr-1.5" />
                {{ submitting ? 'Đang lưu...' : 'Xuất bản ngay' }}
              </GlassButton>
              <GlassButton v-if="activeCreateMode === 'bulk' && formMode !== 'edit'" variant="primary" class="h-10 px-5 text-sm" :disabled="bulkCreating || !bulkReviewRows.length" @click="createBulkDrafts">
                <Loader2 v-if="bulkCreating" :size="15" class="mr-1.5 animate-spin" />
                Tạo nháp hàng loạt
              </GlassButton>
              <GlassButton v-if="activeCreateMode === 'smart' && formMode !== 'edit'" variant="primary" @click="generateSmartDraft" :disabled="generating || !schedulingContext.canPrepareSchedule">
                <Loader2 v-if="generating" class="animate-spin" :size="16" />
                <Sparkles v-else :size="16" />
                Bắt đầu tạo
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
