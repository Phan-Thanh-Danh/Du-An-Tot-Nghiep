<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Users, UserPlus, Search, Filter, FileSpreadsheet, Lock, Unlock, Eye, Edit3,
  BookOpen, Award, CheckCircle, AlertTriangle, ShieldCheck, ChevronRight,
  GraduationCap, Calendar, Clock, Loader2
} from 'lucide-vue-next'
import { bghPersonnelApi } from '@/services/bghPersonnelApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import TeacherPersonnelModal from './TeacherPersonnelModal.vue'

const router = useRouter()

// State
const loading = ref(false)
const searchLoading = ref(false)
const teachers = ref([])
const totalItems = ref(0)
const currentPage = ref(1)
const pageSize = 10
const totalPages = ref(1)

// Filters
const keyword = ref('')
const orgFilter = ref('')
const majorFilter = ref('')
const statusFilter = ref('')

const orgsList = ref([])
const majorsList = ref([])

// Modals
const showModal = ref(false)
const selectedTeacher = ref(null)

const showLockModal = ref(false)
const lockingTeacher = ref(null)
const lockReason = ref('')
const locking = ref(false)

// Toast
const toastMsg = ref('')
const showToast = ref(false)
function triggerToast(msg) {
  toastMsg.value = msg
  showToast.value = true
  setTimeout(() => { showToast.value = false }, 3500)
}

// Quick KPI computations
const activeCount = computed(() => teachers.value.filter(t => t.trangThai === 'hoat_dong').length)
const totalClassesSum = computed(() => teachers.value.reduce((acc, t) => acc + (t.soLopHocKyHienTai || 0), 0))
const avgRatingOverall = computed(() => {
  const rated = teachers.value.filter(t => t.diemDanhGiaTrungBinh > 0)
  if (rated.length === 0) return 4.8
  return (rated.reduce((acc, t) => acc + t.diemDanhGiaTrungBinh, 0) / rated.length).toFixed(1)
})

let searchTimer = null

async function loadData() {
  loading.value = true
  try {
    const res = await bghPersonnelApi.getTeachers({
      pageIndex: currentPage.value,
      pageSize,
      keyword: keyword.value.trim(),
      maDonVi: orgFilter.value || undefined,
      maChuyenNganh: majorFilter.value || undefined,
      trangThai: statusFilter.value || undefined
    })
    const data = unwrapApiData(res) || {}
    teachers.value = data.items || []
    totalItems.value = data.totalItems || 0
    totalPages.value = Math.max(1, data.totalPages || 1)
  } catch (err) {
    console.error('Lỗi tải danh sách giảng viên:', err)
  } finally {
    loading.value = false
    searchLoading.value = false
  }
}

async function loadMetadata() {
  try {
    const [orgsRes, majorsRes] = await Promise.all([
      apiRequest('/api/organizations'),
      apiRequest('/api/chuyen-nganh')
    ])
    orgsList.value = (unwrapApiData(orgsRes) || []).map(o => ({
      maDonVi: o.id || o.maDonVi,
      tenDonVi: o.name || o.tenDonVi
    }))
    majorsList.value = unwrapApiData(majorsRes) || []
  } catch (err) {
    console.error('Lỗi tải metadata:', err)
  }
}

function onSearchInput(e) {
  searchLoading.value = true
  if (e?.target?.value !== undefined) keyword.value = e.target.value
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    currentPage.value = 1
    loadData()
  }, 300)
}

watch([orgFilter, majorFilter, statusFilter], () => {
  currentPage.value = 1
  loadData()
})

function openCreateModal() {
  selectedTeacher.value = null
  showModal.value = true
}

async function openEditModal(teacher) {
  selectedTeacher.value = teacher
  showModal.value = true
  try {
    const res = await bghPersonnelApi.getTeacherDetail(teacher.maNguoiDung || teacher.id)
    const detail = res?.data || res
    if (detail && (selectedTeacher.value?.maNguoiDung === (teacher.maNguoiDung || teacher.id) || selectedTeacher.value?.id === (teacher.maNguoiDung || teacher.id))) {
      selectedTeacher.value = detail
    }
  } catch (err) {
    console.error('Failed to load teacher detail for modal:', err)
  }
}

function goToDetail(teacherId) {
  router.push(`/bgh/human-resources/${teacherId}`)
}

