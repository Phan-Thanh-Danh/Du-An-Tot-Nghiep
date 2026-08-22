<script setup>
import { ref, computed, watch, nextTick, useId } from 'vue'
import { X, Search, Loader2 } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { rewardDisciplineApi } from '@/services/rewardDisciplineApi'
import { unwrapApiData } from '@/services/apiClient'
import { usePopupStore } from '@/stores/popup'
import { adminUserApi } from '@/services/adminUserService'

const props = defineProps({
  modelValue: Boolean,
})
const emit = defineEmits(['update:modelValue', 'created'])

const popupStore = usePopupStore()
const dialogTitleId = `create-discipline-dialog-${useId()}`
const loading = ref(false)

const DISCIPLINE_LEVEL_OPTIONS = [
  { value: 'nhe', label: 'Nhẹ' },
  { value: 'trung_binh', label: 'Trung bình' },
  { value: 'nghiem_trong', label: 'Nghiêm trọng' },
]

const DISCIPLINE_ACTION_OPTIONS = [
  { value: 'nhac_nho', label: 'Nhắc nhở' },
  { value: 'khien_trach', label: 'Khiển trách' },
  { value: 'canh_cao', label: 'Cảnh cáo' },
  { value: 'dinh_chi', label: 'Đình chỉ' },
  { value: 'khac', label: 'Khác' },
]

// Form State
const formData = ref({
  maHocSinh: '',
  tieuDe: '',
  moTaViPham: '',
  ngayViPham: new Date().toISOString().split('T')[0],
  mucDoKyLuat: 'nhe',
  hinhThucXuLy: 'nhac_nho',
  canCuXuLy: '',
  ghiChuNoiBo: ''
})

// Student Search State
const searchStudentQuery = ref('')
const searchingStudent = ref(false)
const studentResults = ref([])
const selectedStudent = ref(null)
const showStudentDropdown = ref(false)

let searchTimeout = null
const handleSearchStudent = () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  if (!searchStudentQuery.value.trim()) {
    studentResults.value = []
    showStudentDropdown.value = false
    return
  }
  
  searchTimeout = setTimeout(async () => {
    searchingStudent.value = true
    showStudentDropdown.value = true
    try {
      // Giả sử getUsers hỗ trợ search theo từ khóa và chỉ định role/loại sinh viên
      const res = await adminUserApi.getUsers({
        search: searchStudentQuery.value,
        role: 'Student', // Backend yêu cầu gửi roleCode hoặc type là sinh viên
        pageIndex: 1,
        pageSize: 10
      })
      const data = unwrapApiData(res)
      studentResults.value = data?.items ?? data?.Items ?? []
    } catch (error) {
      console.error('Lỗi tìm sinh viên:', error)
      studentResults.value = []
    } finally {
      searchingStudent.value = false
    }
  }, 500)
}

const selectStudent = (student) => {
  selectedStudent.value = student
  formData.value.maHocSinh = student.maNguoiDung ?? student.MaNguoiDung
  searchStudentQuery.value = `${student.hoTen || student.HoTen} (${student.mssv || student.Mssv || student.email || student.Email})`
  showStudentDropdown.value = false
}

const close = () => {
  emit('update:modelValue', false)
  resetForm()
}

const resetForm = () => {
  formData.value = {
    maHocSinh: '',
    tieuDe: '',
    moTaViPham: '',
    ngayViPham: new Date().toISOString().split('T')[0],
    mucDoKyLuat: 'nhe',
    hinhThucXuLy: 'nhac_nho',
    canCuXuLy: '',
    ghiChuNoiBo: ''
  }
  searchStudentQuery.value = ''
  selectedStudent.value = null
  studentResults.value = []
  showStudentDropdown.value = false
}

const submitForm = async () => {
  if (!formData.value.maHocSinh) return popupStore.error('Lỗi', 'Vui lòng chọn học sinh vi phạm.')
  if (formData.value.tieuDe.trim().length < 5) return popupStore.error('Lỗi', 'Tiêu đề phải dài tối thiểu 5 ký tự.')
  if (formData.value.moTaViPham.trim().length < 10) return popupStore.error('Lỗi', 'Mô tả vi phạm phải dài tối thiểu 10 ký tự.')
  if (!formData.value.ngayViPham) return popupStore.error('Lỗi', 'Ngày vi phạm không được để trống.')

  loading.value = true
  try {
    await rewardDisciplineApi.createDisciplineRecord({
      ...formData.value,
      maHocSinh: Number(formData.value.maHocSinh)
    })
    popupStore.success('Thành công', 'Hồ sơ kỷ luật đã được tạo thành công.')
    emit('created')
    close()
  } catch (err) {
    let msg = err?.message || 'Không thể tạo hồ sơ kỷ luật.'
    if (err?.details?.errors) {
      const fieldErrors = Object.values(err.details.errors).flat()
      if (fieldErrors.length > 0) {
        msg = fieldErrors.join(' ')
      }
    }
    popupStore.error('Tạo hồ sơ thất bại', msg)
  } finally {
    loading.value = false
  }
}

// Click outside student dropdown
const handleOutsideClick = (e) => {
  if (!e.target.closest('.student-search-container')) {
    showStudentDropdown.value = false
    if (!selectedStudent.value) searchStudentQuery.value = ''
  }
}

