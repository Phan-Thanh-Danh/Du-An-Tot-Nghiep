<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { trainingProgramApi } from '@/services/trainingProgramApi'
import { organizationApi } from '@/services/organizationService'
import CurriculumEditorModal from './components/CurriculumEditorModal.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { 
  BookOpen, Plus, Copy, Edit2, Trash2, CheckCircle2, XCircle, 
  AlertCircle, Search, RefreshCw, GitCompare, History, Filter, 
  ChevronLeft, ChevronRight, Send, Lock, Unlock, Archive, ExternalLink, ShieldCheck
} from 'lucide-vue-next'

const popup = usePopupStore()

// Active Tab: 'list', 'compare', 'audit'
const activeTab = ref('list')

// Data State
const loading = ref(false)
const programs = ref([])
const totalItems = ref(0)
const totalPages = ref(1)
const pageIndex = ref(1)
const pageSize = ref(20)

// Filters
const searchQuery = ref('')
const selectedCampus = ref('')
const selectedMajor = ref('')
const selectedStatus = ref('')

// Filter Options
const campusOptions = ref([{ value: '', label: 'Tất cả cơ sở' }])
const majorOptions = ref([{ value: '', label: 'Tất cả chuyên ngành' }])
const statusOptions = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'draft', label: 'Nháp (Draft)' },
  { value: 'active', label: 'Đang hoạt động (Active)' },
  { value: 'inactive', label: 'Tạm ngưng (Inactive)' },
  { value: 'archived', label: 'Lưu trữ (Archived)' }
]

const programOptions = computed(() => {
  return programs.value.map(p => ({
    value: p.maChuongTrinh || p.MaChuongTrinh,
    label: `${p.maCodeChuongTrinh || p.MaCodeChuongTrinh} - ${p.tenChuongTrinh || p.TenChuongTrinh} (v${p.version || p.Version})`
  }))
})

// Modal & Drawer State
const isCreateEditModalOpen = ref(false)
const modalMode = ref('create') // 'create', 'edit'
const programForm = ref({
  id: 0,
  maCodeChuongTrinh: '',
  tenChuongTrinh: '',
  version: '1.0',
  maChuyenNganh: 1,
  maKhoaTuyenSinh: 1,
  soHocKy: 7,
  thoiGianDaoTaoThang: 42,
  tongTinChiYeuCau: 120,
  soTinChiToiThieuMoiKy: 12,
  soTinChiToiDaMoiKy: 24,
  moTa: ''
})

// Clone Modal State
const isCloneModalOpen = ref(false)
const cloneSourceProgram = ref(null)
const cloneForm = ref({
  maCodeChuongTrinh: '',
  tenChuongTrinh: '',
  version: '2.0',
  maKhoaTuyenSinh: 1,
  ghiChuThayDoi: ''
})

// Assign Modal State
const isAssignModalOpen = ref(false)
const assignProgram = ref(null)
const assignForm = ref({
  maKhoaTuyenSinhIds: [1],
  maDonViIds: [],
  ngayHieuLuc: '',
  ngayHetHieuLuc: ''
})

// Curriculum Editor Modal State
const isCurriculumModalOpen = ref(false)
const activeCurriculumProgram = ref(null)

// Compare Tool State
const compareSourceId = ref('')
const compareTargetId = ref('')
const compareLoading = ref(false)
const compareResult = ref(null)

// Load Data
const loadPrograms = async () => {
  loading.value = true
  try {
    const res = await trainingProgramApi.list({
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
      keyword: searchQuery.value,
      maDonVi: selectedCampus.value || undefined,
      maChuyenNganh: selectedMajor.value || undefined,
      trangThai: selectedStatus.value || undefined
    })

    const data = res?.data || res?.Data || res
    programs.value = Array.isArray(data) ? data : (data?.items || data?.Items || [])
    totalItems.value = data?.totalItems || data?.TotalItems || programs.value.length
    totalPages.value = data?.totalPages || data?.TotalPages || Math.ceil(totalItems.value / pageSize.value)
  } catch (err) {
    console.error('Error loading programs:', err)
    popup.error('Lỗi', 'Không thể tải danh sách chương trình đào tạo.')
  } finally {
    loading.value = false
  }
}

const loadOrganizations = async () => {
  try {
    const res = await organizationApi.getAll()
    const list = Array.isArray(res) ? res : (res?.data || res?.Data || [])
    const campuses = list.filter(o => o.capDo === 1 || o.CapDo === 1 || o.loaiDonVi === 'Campus')
    campusOptions.value = [
      { value: '', label: 'Tất cả cơ sở' },
      ...campuses.map(c => ({ value: c.id || c.Id, label: c.tenDonVi || c.TenDonVi }))
    ]
  } catch (err) {
    console.error('Error loading organizations:', err)
  }
}

onMounted(() => {
  loadPrograms()
  loadOrganizations()
})

watch([searchQuery, selectedCampus, selectedMajor, selectedStatus, pageIndex, pageSize], () => {
  loadPrograms()
})

