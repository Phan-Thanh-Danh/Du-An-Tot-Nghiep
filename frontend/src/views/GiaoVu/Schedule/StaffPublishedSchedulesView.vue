<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  CheckCircle, X, Users, BookOpen,
  AlertCircle, UserCircle2, CalendarCheck,
} from 'lucide-vue-next'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { scheduleApi } from '@/services/scheduleApi'
import { staffApi } from '@/services/staffApi'
import { academicTermApi } from '@/services/academicTermApi'
import { courseApi } from '@/services/courseApi'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const authorizedCampusId = computed(() => Number(authStore.user?.campusId || authStore.user?.CampusId || authStore.user?.maDonVi || authStore.user?.MaDonVi || 0))

const loading = ref(true)
const apiError = ref('')
const schedules = ref([])
const selected = ref(null)
const filterMaDonVi = ref('')
const filterMaHocKy = ref('')
const campusOptions = ref([])
const termOptions = ref([])
const tkbCount = ref(0)
const activeTab = ref('lichHoc')

function unwrap(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  if (Array.isArray(data?.data)) return data.data
  if (Array.isArray(data?.Data)) return data.Data
  return []
}

function normalizeTerm(t) {
  const maHocKy = t.maHocKy ?? t.MaHocKy ?? t.id ?? t.Id
  return {
    maHocKy,
    tenHocKy: t.tenHocKy ?? t.TenHocKy ?? t.ten ?? t.Ten ?? `Học kỳ ${maHocKy ?? ''}`,
    ngayBatDau: t.ngayBatDau ?? t.NgayBatDau,
    ngayKetThuc: t.ngayKetThuc ?? t.NgayKetThuc,
  }
}

function normalizeCourse(c) {
  return {
    maKhoaHoc: c.maKhoaHoc ?? c.MaKhoaHoc ?? c.id ?? c.Id,
    maDonVi: c.maDonVi ?? c.MaDonVi,
    tenDonVi: c.tenDonVi ?? c.TenDonVi ?? c.tenCoSo ?? c.TenCoSo,
  }
}

