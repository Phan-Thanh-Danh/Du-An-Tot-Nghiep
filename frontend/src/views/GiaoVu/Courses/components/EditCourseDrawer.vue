<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { courseApi } from '@/services/courseApi'
import { X, Loader2, AlertTriangle } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import TeacherAssignmentSelect from './TeacherAssignmentSelect.vue'
import CourseStatusBadge from './CourseStatusBadge.vue'

const emit = defineEmits(['close', 'done'])
const props = defineProps({
  course: { type: Object, required: true },
  academicTerms: { type: Array, default: () => [] },
  teachers: { type: Array, default: () => [] },
  classes: { type: Array, default: () => [] },
  subjects: { type: Array, default: () => [] },
})

const popupStore = usePopupStore()

const form = ref({
  tieuDe: '',
  moTa: '',
  maGiaoVien: null,
  maLop: null,
  maHocKy: null,
  trangThai: 'nhap',
  urlAnhBia: '',
})

const submitting = ref(false)
const changedFields = ref(new Set())

function initForm() {
  const c = props.course
  form.value = {
    tieuDe: c.tieuDe || '',
    moTa: c.moTa || '',
    maGiaoVien: c.maGiaoVien || null,
    maLop: c.maLop || null,
    maHocKy: c.maHocKy || null,
    trangThai: c.trangThai || 'nhap',
    urlAnhBia: c.urlAnhBia || '',
  }
  changedFields.value = new Set()
}

onMounted(initForm)

function markChanged(field) {
  changedFields.value.add(field)
}

const hasChanges = computed(() => changedFields.value.size > 0)

const hasTKB = computed(() => props.course.hasTKB)

const showChangeWarning = computed(() => {
  if (!hasChanges.value) return null
  const warn = []
  if (hasTKB.value && changedFields.value.has('maGiaoVien')) {
    warn.push('thay đổi giảng viên')
  }
  if (hasTKB.value && changedFields.value.has('maLop')) {
    warn.push('thay đổi lớp')
  }
  return warn.length ? warn : null
})

const isFormValid = computed(() => {
  return form.value.tieuDe?.trim()?.length > 0
})

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return
  submitting.value = true
  try {
    const payload = {
      tieuDe: form.value.tieuDe,
      moTa: form.value.moTa || null,
      maGiaoVien: form.value.maGiaoVien,
      maLop: form.value.maLop,
      maHocKy: form.value.maHocKy,
      trangThai: form.value.trangThai,
      urlAnhBia: form.value.urlAnhBia || null
    }

    await courseApi.updateCourse(props.course.maKhoaHoc, payload)
    popupStore.success('Cập nhật thành công', 'Khóa học đã được cập nhật.')
    emit('done')
    emit('close')
  } catch (err) {
    popupStore.error('Cập nhật thất bại', err.message || 'Không thể cập nhật khóa học.')
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
            <h2 class="text-base font-bold text-heading">Sửa khóa học</h2>
          </div>
          <CourseStatusBadge :status="course.trangThai" />
        </div>

        <div v-if="showChangeWarning" class="mx-6 mt-4 flex items-start gap-2 rounded-xl bg-(--color-warning-bg) px-4 py-3">
          <AlertTriangle :size="16" class="text-(--color-warning-text) shrink-0 mt-0.5" />
          <div class="text-xs text-(--color-warning-text)">
            <p class="font-bold">Cảnh báo</p>
            <p>Khóa học đã có thời khóa biểu. Việc {{ showChangeWarning.join(' và ') }} có thể ảnh hưởng đến lịch giảng dạy hiện tại.</p>
          </div>
        </div>

        <div class="flex-1 overflow-y-auto p-6 space-y-5">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Mã khóa học</label>
              <p class="h-10 flex items-center text-sm font-bold text-heading font-mono">#{{ course.maKhoaHoc }}</p>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Môn học</label>
              <p class="h-10 flex items-center text-sm text-body font-medium">{{ course.tenMonHoc }} ({{ course.maMonHocCode || '' }})</p>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Lớp hiện tại</label>
              <p class="h-10 flex items-center text-sm text-body">{{ course.tenLop || '—' }}</p>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Giảng viên hiện tại</label>
              <p class="h-10 flex items-center text-sm text-body">{{ course.tenGiaoVien || '—' }}</p>
            </div>
          </div>

          <div>
            <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Tiêu đề</label>
            <input v-model="form.tieuDe" @input="markChanged('tieuDe')" placeholder="Tiêu đề khóa học"
              class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus) placeholder:text-placeholder">
          </div>

          <div>
            <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Mô tả</label>
            <textarea v-model="form.moTa" @input="markChanged('moTa')" rows="3" placeholder="Mô tả khóa học"
              class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus) placeholder:text-placeholder resize-none" />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Học kỳ</label>
              <LmsSelect v-model="form.maHocKy" :options="academicTerms" placeholder="Chọn học kỳ"
                @update:model-value="markChanged('maHocKy')" />
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Môn học</label>
              <LmsSelect v-model="form.maMonHoc" :options="subjects" placeholder="Chọn môn học"
                @update:model-value="markChanged('maMonHoc')" />
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Giảng viên</label>
              <TeacherAssignmentSelect v-model="form.maGiaoVien" :ma-hoc-ky="form.maHocKy" :ma-mon-hoc="form.maMonHoc" :ma-lop-ids="[form.maLop].filter(Boolean)" :teachers="teachers" :disabled="hasTKB" @update:model-value="markChanged('maGiaoVien')" />
              <p v-if="hasTKB" class="text-[10px] text-(--color-warning-text) mt-1">Đã có TKB — không thể đổi giảng viên.</p>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Lớp</label>
              <LmsSelect v-model="form.maLop" :options="classes" placeholder="Chọn lớp"
                :disabled="hasTKB" @update:model-value="markChanged('maLop')" />
              <p v-if="hasTKB" class="text-[10px] text-(--color-warning-text) mt-1">Đã có TKB — không thể đổi lớp.</p>
            </div>
            <div class="col-span-2">
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Trạng thái</label>
              <select v-model="form.trangThai" @change="markChanged('trangThai')"
                class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus)">
                <option value="nhap">Nháp</option>
                <option value="da_xuat_ban">Đã xuất bản</option>
                <option value="luu_tru">Lưu trữ</option>
              </select>
            </div>
          </div>

          <div>
            <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">URL ảnh bìa</label>
            <input v-model="form.urlAnhBia" @input="markChanged('urlAnhBia')" placeholder="https://..."
              class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-xl text-sm text-heading outline-none focus:ring-2 focus:ring-(--border-focus) placeholder:text-placeholder">
          </div>
        </div>

        <div class="px-6 py-4 border-t border-default bg-(--surface-input) flex items-center justify-between shrink-0">
          <p class="text-xs text-muted">
            <template v-if="!hasChanges">Chưa có thay đổi</template>
            <template v-else>{{ changedFields.size }} thay đổi</template>
          </p>
          <div class="flex items-center gap-2">
            <GlassButton variant="secondary" @click="emit('close')">Hủy</GlassButton>
            <GlassButton variant="primary" :disabled="!hasChanges || !isFormValid || submitting" @click="handleSubmit">
              <Loader2 v-if="submitting" :size="14" class="animate-spin mr-1" />
              Lưu thay đổi
            </GlassButton>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