// Action Handlers
const openCreateModal = () => {
  modalMode.value = 'create'
  programForm.value = {
    id: 0,
    maCodeChuongTrinh: `CTDT_${new Date().getFullYear()}`,
    tenChuongTrinh: '',
    version: '1.0',
    maChuyenNganh: 1,
    maKhoaTuyenSinh: 1,
    soHocKy: 7,
    thoiGianDaoTaoThang: 42,
    tongTinChiYeuCau: 120,
    soTinChiToiThieuMoiKy: 12,
    soTinChiToiDaMoiKy: 24,
    moTa: ''
  }
  isCreateEditModalOpen.value = true
}

const openEditModal = (prog) => {
  modalMode.value = 'edit'
  programForm.value = {
    id: prog.maChuongTrinh || prog.MaChuongTrinh,
    maCodeChuongTrinh: prog.maCodeChuongTrinh || prog.MaCodeChuongTrinh || '',
    tenChuongTrinh: prog.tenChuongTrinh || prog.TenChuongTrinh || '',
    version: prog.version || prog.Version || '1.0',
    maChuyenNganh: prog.maChuyenNganh || prog.MaChuyenNganh || 1,
    maKhoaTuyenSinh: prog.maKhoaTuyenSinh || prog.MaKhoaTuyenSinh || 1,
    soHocKy: prog.soHocKy || prog.SoHocKy || 7,
    thoiGianDaoTaoThang: prog.thoiGianDaoTaoThang || prog.ThoiGianDaoTaoThang || 42,
    tongTinChiYeuCau: prog.tongTinChiYeuCau || prog.TongTinChiYeuCau || 120,
    soTinChiToiThieuMoiKy: prog.soTinChiToiThieuMoiKy || prog.SoTinChiToiThieuMoiKy || 12,
    soTinChiToiDaMoiKy: prog.soTinChiToiDaMoiKy || prog.SoTinChiToiDaMoiKy || 24,
    moTa: prog.moTa || prog.MoTa || ''
  }
  isCreateEditModalOpen.value = true
}

const submitProgramForm = async () => {
  if (!programForm.value.maCodeChuongTrinh || !programForm.value.tenChuongTrinh) {
    popup.warning('Thiếu thông tin', 'Vui lòng nhập Mã và Tên chương trình đào tạo.')
    return
  }

  try {
    if (modalMode.value === 'create') {
      await trainingProgramApi.create({
        maCodeChuongTrinh: programForm.value.maCodeChuongTrinh.trim(),
        tenChuongTrinh: programForm.value.tenChuongTrinh.trim(),
        version: programForm.value.version.trim(),
        maChuyenNganh: Number(programForm.value.maChuyenNganh),
        maKhoaTuyenSinh: Number(programForm.value.maKhoaTuyenSinh),
        soHocKy: Number(programForm.value.soHocKy),
        thoiGianDaoTaoThang: Number(programForm.value.thoiGianDaoTaoThang),
        tongTinChiYeuCau: Number(programForm.value.tongTinChiYeuCau),
        soTinChiToiThieuMoiKy: Number(programForm.value.soTinChiToiThieuMoiKy),
        soTinChiToiDaMoiKy: Number(programForm.value.soTinChiToiDaMoiKy),
        moTa: programForm.value.moTa
      })
      popup.success('Thành công', 'Đã khởi tạo khung chương trình đào tạo mới.')
    } else {
      await trainingProgramApi.update(programForm.value.id, {
        maCodeChuongTrinh: programForm.value.maCodeChuongTrinh.trim(),
        tenChuongTrinh: programForm.value.tenChuongTrinh.trim(),
        version: programForm.value.version.trim(),
        maChuyenNganh: Number(programForm.value.maChuyenNganh),
        maKhoaTuyenSinh: Number(programForm.value.maKhoaTuyenSinh),
        soHocKy: Number(programForm.value.soHocKy),
        thoiGianDaoTaoThang: Number(programForm.value.thoiGianDaoTaoThang),
        tongTinChiYeuCau: Number(programForm.value.tongTinChiYeuCau),
        soTinChiToiThieuMoiKy: Number(programForm.value.soTinChiToiThieuMoiKy),
        soTinChiToiDaMoiKy: Number(programForm.value.soTinChiToiDaMoiKy),
        moTa: programForm.value.moTa
      })
      popup.success('Thành công', 'Đã cập nhật thông tin chương trình đào tạo.')
    }
    isCreateEditModalOpen.value = false
    loadPrograms()
  } catch (err) {
    console.error('Error submitting program form:', err)
    popup.error('Lỗi', err.message || 'Không thể lưu chương trình đào tạo.')
  }
}

// Clone Program
const openCloneModal = (prog) => {
  cloneSourceProgram.value = prog
  const currentCode = prog.maCodeChuongTrinh || prog.MaCodeChuongTrinh
  cloneForm.value = {
    maCodeChuongTrinh: `${currentCode}_v2`,
    tenChuongTrinh: `${prog.tenChuongTrinh || prog.TenChuongTrinh} (Sao chép)`,
    version: '2.0',
    maKhoaTuyenSinh: (prog.maKhoaTuyenSinh || prog.MaKhoaTuyenSinh || 1) + 1,
    ghiChuThayDoi: `Sao chép phiên bản từ ${prog.version || prog.Version || 'v1.0'}`
  }
  isCloneModalOpen.value = true
}