watch(() => props.modelValue, (val) => {
  if (val) {
    document.addEventListener('click', handleOutsideClick)
  } else {
    document.removeEventListener('click', handleOutsideClick)
  }
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="modelValue"
      class="confirm-dialog fixed inset-0 z-(--z-modal) flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-labelledby="dialogTitleId"
      @keydown.esc="close"
    >
      <div
        class="confirm-dialog-scrim absolute inset-0 bg-slate-900/50 backdrop-blur-sm"
        aria-hidden="true"
        @click="close"
      />

      <GlassPanel
        variant="readable"
        density="comfortable"
        :clip="false"
        class="relative w-full max-w-2xl max-h-[90vh] flex flex-col shadow-xl"
      >
        <div class="flex items-center justify-between p-4 border-b border-(--border-default) shrink-0">
          <h2 :id="dialogTitleId" class="text-lg font-semibold text-(--text-heading)">
            Lập hồ sơ kỷ luật mới
          </h2>
          <button
            type="button"
            class="flex h-8 w-8 items-center justify-center rounded-md hover:bg-(--surface-hover) text-(--text-muted) transition-colors"
            :disabled="loading"
            @click="close"
          >
            <X :size="20" />
          </button>
        </div>

        <div class="p-5 overflow-y-auto flex-1 space-y-4">
          <!-- Student Search -->
          <div class="student-search-container relative">
            <label class="block text-sm font-medium text-(--text-heading) mb-1">Sinh viên vi phạm <span class="text-red-500">*</span></label>
            <div class="relative">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" />
              <input 
                v-model="searchStudentQuery" 
                @input="handleSearchStudent"
                @focus="handleSearchStudent"
                type="text" 
                class="w-full pl-9 pr-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow"
                placeholder="Tìm kiếm theo tên, mã sinh viên hoặc email..."
              />
              <Loader2 v-if="searchingStudent" :size="16" class="absolute right-3 top-1/2 -translate-y-1/2 text-(--text-muted) animate-spin" />
            </div>

            <div v-if="showStudentDropdown && studentResults.length > 0" class="absolute z-10 mt-1 w-full bg-(--surface-card) border border-(--border-default) rounded-lg shadow-lg max-h-[200px] overflow-y-auto">
              <div 
                v-for="st in studentResults" :key="st.maNguoiDung ?? st.MaNguoiDung"
                class="px-3 py-2 hover:bg-(--surface-hover) cursor-pointer border-b border-(--border-default) last:border-0"
                @click="selectStudent(st)"
              >
                <div class="font-medium text-sm text-(--text-heading)">{{ st.hoTen ?? st.HoTen }}</div>
                <div class="text-xs text-(--text-muted)">{{ st.mssv || st.Mssv || st.email || st.Email }} • {{ st.tenDonVi || st.TenDonVi || 'Chưa rõ đơn vị' }}</div>
              </div>
            </div>
            <div v-else-if="showStudentDropdown && searchStudentQuery.trim() && !searchingStudent" class="absolute z-10 mt-1 w-full bg-(--surface-card) border border-(--border-default) rounded-lg shadow-lg p-3 text-sm text-center text-(--text-muted)">
              Không tìm thấy sinh viên nào.
            </div>
          </div>

          <!-- Basic Info -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="md:col-span-2">
              <label class="block text-sm font-medium text-(--text-heading) mb-1">Tiêu đề hồ sơ <span class="text-red-500">*</span></label>
              <input v-model="formData.tieuDe" type="text" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none" placeholder="Ví dụ: Vi phạm quy chế thi..." />
            </div>

            <div>
              <label class="block text-sm font-medium text-(--text-heading) mb-1">Ngày vi phạm <span class="text-red-500">*</span></label>
              <input v-model="formData.ngayViPham" type="date" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none" />
            </div>

            <div>
              <label class="block text-sm font-medium text-(--text-heading) mb-1">Mức độ kỷ luật <span class="text-red-500">*</span></label>
              <select v-model="formData.mucDoKyLuat" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none">
                <option v-for="opt in DISCIPLINE_LEVEL_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-(--text-heading) mb-1">Hình thức dự kiến <span class="text-red-500">*</span></label>
              <select v-model="formData.hinhThucXuLy" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none">
                <option v-for="opt in DISCIPLINE_ACTION_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
              </select>
            </div>
            
            <div class="md:col-span-2">
              <label class="block text-sm font-medium text-(--text-heading) mb-1">Căn cứ xử lý</label>
              <input v-model="formData.canCuXuLy" type="text" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none" placeholder="Ví dụ: Quy chế đào tạo số 123/QĐ..." />
            </div>
          </div>

          <!-- Description -->
          <div>
            <label class="block text-sm font-medium text-(--text-heading) mb-1">Mô tả vi phạm chi tiết <span class="text-red-500">*</span></label>
            <textarea v-model="formData.moTaViPham" rows="4" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none resize-none" placeholder="Mô tả cụ thể hành vi vi phạm của sinh viên..."></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-(--text-heading) mb-1">Ghi chú nội bộ</label>
            <textarea v-model="formData.ghiChuNoiBo" rows="2" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none resize-none" placeholder="Ghi chú dành riêng cho quản trị viên, BGH..."></textarea>
          </div>
        </div>

        <div class="p-4 border-t border-(--border-default) shrink-0 flex justify-end gap-3 bg-(--surface-modal)">
          <GlassButton variant="secondary" :disabled="loading" @click="close">
            Hủy bỏ
          </GlassButton>
          <GlassButton variant="primary" :loading="loading" @click="submitForm">
            Tạo hồ sơ
          </GlassButton>
        </div>
      </GlassPanel>
    </div>
  </Teleport>
</template>

<style scoped>
.confirm-dialog {
  color: var(--text-body);
}
</style>
