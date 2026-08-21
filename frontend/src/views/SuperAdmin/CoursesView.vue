<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, X, Eye, BookOpen, RefreshCw, Info } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { courseApi } from '@/services/courseApi'
import { unwrapApiData } from '@/services/apiClient'
import { organizationApi } from '@/services/organizationService'
import academicSchedulingApi from '@/services/academicSchedulingApi'
import { subjectApi } from '@/services/subjectApi'

const STATUS_LABELS = {
  nhap: 'Bản nháp',
  da_xuat_ban: 'Đã xuất bản',
  luu_tru: 'Đã lưu trữ',
}

const columns = [
  { key: 'code', label: 'Mã (ID)' },
  { key: 'title', label: 'Tên khóa học' },
  { key: 'subject', label: 'Môn học' },
  { key: 'teacher', label: 'Giảng viên' },
  { key: 'term', label: 'Học kỳ' },
  { key: 'className', label: 'Lớp' },
  { key: 'org', label: 'Cơ sở' },
  { key: 'status', label: 'Trạng thái' },
]

const loading = ref(false)
const error = ref('')
const rows = ref([])

const keyword = ref('')
const appliedKeyword = ref('')
const filterMaDonVi = ref(null)
const filterMaNganh = ref(null)
const filterMaChuyenNganh = ref(null)
const filterMaMonHoc = ref(null)

const orgs = ref([])
const majors = ref([])
const specializations = ref([])
const subjects = ref([])

const showDetail = ref(false)
const detail = ref(null)
const detailLoading = ref(false)

const orgOptions = computed(() =>
  orgs.value.map(o => ({ value: o.id, label: o.name }))
)
const majorOptions = computed(() =>
  majors.value.map(m => ({ value: m.maNganh, label: m.tenNganh }))
)
const specializationOptions = computed(() =>
  specializations.value
    .filter(s => !filterMaNganh.value || s.maNganh === filterMaNganh.value)
    .map(s => ({ value: s.maChuyenNganh, label: s.tenChuyenNganh }))
)
const subjectOptions = computed(() => {
  const ng = filterMaNganh.value
  const cn = filterMaChuyenNganh.value
  return subjects.value
    .filter(s => (!ng || s.maNganh === ng) && (!cn || s.maChuyenNganh === cn))
    .map(s => ({ value: s.maMonHoc, label: `${s.maCodeMonHoc} - ${s.tenMonHoc}` }))
})

const hasFilters = computed(() =>
  appliedKeyword.value || filterMaDonVi.value || filterMaNganh.value || filterMaChuyenNganh.value || filterMaMonHoc.value
)

function mapRow(course) {
  return {
    id: course.maKhoaHoc || course.MaKhoaHoc,
    code: String(course.maKhoaHoc || course.MaKhoaHoc || ''),
    title: course.tieuDe || course.TieuDe || 'Chưa có dữ liệu',
    subject: course.tenMonHoc || course.TenMonHoc || course.maCodeMonHoc || course.MaCodeMonHoc || 'Chưa có dữ liệu',
    teacher: course.tenGiaoVien || course.TenGiaoVien || 'Chưa phân công',
    term: course.tenHocKy || course.TenHocKy || '—',
    className: course.tenLop || course.TenLop || '—',
    org: course.tenDonVi || course.TenDonVi || '—',
    status: STATUS_LABELS[course.trangThai || course.TrangThai] || course.trangThai || course.TrangThai || '—',
  }
}

function statusClass(status) {
  if (status === 'Đã xuất bản') return 'bg-(--color-success-bg) text-(--color-success-text)'
  if (status === 'Bản nháp') return 'bg-(--color-warning-bg) text-(--color-warning-text)'
  if (status === 'Đã lưu trữ') return 'bg-(--color-info-bg) text-(--color-info-text)'
  return 'bg-(--surface-input) text-label'
}

async function fetchAllPages(fetchPage) {
  const all = []
  let pageIndex = 1
  while (true) {
    const items = await fetchPage(pageIndex)
    all.push(...items)
    if (items.length < 100) break
    pageIndex++
  }
  return all
}

