<script setup>
import { ref, computed, watch } from 'vue'
import { Users, Loader2, Check, AlertTriangle, ShieldCheck, ShieldAlert, BadgeInfo, X } from 'lucide-vue-next'
import { courseApi } from '@/services/courseApi'
import GlassButton from '@/components/ui/GlassButton.vue'
import LmsSelect from '@/components/LmsSelect.vue'

const props = defineProps({
  modelValue: { type: Number, default: null },
  maHocKy: { type: Number, default: null },
  maMonHoc: { type: Number, default: null },
  maLopIds: { type: Array, default: () => [] },
  disabled: { type: Boolean, default: false },
  teachers: { type: Array, default: () => [] }, // Fallback list if API fails
})
const emit = defineEmits(['update:modelValue'])

const showModal = ref(false)
const loading = ref(false)
const candidates = ref([])
const errorMsg = ref(null)

const localValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

async function openSuggestions() {
  if (!props.maMonHoc || !props.maHocKy || props.maLopIds.length === 0) {
    errorMsg.value = 'Vui lòng chọn môn học, học kỳ và lớp trước khi phân công'
    candidates.value = []
    showModal.value = true
    return
  }

  showModal.value = true
  loading.value = true
  errorMsg.value = null

  try {
    const payload = {
      maMonHoc: props.maMonHoc,
      maHocKy: props.maHocKy,
      maLopIds: props.maLopIds
    }
    const res = await courseApi.getAssignmentSuggestions(payload)
    candidates.value = res.data?.candidates || res.candidates || []
  } catch (err) {
    errorMsg.value = err.message || 'Lỗi tải gợi ý phân công'
  } finally {
    loading.value = false
  }
}

function selectCandidate(c) {
  if (c.status === 'Excluded') return
  localValue.value = c.teacherId
  showModal.value = false
}

const triggerLabel = computed(() => {
  if (!localValue.value) return 'Chọn giảng viên'
  const t = props.teachers.find(x => x.value === localValue.value)
  if (t) return t.label
  // Try to find in candidates if it was selected from there
  const c = candidates.value.find(x => x.teacherId === localValue.value)
  if (c) return c.teacherName
  return 'Đã chọn'
})
</script>

