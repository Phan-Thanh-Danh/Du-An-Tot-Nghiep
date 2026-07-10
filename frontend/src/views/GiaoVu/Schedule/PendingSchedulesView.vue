<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import {
  Clock, Filter, AlertCircle, Send, Eye, X, Loader2, CheckCircle
} from 'lucide-vue-next'
import { useRoute } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import { academicTermApi } from '@/services/academicTermApi'
import { courseApi } from '@/services/courseApi'
import { scheduleApi } from '@/services/scheduleApi'

const loading = ref(false)
const loadingFilters = ref(false)
const error = ref('')
const schedules = ref([])
const selectedItem = ref(null)
const publishing = ref(false)
const popupStore = usePopupStore()
const route = useRoute()
const filterMaDonVi = ref('')
const filterMaHocKy = ref('')
const highlightDraftId = ref('')
const campusOptions = ref([])
const termOptions = ref([])

const statusLabels = {
  pending: { label: 'Bản nháp', variant: 'warning' },
  returned: { label: 'Cần chỉnh sửa', variant: 'danger' },
  published: { label: 'Đã xuất bản', variant: 'success' },
  draft: { label: 'Bản nháp', variant: 'info' },
}

const visibleSchedules = computed(() => schedules.value)
const getStatusLabel = (status) => statusLabels[status] || statusLabels.draft

function unwrap(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  if (Array.isArray(data?.data)) return data.data
  if (Array.isArray(data?.Data)) return data.Data
  return []
}

function unwrapList(response) {
  return unwrap(response)
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
    maHocKy: c.maHocKy ?? c.MaHocKy,
    tenHocKy: c.tenHocKy ?? c.TenHocKy,
  }
}

function mapDraft(item) {
  const id = item.draftId ?? item.DraftId ?? item.id ?? item.Id
  const maDonVi = item.maDonVi ?? item.MaDonVi
  const maHocKy = item.maHocKy ?? item.MaHocKy
  return {
    id,
    maDonVi,
    maHocKy,
    term: item.tenHocKy ?? item.TenHocKy ?? item.hocKy ?? item.HocKy ?? (maHocKy ? `Học kỳ ${maHocKy}` : 'Chưa xác định học kỳ'),
    type: item.loaiLich ?? item.LoaiLich ?? 'Bản nháp',
    department: item.tenDonVi ?? item.TenDonVi ?? item.donVi ?? item.DonVi ?? (maDonVi ? `Cơ sở ${maDonVi}` : 'Chưa xác định đơn vị'),
    created: formatDate(item.createdAt ?? item.CreatedAt ?? item.ngayTao ?? item.NgayTao),
    submitted: formatDate(item.submittedAt ?? item.SubmittedAt ?? item.ngayGuiDuyet ?? item.NgayGuiDuyet),
    status: item.status ?? item.Status ?? item.trangThai ?? item.TrangThai ?? 'draft',
    metrics: {
      classes: item.classCount ?? item.soLop ?? item.SoLop ?? item.tongCourse ?? item.TongCourse ?? 0,
      teachers: item.teacherCount ?? item.soGiangVien ?? item.SoGiangVien ?? 0,
      hours: item.totalHours ?? item.tongTiet ?? item.TongTiet ?? 0,
    },
    note: item.note ?? item.ghiChu ?? item.GhiChu ?? '',
  }
}

function formatDate(value) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return String(value)
  return date.toLocaleDateString('vi-VN')
}

function syncQueryContext() {
  filterMaDonVi.value = route.query.maDonVi ? Number(route.query.maDonVi) : ''
  filterMaHocKy.value = route.query.maHocKy ? Number(route.query.maHocKy) : ''
  highlightDraftId.value = route.query.draftId ? String(route.query.draftId) : ''
}

