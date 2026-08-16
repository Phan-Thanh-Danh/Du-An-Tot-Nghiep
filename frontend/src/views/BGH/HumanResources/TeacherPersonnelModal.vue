<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import {
  X, Save, User, Mail, Phone, Lock, Building, BookOpen, Award,
  AlertCircle, Search, Check, ChevronDown, Sparkles, AlertTriangle
} from 'lucide-vue-next'
import { bghPersonnelApi } from '@/services/bghPersonnelApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'

const props = defineProps({
  show: { type: Boolean, default: false },
  teacher: { type: Object, default: null },
  orgs: { type: Array, default: () => [] }
})

const emit = defineEmits(['close', 'saved'])

const isEdit = computed(() => !!props.teacher)
const saving = ref(false)
const errorMsg = ref('')
const loadingMetadata = ref(false)

const majorsList = ref([])
const subjectsList = ref([])
const subjectSearch = ref('')

// Custom dropdowns open state
const orgDropdownOpen = ref(false)
const majorDropdownOpen = ref(false)
const statusDropdownOpen = ref(false)

const majorSearch = ref('')

const form = ref({
  hoTen: '',
  email: '',
  soDienThoai: '',
  matKhau: '',
  maDonVi: '',
  maChuyenNganhChinh: '',
  danhSachMonDuocPhepDay: [],
  trangThai: 'hoat_dong',
  lyDo: ''
})

async function loadMetadata() {
  loadingMetadata.value = true
  try {
    const [majorsRes, subjectsRes] = await Promise.all([
      apiRequest('/api/master-data/specializations?pageSize=100'),
      apiRequest('/api/master-data/subjects?pageSize=200')
    ])
    
    const rawMajors = unwrapApiData(majorsRes)
    majorsList.value = rawMajors?.items || (Array.isArray(rawMajors) ? rawMajors : [])
    
    const rawSubjects = unwrapApiData(subjectsRes)
    subjectsList.value = rawSubjects?.items || (Array.isArray(rawSubjects) ? rawSubjects : [])
  } catch (err) {
    console.error('Lỗi khi tải danh mục chuyên ngành / môn học:', err)
  } finally {
    loadingMetadata.value = false
  }
}

watch(() => props.show, (val) => {
  if (val) {
    errorMsg.value = ''
    subjectSearch.value = ''
    majorSearch.value = ''
    orgDropdownOpen.value = false
    majorDropdownOpen.value = false
    statusDropdownOpen.value = false

    if (majorsList.value.length === 0 || subjectsList.value.length === 0) {
      loadMetadata()
    }
    if (props.teacher) {
      const teacherId = props.teacher.maNguoiDung || props.teacher.id
      const mainMajor = props.teacher.chuyenNganhList?.find(c => c.laChuyenMonChinh) || props.teacher.chuyenNganhList?.[0]
      form.value = {
        hoTen: props.teacher.hoTen || '',
        email: props.teacher.email || '',
        soDienThoai: props.teacher.soDienThoai || '',
        matKhau: '',
        maDonVi: props.teacher.maDonVi || props.orgs[0]?.maDonVi || '',
        maChuyenNganhChinh: mainMajor ? (mainMajor.maChuyenNganh || mainMajor.id) : '',
        danhSachMonDuocPhepDay: (props.teacher.monHocList || []).map(m => m.maMonHoc || m.id),
        trangThai: props.teacher.trangThai || 'hoat_dong',
        lyDo: ''
      }

      // Always fetch latest teacher detail to ensure all assigned & capability subjects are fully loaded
      if (teacherId) {
        bghPersonnelApi.getTeacherDetail(teacherId).then(res => {
          const detail = res?.data || res
          if (detail) {
            const mm = detail.chuyenNganhList?.find(c => c.laChuyenMonChinh) || detail.chuyenNganhList?.[0]
            if (mm && !form.value.maChuyenNganhChinh) {
              form.value.maChuyenNganhChinh = mm.maChuyenNganh || mm.id
            }
            if (detail.monHocList && detail.monHocList.length > 0) {
              form.value.danhSachMonDuocPhepDay = detail.monHocList.map(m => m.maMonHoc || m.id)
            }
            if (detail.hoTen) form.value.hoTen = detail.hoTen
            if (detail.soDienThoai) form.value.soDienThoai = detail.soDienThoai
            if (detail.trangThai) form.value.trangThai = detail.trangThai
            if (detail.maDonVi) form.value.maDonVi = detail.maDonVi
          }
        }).catch(err => {
          console.error('Failed to load full teacher detail for modal:', err)
        })
      }
    } else {
      form.value = {
        hoTen: '',
        email: '',
        soDienThoai: '',
        matKhau: '123456',
        maDonVi: props.orgs[0]?.maDonVi || '',
        maChuyenNganhChinh: '',
        danhSachMonDuocPhepDay: [],
        trangThai: 'hoat_dong',
        lyDo: ''
      }
    }
  }
})

