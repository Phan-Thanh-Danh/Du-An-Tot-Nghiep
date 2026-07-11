<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { courseApi } from '@/services/courseApi'
import { X, Loader2, Search, Check, ChevronDown, ChevronUp, BookOpen, Users, GraduationCap, Calendar, Eye } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import TeacherAssignmentSelect from './TeacherAssignmentSelect.vue'

const emit = defineEmits(['close', 'done'])
const props = defineProps({
  academicTerms: { type: Array, default: () => [] },
  subjects: { type: Array, default: () => [] },
  teachers: { type: Array, default: () => [] },
  classes: { type: Array, default: () => [] },
  majors: { type: Array, default: () => [] },
  specializations: { type: Array, default: () => [] },
})

const popupStore = usePopupStore()

const form = ref({
  maNganh: null,
  maChuyenNganh: null,
  maHocKy: null,
  maMonHoc: null,
  maGiaoVien: null,
  tieuDe: '',
  moTa: '',
  trangThai: 'nhap',
})

const selectedClassIds = ref([])
const classSearch = ref('')
const submitting = ref(false)
const showAdvanced = ref(false)

onMounted(() => {
  if (props.academicTerms.length > 0) {
    form.value.maHocKy = props.academicTerms[0].value
  }
})

const filteredClasses = computed(() => {
  const kw = classSearch.value.toLowerCase()
  return props.classes.filter(c => c.label.toLowerCase().includes(kw))
})

const isMaxClasses = computed(() => selectedClassIds.value.length >= 5)
const selectedCount = computed(() => selectedClassIds.value.length)

const remainingSlots = computed(() => 5 - selectedCount.value)

function toggleClass(classId) {
  const idx = selectedClassIds.value.indexOf(classId)
  if (idx >= 0) {
    selectedClassIds.value.splice(idx, 1)
  } else {
    if (selectedClassIds.value.length >= 5) return
    selectedClassIds.value.push(classId)
  }
}

function removeClass(classId) {
  const idx = selectedClassIds.value.indexOf(classId)
  if (idx >= 0) selectedClassIds.value.splice(idx, 1)
}

function getClassLabel(classId) {
  return props.classes.find(c => c.value === classId)?.label || `#${classId}`
}

const selectedSubject = computed(() => {
  if (!form.value.maMonHoc) return null
  return props.subjects.find(s => s.value === form.value.maMonHoc)
})

const selectedTeacher = computed(() => {
  if (!form.value.maGiaoVien) return null
  return props.teachers.find(t => t.value === form.value.maGiaoVien)
})

const selectedTerm = computed(() => {
  if (!form.value.maHocKy) return null
  return props.academicTerms.find(t => t.value === form.value.maHocKy)
})

const previewTitle = computed(() => {
  if (form.value.tieuDe) return form.value.tieuDe
  const mon = selectedSubject.value?.label?.split('(')[0]?.trim()
  if (!mon || selectedCount.value === 0) return null
  if (selectedCount.value === 1) {
    const lop = getClassLabel(selectedClassIds.value[0])
    return `${mon} - ${lop}`
  }
  return `${mon} - ${selectedCount.value} lớp`
})

const isFormValid = computed(() => {
  return form.value.maMonHoc && form.value.maGiaoVien && selectedCount.value > 0
})

const missingFields = computed(() => {
  const missing = []
  if (!form.value.maMonHoc) missing.push('môn học')
  if (!form.value.maGiaoVien) missing.push('giảng viên')
  if (selectedCount.value === 0) missing.push('lớp (ít nhất 1)')
  return missing
})