async function loadFilterOptions() {
  loadingFilters.value = true
  try {
    const [termsRes, coursesRes] = await Promise.all([
      academicTermApi.list({ PageIndex: 1, PageSize: 100 }),
      courseApi.getCourses({ PageIndex: 1, PageSize: 100 }),
    ])

    termOptions.value = unwrapList(termsRes)
      .map(normalizeTerm)
      .filter(t => t.maHocKy)
      .sort((a, b) => Number(b.maHocKy) - Number(a.maHocKy))

    const campusMap = new Map()
    unwrapList(coursesRes)
      .map(normalizeCourse)
      .forEach((course) => {
        if (!course.maDonVi) return
        campusMap.set(
          Number(course.maDonVi),
          course.tenDonVi || `Cơ sở ${course.maDonVi}`,
        )
      })

    campusOptions.value = [...campusMap.entries()]
      .map(([value, label]) => ({ value, label }))
      .sort((a, b) => String(a.label).localeCompare(String(b.label), 'vi'))

    if (!filterMaDonVi.value && campusOptions.value.length === 1) {
      filterMaDonVi.value = campusOptions.value[0].value
    }

    if (!filterMaHocKy.value && termOptions.value.length === 1) {
      filterMaHocKy.value = termOptions.value[0].maHocKy
    }
  } catch (e) {
    console.error('Load pending draft filters failed', e)
    error.value = e?.message || 'Không thể tải danh sách cơ sở/học kỳ.'
  } finally {
    loadingFilters.value = false
  }
}

async function loadSchedules() {
  loading.value = true
  error.value = ''
  try {
    if (!filterMaDonVi.value || !filterMaHocKy.value) {
      schedules.value = []
      selectedItem.value = null
      return
    }

    const response = await scheduleApi.listDrafts({
      maDonVi: Number(filterMaDonVi.value),
      maHocKy: Number(filterMaHocKy.value),
    })
    const items = unwrap(response).map(mapDraft)
    schedules.value = items

    if (highlightDraftId.value) {
      selectedItem.value =
        items.find(x => String(x.id) === String(highlightDraftId.value)) ||
        null
    }
  } catch (e) {
    error.value = e.message || 'Không thể tải danh sách bản nháp thời khóa biểu.'
    schedules.value = []
  } finally {
    loading.value = false
  }
}

async function publishDraft(item) {
  if (!item) return
  publishing.value = true
  try {
    await scheduleApi.publishDraft({ draftId: item.id })
    popupStore.success('Thành công', 'Đã xuất bản thời khóa biểu.')
    await loadSchedules()
    selectedItem.value = null
  } catch (e) {
    popupStore.error('Lỗi', e.message || 'Không thể xuất bản bản nháp.')
  } finally {
    publishing.value = false
  }
}

onMounted(async () => {
  syncQueryContext()
  await loadFilterOptions()
  if (filterMaDonVi.value && filterMaHocKy.value) {
    await loadSchedules()
  }
})

