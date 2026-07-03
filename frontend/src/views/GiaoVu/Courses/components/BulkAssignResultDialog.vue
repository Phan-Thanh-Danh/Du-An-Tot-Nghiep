<script setup>
import { X, CheckCircle2, AlertCircle } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'

defineProps({
  result: { type: Object, default: () => ({ created: [], skipped: [] }) },
})

const emit = defineEmits(['close', 'viewList'])
</script>

<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-[60] flex items-center justify-center">
      <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm" @click="emit('close')" />
      <div class="relative z-[60] surface-card border border-card rounded-2xl shadow-xl w-full max-w-lg mx-4 max-h-[80vh] flex flex-col">
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-default shrink-0">
          <h3 class="text-base font-bold text-heading">Kết quả tạo khóa học</h3>
          <button class="h-8 w-8 rounded-lg hover:bg-(--surface-input) flex items-center justify-center text-muted"
            @click="emit('close')">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto p-6 space-y-5">
          <!-- Summary -->
          <div class="surface-solid border border-default rounded-xl p-4">
            <div class="flex items-center gap-3">
              <div class="h-10 w-10 rounded-full bg-(--color-info-bg) flex items-center justify-center shrink-0">
                <CheckCircle2 :size="20" class="text-(--color-info-text)" />
              </div>
              <div>
                <p class="text-sm font-bold text-heading">
                  Tạo thành công {{ result.created?.length || 0 }} / {{ (result.created?.length || 0) + (result.skipped?.length || 0) }} lớp
                </p>
                <p v-if="result.skipped?.length" class="text-xs text-muted mt-0.5">
                  {{ result.skipped.length }} lớp bị bỏ qua do trùng lặp hoặc lỗi.
                </p>
              </div>
            </div>
          </div>

          <!-- Created list -->
          <div v-if="result.created?.length">
            <h4 class="text-xs font-bold text-muted uppercase tracking-wide mb-2">Đã tạo thành công</h4>
            <div class="space-y-2">
              <div v-for="course in result.created" :key="course.maKhoaHoc || course.maLop"
                class="flex items-center gap-3 rounded-xl border border-default surface-solid px-3 py-2.5">
                <div class="h-6 w-6 rounded-full bg-(--color-success-bg) flex items-center justify-center shrink-0">
                  <CheckCircle2 :size="14" class="text-(--color-success-text)" />
                </div>
                <div class="min-w-0 flex-1">
                  <p class="text-sm font-bold text-heading truncate">{{ course.tenLop || getClassLabel(course.maLop) }}</p>
                  <p class="text-xs text-muted truncate">{{ course.tieuDe || course.tenMonHoc || '' }}</p>
                </div>
                <span class="text-[11px] font-mono font-bold text-muted shrink-0">#{{ course.maKhoaHoc }}</span>
              </div>
            </div>
          </div>

          <!-- Skipped list -->
          <div v-if="result.skipped?.length">
            <h4 class="text-xs font-bold text-muted uppercase tracking-wide mb-2">Bỏ qua</h4>
            <div class="space-y-2">
              <div v-for="item in result.skipped" :key="item.maLop"
                class="flex items-start gap-3 rounded-xl border border-default surface-solid px-3 py-2.5">
                <div class="h-6 w-6 rounded-full bg-(--color-warning-bg) flex items-center justify-center shrink-0 mt-0.5">
                  <AlertCircle :size="14" class="text-(--color-warning-text)" />
                </div>
                <div class="min-w-0 flex-1">
                  <p class="text-sm font-bold text-heading truncate">{{ item.tenLop || item.maLop }}</p>
                  <p class="text-xs text-muted">{{ item.lyDo || 'Đã tồn tại khóa học cho môn/học kỳ này.' }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-6 py-4 border-t border-default bg-(--surface-input) flex items-center justify-end gap-2 shrink-0">
          <GlassButton variant="primary" @click="emit('close')">Đóng</GlassButton>
        </div>
      </div>
    </div>
  </Teleport>
</template>