function formatDateTime(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return String(value)
  return d.toLocaleString('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })
}

function initialOf(name) {
  if (!name) return '?'
  const trimmed = String(name).trim()
  return trimmed ? trimmed.charAt(0).toUpperCase() : '?'
}

function mapDraft(item) {
  const id = item.draftId ?? item.DraftId ?? item.id ?? item.Id
  const maDonVi = item.maDonVi ?? item.MaDonVi
  const maHocKy = item.maHocKy ?? item.MaHocKy
  const term = termOptions.value.find(t => Number(t.maHocKy) === Number(maHocKy))
  return {
    id,
    maDonVi,
    maHocKy,
    term: term?.tenHocKy ?? item.tenHocKy ?? item.TenHocKy ?? (maHocKy ? `Học kỳ ${maHocKy}` : '—'),
    department: campusOptions.value.find(c => Number(c.value) === Number(maDonVi))?.label
      ?? item.tenDonVi ?? item.TenDonVi ?? (maDonVi ? `Cơ sở ${maDonVi}` : '—'),
    status: item.trangThai ?? item.TrangThai ?? 'draft',
    ngayTao: item.ngayTao ?? item.NgayTao,
    ngayXuatBan: item.ngayXuatBan ?? item.NgayXuatBan ?? item.ngayTao ?? item.NgayTao,
    nguoiYeuCau: item.nguoiYeuCau ?? item.NguoiYeuCau,
    tenNguoiYeuCau: item.tenNguoiYeuCau ?? item.TenNguoiYeuCau ?? '',
    metrics: {
      classes: item.tongCourse ?? item.TongCourse ?? item.soLop ?? item.SoLop ?? 0,
      xepDuoc: item.soXepDuoc ?? item.SoXepDuoc ?? 0,
      khongXepDuoc: item.soKhongXepDuoc ?? item.SoKhongXepDuoc ?? 0,
      score: item.score ?? item.Score ?? 0,
    },
    items: Array.isArray(item.items) ? item.items : Array.isArray(item.Items) ? item.Items : [],
    raw: item,
  }
}

const publishedSchedules = computed(() => {
  return schedules.value
    .filter(s => s.status === 'da_xuat_ban')
    .sort((a, b) => {
      const ta = new Date(a.ngayXuatBan).getTime()
      const tb = new Date(b.ngayXuatBan).getTime()
      if (!Number.isNaN(ta) && !Number.isNaN(tb)) return tb - ta
      return 0
    })
})

const newestSchedule = computed(() => publishedSchedules.value[0] ?? null)
const otherSchedules = computed(() => publishedSchedules.value.slice(1))

const stats = computed(() => ({
  total: publishedSchedules.value.length,
  lopTong: publishedSchedules.value.reduce((a, b) => a + Number(b.metrics.classes || 0), 0),
  tkb: tkbCount.value,
}))

const selectedItems = computed(() => selected.value?.items ?? [])

const distinctStats = computed(() => {
  const items = selectedItems.value
  return {
    mon: new Set(items.map(i => i.tenMonHoc ?? i.TenMonHoc).filter(Boolean)).size,
    lop: new Set(items.map(i => i.tenLop ?? i.TenLop).filter(Boolean)).size,
    gv: new Set(items.map(i => i.tenGiaoVien ?? i.TenGiaoVien).filter(Boolean)).size,
  }
})

const itemStatusLabel = s => ({
  xep_duoc: 'Xếp được', khong_xep_duoc: 'Chưa xếp được',
}[s] || s)

async function loadFilterOptions() {
  try {
    const [termsRes, coursesRes, contextRes] = await Promise.all([
      academicTermApi.list({ PageIndex: 1, PageSize: 100 }),
      courseApi.getCourses({ PageIndex: 1, PageSize: 100 }),
      staffApi.getSchedulingContext().catch(() => null),
    ])

    termOptions.value = unwrap(termsRes)
      .map(normalizeTerm)
      .filter(t => t.maHocKy)
      .sort((a, b) => Number(b.maHocKy) - Number(a.maHocKy))

    const campusMap = new Map()
    unwrap(coursesRes)
      .map(normalizeCourse)
      .forEach((course) => {
        if (!course.maDonVi) return
        campusMap.set(Number(course.maDonVi), course.tenDonVi || `Cơ sở ${course.maDonVi}`)
      })
    const allCampuses = [...campusMap.entries()]
      .map(([value, label]) => ({ value, label }))
      .sort((a, b) => String(a.label).localeCompare(String(b.label), 'vi'))
    campusOptions.value = authorizedCampusId.value
      ? allCampuses.filter(c => Number(c.value) === authorizedCampusId.value)
      : allCampuses

    if (authorizedCampusId.value) {
      filterMaDonVi.value = authorizedCampusId.value
    }

    if (!filterMaDonVi.value && campusOptions.value.length === 1) {
      filterMaDonVi.value = campusOptions.value[0].value
    }

    const currentTerm = contextRes?.currentTerm ?? contextRes?.CurrentTerm
    const schedulableTerm = contextRes?.schedulableTerm ?? contextRes?.SchedulableTerm
    const preferredTerm = Number(currentTerm?.maHocKy ?? currentTerm?.MaHocKy)
      || Number(schedulableTerm?.maHocKy ?? schedulableTerm?.MaHocKy)

    if (!filterMaHocKy.value && preferredTerm) {
      filterMaHocKy.value = preferredTerm
    }
    if (!filterMaHocKy.value && termOptions.value.length > 0) {
      filterMaHocKy.value = termOptions.value[0].maHocKy
    }
  } catch (e) {
    console.error('Load published filters failed', e)
    apiError.value = e?.message || 'Không thể tải danh sách cơ sở/học kỳ.'
  }
}

async function loadTkbCount() {
  tkbCount.value = 0
  if (!filterMaHocKy.value) return
  try {
    const res = await scheduleApi.list({
      TrangThai: 'da_xuat_ban',
      maHocKy: Number(filterMaHocKy.value),
      PageIndex: 1,
      PageSize: 1,
    })
    const payload = res?.data ?? res?.Data ?? res
    tkbCount.value = Number(
      payload?.totalItems ?? payload?.TotalItems ?? payload?.total ?? payload?.Total ?? 0,
    )
  } catch (e) {
    console.error('Load published TKB count failed', e.statusCode, e.details || e)
  }
}

async function loadSchedules() {
  loading.value = true
  apiError.value = ''
  selected.value = null
  try {
    if (!filterMaDonVi.value || !filterMaHocKy.value) {
      schedules.value = []
      return
    }
    const response = await scheduleApi.listDrafts({
      maDonVi: Number(filterMaDonVi.value),
      maHocKy: Number(filterMaHocKy.value),
    })
    schedules.value = unwrap(response).map(mapDraft)
    if (newestSchedule.value) {
      selected.value = newestSchedule.value
    }
  } catch (e) {
    console.error('Published schedules load failed', e.statusCode, e.details || e)
    apiError.value = e?.message || 'Không thể tải danh sách lịch đã công bố.'
    schedules.value = []
  } finally {
    loading.value = false
  }
}

function pickSchedule(item) {
  selected.value = item
}

onMounted(async () => {
  loading.value = true
  await loadFilterOptions()
  await loadTkbCount()
  await loadSchedules()
})
</script>

<template>
  <div class="published-view space-y-4 max-w-full">

    <div v-if="loading" class="p-4 space-y-4">
      <SkeletonDashboard :cards="4" :rows="2" />
      <SkeletonTable :rows="6" :columns="5" />
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadSchedules" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <!-- Header -->
      <div>
        <div class="flex items-center gap-2">
          <CheckCircle class="text-emerald-500" :size="22" />
          <h1 class="text-xl font-bold text-(--text-heading)">Lịch đã công bố</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Theo dõi bộ thời khóa biểu mới nhất đã công bố, xem chi tiết và tài khoản phụ trách công bố.</p>
      </div>

      <!-- Filters -->
      <div class="flex flex-wrap items-center gap-3">
        <label class="flex flex-col gap-1 text-xs font-semibold text-(--text-muted)">
          <span>Cơ sở</span>
          <select v-model.number="filterMaDonVi" @change="loadSchedules"
            class="h-9 min-w-[200px] px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option value="">Chọn cơ sở</option>
            <option v-for="c in campusOptions" :key="c.value" :value="c.value">{{ c.label }}</option>
          </select>
        </label>
        <label class="flex flex-col gap-1 text-xs font-semibold text-(--text-muted)">
          <span>Học kỳ</span>
          <select v-model.number="filterMaHocKy" @change="loadTkbCount(); loadSchedules()"
            class="h-9 min-w-[220px] px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option value="">Chọn học kỳ</option>
            <option v-for="t in termOptions" :key="t.maHocKy" :value="t.maHocKy">{{ t.tenHocKy }}</option>
          </select>
        </label>
      </div>

      <!-- Stat pills -->
      <div class="flex flex-wrap gap-2">
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--color-success-bg) text-sm">
          <span class="font-bold text-xl text-(--color-success-text)">{{ stats.total }}</span>
          <span class="text-(--text-muted)">Bộ TKB đã công bố</span>
        </div>
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
          <BookOpen :size="13" class="text-(--text-muted)" />
          <span class="font-bold text-(--text-heading)">{{ stats.tkb }}</span>
          <span class="text-(--text-muted)">Thời khóa biểu chi tiết</span>
        </div>
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
          <Users :size="13" class="text-(--text-muted)" />
          <span class="font-bold text-(--text-heading)">{{ stats.lopTong }}</span>
          <span class="text-(--text-muted)">Khóa học</span>
        </div>
      </div>

      <!-- Tab bar -->
      <div class="flex gap-1 bg-(--surface-input) p-1 rounded-xl w-fit border border-(--border-default)">
        <button
          v-for="tab in [{ v: 'lichHoc', l: 'Thời khóa biểu' }, { v: 'thayDoi', l: 'Thay đổi phát sinh' }]" :key="tab.v"
          class="px-4 py-1.5 rounded-lg text-sm font-medium transition-all"
          :class="activeTab === tab.v ? 'bg-(--surface-card) text-(--text-heading) shadow-sm' : 'text-(--text-muted) hover:text-(--text-heading)'"
          @click="activeTab = tab.v"
        >{{ tab.l }}</button>
      </div>

      <!-- ── Tab: Lịch học đã công bố ── -->
      <div v-if="activeTab === 'lichHoc'" class="flex gap-4 items-start">

        <!-- List -->
        <div class="flex-1 min-w-0 space-y-2">
          <p class="text-xs text-(--text-muted)">{{ publishedSchedules.length }} bộ TKB đã công bố trong học kỳ đã chọn</p>

          <!-- Newest -->
          <div
            v-if="newestSchedule"
            class="surface-card border border-(--border-card) rounded-2xl shadow-md border-l-4 border-l-emerald-500 cursor-pointer transition-all hover:shadow-lg"
            :class="selected?.id === newestSchedule.id ? 'ring-2 ring-(--lg-primary)' : ''"
            @click="pickSchedule(newestSchedule)"
          >
            <div class="p-4 flex items-center gap-4 flex-wrap">
              <div class="w-10 h-10 rounded-xl bg-(--color-success-bg) text-(--color-success-text) flex items-center justify-center shrink-0">
                <CheckCircle :size="20" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <span class="text-sm font-bold text-(--text-heading) font-mono">{{ newestSchedule.id.slice(0, 8) }}</span>
                  <GlassBadge variant="success" size="sm">Mới nhất</GlassBadge>
                  <GlassBadge variant="success" size="sm">Đã công bố</GlassBadge>
                </div>
                <p class="text-xs text-(--text-muted) mt-0.5">{{ newestSchedule.term }} · {{ newestSchedule.department }}</p>
                <p class="text-xs text-(--text-muted) flex items-center gap-1 mt-0.5">
                  <UserCircle2 :size="12" />
                  {{ newestSchedule.tenNguoiYeuCau || 'Giáo vụ' }} · Công bố:
                  {{ formatDateTime(newestSchedule.ngayXuatBan) }}
                </p>
              </div>
              <div class="flex gap-4 shrink-0 text-center">
                <div>
                  <p class="text-lg font-bold text-(--text-heading)">{{ newestSchedule.metrics.xepDuoc }}</p>
                  <p class="text-[10px] text-(--text-muted)">Xếp được</p>
                </div>
                <div>
                  <p class="text-lg font-bold text-(--text-heading)">{{ newestSchedule.metrics.classes }}</p>
                  <p class="text-[10px] text-(--text-muted)">Khóa học</p>
                </div>
                <div>
                  <p class="text-lg font-bold text-(--lg-primary)">{{ Math.round(newestSchedule.metrics.score) }}</p>
                  <p class="text-[10px] text-(--text-muted)">Điểm</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Others -->
          <div
            v-for="s in otherSchedules" :key="s.id"
            class="surface-card border border-(--border-card) rounded-2xl shadow-sm cursor-pointer transition-all hover:shadow-md"
            :class="selected?.id === s.id ? 'ring-2 ring-(--lg-primary)' : ''"
            @click="pickSchedule(s)"
          >
            <div class="p-4 flex items-center gap-4 flex-wrap">
              <div class="w-10 h-10 rounded-xl bg-(--surface-input) text-(--text-muted) flex items-center justify-center shrink-0">
                <CalendarCheck :size="20" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <span class="text-sm font-bold text-(--text-heading) font-mono">{{ s.id.slice(0, 8) }}</span>
                  <GlassBadge variant="success" size="sm">Đã công bố</GlassBadge>
                </div>
                <p class="text-xs text-(--text-muted) mt-0.5">{{ s.term }} · {{ s.department }}</p>
                <p class="text-xs text-(--text-muted) flex items-center gap-1 mt-0.5">
                  <UserCircle2 :size="12" />
                  {{ s.tenNguoiYeuCau || 'Giáo vụ' }} · Công bố:
                  {{ formatDateTime(s.ngayXuatBan) }}
                </p>
              </div>
              <div class="flex gap-4 shrink-0 text-center">
                <div>
                  <p class="text-lg font-bold text-(--text-heading)">{{ s.metrics.xepDuoc }}</p>
                  <p class="text-[10px] text-(--text-muted)">Xếp được</p>
                </div>
                <div>
                  <p class="text-lg font-bold text-(--text-heading)">{{ s.metrics.classes }}</p>
                  <p class="text-[10px] text-(--text-muted)">Khóa học</p>
                </div>
                <div>
                  <p class="text-lg font-bold text-(--lg-primary)">{{ Math.round(s.metrics.score) }}</p>
                  <p class="text-[10px] text-(--text-muted)">Điểm</p>
                </div>
              </div>
            </div>
          </div>

          <EmptyState
            v-if="publishedSchedules.length === 0"
            title="Chưa có lịch công bố"
            description="Chưa có bộ thời khóa biểu nào được công bố trong cơ sở/học kỳ đã chọn."
          />
        </div>

        <!-- Detail panel -->
        <transition name="panel-slide">
          <div
            v-if="selected"
            class="w-96 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg overflow-hidden"
            style="position: sticky; top: 80px"
          >
            <div class="p-4 border-b border-(--border-default) flex items-center justify-between bg-(--color-success-bg)">
              <div class="flex items-center gap-2">
                <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
                <span class="text-xs font-bold text-(--color-success-text) uppercase tracking-wide">Đã công bố</span>
              </div>
              <button @click="selected = null" class="p-1 rounded-lg hover:bg-(--surface-input) text-(--text-muted)">
                <X :size="15" />
              </button>
            </div>

            <div class="p-4 space-y-4 max-h-[calc(100vh-180px)] overflow-y-auto">
              <!-- Publisher -->
              <div class="flex items-center gap-3 bg-(--surface-input) border border-(--border-default) rounded-xl p-3">
                <div class="w-10 h-10 rounded-full bg-(--lg-primary) text-white flex items-center justify-center font-bold shrink-0">
                  {{ initialOf(selected.tenNguoiYeuCau) }}
                </div>
                <div class="min-w-0">
                  <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide">Tài khoản công bố</p>
                  <p class="text-sm font-bold text-(--text-heading) truncate">{{ selected.tenNguoiYeuCau || 'Giáo vụ' }}</p>
                  <p class="text-xs text-(--text-muted)">{{ formatDateTime(selected.ngayXuatBan) }}</p>
                </div>
              </div>

              <!-- Info -->
              <div>
                <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Thông tin chung</p>
                <div class="space-y-2 text-xs text-(--text-body)">
                  <div class="flex justify-between gap-2">
                    <span class="text-(--text-muted) shrink-0">Mã bản nháp:</span>
                    <span class="font-mono font-bold text-(--text-heading) break-all text-right">{{ selected.id }}</span>
                  </div>
                  <div class="flex justify-between gap-2">
                    <span class="text-(--text-muted) shrink-0">Học kỳ:</span>
                    <span class="font-medium text-right">{{ selected.term }}</span>
                  </div>
                  <div class="flex justify-between gap-2">
                    <span class="text-(--text-muted) shrink-0">Đơn vị:</span>
                    <span class="font-medium text-right">{{ selected.department }}</span>
                  </div>
                  <div class="flex justify-between gap-2">
                    <span class="text-(--text-muted) shrink-0">Ngày tạo:</span>
                    <span class="font-medium text-right">{{ formatDateTime(selected.ngayTao) }}</span>
                  </div>
                  <div class="flex justify-between gap-2">
                    <span class="text-(--text-muted) shrink-0">Ngày công bố:</span>
                    <span class="font-medium text-right">{{ formatDateTime(selected.ngayXuatBan) }}</span>
                  </div>
                </div>
              </div>

              <!-- Metrics -->
              <div class="grid grid-cols-4 gap-2">
                <div v-for="(val, key) in { 'Xếp': selected.metrics.xepDuoc, 'Chưa': selected.metrics.khongXepDuoc, 'Khóa': selected.metrics.classes, 'Điểm': Math.round(selected.metrics.score) }" :key="key"
                     class="bg-(--surface-input) rounded-xl p-2 border border-(--border-default) text-center">
                  <p class="text-base font-bold text-(--text-heading)">{{ val }}</p>
                  <p class="text-[10px] text-(--text-muted)">{{ key }}</p>
                </div>
              </div>

              <!-- Distinct stats -->
              <div class="flex flex-wrap gap-1.5">
                <GlassBadge variant="info" size="sm">{{ distinctStats.mon }} môn</GlassBadge>
                <GlassBadge variant="info" size="sm">{{ distinctStats.lop }} lớp</GlassBadge>
                <GlassBadge variant="info" size="sm">{{ distinctStats.gv }} giảng viên</GlassBadge>
              </div>

              <!-- Items -->
              <div>
                <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Chi tiết xếp lịch ({{ selectedItems.length }})</p>
                <div class="max-h-[380px] overflow-y-auto pr-1 space-y-2">
                  <div v-for="it in selectedItems" :key="it.maDraftItem ?? it.MaDraftItem"
                       class="border border-(--border-default) rounded-xl p-3 bg-(--surface-card)">
                    <div class="flex justify-between items-center gap-2 mb-1">
                      <span class="text-sm font-bold text-(--text-heading) truncate">{{ it.tenMonHoc ?? it.TenMonHoc ?? `Khóa học ${it.maKhoaHoc ?? it.MaKhoaHoc}` }}</span>
                      <span class="font-mono text-xs text-(--lg-primary) shrink-0">{{ Math.round(it.score ?? it.Score ?? 0) }}đ</span>
                    </div>
                    <div class="text-xs text-(--text-muted) space-y-0.5">
                      <p v-if="it.tenLop ?? it.TenLop" class="text-(--text-body)">
                        Lớp: {{ it.tenLop ?? it.TenLop }}
                        <span v-if="it.maCodeLop || it.MaCodeLop" class="font-mono">({{ it.maCodeLop ?? it.MaCodeLop }})</span>
                      </p>
                      <p v-if="it.tenGiaoVien ?? it.TenGiaoVien" class="text-(--text-body)">Giảng viên: {{ it.tenGiaoVien ?? it.TenGiaoVien }}</p>
                      <p v-if="it.thuTrongTuan || it.maCaHoc || it.tenPhong" class="text-(--text-body)">
                        Thứ {{ it.thuTrongTuan ?? it.ThuTrongTuan }} · {{ it.tenCa ?? it.TenCa ?? '' }} · {{ it.tenPhong ?? it.TenPhong ?? '' }}
                      </p>
                      <div class="flex items-center gap-1.5 pt-1">
                        <GlassBadge :variant="(it.trangThai ?? it.TrangThai) === 'xep_duoc' ? 'success' : 'danger'" size="sm">
                          {{ itemStatusLabel(it.trangThai ?? it.TrangThai) }}
                        </GlassBadge>
                        <GlassBadge v-if="it.mucDoPhuHop || it.MucDoPhuHop" variant="warning" size="sm">
                          Phù hợp {{ it.mucDoPhuHop ?? it.MucDoPhuHop }}%
                        </GlassBadge>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </transition>
      </div>

      <!-- ── Tab: Thay đổi phát sinh ── -->
      <div v-if="activeTab === 'thayDoi'" class="space-y-2">
        <EmptyState
          v-if="true"
          title="Không có thay đổi"
          description="Chưa có biến động nào sau khi lịch được công bố. Giáo vụ sẽ được thông báo khi phát sinh thay đổi từ giảng viên."
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
.panel-slide-enter-active, .panel-slide-leave-active { transition: opacity .2s ease, transform .2s ease; }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: translateX(16px); }
</style>