const completionPercent = computed(() => {
  let score = 0
  if (form.value.maMonHoc) score += 34
  if (form.value.maGiaoVien) score += 33
  if (selectedCount.value > 0) score += 33
  return score
})

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return
  submitting.value = true
  try {
    const payload = {
      maMonHoc: form.value.maMonHoc,
      maGiaoVien: form.value.maGiaoVien,
      maHocKy: form.value.maHocKy || undefined,
      maLopIds: [...selectedClassIds.value],
      tieuDe: form.value.tieuDe || undefined,
      moTa: form.value.moTa || undefined,
      trangThai: form.value.trangThai || 'nhap',
    }
    const res = await courseApi.bulkAssign(payload)
    const data = res.data || res
    emit('done', {
      created: data.created || [],
      skipped: data.skipped || [],
    })
    popupStore.success('Phân phối thành công', `Đã tạo ${data.created?.length || 0} khóa học.`)
  } catch (err) {
    popupStore.error('Phân phối thất bại', err.message || 'Không thể tạo khóa học.')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50">
      <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm" @click="emit('close')" />
      <div class="fixed inset-y-0 right-0 z-50 w-full sm:w-[600px] lg:w-[680px]
                  transform transition-transform duration-300 translate-x-0
                  surface-card border-l border-card shadow-2xl flex flex-col">
        <div class="flex items-center justify-between px-6 py-4 border-b border-default shrink-0">
          <div class="flex items-center gap-3">
            <button class="h-8 w-8 rounded-lg hover:bg-(--surface-input) flex items-center justify-center text-muted"
              @click="emit('close')">
              <X :size="18" />
            </button>
            <div>
              <h2 class="text-base font-bold text-heading">Phân phối môn học</h2>
              <p class="text-[11px] text-muted mt-0.5">Chọn môn, giảng viên và lớp để tạo khóa học</p>
            </div>
          </div>
        </div>

        <div class="flex-1 overflow-y-auto p-6 space-y-6">
          <!-- Progress indicator -->
          <div class="flex items-center gap-2">
            <div class="flex-1 h-1.5 rounded-full bg-(--surface-input) overflow-hidden">
              <div class="h-full rounded-full bg-(--lg-primary) transition-all duration-500"
                :style="{ width: completionPercent + '%' }" />
            </div>
            <span class="text-[10px] font-bold text-muted whitespace-nowrap">{{ missingFields.length > 0 ? `Thiếu: ${missingFields.join(', ')}` : 'Sẵn sàng' }}</span>
          </div>

          <!-- Step 1: Môn học + Học kỳ -->
          <div class="surface-card border border-card rounded-xl p-4 space-y-4">
            <div class="flex items-center gap-2 text-heading">
              <div class="h-6 w-6 rounded-lg bg-(--lg-primary) flex items-center justify-center">
                <BookOpen :size="13" class="text-white" />
              </div>
              <span class="text-sm font-bold">Môn học</span>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1.5">
                  Ngành đào tạo
                </label>
                <LmsSelect v-model="form.maNganh" :options="majors" placeholder="Chọn ngành đào tạo" />
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1.5">
                  Chuyên ngành
                </label>
                <LmsSelect v-model="form.maChuyenNganh" :options="specializations" placeholder="Chọn chuyên ngành" />
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1.5">
                  Môn học <span class="text-(--color-danger-text)">*</span>
                </label>
                <LmsSelect v-model="form.maMonHoc" :options="subjects" placeholder="Chọn môn học" />
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1.5">
                  Học kỳ
                </label>
                <LmsSelect v-model="form.maHocKy" :options="academicTerms" placeholder="Chọn học kỳ" />
              </div>
            </div>
            <div v-if="selectedSubject" class="flex items-center gap-2 rounded-lg bg-(--color-info-bg) px-3 py-2">
              <GraduationCap :size="14" class="text-(--color-info-text)" />
              <span class="text-xs text-(--color-info-text)">
                <span class="font-bold">{{ selectedSubject.label }}</span>
                <template v-if="selectedTerm"> — {{ selectedTerm.label }}</template>
              </span>
            </div>
          </div>

          <!-- Step 2: Giảng viên -->
          <div class="surface-card border border-card rounded-xl p-4 space-y-3">
            <div class="flex items-center gap-2 text-heading">
              <div class="h-6 w-6 rounded-lg bg-(--lg-primary) flex items-center justify-center">
                <Users :size="13" class="text-white" />
              </div>
              <span class="text-sm font-bold">Giảng viên phụ trách</span>
            </div>
            <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1.5">
                  Giảng viên <span class="text-(--color-danger-text)">*</span>
                </label>
                <TeacherAssignmentSelect v-model="form.maGiaoVien" :ma-hoc-ky="form.maHocKy" :ma-mon-hoc="form.maMonHoc" :ma-lop-ids="selectedClassIds" :teachers="teachers" />
                <p v-if="selectedTeacher" class="text-[11px] text-muted mt-1.5">
                  <span class="font-medium">{{ selectedTeacher.label }}</span> sẽ phụ trách tất cả khóa học được tạo.
                </p>
            </div>
          </div>

          <!-- Step 3: Lớp hành chính -->
          <div class="surface-card border border-card rounded-xl p-4 space-y-3">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 text-heading">
                <div class="h-6 w-6 rounded-lg bg-(--lg-primary) flex items-center justify-center">
                  <Users :size="13" class="text-white" />
                </div>
                <span class="text-sm font-bold">Lớp hành chính</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-[11px] font-bold" :class="isMaxClasses ? 'text-(--color-warning-text)' : 'text-muted'">
                  {{ selectedCount }}/5
                </span>
                <div v-if="remainingSlots > 0 && selectedCount > 0"
                  class="text-[10px] text-muted">(còn {{ remainingSlots }})</div>
              </div>
            </div>
            <p class="text-[11px] text-muted">Mỗi lớp sẽ tạo một khóa học riêng. Tối đa <strong>5 lớp</strong> mỗi lần.</p>

            <div v-if="selectedCount > 0" class="flex flex-wrap gap-1.5">
              <span v-for="id in selectedClassIds" :key="id"
                class="inline-flex items-center gap-1 rounded-full bg-(--color-info-bg) text-(--color-info-text) px-2.5 py-1 text-[11px] font-bold">
                {{ getClassLabel(id) }}
                <button class="hover:text-(--color-danger-text) leading-none" @click="removeClass(id)">&times;</button>
              </span>
            </div>

            <div class="surface-input border border-input rounded-xl overflow-hidden">
              <div class="flex items-center gap-2 px-3 border-b border-input">
                <Search :size="14" class="text-muted shrink-0" />
                <input v-model="classSearch" placeholder="Tìm lớp..."
                  class="h-9 w-full bg-transparent text-sm text-heading outline-none placeholder:text-placeholder">
              </div>
              <div class="max-h-48 overflow-y-auto">
                <div v-for="cls in filteredClasses" :key="cls.value"
                  class="flex items-center gap-3 px-3 py-2.5 cursor-pointer hover:bg-(--surface-hover) transition-colors border-b border-input last:border-0"
                  :class="{ 'opacity-50 pointer-events-none': isMaxClasses && !selectedClassIds.includes(cls.value) }"
                  @click="toggleClass(cls.value)">
                  <div class="h-4 w-4 rounded border-2 flex items-center justify-center shrink-0 transition-colors"
                    :class="selectedClassIds.includes(cls.value)
                      ? 'bg-(--lg-primary) border-(--lg-primary)'
                      : 'border-default'">
                    <Check v-if="selectedClassIds.includes(cls.value)" :size="12" class="text-white" />
                  </div>
                  <span class="flex-1 text-sm font-medium" :class="selectedClassIds.includes(cls.value) ? 'text-heading' : 'text-body'">{{ cls.label }}</span>
                  <span class="text-[10px] text-muted bg-(--surface-card) rounded px-1.5 py-0.5">Lớp</span>
                </div>
                <div v-if="filteredClasses.length === 0" class="px-3 py-4 text-center text-xs text-muted">
                  Không tìm thấy lớp nào
                </div>
              </div>
            </div>
          </div>

          <!-- Preview -->
          <div v-if="isFormValid"
            class="surface-card border border-card rounded-xl p-4 space-y-3">
            <div class="flex items-center gap-2">
              <Eye :size="14" class="text-(--lg-primary)" />
              <span class="text-xs font-bold text-heading uppercase tracking-wide">Xem trước</span>
            </div>
            <div class="rounded-lg bg-(--surface-input) p-3 space-y-2">
              <div class="flex items-center justify-between text-xs">
                <span class="text-muted">Số khóa học sẽ tạo</span>
                <span class="font-bold text-heading">{{ selectedCount }}</span>
              </div>
              <div class="flex items-center justify-between text-xs">
                <span class="text-muted">Môn học</span>
                <span class="font-medium text-heading text-right">{{ selectedSubject?.label || '—' }}</span>
              </div>
              <div class="flex items-center justify-between text-xs">
                <span class="text-muted">Giảng viên</span>
                <span class="font-medium text-heading">{{ selectedTeacher?.label || '—' }}</span>
              </div>
              <div class="flex items-center justify-between text-xs">
                <span class="text-muted">Lớp</span>
                <span class="font-medium text-heading text-right">{{ selectedCount }} lớp ({{ selectedClassIds.map(id => getClassLabel(id)).join(', ') }})</span>
              </div>
              <div v-if="previewTitle" class="flex items-center justify-between text-xs">
                <span class="text-muted">Tiêu đề dự kiến</span>
                <span class="font-medium text-heading text-right max-w-[200px] truncate">{{ previewTitle }}</span>
              </div>
            </div>
            <div v-if="form.trangThai === 'nhap'" class="flex items-center gap-1.5 text-[11px] text-(--color-warning-text)">
              <div class="w-1.5 h-1.5 rounded-full bg-(--color-warning-text)" />
              Trạng thái <strong>Nháp</strong> — cần xuất bản sau
            </div>
          </div>

          <!-- Advanced options (collapsible) -->
          <div class="surface-card border border-card rounded-xl overflow-hidden">
            <button @click="showAdvanced = !showAdvanced"
              class="w-full flex items-center justify-between px-4 py-3 text-left hover:bg-(--surface-hover) transition-colors">
              <span class="text-xs font-bold text-heading uppercase tracking-wide">Tùy chọn nâng cao</span>
              <component :is="showAdvanced ? ChevronUp : ChevronDown" :size="14" class="text-muted" />
            </button>
            <div v-if="showAdvanced" class="px-4 pb-4 space-y-4 border-t border-default pt-4">
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Tiêu đề</label>
                <input v-model="form.tieuDe" placeholder="Để trống tự động sinh theo môn + lớp"
                  class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus) placeholder:text-placeholder">
                <p v-if="form.tieuDe" class="text-[10px] text-muted mt-1">Tùy chỉnh: "{{ form.tieuDe }}"</p>
                <p v-else class="text-[10px] text-muted mt-1">Tự động: "{{ previewTitle || '...' }}"</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Mô tả</label>
                <textarea v-model="form.moTa" rows="2" placeholder="Mô tả khóa học (không bắt buộc)"
                  class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus) placeholder:text-placeholder resize-none" />
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Trạng thái</label>
                <select v-model="form.trangThai"
                  class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus)">
                  <option value="nhap">Nháp — chưa hiển thị với sinh viên</option>
                  <option value="da_xuat_ban">Đã xuất bản — hiển thị ngay</option>
                </select>
              </div>
            </div>
          </div>
        </div>

        <div class="px-6 py-4 border-t border-default bg-(--surface-input) flex items-center justify-between shrink-0">
          <div class="flex items-center gap-2 text-xs text-muted">
            <Calendar :size="13" />
            <span v-if="selectedTerm">{{ selectedTerm.label }}</span>
            <span v-else>Chưa chọn học kỳ</span>
          </div>
          <div class="flex items-center gap-2">
            <GlassButton variant="secondary" @click="emit('close')">Hủy</GlassButton>
            <GlassButton variant="primary" :disabled="!isFormValid || submitting" @click="handleSubmit">
              <Loader2 v-if="submitting" :size="14" class="animate-spin mr-1" />
              <template v-else>
                Phân phối ({{ selectedCount }})
              </template>
            </GlassButton>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