// Current selected objects
const selectedOrgObj = computed(() => props.orgs.find(o => o.maDonVi === form.value.maDonVi))
const selectedMajorObj = computed(() => majorsList.value.find(m => (m.maChuyenNganh || m.id) === form.value.maChuyenNganhChinh))

const statusOptions = [
  { value: 'hoat_dong', label: 'Đang hoạt động (Active)', color: 'emerald' },
  { value: 'bi_khoa', label: 'Tạm khóa tài khoản (Locked)', color: 'rose' },
  { value: 'tam_nghi', label: 'Nghỉ phép / Tạm hoãn công tác', color: 'amber' }
]
const selectedStatusObj = computed(() => statusOptions.find(s => s.value === form.value.trangThai) || statusOptions[0])

// Filtered majors by search
const filteredMajors = computed(() => {
  if (!majorSearch.value.trim()) return majorsList.value
  const q = majorSearch.value.toLowerCase().trim()
  return majorsList.value.filter(m =>
    (m.tenChuyenNganh && m.tenChuyenNganh.toLowerCase().includes(q)) ||
    (m.maChuyenNganhCode && m.maChuyenNganhCode.toLowerCase().includes(q))
  )
})

// Calculate live suitability of any subject against teacher's selected major
function getSubjectSuitability(subject) {
  if (!form.value.maChuyenNganhChinh || !selectedMajorObj.value) {
    return { score: 70, label: 'Chưa gán ngành', type: 'neutral' }
  }

  const teacherMajorId = form.value.maChuyenNganhChinh
  const teacherMajorNganh = selectedMajorObj.value.maNganh

  // 1. Cùng chuyên ngành trực tiếp
  if (subject.maChuyenNganh && subject.maChuyenNganh === teacherMajorId) {
    return { score: 95, label: 'Cùng chuyên ngành (95%)', type: 'match' }
  }

  // 2. Cùng ngành đào tạo lớn
  if (subject.maNganh && teacherMajorNganh && subject.maNganh === teacherMajorNganh) {
    return { score: 80, label: 'Cùng khối ngành (80%)', type: 'related' }
  }

  // 3. Môn cơ sở chung
  if (!subject.maNganh && !subject.maChuyenNganh) {
    return { score: 70, label: 'Môn đại cương (70%)', type: 'general' }
  }

  // 4. Trái ngành
  return { score: 35, label: 'Trái ngành đào tạo (35%)', type: 'unrelated' }
}

// Filtered subjects
const filteredSubjects = computed(() => {
  if (!subjectSearch.value.trim()) return subjectsList.value
  const q = subjectSearch.value.toLowerCase().trim()
  return subjectsList.value.filter(s =>
    (s.tenMonHoc && s.tenMonHoc.toLowerCase().includes(q)) ||
    (s.maCodeMonHoc && s.maCodeMonHoc.toLowerCase().includes(q))
  )
})

function toggleSubject(subId) {
  const idx = form.value.danhSachMonDuocPhepDay.indexOf(subId)
  if (idx > -1) {
    form.value.danhSachMonDuocPhepDay.splice(idx, 1)
  } else {
    form.value.danhSachMonDuocPhepDay.push(subId)
  }
}

function selectAllFilteredSubjects() {
  filteredSubjects.value.forEach(s => {
    const id = s.maMonHoc || s.id
    if (!form.value.danhSachMonDuocPhepDay.includes(id)) {
      form.value.danhSachMonDuocPhepDay.push(id)
    }
  })
}

function clearAllSubjects() {
  form.value.danhSachMonDuocPhepDay = []
}

// Close all custom dropdowns when clicking outside
function closeAllDropdowns(e) {
  if (!e.target.closest('.custom-dropdown-container')) {
    orgDropdownOpen.value = false
    majorDropdownOpen.value = false
    statusDropdownOpen.value = false
  }
}

onMounted(() => {
  loadMetadata()
  document.addEventListener('click', closeAllDropdowns)
})

onUnmounted(() => {
  document.removeEventListener('click', closeAllDropdowns)
})