function openToggleLockModal(teacher) {
  lockingTeacher.value = teacher
  lockReason.value = teacher.trangThai === 'hoat_dong' ? 'Tạm dừng công tác' : 'Kích hoạt lại giảng dạy'
  showLockModal.value = true
}

async function handleConfirmLock() {
  if (!lockingTeacher.value) return
  locking.value = true
  try {
    await bghPersonnelApi.toggleLockTeacher(lockingTeacher.value.maNguoiDung, lockReason.value)
    showLockModal.value = false
    triggerToast(`Đã ${lockingTeacher.value.trangThai === 'hoat_dong' ? 'khóa' : 'mở khóa'} tài khoản ${lockingTeacher.value.hoTen}!`)
    await loadData()
  } catch (err) {
    alert(err?.message || 'Lỗi khi cập nhật trạng thái.')
  } finally {
    locking.value = false
  }
}

onMounted(() => {
  loadMetadata()
  loadData()
})

onUnmounted(() => {
  clearTimeout(searchTimer)
})
</script>

<template>
  <div class="space-y-5 pb-12">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-lg bg-blue-500/10 text-blue-600 dark:text-blue-400 flex items-center justify-center font-bold">
            <Users :size="18" />
          </div>
          <h1 class="text-xl font-bold text-heading">Nhân Sự Giảng Viên</h1>
        </div>
        <p class="text-xs text-muted mt-1">Quản lý hồ sơ chuyên môn, tải giảng dạy và nhật ký ca dạy thuộc cơ sở</p>
      </div>

      <div class="flex items-center gap-2">
        <button
          @click="openCreateModal"
          class="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-xl transition-all shadow-md shadow-blue-500/20 cursor-pointer"
        >
          <UserPlus :size="16" />
          <span>Thêm giảng viên</span>
        </button>
      </div>
    </div>

    <!-- KPI Cards -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3.5">
      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3.5 shadow-sm">
        <div class="w-11 h-11 rounded-xl bg-blue-500/10 text-blue-600 dark:text-blue-400 flex items-center justify-center shrink-0">
          <Users :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-semibold text-muted uppercase">Tổng giảng viên</p>
          <h3 class="text-xl font-black text-heading mt-0.5">{{ totalItems }}</h3>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3.5 shadow-sm">
        <div class="w-11 h-11 rounded-xl bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
          <CheckCircle :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-semibold text-muted uppercase">Đang hoạt động</p>
          <h3 class="text-xl font-black text-emerald-600 dark:text-emerald-400 mt-0.5">{{ activeCount }} / {{ totalItems }}</h3>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3.5 shadow-sm">
        <div class="w-11 h-11 rounded-xl bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 flex items-center justify-center shrink-0">
          <GraduationCap :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-semibold text-muted uppercase">Tổng lớp phụ trách</p>
          <h3 class="text-xl font-black text-indigo-600 dark:text-indigo-400 mt-0.5">{{ totalClassesSum }} lớp</h3>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3.5 shadow-sm">
        <div class="w-11 h-11 rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-400 flex items-center justify-center shrink-0">
          <Award :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-semibold text-muted uppercase">Đánh giá trung bình</p>
          <h3 class="text-xl font-black text-amber-600 dark:text-amber-400 mt-0.5">{{ avgRatingOverall }} / 5.0 ⭐</h3>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-wrap gap-3.5 items-end">
      <div class="flex-1 min-w-[220px]">
        <label class="block text-xs font-bold text-heading mb-1.5">Tìm kiếm giảng viên</label>
        <div class="relative">
          <Loader2 v-if="searchLoading" class="absolute left-3 top-1/2 -translate-y-1/2 text-blue-500 animate-spin" :size="16" />
          <Search v-else class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
          <input
            v-model="keyword"
            @input="onSearchInput"
            type="text"
            placeholder="Tìm theo Tên, Email, SĐT, Mã GV..."
            class="w-full pl-9 pr-3.5 py-2 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500"
          />
        </div>
      </div>

      <div class="w-full sm:w-48">
        <label class="block text-xs font-bold text-heading mb-1.5">Cơ sở</label>
        <select v-model="orgFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500">
          <option value="">Tất cả cơ sở</option>
          <option v-for="org in orgsList" :key="org.maDonVi" :value="org.maDonVi">{{ org.tenDonVi }}</option>
        </select>
      </div>

      <div class="w-full sm:w-48">
        <label class="block text-xs font-bold text-heading mb-1.5">Chuyên ngành</label>
        <select v-model="majorFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500">
          <option value="">Tất cả chuyên ngành</option>
          <option v-for="m in majorsList" :key="m.maChuyenNganh || m.id" :value="m.maChuyenNganh || m.id">{{ m.tenChuyenNganh || m.name }}</option>
        </select>
      </div>

      <div class="w-full sm:w-36">
        <label class="block text-xs font-bold text-heading mb-1.5">Trạng thái</label>
        <select v-model="statusFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500">
          <option value="">Tất cả</option>
          <option value="hoat_dong">Hoạt động</option>
          <option value="bi_khoa">Bị khóa</option>
        </select>
      </div>
    </div>

    <!-- Data Table -->
    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden flex flex-col">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs text-body whitespace-nowrap">
          <thead class="bg-(--surface-input)/60 border-b border-card font-bold text-heading uppercase tracking-wider text-[10px]">
            <tr>
              <th class="px-4 py-3.5">Mã GV</th>
              <th class="px-4 py-3.5">Giảng viên</th>
              <th class="px-4 py-3.5">Cơ sở</th>
              <th class="px-4 py-3.5">Chuyên ngành chính</th>
              <th class="px-4 py-3.5 text-center">Môn được dạy</th>
              <th class="px-4 py-3.5 text-center">Lớp phụ trách</th>
              <th class="px-4 py-3.5 text-center">Đánh giá SV</th>
              <th class="px-4 py-3.5 text-center">Trạng thái</th>
              <th class="px-4 py-3.5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card">
            <tr v-if="loading && teachers.length === 0">
              <td colspan="9" class="p-8 text-center text-muted">
                <Loader2 class="w-6 h-6 animate-spin mx-auto text-blue-500 mb-2" />
                <span>Đang tải danh sách nhân sự giảng viên...</span>
              </td>
            </tr>
            <tr v-else-if="teachers.length === 0">
              <td colspan="9" class="p-8 text-center text-muted">
                <Users class="w-8 h-8 mx-auto text-muted mb-2 opacity-50" />
                <span>Không tìm thấy giảng viên nào phù hợp với bộ lọc.</span>
              </td>
            </tr>
            <tr
              v-for="t in teachers"
              :key="t.maNguoiDung"
              class="hover:bg-(--surface-input)/40 transition-colors cursor-pointer group"
              @click="goToDetail(t.maNguoiDung)"
            >
              <td class="px-4 py-3.5 font-bold text-heading">{{ t.maGiangVien }}</td>
              <td class="px-4 py-3.5">
                <div class="font-bold text-heading group-hover:text-blue-600 transition-colors">{{ t.hoTen }}</div>
                <div class="text-[11px] text-muted">{{ t.email }}</div>
              </td>
              <td class="px-4 py-3.5 text-muted">{{ t.tenDonVi }}</td>
              <td class="px-4 py-3.5 font-medium text-heading">{{ t.chuyenNganhChinh }}</td>
              <td class="px-4 py-3.5 text-center">
                <span class="px-2 py-0.5 rounded-full bg-blue-500/10 text-blue-600 dark:text-blue-400 font-bold text-[11px]">
                  {{ t.soMonDuocPhepDay }} môn
                </span>
              </td>
              <td class="px-4 py-3.5 text-center font-bold text-heading">
                {{ t.soLopHocKyHienTai }} lớp
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="font-bold text-amber-500">{{ t.diemDanhGiaTrungBinh > 0 ? t.diemDanhGiaTrungBinh : 4.8 }} ⭐</span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span
                  class="px-2.5 py-1 rounded-full text-[10px] font-bold"
                  :class="t.trangThai === 'hoat_dong' ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' : 'bg-rose-500/10 text-rose-600 dark:text-rose-400'"
                >
                  {{ t.trangThai === 'hoat_dong' ? 'Đang hoạt động' : 'Đã khóa' }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-right" @click.stop>
                <div class="flex items-center justify-end gap-1.5">
                  <button
                    @click="goToDetail(t.maNguoiDung)"
                    class="p-1.5 rounded-lg hover:bg-(--surface-input) text-muted hover:text-heading transition-colors"
                    title="Xem chi tiết hồ sơ"
                  >
                    <Eye :size="15" />
                  </button>
                  <button
                    @click="openEditModal(t)"
                    class="p-1.5 rounded-lg hover:bg-(--surface-input) text-blue-600 hover:text-blue-700 transition-colors"
                    title="Chỉnh sửa chuyên môn"
                  >
                    <Edit3 :size="15" />
                  </button>
                  <button
                    @click="openToggleLockModal(t)"
                    class="p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"
                    :class="t.trangThai === 'hoat_dong' ? 'text-rose-500 hover:text-rose-600' : 'text-emerald-500 hover:text-emerald-600'"
                    :title="t.trangThai === 'hoat_dong' ? 'Khóa tài khoản' : 'Mở khóa tài khoản'"
                  >
                    <Lock v-if="t.trangThai === 'hoat_dong'" :size="15" />
                    <Unlock v-else :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="px-4 py-3 border-t border-card flex items-center justify-between bg-(--surface-input)/30 text-xs text-muted">
        <div>Hiển thị <strong>{{ teachers.length }}</strong> / {{ totalItems }} giảng viên</div>
        <div class="flex items-center gap-2">
          <button
            @click="currentPage > 1 && (currentPage--, loadData())"
            :disabled="currentPage <= 1"
            class="px-3 py-1.5 rounded-lg border border-input hover:bg-(--surface-input) disabled:opacity-40 transition-colors font-semibold"
          >
            Trang trước
          </button>
          <span class="font-bold text-heading">{{ currentPage }} / {{ totalPages }}</span>
          <button
            @click="currentPage < totalPages && (currentPage++, loadData())"
            :disabled="currentPage >= totalPages"
            class="px-3 py-1.5 rounded-lg border border-input hover:bg-(--surface-input) disabled:opacity-40 transition-colors font-semibold"
          >
            Trang sau
          </button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <TeacherPersonnelModal
      :show="showModal"
      :teacher="selectedTeacher"
      :orgs="orgsList"
      @close="showModal = false"
      @saved="loadData"
    />

    <!-- Lock/Unlock Modal -->
    <div v-if="showLockModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
      <div class="surface-card border border-card w-full max-w-md rounded-2xl shadow-2xl overflow-hidden p-6 space-y-4">
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 rounded-xl flex items-center justify-center"
            :class="lockingTeacher?.trangThai === 'hoat_dong' ? 'bg-rose-500/10 text-rose-600' : 'bg-emerald-500/10 text-emerald-600'"
          >
            <Lock v-if="lockingTeacher?.trangThai === 'hoat_dong'" :size="20" />
            <Unlock v-else :size="20" />
          </div>
          <div>
            <h3 class="text-base font-bold text-heading">
              {{ lockingTeacher?.trangThai === 'hoat_dong' ? 'Khóa Tài Khoản Giảng Viên' : 'Mở Khóa Giảng Viên' }}
            </h3>
            <p class="text-xs text-muted">{{ lockingTeacher?.hoTen }} ({{ lockingTeacher?.email }})</p>
          </div>
        </div>

        <div>
          <label class="block text-xs font-bold text-heading mb-1.5">Lý do thực hiện (Bắt buộc ghi log Audit) <span class="text-rose-500">*</span></label>
          <textarea
            v-model="lockReason"
            rows="3"
            placeholder="Nhập lý do BGH thay đổi trạng thái hoạt động..."
            class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500"
          ></textarea>
        </div>

        <div class="flex items-center justify-end gap-2 pt-2">
          <button @click="showLockModal = false" class="px-4 py-2 text-xs font-bold text-body hover:bg-(--surface-input) rounded-xl">
            Hủy
          </button>
          <button
            @click="handleConfirmLock"
            :disabled="locking || !lockReason.trim()"
            class="px-4 py-2 rounded-xl text-xs font-bold text-white transition-all disabled:opacity-50"
            :class="lockingTeacher?.trangThai === 'hoat_dong' ? 'bg-rose-600 hover:bg-rose-700' : 'bg-emerald-600 hover:bg-emerald-700'"
          >
            {{ locking ? 'Đang xử lý...' : (lockingTeacher?.trangThai === 'hoat_dong' ? 'Xác nhận Khóa' : 'Xác nhận Mở khóa') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Toast -->
    <div
      v-if="showToast"
      class="fixed bottom-6 right-6 z-50 px-4 py-2.5 rounded-xl bg-emerald-600 text-white text-xs font-bold shadow-lg animate-slide-up flex items-center gap-2"
    >
      <CheckCircle :size="16" />
      <span>{{ toastMsg }}</span>
    </div>
  </div>
</template>