const submitCloneForm = async () => {
  if (!cloneForm.value.maCodeChuongTrinh || !cloneForm.value.tenChuongTrinh) {
    popup.warning('Thiếu thông tin', 'Vui lòng điền mã và tên chương trình mới.')
    return
  }

  const sourceId = cloneSourceProgram.value.maChuongTrinh || cloneSourceProgram.value.MaChuongTrinh
  try {
    await trainingProgramApi.clone(sourceId, {
      maCodeChuongTrinh: cloneForm.value.maCodeChuongTrinh.trim(),
      tenChuongTrinh: cloneForm.value.tenChuongTrinh.trim(),
      version: cloneForm.value.version.trim(),
      maKhoaTuyenSinh: Number(cloneForm.value.maKhoaTuyenSinh),
      ghiChuThayDoi: cloneForm.value.ghiChuThayDoi
    })
    popup.success('Nhân bản thành công', 'Đã nhân bản toàn bộ danh mục môn học sang phiên bản mới.')
    isCloneModalOpen.value = false
    loadPrograms()
  } catch (err) {
    console.error('Error cloning program:', err)
    popup.error('Lỗi', err.message || 'Không thể nhân bản chương trình đào tạo.')
  }
}

// Assign Program
const openAssignModal = (prog) => {
  assignProgram.value = prog
  assignForm.value = {
    maKhoaTuyenSinhIds: [prog.maKhoaTuyenSinh || prog.MaKhoaTuyenSinh || 1],
    maDonViIds: [],
    ngayHieuLuc: new Date().toISOString().split('T')[0],
    ngayHetHieuLuc: ''
  }
  isAssignModalOpen.value = true
}

const submitAssignForm = async () => {
  const progId = assignProgram.value.maChuongTrinh || assignProgram.value.MaChuongTrinh
  try {
    await trainingProgramApi.assign(progId, {
      maKhoaTuyenSinhIds: assignForm.value.maKhoaTuyenSinhIds,
      maDonViIds: assignForm.value.maDonViIds,
      ngayHieuLuc: assignForm.value.ngayHieuLuc || undefined,
      ngayHetHieuLuc: assignForm.value.ngayHetHieuLuc || undefined
    })
    popup.success('Đã áp dụng', 'Đã kích hoạt và gán áp dụng chương trình đào tạo.')
    isAssignModalOpen.value = false
    loadPrograms()
  } catch (err) {
    console.error('Error assigning program:', err)
    popup.error('Lỗi', err.message || 'Không thể gán áp dụng chương trình đào tạo.')
  }
}

// Curriculum Modal
const openCurriculumEditor = (prog) => {
  activeCurriculumProgram.value = prog
  isCurriculumModalOpen.value = true
}

// Quick Actions
const toggleStatus = async (prog, action) => {
  const id = prog.maChuongTrinh || prog.MaChuongTrinh
  try {
    if (action === 'activate') {
      await trainingProgramApi.activate(id)
      popup.success('Đã kích hoạt', 'Chương trình đào tạo đã chuyển sang trạng thái Active.')
    } else if (action === 'deactivate') {
      await trainingProgramApi.deactivate(id)
      popup.success('Đã tạm ngưng', 'Chương trình đào tạo đã chuyển sang trạng thái Inactive.')
    } else if (action === 'archive') {
      await trainingProgramApi.archive(id)
      popup.success('Đã lưu trữ', 'Chương trình đào tạo đã chuyển sang trạng thái Archived (Read-only).')
    } else if (action === 'delete') {
      if (!confirm(`Bạn có chắc chắn muốn vô hiệu hóa chương trình "${prog.tenChuongTrinh}"?`)) return
      await trainingProgramApi.delete(id)
      popup.success('Đã vô hiệu hóa', 'Chương trình đào tạo đã ngưng hoạt động.')
    }
    loadPrograms()
  } catch (err) {
    console.error(`Error performing ${action}:`, err)
    popup.error('Lỗi', err.message || 'Không thể thay đổi trạng thái.')
  }
}

// Compare Tool Run
const runCompare = async () => {
  if (!compareSourceId.value || !compareTargetId.value) {
    popup.warning('Chọn phiên bản', 'Vui lòng chọn cả phiên bản nguồn và phiên bản đích để so sánh.')
    return
  }
  if (compareSourceId.value === compareTargetId.value) {
    popup.warning('Trùng phiên bản', 'Vui lòng chọn 2 phiên bản khác nhau.')
    return
  }

  compareLoading.value = true
  try {
    const res = await trainingProgramApi.compare(compareSourceId.value, compareTargetId.value)
    compareResult.value = res?.data || res?.Data || res
  } catch (err) {
    console.error('Error running compare:', err)
    popup.error('Lỗi', 'Không thể so sánh 2 phiên bản chương trình.')
  } finally {
    compareLoading.value = false
  }
}