async function handleSubmit() {
  if (!form.value.hoTen.trim()) {
    errorMsg.value = 'Vui lòng nhập họ và tên giảng viên.'
    return
  }
  if (!isEdit.value && !form.value.email.trim()) {
    errorMsg.value = 'Vui lòng nhập email đăng nhập.'
    return
  }
  if (!isEdit.value && !form.value.matKhau) {
    errorMsg.value = 'Vui lòng nhập mật khẩu khởi tạo.'
    return
  }
  if (!form.value.maDonVi) {
    errorMsg.value = 'Vui lòng chọn cơ sở trực thuộc.'
    return
  }

  saving.value = true
  errorMsg.value = ''
  try {
    if (isEdit.value) {
      const payload = {
        hoTen: form.value.hoTen.trim(),
        soDienThoai: form.value.soDienThoai.trim(),
        trangThai: form.value.trangThai,
        maChuyenNganhChinh: form.value.maChuyenNganhChinh ? parseInt(form.value.maChuyenNganhChinh, 10) : null,
        danhSachMonHoc: form.value.danhSachMonDuocPhepDay.map(subId => {
          const subObj = subjectsList.value.find(s => (s.maMonHoc || s.id) === subId)
          const auto = subObj ? getSubjectSuitability(subObj) : { score: 85 }
          const old = props.teacher.monHocList?.find(m => m.maMonHoc === subId)
          return {
            maMonHoc: subId,
            mucDoPhuHop: auto.score,
            soNamKinhNghiem: old ? old.soNamKinhNghiem : (auto.score >= 80 ? 2 : 1),
            laMonChinh: auto.score === 100,
            conHoatDong: true
          }
        }),
        lyDo: form.value.lyDo.trim() || 'BGH cập nhật hồ sơ'
      }
      await bghPersonnelApi.updateTeacher(props.teacher.maNguoiDung || props.teacher.id, payload)
    } else {
      const payload = {
        hoTen: form.value.hoTen.trim(),
        email: form.value.email.trim(),
        soDienThoai: form.value.soDienThoai.trim(),
        matKhau: form.value.matKhau,
        maDonVi: parseInt(form.value.maDonVi, 10),
        maChuyenNganhChinh: form.value.maChuyenNganhChinh ? parseInt(form.value.maChuyenNganhChinh, 10) : null,
        danhSachMonDuocPhepDay: form.value.danhSachMonDuocPhepDay
      }
      await bghPersonnelApi.createTeacher(payload)
    }
    emit('saved')
    emit('close')
  } catch (err) {
    errorMsg.value = err?.response?.data?.message || err?.message || 'Lỗi khi lưu thông tin giảng viên.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
    <div class="surface-card border border-card w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh]">
      <!-- Header -->
      <div class="px-6 py-4 border-b border-card flex items-center justify-between bg-(--surface-input)/40">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-600 dark:text-blue-400 flex items-center justify-center font-bold">
            <User :size="20" />
          </div>
          <div>
            <h3 class="text-base font-bold text-heading">
              {{ isEdit ? 'Cập Nhật Hồ Sơ Giảng Viên' : 'Thêm Mới Giảng Viên Cơ Sở' }}
            </h3>
            <p class="text-xs text-muted">
              {{ isEdit ? `Mã GV: GV${props.teacher?.maNguoiDung?.toString().padStart(4, '0')}` : 'Khởi tạo tài khoản và phân công chuyên môn' }}
            </p>
          </div>
        </div>
        <button
          @click="$emit('close')"
          class="w-8 h-8 rounded-lg flex items-center justify-center text-muted hover:text-heading hover:bg-(--surface-input) transition-colors cursor-pointer"
        >
          <X :size="18" />
        </button>
      </div>

      <!-- Body -->
      <div class="p-6 overflow-y-auto space-y-4 flex-1">
        <div v-if="errorMsg" class="p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-600 dark:text-rose-400 text-xs flex items-center gap-2">
          <AlertCircle :size="16" class="shrink-0" />
          <span>{{ errorMsg }}</span>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Họ tên -->
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Họ và tên <span class="text-rose-500">*</span></label>
            <input
              v-model="form.hoTen"
              type="text"
              placeholder="Nguyễn Văn A"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
            />
          </div>

          <!-- Email -->
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Email đăng nhập <span class="text-rose-500">*</span></label>
            <input
              v-model="form.email"
              :disabled="isEdit"
              type="email"
              placeholder="gv@edulms.local"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 disabled:opacity-60 disabled:cursor-not-allowed transition-all"
            />
          </div>

          <!-- Số điện thoại -->
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Số điện thoại</label>
            <input
              v-model="form.soDienThoai"
              type="text"
              placeholder="0987654321"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
            />
          </div>

          <!-- Mật khẩu khởi tạo (Chỉ khi thêm mới) -->
          <div v-if="!isEdit">
            <label class="block text-xs font-bold text-heading mb-1.5">Mật khẩu khởi tạo <span class="text-rose-500">*</span></label>
            <input
              v-model="form.matKhau"
              type="text"
              placeholder="123456"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
            />
          </div>

          <!-- CUSTOM SELECT: Cơ sở trực thuộc -->
          <div class="custom-dropdown-container relative">
            <label class="block text-xs font-bold text-heading mb-1.5">Cơ sở trực thuộc <span class="text-rose-500">*</span></label>
            <div
              @click="!isEdit && (orgDropdownOpen = !orgDropdownOpen, majorDropdownOpen = false, statusDropdownOpen = false)"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body flex items-center justify-between transition-all select-none"
              :class="[
                isEdit ? 'opacity-60 cursor-not-allowed' : 'cursor-pointer hover:border-blue-500',
                orgDropdownOpen ? 'border-blue-500 ring-2 ring-blue-500/20' : ''
              ]"
            >
              <span class="truncate font-medium text-heading">
                {{ selectedOrgObj ? selectedOrgObj.tenDonVi : '-- Chọn cơ sở --' }}
              </span>
              <ChevronDown :size="15" class="text-muted shrink-0 transition-transform" :class="orgDropdownOpen ? 'rotate-180 text-blue-500' : ''" />
            </div>

            <!-- Dropdown Menu -->
            <div
              v-if="orgDropdownOpen && !isEdit"
              class="absolute z-50 left-0 right-0 mt-1 surface-card border border-card rounded-xl shadow-xl max-h-48 overflow-y-auto p-1 animate-fade-in"
            >
              <div
                v-for="org in orgs"
                :key="org.maDonVi"
                @click="form.maDonVi = org.maDonVi; orgDropdownOpen = false"
                class="px-3 py-2 rounded-lg text-xs hover:bg-(--surface-input) flex items-center justify-between cursor-pointer transition-colors"
                :class="form.maDonVi === org.maDonVi ? 'bg-blue-500/10 text-blue-600 font-bold' : 'text-body'"
              >
                <span>{{ org.tenDonVi }}</span>
                <Check v-if="form.maDonVi === org.maDonVi" :size="14" class="text-blue-600" />
              </div>
            </div>
          </div>

          <!-- CUSTOM SELECT: Chuyên ngành chính -->
          <div class="custom-dropdown-container relative">
            <label class="block text-xs font-bold text-heading mb-1.5 flex items-center justify-between">
              <span>Chuyên ngành chính</span>
              <span v-if="loadingMetadata" class="text-[10px] text-blue-500">Đang tải...</span>
            </label>
            <div
              @click="majorDropdownOpen = !majorDropdownOpen; orgDropdownOpen = false; statusDropdownOpen = false"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body flex items-center justify-between cursor-pointer hover:border-blue-500 transition-all select-none"
              :class="majorDropdownOpen ? 'border-blue-500 ring-2 ring-blue-500/20' : ''"
            >
              <span class="truncate font-medium" :class="selectedMajorObj ? 'text-heading font-bold' : 'text-muted'">
                {{ selectedMajorObj ? selectedMajorObj.tenChuyenNganh : '-- Chưa gán chuyên ngành --' }}
              </span>
              <ChevronDown :size="15" class="text-muted shrink-0 transition-transform" :class="majorDropdownOpen ? 'rotate-180 text-blue-500' : ''" />
            </div>

            <!-- Dropdown Menu with Search -->
            <div
              v-if="majorDropdownOpen"
              class="absolute z-50 left-0 right-0 mt-1 surface-card border border-card rounded-xl shadow-xl max-h-60 overflow-y-auto p-1.5 animate-fade-in space-y-1"
            >
              <div class="p-1">
                <input
                  v-model="majorSearch"
                  type="text"
                  placeholder="Tìm chuyên ngành..."
                  class="w-full px-2.5 py-1.5 bg-(--surface-input) border border-input rounded-lg text-xs text-body focus:outline-none focus:border-blue-500"
                  @click.stop
                />
              </div>
              <div
                @click="form.maChuyenNganhChinh = ''; majorDropdownOpen = false"
                class="px-3 py-2 rounded-lg text-xs hover:bg-(--surface-input) flex items-center justify-between cursor-pointer text-muted"
              >
                <span>-- Chưa gán chuyên ngành --</span>
              </div>
              <div
                v-for="m in filteredMajors"
                :key="m.maChuyenNganh || m.id"
                @click="form.maChuyenNganhChinh = m.maChuyenNganh || m.id; majorDropdownOpen = false"
                class="px-3 py-2 rounded-lg text-xs hover:bg-(--surface-input) flex items-center justify-between cursor-pointer transition-colors"
                :class="form.maChuyenNganhChinh === (m.maChuyenNganh || m.id) ? 'bg-blue-500/10 text-blue-600 font-bold' : 'text-body'"
              >
                <span>{{ m.tenChuyenNganh }}</span>
                <Check v-if="form.maChuyenNganhChinh === (m.maChuyenNganh || m.id)" :size="14" class="text-blue-600" />
              </div>
            </div>
          </div>

          <!-- CUSTOM SELECT: Trạng thái tài khoản -->
          <div v-if="isEdit" class="custom-dropdown-container relative md:col-span-2">
            <label class="block text-xs font-bold text-heading mb-1.5">Trạng thái tài khoản</label>
            <div
              @click="statusDropdownOpen = !statusDropdownOpen; orgDropdownOpen = false; majorDropdownOpen = false"
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body flex items-center justify-between cursor-pointer hover:border-blue-500 transition-all select-none"
              :class="statusDropdownOpen ? 'border-blue-500 ring-2 ring-blue-500/20' : ''"
            >
              <div class="flex items-center gap-2">
                <span class="w-2 h-2 rounded-full" :class="selectedStatusObj.value === 'hoat_dong' ? 'bg-emerald-500' : 'bg-rose-500'"></span>
                <span class="font-bold text-heading">{{ selectedStatusObj.label }}</span>
              </div>
              <ChevronDown :size="15" class="text-muted shrink-0 transition-transform" :class="statusDropdownOpen ? 'rotate-180 text-blue-500' : ''" />
            </div>

            <!-- Dropdown Menu -->
            <div
              v-if="statusDropdownOpen"
              class="absolute z-50 left-0 right-0 mt-1 surface-card border border-card rounded-xl shadow-xl p-1 animate-fade-in"
            >
              <div
                v-for="st in statusOptions"
                :key="st.value"
                @click="form.trangThai = st.value; statusDropdownOpen = false"
                class="px-3 py-2 rounded-lg text-xs hover:bg-(--surface-input) flex items-center justify-between cursor-pointer transition-colors"
                :class="form.trangThai === st.value ? 'bg-blue-500/10 text-blue-600 font-bold' : 'text-body'"
              >
                <span>{{ st.label }}</span>
                <Check v-if="form.trangThai === st.value" :size="14" class="text-blue-600" />
              </div>
            </div>
          </div>

          <!-- Lý do thay đổi hồ sơ (Đã loại bỏ chữ Ghi log Audit) -->
          <div v-if="isEdit" class="md:col-span-2">
            <label class="block text-xs font-bold text-heading mb-1.5">Lý do thay đổi hồ sơ</label>
            <input
              v-model="form.lyDo"
              type="text"
              placeholder="Ví dụ: Bổ sung môn giảng dạy mới, cập nhật chuyên môn theo quyết định..."
              class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
            />
          </div>
        </div>

        <!-- Danh sách môn học được phép giảng dạy với live Suitability calculation -->
        <div class="pt-3 border-t border-card space-y-2.5">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
            <div>
              <label class="block text-xs font-bold text-heading">
                Danh sách môn được phép giảng dạy:
              </label>
              <span class="text-[11px] text-muted">
                Đã chọn: <strong class="text-blue-600 dark:text-blue-400 font-bold">{{ form.danhSachMonDuocPhepDay.length }}</strong> môn
              </span>
            </div>

            <!-- Search inside subject picker -->
            <div class="flex items-center gap-2">
              <div class="relative w-full sm:w-56">
                <Search :size="14" class="absolute left-2.5 top-1/2 -translate-y-1/2 text-muted" />
                <input
                  v-model="subjectSearch"
                  type="text"
                  placeholder="Tìm môn học..."
                  class="w-full pl-8 pr-2.5 py-1.5 bg-(--surface-input) border border-input rounded-lg text-xs text-body focus:outline-none focus:border-blue-500"
                />
              </div>
              <button
                type="button"
                @click="selectAllFilteredSubjects"
                class="px-2 py-1 text-[11px] font-bold text-blue-600 hover:bg-blue-500/10 rounded-lg whitespace-nowrap cursor-pointer"
              >
                Chọn hết
              </button>
              <button
                type="button"
                @click="clearAllSubjects"
                class="px-2 py-1 text-[11px] font-bold text-muted hover:text-rose-500 rounded-lg whitespace-nowrap cursor-pointer"
              >
                Bỏ chọn
              </button>
            </div>
          </div>

          <!-- Subject list cards -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-56 overflow-y-auto p-2.5 bg-(--surface-input)/30 rounded-xl border border-input">
            <div
              v-if="loadingMetadata"
              class="col-span-2 py-8 text-center text-muted text-xs flex items-center justify-center gap-2"
            >
              <div class="animate-spin w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full"></div>
              <span>Đang tải danh mục môn học...</span>
            </div>

            <div
              v-else-if="filteredSubjects.length === 0"
              class="col-span-2 py-8 text-center text-muted text-xs"
            >
              Không tìm thấy môn học nào khớp với từ khóa "{{ subjectSearch }}".
            </div>

            <div
              v-else
              v-for="sub in filteredSubjects"
              :key="sub.maMonHoc || sub.id"
              @click="toggleSubject(sub.maMonHoc || sub.id)"
              class="p-2.5 rounded-xl border transition-all cursor-pointer flex items-center justify-between select-none"
              :class="form.danhSachMonDuocPhepDay.includes(sub.maMonHoc || sub.id) ? 'bg-blue-500/10 border-blue-500/30 text-heading shadow-xs' : 'bg-(--surface-input)/40 border-card hover:bg-(--surface-input) text-body'"
            >
              <div class="flex items-center gap-2.5 min-w-0 pr-2">
                <div
                  class="w-4 h-4 rounded flex items-center justify-center shrink-0 transition-colors"
                  :class="form.danhSachMonDuocPhepDay.includes(sub.maMonHoc || sub.id) ? 'bg-blue-600 text-white' : 'border border-input bg-(--surface-input)'"
                >
                  <Check v-if="form.danhSachMonDuocPhepDay.includes(sub.maMonHoc || sub.id)" :size="12" class="stroke-[3]" />
                </div>
                <div class="truncate">
                  <div class="text-xs font-bold text-heading truncate">{{ sub.tenMonHoc || sub.name }}</div>
                  <div class="text-[10px] text-muted font-mono">{{ sub.maCodeMonHoc || sub.code || 'Mã môn' }} · {{ sub.soTinChi || 3 }} TC</div>
                </div>
              </div>

              <!-- Real Dynamic Suitability Badge -->
              <div class="shrink-0 flex items-center gap-1">
                <span
                  v-if="getSubjectSuitability(sub).score >= 90"
                  class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                  title="Cùng chuyên ngành chính"
                >
                  {{ getSubjectSuitability(sub).score }}%
                </span>
                <span
                  v-else-if="getSubjectSuitability(sub).score >= 80"
                  class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-blue-500/10 text-blue-600 dark:text-blue-400"
                  title="Cùng khối ngành"
                >
                  {{ getSubjectSuitability(sub).score }}%
                </span>
                <span
                  v-else-if="getSubjectSuitability(sub).score >= 70"
                  class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-500/10 text-slate-600 dark:text-slate-400"
                  title="Môn đại cương"
                >
                  {{ getSubjectSuitability(sub).score }}%
                </span>
                <span
                  v-else
                  class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-500/10 text-amber-600 dark:text-amber-400 flex items-center gap-0.5"
                  title="Trái ngành đào tạo"
                >
                  <AlertTriangle :size="10" />
                  <span>{{ getSubjectSuitability(sub).score }}%</span>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="px-6 py-4 border-t border-card bg-(--surface-input)/30 flex items-center justify-end gap-3">
        <button
          @click="$emit('close')"
          type="button"
          class="px-4 py-2 text-xs font-bold text-body hover:bg-(--surface-input) rounded-xl transition-colors cursor-pointer"
        >
          Hủy bỏ
        </button>
        <button
          @click="handleSubmit"
          :disabled="saving"
          type="button"
          class="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold shadow-md shadow-blue-500/20 transition-all disabled:opacity-50 cursor-pointer"
        >
          <Save :size="16" />
          <span>{{ saving ? 'Đang lưu...' : (isEdit ? 'Lưu thay đổi' : 'Tạo giảng viên') }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