<template>
  <div>
    <!-- The Trigger Button replacing the Select -->
    <div class="relative">
      <div v-if="!disabled" 
           @click="openSuggestions"
           class="h-10 w-full bg-(--surface-input) border border-(--border-input) rounded-xl px-3 flex items-center justify-between cursor-pointer hover:border-(--lg-primary) transition-colors text-sm">
        <span :class="!localValue ? 'text-muted' : 'text-heading'">{{ triggerLabel }}</span>
        <Users :size="15" class="text-(--lg-primary)" />
      </div>
      <div v-else class="h-10 w-full bg-slate-50 border border-slate-200 rounded-xl px-3 flex items-center justify-between text-muted text-sm cursor-not-allowed">
        <span>{{ triggerLabel }}</span>
        <Users :size="15" class="opacity-50" />
      </div>
    </div>

    <!-- The Suggestions Drawer/Modal -->
    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-[60]">
        <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm" @click="showModal = false" />
        <div class="fixed inset-y-0 right-0 z-[60] w-full sm:w-[450px] lg:w-[500px] bg-(--surface-sidebar) shadow-2xl flex flex-col border-l border-(--border-default) transition-transform duration-300">
          
          <div class="p-5 flex items-center justify-between border-b border-(--border-default) bg-gradient-to-r from-(--sidebar-active-start) to-(--sidebar-active-end)">
            <h2 class="text-base font-bold text-white flex items-center gap-2">
              <Users :size="18" /> Gợi ý phân công giảng viên
            </h2>
            <button @click="showModal = false" class="text-white/80 hover:text-white p-1 rounded-lg hover:bg-white/10 transition-colors">
              <X :size="18" />
            </button>
          </div>

          <div class="flex-1 overflow-y-auto p-5 bg-(--surface-card)">
            
            <div v-if="loading" class="flex flex-col items-center justify-center py-10">
              <Loader2 :size="30" class="animate-spin text-(--lg-primary) mb-4" />
              <p class="text-sm text-muted">Đang phân tích gợi ý phân công...</p>
            </div>

            <div v-else-if="errorMsg" class="flex flex-col items-center justify-center py-10 text-center">
              <ShieldAlert :size="40" class="text-(--color-danger-text) mb-4" />
              <p class="text-sm font-bold text-heading">Không thể hiển thị gợi ý</p>
              <p class="text-xs text-muted mt-1 max-w-xs">{{ errorMsg }}</p>
            </div>

            <div v-else-if="candidates.length === 0" class="flex flex-col items-center justify-center py-10 text-center">
              <Users :size="40" class="text-muted mb-4 opacity-50" />
              <p class="text-sm font-bold text-heading">Không tìm thấy giảng viên phù hợp</p>
            </div>

            <div v-else class="space-y-3">
              <div v-for="c in candidates" :key="c.teacherId" 
                   @click="selectCandidate(c)"
                   class="relative p-4 rounded-xl border transition-all duration-200"
                   :class="[
                     c.status === 'Excluded' ? 'border-(--border-default) bg-(--surface-input) cursor-not-allowed opacity-75' : 
                     localValue === c.teacherId ? 'border-(--lg-primary) bg-blue-50/50 shadow-sm cursor-pointer' : 
                     'border-(--border-card) bg-(--surface-card) hover:border-(--lg-primary)/50 hover:shadow-sm cursor-pointer'
                   ]">
                
                <div class="flex justify-between items-start mb-2">
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-full bg-(--lg-primary)/10 text-(--lg-primary) flex items-center justify-center font-bold text-sm">
                      {{ c.teacherName.charAt(0) }}
                    </div>
                    <div>
                      <p class="text-sm font-bold text-heading leading-tight">{{ c.teacherName }}</p>
                      <p class="text-xs text-muted mt-0.5">{{ c.departmentName || 'Chưa phân khoa' }}</p>
                    </div>
                  </div>
                  
                  <div class="flex flex-col items-end">
                    <span v-if="c.status === 'Recommended'" class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-(--color-success-bg) text-(--color-success-text)">
                      Gợi ý hàng đầu
                    </span>
                    <span v-else-if="c.status === 'Eligible'" class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-(--color-info-bg) text-(--color-info-text)">
                      Phù hợp
                    </span>
                    <span v-else-if="c.status === 'Excluded'" class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-(--color-danger-bg) text-(--color-danger-text)">
                      Không đủ ĐK
                    </span>
                    
                    <div v-if="c.status !== 'Excluded'" class="mt-1.5 flex items-center gap-1 font-bold text-xl text-(--lg-primary)">
                      {{ Math.round(c.finalScore) }} <span class="text-[10px] font-normal text-muted">điểm</span>
                    </div>
                  </div>
                </div>

                <div class="grid grid-cols-2 gap-3 mt-4 pt-3 border-t border-(--border-default)">
                  <div>
                    <p class="text-[10px] font-semibold text-muted uppercase tracking-wider mb-1">Chuyên môn</p>
                    <p class="text-xs font-medium text-heading">{{ Math.round(c.capabilityScore) }}đ <span class="text-muted font-normal text-[10px]">/ {{ c.experienceYears }} năm</span></p>
                  </div>
                  <div>
                    <p class="text-[10px] font-semibold text-muted uppercase tracking-wider mb-1">Workload</p>
                    <p class="text-xs font-medium" :class="c.workloadScore > 50 ? 'text-(--color-success-text)' : 'text-(--color-warning-text)'">
                      {{ c.assignedCredits }} / {{ c.maxCredits }} TC
                    </p>
                  </div>
                </div>

                <div v-if="c.preferenceScore > 0" class="mt-3 text-[11px] text-(--color-info-text) flex items-center gap-1.5 font-medium bg-(--color-info-bg) p-2 rounded-lg">
                  <Check :size="14" /> Phù hợp với nguyện vọng đăng ký (+{{ Math.round(c.preferenceScore) }}đ)
                </div>

                <div v-if="c.reasons && c.reasons.length > 0" class="mt-3 space-y-1.5">
                  <div v-for="r in c.reasons" :key="r" class="text-[11px] text-(--color-danger-text) flex items-start gap-1.5 font-medium">
                    <ShieldAlert :size="14" class="mt-0.5 shrink-0" /> {{ r }}
                  </div>
                </div>

                <!-- Check icon when selected -->
                <div v-if="localValue === c.teacherId" class="absolute -top-2 -right-2 w-6 h-6 rounded-full bg-(--lg-primary) text-white flex items-center justify-center shadow-md">
                  <Check :size="14" />
                </div>
              </div>
            </div>
            
          </div>
          
          <div class="p-4 border-t border-(--border-default) bg-(--surface-input) flex justify-end gap-3">
            <GlassButton @click="showModal = false" variant="outline">Hủy bỏ</GlassButton>
          </div>
          
        </div>
      </div>
    </Teleport>
  </div>
</template>