async function fetchCourses() {
  loading.value = true
  error.value = ''
  try {
    const params = { pageSize: 100 }
    if (appliedKeyword.value) params.keyword = appliedKeyword.value
    if (filterMaDonVi.value) params.maDonVi = filterMaDonVi.value
    if (filterMaNganh.value) params.maNganh = filterMaNganh.value
    if (filterMaChuyenNganh.value) params.maChuyenNganh = filterMaChuyenNganh.value
    if (filterMaMonHoc.value) params.maMonHoc = filterMaMonHoc.value

    const courses = await fetchAllPages((pageIndex) => {
      const pageParams = { ...params, pageIndex }
      return apiRequestWithQuery(pageParams)
    })
    rows.value = courses.map(mapRow)
  } catch (e) {
    error.value = e?.message || 'Không tải được dữ liệu từ API.'
    rows.value = []
  } finally {
    loading.value = false
  }
}

async function apiRequestWithQuery(params) {
  const response = await courseApi.getCourses(params)
  const data = unwrapApiData(response)
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

function applySearch() {
  appliedKeyword.value = keyword.value.trim()
  fetchCourses()
}

function clearFilters() {
  keyword.value = ''
  appliedKeyword.value = ''
  filterMaDonVi.value = null
  filterMaNganh.value = null
  filterMaChuyenNganh.value = null
  filterMaMonHoc.value = null
  fetchCourses()
}

async function loadOrganizations() {
  try {
    orgs.value = unwrapApiData(await organizationApi.getAll()) || []
  } catch {
    orgs.value = []
  }
}

async function loadMajors() {
  try {
    majors.value = await academicSchedulingApi.getMajors()
  } catch {
    majors.value = []
  }
}

async function loadSpecializations() {
  if (!filterMaNganh.value) {
    specializations.value = []
    filterMaChuyenNganh.value = null
    filterMaMonHoc.value = null
    return
  }
  try {
    specializations.value = await academicSchedulingApi.getSpecializations(filterMaNganh.value)
  } catch {
    specializations.value = []
  }
  const stillValid = specializations.value.some(s => s.maChuyenNganh === filterMaChuyenNganh.value)
  if (!stillValid) filterMaChuyenNganh.value = null
  fetchCourses()
}

async function loadSubjects() {
  try {
    subjects.value = await fetchAllPages((pageIndex) =>
      subjectApi.list({ pageIndex, pageSize: 100 })
    )
  } catch {
    subjects.value = []
  }
}

function formatDate(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  return d.toLocaleDateString('vi-VN')
}

async function openDetail(row) {
  showDetail.value = true
  detail.value = null
  detailLoading.value = true
  try {
    detail.value = unwrapApiData(await courseApi.getCourseDetail(row.id)) || {}
  } catch {
    detail.value = {}
  } finally {
    detailLoading.value = false
  }
}

onMounted(() => {
  fetchCourses()
  loadOrganizations()
  loadMajors()
  loadSubjects()
})
</script>

<template>
  <section class="space-y-4">
    <header class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <p class="text-sm font-semibold text-label">SuperAdmin</p>
        <h1 class="text-2xl font-bold text-heading">Quản lý khóa học</h1>
        <p class="mt-1 text-sm text-body">
          Danh sách khóa học lấy từ API thật. Bấm vào một khóa học để xem chi tiết.
        </p>
      </div>

      <GlassButton variant="secondary" class="shrink-0" :disabled="loading" @click="fetchCourses">
        <RefreshCw size="15" class="mr-1.5" :class="{ 'animate-spin': loading }" /> Tải lại
      </GlassButton>
    </header>

    <!-- Bộ lọc -->
    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
        <div class="relative">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" size="15" />
          <input
            v-model="keyword"
            type="text"
            placeholder="Tìm tên khóa học, môn, giảng viên, lớp..."
            class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)"
            @keydown.enter="applySearch"
          />
        </div>
        <LmsSelect v-model="filterMaDonVi" :options="orgOptions" placeholder="Tất cả cơ sở" searchable @change="fetchCourses" />
        <LmsSelect v-model="filterMaNganh" :options="majorOptions" placeholder="Tất cả ngành" searchable @change="loadSpecializations" />
        <LmsSelect
          v-model="filterMaChuyenNganh"
          :options="specializationOptions"
          placeholder="Tất cả chuyên ngành"
          :disabled="!filterMaNganh"
          searchable
          @change="fetchCourses"
        />
        <LmsSelect
          v-model="filterMaMonHoc"
          :options="subjectOptions"
          placeholder="Tất cả môn học"
          searchable
          @change="fetchCourses"
        />
      </div>
      <div class="flex items-center gap-2 mt-3">
        <GlassButton variant="primary" class="h-9 px-4 text-xs" @click="applySearch">
          <Search size="13" class="mr-1" /> Tìm kiếm
        </GlassButton>
        <button
          v-if="hasFilters"
          @click="clearFilters"
          class="h-9 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors"
        >
          <X size="14" /> Xóa lọc
        </button>
        <span class="ml-auto text-xs text-label">Tìm thấy {{ rows.length }} khóa học</span>
      </div>
    </div>

    <div v-if="error" class="rounded-lg border border-(--color-danger-text) bg-(--color-danger-bg) p-4 text-sm text-(--color-danger-text)">
      <div class="flex items-start gap-2">
        <Info class="mt-0.5 h-4 w-4 shrink-0" />
        <span>{{ error }}</span>
      </div>
    </div>

    <div class="overflow-hidden rounded-2xl border border-card surface-card">
      <div v-if="loading" class="p-6">
        <LoadingSkeleton :lines="6" />
      </div>
      <div v-else-if="rows.length === 0" class="p-6">
        <EmptyState title="Không tìm thấy khóa học nào" description="Thử thay đổi từ khóa hoặc bộ lọc." />
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-(--border-card) text-sm">
          <thead class="bg-(--surface-input) text-left text-xs font-semibold uppercase text-label">
            <tr>
              <th v-for="column in columns" :key="column.key" class="px-4 py-3">
                {{ column.label }}
              </th>
              <th class="px-4 py-3 text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr
              v-for="row in rows"
              :key="row.id"
              class="hover:bg-(--surface-hover) cursor-pointer transition-colors"
              @click="openDetail(row)"
            >
              <td class="px-4 py-3 font-mono text-xs font-bold text-muted">{{ row.code }}</td>
              <td class="px-4 py-3 font-bold text-heading max-w-[300px] truncate">{{ row.title }}</td>
              <td class="px-4 py-3 text-body">{{ row.subject }}</td>
              <td class="px-4 py-3 text-body">{{ row.teacher }}</td>
              <td class="px-4 py-3 text-body">{{ row.term }}</td>
              <td class="px-4 py-3 text-body">{{ row.className }}</td>
              <td class="px-4 py-3 text-body">{{ row.org }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold" :class="statusClass(row.status)">
                  {{ row.status }}
                </span>
              </td>
              <td class="px-4 py-3 text-center" @click.stop>
                <button
                  class="inline-flex items-center gap-1 h-8 px-3 rounded-lg text-xs font-bold text-(--sidebar-accent) hover:bg-(--accent-primary-soft) transition-colors"
                  @click="openDetail(row)"
                >
                  <Eye size="13" /> Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal chi tiết -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div
          v-if="showDetail"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4"
          @click.self="showDetail = false"
        >
          <div class="w-full max-w-3xl lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-heading">Chi tiết khóa học</h3>
              <button @click="showDetail = false" class="text-muted hover:text-heading p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors">
                <X size="18" />
              </button>
            </div>

            <div v-if="detailLoading" class="px-6 py-8">
              <LoadingSkeleton :lines="8" />
            </div>

            <div v-else-if="detail" class="px-6 py-5 overflow-y-auto space-y-5" style="max-height: calc(90vh - 140px)">
              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2 flex-wrap">
                  <h4 class="text-base font-bold text-heading">{{ detail.tieuDe }}</h4>
                  <span
                    class="px-2.5 py-1 rounded-full text-[11px] font-bold"
                    :class="statusClass(STATUS_LABELS[detail.trangThai] || detail.trangThai)"
                  >
                    {{ STATUS_LABELS[detail.trangThai] || detail.trangThai }}
                  </span>
                </div>
                <p class="text-xs text-label">Khóa học #{{ detail.maKhoaHoc }} · Tạo lúc {{ formatDate(detail.ngayTao) }}</p>
              </div>

              <div class="grid grid-cols-2 sm:grid-cols-3 gap-3">
                <div class="surface-card border border-card rounded-xl p-3">
                  <p class="text-[11px] font-semibold text-label uppercase">Môn học</p>
                  <p class="text-sm font-bold text-heading mt-1">{{ detail.tenMonHoc }}</p>
                  <p class="text-xs text-muted font-mono">{{ detail.maMonHocCode }}</p>
                </div>
                <div class="surface-card border border-card rounded-xl p-3">
                  <p class="text-[11px] font-semibold text-label uppercase">Giảng viên</p>
                  <p class="text-sm font-bold text-heading mt-1">{{ detail.tenGiaoVien || 'Chưa phân công' }}</p>
                </div>
                <div class="surface-card border border-card rounded-xl p-3">
                  <p class="text-[11px] font-semibold text-label uppercase">Học kỳ</p>
                  <p class="text-sm font-bold text-heading mt-1">{{ detail.tenHocKy || '—' }}</p>
                </div>
                <div class="surface-card border border-card rounded-xl p-3">
                  <p class="text-[11px] font-semibold text-label uppercase">Lớp</p>
                  <p class="text-sm font-bold text-heading mt-1">{{ detail.tenLop || '—' }}</p>
                </div>
                <div class="surface-card border border-card rounded-xl p-3">
                  <p class="text-[11px] font-semibold text-label uppercase">Cơ sở</p>
                  <p class="text-sm font-bold text-heading mt-1">{{ detail.tenDonVi || '—' }}</p>
                </div>
                <div class="surface-card border border-card rounded-xl p-3">
                  <p class="text-[11px] font-semibold text-label uppercase">Tiến độ khối</p>
                  <p class="text-sm font-bold text-heading mt-1">
                    {{ detail.soBlockHoc ? `${detail.soBlockHoc} block` : '—' }}
                    <span v-if="detail.ngayBatDauBlock" class="text-xs text-muted font-normal block mt-0.5">
                      {{ formatDate(detail.ngayBatDauBlock) }} → {{ formatDate(detail.ngayKetThucBlock) }}
                    </span>
                  </p>
                </div>
              </div>

              <div v-if="detail.moTa" class="surface-card border border-card rounded-xl p-4">
                <p class="text-[11px] font-semibold text-label uppercase mb-1">Mô tả</p>
                <p class="text-sm text-body">{{ detail.moTa }}</p>
              </div>

              <div>
                <p class="text-sm font-bold text-heading mb-2 flex items-center gap-2">
                  <BookOpen size="15" class="text-(--sidebar-accent)" />
                  Nội dung khóa học
                  <span class="text-xs font-normal text-label">
                    ({{ (detail.chuongs || []).length }} chương · {{ (detail.lessons || []).length }} bài học)
                  </span>
                </p>

                <div v-if="detail.chuongs && detail.chuongs.length" class="flex flex-col gap-2">
                  <div v-for="ch in detail.chuongs" :key="ch.maChuong" class="surface-card border border-card rounded-xl p-3">
                    <p class="text-sm font-bold text-heading">
                      Chương {{ ch.thuTu }}: {{ ch.tieuDe }}
                      <span class="text-xs font-normal text-label">({{ (ch.baiHocs || []).length }} bài)</span>
                    </p>
                    <div v-if="ch.baiHocs && ch.baiHocs.length" class="mt-2 flex flex-col gap-1">
                      <p v-for="b in ch.baiHocs" :key="b.maBaiHoc" class="text-xs text-body flex items-center gap-1.5">
                        <span class="h-1 w-1 rounded-full bg-(--sidebar-accent) shrink-0" />
                        {{ b.tieuDe }}
                        <span class="text-muted">({{ b.loaiBaiHoc || '—' }})</span>
                      </p>
                    </div>
                  </div>
                </div>

                <div v-else-if="detail.lessons && detail.lessons.length" class="surface-card border border-card rounded-xl p-3 flex flex-col gap-1">
                  <p v-for="b in detail.lessons" :key="b.maBaiHoc" class="text-xs text-body">
                    {{ b.tieuDe }} <span class="text-muted">({{ b.loaiBaiHoc || '—' }})</span>
                  </p>
                </div>

                <p v-else class="text-xs text-label italic">Khóa học chưa có nội dung bài giảng.</p>
              </div>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>
  </section>
</template>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