// Status Badges
const getStatusBadge = (status) => {
  const s = (status || '').toLowerCase()
  if (s === 'active') return { label: 'Đang hoạt động', class: 'bg-emerald-500/15 text-emerald-600 border border-emerald-300' }
  if (s === 'inactive') return { label: 'Tạm ngưng', class: 'bg-amber-500/15 text-amber-600 border border-amber-300' }
  if (s === 'archived') return { label: 'Đã lưu trữ', class: 'bg-slate-500/15 text-slate-600 border border-slate-300' }
  return { label: 'Nháp (Draft)', class: 'bg-teal-500/15 text-teal-600 border border-teal-300' }
}
</script>

<template>
  <div class="space-y-5 pb-12">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-heading flex items-center gap-2">
          <BookOpen class="text-teal-600" :size="28" />
          Quản lý Khung Chương trình Đào tạo (Curriculum SIS)
        </h1>
        <p class="text-xs text-label mt-1">
          Xây dựng, phân bổ môn học theo học kỳ, nhân bản phiên bản (Clone/Upgrade) và quản lý áp dụng theo khóa tuyển sinh / cơ sở.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <button @click="loadPrograms" class="glass-btn secondary shadow-sm inline-flex items-center gap-2" title="Tải lại">
          <RefreshCw :size="16" :class="{ 'animate-spin': loading }" /> Tải lại
        </button>
        <button @click="openCreateModal" class="glass-btn primary shadow-sm inline-flex items-center gap-2">
          <Plus :size="16" /> Tạo Khung CTĐT mới
        </button>
      </div>
    </div>

    <!-- Navigation Tabs -->
    <div class="flex items-center gap-2 border-b border-slate-500/10">
      <button 
        @click="activeTab = 'list'"
        class="px-4 py-2.5 text-xs font-bold transition flex items-center gap-2 border-b-2"
        :class="activeTab === 'list' ? 'border-teal-600 text-teal-600 dark:text-teal-400' : 'border-transparent text-label hover:text-heading'"
      >
        <BookOpen :size="16" /> Danh sách Khung CTĐT & Áp dụng
      </button>

      <button 
        @click="activeTab = 'compare'"
        class="px-4 py-2.5 text-xs font-bold transition flex items-center gap-2 border-b-2"
        :class="activeTab === 'compare' ? 'border-teal-600 text-teal-600 dark:text-teal-400' : 'border-transparent text-label hover:text-heading'"
      >
        <GitCompare :size="16" /> So sánh Phiên bản (Compare Diff)
      </button>

      <button 
        @click="activeTab = 'audit'"
        class="px-4 py-2.5 text-xs font-bold transition flex items-center gap-2 border-b-2"
        :class="activeTab === 'audit' ? 'border-teal-600 text-teal-600 dark:text-teal-400' : 'border-transparent text-label hover:text-heading'"
      >
        <History :size="16" /> Nhật ký Kiểm toán & Lịch sử
      </button>
    </div>

    <!-- TAB 1: DANH SÁCH & PHÂN BỔ -->
    <div v-if="activeTab === 'list'" class="space-y-4">
      <!-- Filter Bar -->
      <div class="surface-card p-4 rounded-2xl border border-card flex flex-wrap items-center gap-3">
        <div class="flex-1 min-w-[220px] relative">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-label" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm theo Mã CTĐT, Tên chương trình, Phiên bản..." 
            class="glass-input pl-9 w-full text-xs"
          />
        </div>

        <div class="w-48">
          <LmsSelect v-model="selectedCampus" :options="campusOptions" placeholder="Tất cả cơ sở" />
        </div>

        <div class="w-44">
          <LmsSelect v-model="selectedStatus" :options="statusOptions" placeholder="Tất cả trạng thái" />
        </div>
      </div>

      <!-- Data Table -->
      <div class="surface-card rounded-2xl border border-card overflow-hidden shadow-sm">
        <div v-if="loading" class="py-16 text-center text-label text-sm flex items-center justify-center gap-2">
          <RefreshCw :size="20" class="animate-spin" /> Đang tải danh sách chương trình đào tạo...
        </div>

        <div v-else-if="programs.length === 0" class="py-16 text-center text-label text-sm flex flex-col items-center gap-2">
          <AlertCircle :size="32" class="text-slate-400" />
          <p>Không tìm thấy chương trình đào tạo nào phù hợp.</p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-sm text-body whitespace-nowrap">
            <thead class="bg-slate-500/5 text-xs text-label font-bold uppercase border-b border-slate-500/10">
              <tr>
                <th class="px-4 py-3">Mã CTĐT</th>
                <th class="px-4 py-3">Tên chương trình</th>
                <th class="px-4 py-3">Chuyên ngành</th>
                <th class="px-4 py-3">Phiên bản</th>
                <th class="px-4 py-3">Cấu trúc Tín chỉ</th>
                <th class="px-4 py-3">Khóa tuyển sinh</th>
                <th class="px-4 py-3">Trạng thái</th>
                <th class="px-4 py-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-500/10">
              <tr v-for="prog in programs" :key="prog.maChuongTrinh || prog.MaChuongTrinh" class="hover:bg-slate-500/5 transition">
                <td class="px-4 py-3">
                  <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-teal-500/15 text-teal-600 border border-teal-300">
                    {{ prog.maCodeChuongTrinh || prog.MaCodeChuongTrinh }}
                  </span>
                </td>

                <td class="px-4 py-3">
                  <div class="font-semibold text-heading">{{ prog.tenChuongTrinh || prog.TenChuongTrinh }}</div>
                  <div v-if="prog.tenNguonChuongTrinh" class="text-[11px] text-label italic">
                    Gốc: {{ prog.tenNguonChuongTrinh }}
                  </div>
                </td>

                <td class="px-4 py-3 text-xs text-label font-medium">
                  {{ prog.tenChuyenNganh || prog.TenChuyenNganh || prog.tenNganh || 'Chưa phân nhánh' }}
                </td>

                <td class="px-4 py-3 text-xs font-bold text-heading">
                  v{{ prog.version || prog.Version || '1.0' }}
                </td>

                <td class="px-4 py-3 text-xs">
                  <span class="font-bold text-teal-600 dark:text-teal-400">{{ prog.tongTinChiYeuCau || prog.TongTinChiYeuCau || 120 }} TCI</span>
                  <span class="text-label"> · {{ prog.soHocKy || prog.SoHocKy || 7 }} kỳ</span>
                </td>

                <td class="px-4 py-3 text-xs">
                  <span class="px-2 py-0.5 rounded bg-slate-500/10 text-heading font-medium">
                    {{ prog.tenKhoa || prog.TenKhoa || `Khóa ${prog.maKhoaTuyenSinh || prog.MaKhoaTuyenSinh}` }}
                  </span>
                </td>

                <td class="px-4 py-3 text-xs">
                  <span :class="getStatusBadge(prog.trangThai || prog.TrangThai).class" class="px-2.5 py-0.5 rounded-full text-[11px] font-bold">
                    {{ getStatusBadge(prog.trangThai || prog.TrangThai).label }}
                  </span>
                </td>

                <td class="px-4 py-3 text-right">
                  <div class="flex items-center justify-end gap-1">
                    <button 
                      @click="openCurriculumEditor(prog)" 
                      class="action-btn text-teal-600 hover:bg-teal-500/10" 
                      title="Quản lý Môn học theo Học kỳ"
                    >
                      <BookOpen :size="15" />
                    </button>

                    <button 
                      @click="openCloneModal(prog)" 
                      class="action-btn text-indigo-600 hover:bg-indigo-500/10" 
                      title="Nhân bản (Clone CTĐT)"
                    >
                      <Copy :size="15" />
                    </button>

                    <button 
                      @click="openAssignModal(prog)" 
                      class="action-btn text-emerald-600 hover:bg-emerald-500/10" 
                      title="Áp dụng cho Khóa / Cơ sở"
                    >
                      <Send :size="15" />
                    </button>

                    <button 
                      @click="openEditModal(prog)" 
                      class="action-btn text-amber-600 hover:bg-amber-500/10" 
                      title="Chỉnh sửa thông tin"
                    >
                      <Edit2 :size="15" />
                    </button>

                    <button 
                      v-if="(prog.trangThai || '').toLowerCase() === 'draft'" 
                      @click="toggleStatus(prog, 'activate')" 
                      class="action-btn text-emerald-600 hover:bg-emerald-500/10" 
                      title="Kích hoạt"
                    >
                      <CheckCircle2 :size="15" />
                    </button>

                    <button 
                      v-if="(prog.trangThai || '').toLowerCase() === 'active'" 
                      @click="toggleStatus(prog, 'deactivate')" 
                      class="action-btn text-amber-600 hover:bg-amber-500/10" 
                      title="Tạm ngưng"
                    >
                      <XCircle :size="15" />
                    </button>

                    <button 
                      @click="toggleStatus(prog, 'archive')" 
                      class="action-btn text-slate-500 hover:bg-slate-500/10" 
                      title="Lưu trữ (Archive)"
                    >
                      <Archive :size="15" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="pagination-bar p-4 border-t border-slate-500/10 flex flex-col sm:flex-row items-center justify-between gap-4">
          <div class="text-xs text-label">
            Trang <strong>{{ pageIndex }}</strong> / <strong>{{ totalPages }}</strong> — Tổng số <strong>{{ totalItems }}</strong> chương trình
          </div>
          <div class="flex items-center gap-2">
            <button 
              @click="pageIndex > 1 && pageIndex--" 
              :disabled="pageIndex <= 1"
              class="glass-btn secondary py-1 px-2.5 text-xs" 
              :class="{ 'opacity-50 cursor-not-allowed': pageIndex <= 1 }"
            >
              <ChevronLeft :size="14" /> Trước
            </button>
            <button 
              @click="pageIndex < totalPages && pageIndex++" 
              :disabled="pageIndex >= totalPages"
              class="glass-btn secondary py-1 px-2.5 text-xs" 
              :class="{ 'opacity-50 cursor-not-allowed': pageIndex >= totalPages }"
            >
              Sau <ChevronRight :size="14" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- TAB 2: SO SÁNH PHIÊN BẢN (COMPARE TOOL) -->
    <div v-else-if="activeTab === 'compare'" class="space-y-5">
      <div class="surface-card p-5 rounded-2xl border border-card flex flex-col md:flex-row items-center gap-4">
        <div class="flex-1 w-full">
          <LmsSelect 
            v-model="compareSourceId" 
            :options="programOptions" 
            label="Chọn Phiên bản Nguồn (Gốc / V1)" 
            placeholder="-- Chọn CTĐT nguồn --" 
          />
        </div>

        <div class="text-label text-sm font-bold shrink-0 pt-4">VS</div>

        <div class="flex-1 w-full">
          <LmsSelect 
            v-model="compareTargetId" 
            :options="programOptions" 
            label="Chọn Phiên bản Đích (Mới / V2)" 
            placeholder="-- Chọn CTĐT đích --" 
          />
        </div>

        <button @click="runCompare" class="glass-btn primary py-2.5 px-5 text-xs font-bold shrink-0 inline-flex items-center gap-2 mt-4 md:mt-0">
          <GitCompare :size="16" /> So sánh Diff
        </button>
      </div>

      <!-- Compare Results -->
      <div v-if="compareLoading" class="py-16 text-center text-label text-sm flex items-center justify-center gap-2">
        <RefreshCw :size="20" class="animate-spin" /> Đang phân tích so sánh 2 phiên bản...
      </div>

      <div v-else-if="compareResult" class="space-y-4">
        <!-- Stats Row -->
        <div class="grid grid-cols-2 sm:grid-cols-5 gap-3">
          <div class="surface-card p-3 rounded-xl border border-emerald-300/40 bg-emerald-500/5 text-center">
            <div class="text-xl font-bold text-emerald-600">+{{ compareResult.totalAdded }}</div>
            <div class="text-[11px] text-label font-medium">Môn thêm mới</div>
          </div>
          <div class="surface-card p-3 rounded-xl border border-rose-300/40 bg-rose-500/5 text-center">
            <div class="text-xl font-bold text-rose-600">-{{ compareResult.totalRemoved }}</div>
            <div class="text-[11px] text-label font-medium">Môn bị cắt giảm</div>
          </div>
          <div class="surface-card p-3 rounded-xl border border-amber-300/40 bg-amber-500/5 text-center">
            <div class="text-xl font-bold text-amber-600">{{ compareResult.totalShifted }}</div>
            <div class="text-[11px] text-label font-medium">Môn đổi Học kỳ</div>
          </div>
          <div class="surface-card p-3 rounded-xl border border-indigo-300/40 bg-indigo-500/5 text-center">
            <div class="text-xl font-bold text-indigo-600">{{ compareResult.totalModified }}</div>
            <div class="text-[11px] text-label font-medium">Môn sửa TCI/Loại</div>
          </div>
          <div class="surface-card p-3 rounded-xl border border-slate-300/40 text-center">
            <div class="text-xl font-bold text-heading">{{ compareResult.totalUnchanged }}</div>
            <div class="text-[11px] text-label font-medium">Môn giữ nguyên</div>
          </div>
        </div>

        <!-- Diff Table -->
        <div class="surface-card rounded-2xl border border-card overflow-hidden">
          <table class="w-full text-left text-sm text-body whitespace-nowrap">
            <thead class="bg-slate-500/5 text-xs text-label font-bold uppercase border-b border-slate-500/10">
              <tr>
                <th class="px-4 py-3">Mã môn</th>
                <th class="px-4 py-3">Tên môn học</th>
                <th class="px-4 py-3">Học kỳ V1 ➔ V2</th>
                <th class="px-4 py-3">Số tín chỉ V1 ➔ V2</th>
                <th class="px-4 py-3">Loại thay đổi</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-500/10">
              <tr v-for="diff in compareResult.differences" :key="diff.maMonHoc" class="hover:bg-slate-500/5 transition">
                <td class="px-4 py-3 font-bold text-heading">{{ diff.maCodeMonHoc }}</td>
                <td class="px-4 py-3 font-semibold text-heading">{{ diff.tenMonHoc }}</td>
                <td class="px-4 py-3 text-xs">
                  <span v-if="diff.diffType === 'added'" class="text-emerald-600 font-bold">Thêm ở HK{{ diff.targetHocKy }}</span>
                  <span v-else-if="diff.diffType === 'removed'" class="text-rose-600 line-through">HK{{ diff.sourceHocKy }}</span>
                  <span v-else-if="diff.sourceHocKy !== diff.targetHocKy" class="text-amber-600 font-bold">HK{{ diff.sourceHocKy }} ➔ HK{{ diff.targetHocKy }}</span>
                  <span v-else class="text-label">HK{{ diff.sourceHocKy }}</span>
                </td>

                <td class="px-4 py-3 text-xs">
                  <span v-if="diff.diffType === 'added'" class="text-emerald-600 font-bold">{{ diff.targetTinChi }} TCI</span>
                  <span v-else-if="diff.diffType === 'removed'" class="text-rose-600 line-through">{{ diff.sourceTinChi }} TCI</span>
                  <span v-else-if="diff.sourceTinChi !== diff.targetTinChi" class="text-indigo-600 font-bold">{{ diff.sourceTinChi }} ➔ {{ diff.targetTinChi }} TCI</span>
                  <span v-else class="text-label">{{ diff.sourceTinChi }} TCI</span>
                </td>

                <td class="px-4 py-3 text-xs">
                  <span v-if="diff.diffType === 'added'" class="px-2 py-0.5 rounded text-[11px] font-bold bg-emerald-500/15 text-emerald-600 border border-emerald-300">+ Thêm mới</span>
                  <span v-else-if="diff.diffType === 'removed'" class="px-2 py-0.5 rounded text-[11px] font-bold bg-rose-500/15 text-rose-600 border border-rose-300">- Cắt giảm</span>
                  <span v-else-if="diff.diffType === 'shifted'" class="px-2 py-0.5 rounded text-[11px] font-bold bg-amber-500/15 text-amber-600 border border-amber-300">🔄 Đổi kỳ</span>
                  <span v-else-if="diff.diffType === 'modified'" class="px-2 py-0.5 rounded text-[11px] font-bold bg-indigo-500/15 text-indigo-600 border border-indigo-300">✏️ Đổi TCI</span>
                  <span v-else class="px-2 py-0.5 rounded text-[11px] font-medium bg-slate-500/10 text-label">Giữ nguyên</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- TAB 3: AUDIT LOG & HISTORY -->
    <div v-else-if="activeTab === 'audit'" class="surface-card p-6 rounded-2xl border border-card space-y-4">
      <h3 class="text-base font-bold text-heading flex items-center gap-2">
        <History :size="18" class="text-teal-600" /> Nhật ký Thay đổi & Kiểm toán Phiên bản CTĐT
      </h3>

      <div class="space-y-3">
        <div class="p-3 surface-input rounded-xl border border-card flex items-start gap-3 text-xs">
          <div class="w-8 h-8 rounded-full bg-teal-500/15 text-teal-600 flex items-center justify-center shrink-0 font-bold">V2</div>
          <div class="flex-1">
            <div class="flex items-center justify-between">
              <span class="font-bold text-heading">Clone CTĐT_2026_v1 ➔ CTĐT_2027_v2</span>
              <span class="text-label text-[11px]">Hôm nay, 14:20</span>
            </div>
            <p class="text-label mt-0.5">Người thực hiện: <strong>Super Admin</strong>. Đã kế thừa nguyên vẹn 36 môn học từ bản V1.0.</p>
          </div>
        </div>

        <div class="p-3 surface-input rounded-xl border border-card flex items-start gap-3 text-xs">
          <div class="w-8 h-8 rounded-full bg-emerald-500/15 text-emerald-600 flex items-center justify-center shrink-0 font-bold">AS</div>
          <div class="flex-1">
            <div class="flex items-center justify-between">
              <span class="font-bold text-heading">Gán áp dụng CTĐT_2026_v1 cho Khóa K2026 tại Cơ sở Hồ Chí Minh</span>
              <span class="text-label text-[11px]">20/08/2026</span>
            </div>
            <p class="text-label mt-0.5">Người thực hiện: <strong>Super Admin</strong>. Áp dụng hiệu lực từ ngày 01/09/2026.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <!-- Create / Edit Program Modal -->
    <div v-if="isCreateEditModalOpen" class="modal-overlay" @click.self="isCreateEditModalOpen = false">
      <div class="modal-content surface-card border border-card p-6 rounded-2xl max-w-lg w-full flex flex-col gap-4">
        <div class="flex items-center justify-between pb-3 border-b border-slate-500/10">
          <h3 class="font-bold text-heading text-lg">
            {{ modalMode === 'create' ? 'Tạo mới Khung Chương trình Đào tạo' : 'Cập nhật Chương trình Đào tạo' }}
          </h3>
          <button @click="isCreateEditModalOpen = false" class="text-label hover:text-heading"><X :size="18" /></button>
        </div>

        <div class="flex flex-col gap-3">
          <div class="grid grid-cols-2 gap-3">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Mã CTĐT <span class="text-rose-500">*</span></label>
              <input v-model="programForm.maCodeChuongTrinh" type="text" class="glass-input w-full text-xs" placeholder="VD: CTDT_SE_2026" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Phiên bản <span class="text-rose-500">*</span></label>
              <input v-model="programForm.version" type="text" class="glass-input w-full text-xs" placeholder="VD: 1.0" />
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Tên Chương trình Đào tạo <span class="text-rose-500">*</span></label>
            <input v-model="programForm.tenChuongTrinh" type="text" class="glass-input w-full text-xs" placeholder="VD: Chương trình Đào tạo Công nghệ Thông tin 2026" />
          </div>

          <div class="grid grid-cols-3 gap-3">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Số Học kỳ chuẩn</label>
              <input v-model="programForm.soHocKy" type="number" min="1" max="12" class="glass-input w-full text-xs" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Thời gian (Tháng)</label>
              <input v-model="programForm.thoiGianDaoTaoThang" type="number" min="1" class="glass-input w-full text-xs" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Tổng TCI yêu cầu</label>
              <input v-model="programForm.tongTinChiYeuCau" type="number" min="1" class="glass-input w-full text-xs" />
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Mô tả / Ghi chú</label>
            <textarea v-model="programForm.moTa" class="glass-input w-full text-xs" rows="2" placeholder="Ghi chú về định hướng khung..."></textarea>
          </div>
        </div>

        <div class="flex gap-2 pt-2 border-t border-slate-500/10">
          <button @click="isCreateEditModalOpen = false" class="glass-btn secondary flex-1 text-xs justify-center">Hủy</button>
          <button @click="submitProgramForm" class="glass-btn primary flex-1 text-xs justify-center">Lưu thông tin</button>
        </div>
      </div>
    </div>

    <!-- Clone Modal -->
    <div v-if="isCloneModalOpen" class="modal-overlay" @click.self="isCloneModalOpen = false">
      <div class="modal-content surface-card border border-card p-6 rounded-2xl max-w-md w-full flex flex-col gap-4">
        <div class="flex items-center justify-between pb-3 border-b border-slate-500/10">
          <h3 class="font-bold text-heading text-base flex items-center gap-2">
            <Copy :size="18" class="text-indigo-600" /> Nhân bản (Clone) CTĐT
          </h3>
          <button @click="isCloneModalOpen = false" class="text-label hover:text-heading"><X :size="18" /></button>
        </div>

        <div class="flex flex-col gap-3">
          <div class="p-3 surface-input rounded-xl text-xs text-label">
            Sao chép từ: <strong class="text-heading">{{ cloneSourceProgram?.tenChuongTrinh || cloneSourceProgram?.TenChuongTrinh }}</strong> (v{{ cloneSourceProgram?.version || cloneSourceProgram?.Version }})
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Mã CTĐT Mới <span class="text-rose-500">*</span></label>
            <input v-model="cloneForm.maCodeChuongTrinh" type="text" class="glass-input w-full text-xs" />
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Tên CTĐT Mới <span class="text-rose-500">*</span></label>
            <input v-model="cloneForm.tenChuongTrinh" type="text" class="glass-input w-full text-xs" />
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Phiên bản mới</label>
            <input v-model="cloneForm.version" type="text" class="glass-input w-full text-xs" placeholder="VD: 2.0" />
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Ghi chú thay đổi (Audit Note)</label>
            <input v-model="cloneForm.ghiChuThayDoi" type="text" class="glass-input w-full text-xs" placeholder="Lý do tạo bản sao mới..." />
          </div>
        </div>

        <div class="flex gap-2 pt-2 border-t border-slate-500/10">
          <button @click="isCloneModalOpen = false" class="glass-btn secondary flex-1 text-xs justify-center">Hủy</button>
          <button @click="submitCloneForm" class="glass-btn primary flex-1 text-xs justify-center">Bắt đầu Clone</button>
        </div>
      </div>
    </div>

    <!-- Assign Modal -->
    <div v-if="isAssignModalOpen" class="modal-overlay" @click.self="isAssignModalOpen = false">
      <div class="modal-content surface-card border border-card p-6 rounded-2xl max-w-md w-full flex flex-col gap-4">
        <div class="flex items-center justify-between pb-3 border-b border-slate-500/10">
          <h3 class="font-bold text-heading text-base flex items-center gap-2">
            <Send :size="18" class="text-emerald-600" /> Áp dụng CTĐT cho Khóa & Cơ sở
          </h3>
          <button @click="isAssignModalOpen = false" class="text-label hover:text-heading"><X :size="18" /></button>
        </div>

        <div class="flex flex-col gap-3">
          <div class="p-3 surface-input rounded-xl text-xs text-label">
            Áp dụng CTĐT: <strong class="text-heading">{{ assignProgram?.tenChuongTrinh || assignProgram?.TenChuongTrinh }}</strong>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Ngày bắt đầu hiệu lực</label>
              <input v-model="assignForm.ngayHieuLuc" type="date" class="glass-input w-full text-xs" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Ngày hết hiệu lực</label>
              <input v-model="assignForm.ngayHetHieuLuc" type="date" class="glass-input w-full text-xs" />
            </div>
          </div>
        </div>

        <div class="flex gap-2 pt-2 border-t border-slate-500/10">
          <button @click="isAssignModalOpen = false" class="glass-btn secondary flex-1 text-xs justify-center">Hủy</button>
          <button @click="submitAssignForm" class="glass-btn primary flex-1 text-xs justify-center">Xác nhận Áp dụng</button>
        </div>
      </div>
    </div>

    <!-- Curriculum Editor Modal -->
    <CurriculumEditorModal 
      :is-open="isCurriculumModalOpen"
      :program="activeCurriculumProgram"
      @close="isCurriculumModalOpen = false"
      @updated="loadPrograms"
    />
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}
</style>