watch(
  () => route.query,
  async () => {
    syncQueryContext()
    if (filterMaDonVi.value && filterMaHocKy.value) {
      await loadSchedules()
    } else {
      schedules.value = []
      selectedItem.value = null
    }
  },
)
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <Clock class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Bản nháp thời khóa biểu</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Danh sách bản nháp thời khóa biểu theo cơ sở và học kỳ. Giáo vụ rà soát trước khi xuất bản.</p>
      </div>
      <div class="flex flex-wrap items-end gap-3">
        <label class="flex flex-col gap-1 text-xs font-semibold text-(--text-muted)">
          <span>Cơ sở</span>
          <select
            v-model.number="filterMaDonVi"
            class="h-10 min-w-[220px] rounded-xl border border-(--border-input) bg-(--surface-input) px-3 text-sm text-(--text-body) outline-none focus:ring-2 focus:ring-(--border-focus)"
          >
            <option value="">Chọn cơ sở</option>
            <option
              v-for="campus in campusOptions"
              :key="campus.value"
              :value="campus.value"
            >
              {{ campus.label }}
            </option>
          </select>
        </label>
        <label class="flex flex-col gap-1 text-xs font-semibold text-(--text-muted)">
          <span>Học kỳ</span>
          <select
            v-model.number="filterMaHocKy"
            class="h-10 min-w-[220px] rounded-xl border border-(--border-input) bg-(--surface-input) px-3 text-sm text-(--text-body) outline-none focus:ring-2 focus:ring-(--border-focus)"
          >
            <option value="">Chọn học kỳ</option>
            <option
              v-for="term in termOptions"
              :key="term.maHocKy"
              :value="term.maHocKy"
            >
              {{ term.tenHocKy }}
            </option>
          </select>
        </label>
        <GlassButton
          variant="secondary"
          class="h-10"
          :disabled="loading || loadingFilters || !filterMaDonVi || !filterMaHocKy"
          @click="loadSchedules"
        >
          <Loader2 v-if="loadingFilters" :size="15" class="mr-1 animate-spin" />
          <Filter v-else :size="15" class="mr-1" />
          Tải bản nháp
        </GlassButton>
      </div>
    </div>

    <!-- Main Content Layout -->
    <div class="flex flex-1 min-h-0 gap-4 flex-col lg:flex-row">
      <!-- Left: List -->
      <div class="flex-1 surface-card border border-(--border-card) rounded-2xl p-4 flex flex-col gap-3 min-w-0 overflow-y-auto">
        <div v-if="!filterMaDonVi || !filterMaHocKy" class="surface-card border border-(--border-card) rounded-2xl p-8 text-sm text-(--text-muted)">
          Vui lòng chọn cơ sở và học kỳ để xem bản nháp thời khóa biểu.
        </div>
        <div v-else-if="loading" class="p-4"><ListSkeleton :items="4" /></div>
        <div v-else-if="error" class="p-8 text-sm text-(--color-danger-text)">{{ error }}</div>
        <div v-else-if="visibleSchedules.length === 0" class="p-8 text-sm text-(--text-muted)">Chưa có bản nháp thời khóa biểu trong cơ sở/học kỳ đã chọn.</div>
        <template v-else>
        <div v-for="item in visibleSchedules" :key="item.id"
             @click="selectedItem = item"
             class="surface-card border rounded-2xl p-4 cursor-pointer transition-all hover:shadow-md relative overflow-hidden group flex flex-col lg:flex-row lg:items-center gap-4"
             :class="[
               selectedItem?.id === item.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary) bg-(--lg-primary)/5' : 'border-(--border-card)',
               String(item.id) === String(highlightDraftId) ? 'ring-2 ring-emerald-400' : ''
             ]">

          <div class="absolute left-0 top-0 bottom-0 w-1" :class="item.status === 'returned' ? 'bg-red-500' : 'bg-amber-500'"></div>

          <!-- Info -->
          <div class="flex-1 pl-2">
            <div class="flex items-center gap-2 mb-1">
              <span class="text-xs font-mono font-bold text-(--text-muted)">{{ item.id }}</span>
              <GlassBadge v-if="String(item.id) === String(highlightDraftId)" variant="success" size="sm">Bản nháp vừa sinh</GlassBadge>
              <GlassBadge :variant="getStatusLabel(item.status).variant" size="sm">{{ getStatusLabel(item.status).label }}</GlassBadge>
            </div>
            <h3 class="font-bold text-(--text-heading) text-base">{{ item.term }}</h3>
            <p class="text-sm text-(--text-body) font-medium">{{ item.department }} - {{ item.type }}</p>
          </div>

          <!-- Metrics -->
          <div class="flex items-center gap-4 py-2 lg:py-0 border-y lg:border-y-0 lg:border-l border-(--border-default) lg:px-4 shrink-0">
             <div class="text-center">
                <p class="text-[10px] uppercase text-(--text-muted) font-bold">Lớp</p>
                <p class="font-bold text-(--text-heading)">{{ item.metrics.classes }}</p>
             </div>
             <div class="text-center">
                <p class="text-[10px] uppercase text-(--text-muted) font-bold">Giảng viên</p>
                <p class="font-bold text-(--text-heading)">{{ item.metrics.teachers }}</p>
             </div>
             <div class="text-center">
                <p class="text-[10px] uppercase text-(--text-muted) font-bold">Tiết/Tuần</p>
                <p class="font-bold text-(--text-heading)">{{ item.metrics.hours }}</p>
             </div>
          </div>

          <!-- Actions -->
          <div class="flex flex-wrap lg:flex-nowrap gap-2 shrink-0 lg:pl-2" @click.stop>
             <GlassButton v-if="item.status === 'pending'" variant="danger" size="sm" class="flex-1 lg:flex-none justify-center">Thu hồi</GlassButton>
             <GlassButton v-if="item.status === 'returned'" variant="primary" size="sm" class="flex-1 lg:flex-none justify-center"><Send :size="14" class="mr-1"/>Gửi lại</GlassButton>
             <GlassButton variant="secondary" size="sm" class="flex-1 lg:flex-none justify-center" @click="selectedItem = item"><Eye :size="14" class="mr-1"/>Chi tiết</GlassButton>
          </div>
        </div>
        </template>
      </div>

      <!-- Right: Detail Panel -->
      <div v-if="selectedItem" class="w-full lg:w-80 shrink-0 flex flex-col gap-3">
        <div class="surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
          <div class="p-4 border-b border-(--border-default) flex justify-between items-center bg-(--surface-input)">
            <h3 class="font-bold text-(--text-heading)">Chi tiết bộ TKB</h3>
            <button class="text-(--text-muted) hover:text-(--text-heading)" @click="selectedItem = null"><X :size="16" /></button>
          </div>

          <div class="p-4 flex-1 overflow-auto space-y-5">
            <div>
              <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Thông tin chung</p>
              <div class="space-y-2 text-sm text-(--text-body)">
                <div class="flex justify-between"><span class="text-(--text-muted)">Mã TKB:</span> <span class="font-mono font-bold">{{ selectedItem.id }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Học kỳ:</span> <span class="font-medium text-right">{{ selectedItem.term }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Đơn vị:</span> <span class="font-medium text-right">{{ selectedItem.department }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Trạng thái:</span>
                  <GlassBadge :variant="getStatusLabel(selectedItem.status).variant" size="sm">{{ getStatusLabel(selectedItem.status).label }}</GlassBadge>
                </div>
              </div>
            </div>

            <div v-if="selectedItem.note" class="p-3 bg-(--color-danger-bg) border border-(--color-danger-border) rounded-xl">
              <p class="text-xs font-bold text-(--color-danger-text) flex items-center gap-1 mb-1"><AlertCircle :size="14"/> Ghi chú rà soát</p>
              <p class="text-sm text-(--color-danger-text) opacity-90">{{ selectedItem.note }}</p>
            </div>

            <div>
              <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Quy trình (Timeline)</p>
              <div class="relative pl-3 border-l-2 border-(--border-default) space-y-4 text-sm mt-2">
                 <div class="relative">
                   <div class="absolute -left-4 w-2 h-2 rounded-full bg-(--text-muted) mt-1.5"></div>
                   <p class="font-bold text-(--text-heading)">Tạo bản nháp</p>
                   <p class="text-xs text-(--text-muted)">{{ selectedItem.created }}</p>
                 </div>
                 <div class="relative">
                   <div class="absolute -left-4 w-2 h-2 rounded-full bg-amber-400 mt-1.5 ring-2 ring-amber-100 dark:ring-amber-900"></div>
                   <p class="font-bold text-(--text-heading)">Sẵn sàng rà soát</p>
                   <p class="text-xs text-(--text-muted)">{{ selectedItem.submitted }}</p>
                 </div>
                 <div v-if="selectedItem.status === 'returned'" class="relative">
                   <div class="absolute -left-4 w-2 h-2 rounded-full bg-red-500 mt-1.5 ring-2 ring-red-100 dark:ring-red-900"></div>
                   <p class="font-bold text-red-600 dark:text-red-400">Yêu cầu chỉnh sửa</p>
                   <p class="text-xs text-(--text-muted)">13/05/2026</p>
                 </div>
              </div>
            </div>
          </div>

          <div class="p-4 border-t border-(--border-default) space-y-2">
            <GlassButton variant="primary" class="w-full justify-center" :disabled="publishing" @click="publishDraft(selectedItem)">
              <Loader2 v-if="publishing" :size="15" class="mr-1.5 animate-spin" />
              <CheckCircle v-else :size="15" class="mr-1.5" />
              Xuất bản lịch
            </GlassButton>
            <GlassButton variant="secondary" class="w-full justify-center">Chỉnh sửa nội dung</GlassButton>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>
