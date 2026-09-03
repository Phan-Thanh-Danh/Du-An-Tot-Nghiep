<template>
  <Transition
    enter-active-class="transition duration-200 ease-out"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition duration-150 ease-in"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="isOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-black/60 backdrop-blur-xs"
      @click.self="handleClose"
    >
      <div
        class="relative w-full max-w-3xl max-h-[90vh] flex flex-col surface-card border border-card rounded-3xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200"
      >
        <!-- Modal Header -->
        <div class="px-6 py-4.5 border-b border-default flex items-center justify-between bg-gradient-to-r from-blue-900/20 via-indigo-900/10 to-transparent">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-2xl bg-gradient-to-br from-indigo-600 via-blue-600 to-cyan-500 flex items-center justify-center text-white shadow-md shadow-indigo-500/20">
              <Sparkles :size="20" class="animate-pulse" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h3 class="text-base font-bold text-heading">{{ title || 'Báo Cáo Phân Tích Chiến Lược BGH' }}</h3>
                <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-extrabold uppercase tracking-wider bg-indigo-500/15 text-indigo-600 dark:text-indigo-400 border border-indigo-500/30">
                  <Brain :size="11" />
                  Qwen 9B Deep Reasoning
                </span>
              </div>
              <p class="text-xs text-muted mt-0.5">{{ subtitle || 'Báo cáo tổng hợp và đề xuất hành động từ trợ lý AI cấp cao' }}</p>
            </div>
          </div>
          <button
            @click="handleClose"
            class="h-8 w-8 rounded-full flex items-center justify-center text-muted hover:text-heading hover:bg-(--surface-input) transition-colors"
          >
            <X :size="18" />
          </button>
        </div>

        <!-- Scope & Input Badges -->
        <div v-if="scopeBadges && scopeBadges.length" class="px-6 py-2.5 bg-(--surface-input)/50 border-b border-default flex flex-wrap items-center gap-2 text-xs">
          <span class="text-muted font-medium flex items-center gap-1"><Database :size="12" /> Dữ liệu nạp vào:</span>
          <span
            v-for="(badge, bIdx) in scopeBadges"
            :key="bIdx"
            class="px-2.5 py-0.5 rounded-md bg-(--surface-card) border border-default text-heading font-medium text-[11px] shadow-2xs"
          >
            {{ badge }}
          </span>
        </div>

        <!-- Modal Body -->
        <div class="flex-1 overflow-y-auto px-6 py-5 space-y-5">
          <!-- Loading State -->
          <div v-if="loading" class="py-16 flex flex-col items-center justify-center text-center space-y-4">
            <div class="relative">
              <div class="h-16 w-16 rounded-3xl bg-gradient-to-tr from-blue-600 to-indigo-600 animate-spin flex items-center justify-center p-0.5 shadow-lg shadow-indigo-500/30">
                <div class="h-full w-full bg-(--surface-card) rounded-[22px] flex items-center justify-center">
                  <Brain :size="28" class="text-indigo-600 dark:text-indigo-400 animate-pulse" />
                </div>
              </div>
            </div>
            <div class="space-y-1 max-w-md">
              <p class="text-sm font-bold text-heading">Mô hình Qwen 9B đang phân tích dữ liệu...</p>
              <p class="text-xs text-muted leading-relaxed">Đang quét toàn bộ chỉ số từ CSDL, đối chiếu quy chế và xây dựng dự báo chiến lược cho Ban Giám Hiệu.</p>
            </div>
          </div>

          <!-- Error State -->
          <div v-else-if="error" class="p-6 rounded-2xl bg-(--color-danger-bg) border border-(--color-danger-text)/20 text-center space-y-3">
            <AlertCircle :size="36" class="text-(--color-danger-text) mx-auto" />
            <p class="text-sm font-bold text-(--color-danger-text)">Không thể hoàn tất phân tích</p>
            <p class="text-xs text-(--color-danger-text)/80 max-w-md mx-auto">{{ error }}</p>
            <button
              @click="$emit('retry')"
              class="px-4 py-2 bg-(--color-danger-text) text-white text-xs font-bold rounded-xl hover:opacity-90 transition-opacity"
            >
              Thử Lại Phân Tích
            </button>
          </div>

          <!-- Result Content -->
          <div v-else class="space-y-4">
            <!-- Formatted Analysis Box -->
            <div class="p-5 rounded-2xl bg-(--surface-input)/30 border border-default space-y-4">
              <div class="prose prose-sm dark:prose-invert max-w-none text-body text-xs sm:text-sm leading-relaxed whitespace-pre-line font-normal">
                {{ reportContent }}
              </div>
            </div>

            <!-- Verification & Quality Badge -->
            <div class="p-3.5 rounded-xl border border-default bg-(--surface-card) flex items-center justify-between text-xs text-muted">
              <div class="flex items-center gap-2">
                <ShieldCheck :size="15" class="text-emerald-500" />
                <span>Số liệu định lượng được đối soát 100% từ CSDL hệ thống.</span>
              </div>
              <span v-if="generatedAt" class="text-[11px] text-placeholder">Xuất bản: {{ formattedTime }}</span>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="px-6 py-4 border-t border-default flex items-center justify-between bg-(--surface-card)">
          <div class="flex items-center gap-2">
            <button
              v-if="reportContent && !loading"
              @click="copyContent"
              class="px-3.5 py-2 rounded-xl border border-input text-xs font-bold text-body hover:bg-(--surface-input) transition-colors flex items-center gap-1.5"
            >
              <Check v-if="copied" :size="14" class="text-emerald-500" />
              <Copy v-else :size="14" />
              {{ copied ? 'Đã sao chép' : 'Sao chép nội dung' }}
            </button>
          </div>
          <div class="flex items-center gap-2">
            <button
              @click="handleClose"
              class="px-5 py-2.5 rounded-xl bg-(--lg-primary) text-white text-xs font-bold hover:bg-(--lg-primary-dark) shadow-sm shadow-blue-600/20 transition-all"
            >
              Đóng Báo Cáo
            </button>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed } from 'vue'
import {
  Sparkles, Brain, X, Database, AlertCircle,
  ShieldCheck, Copy, Check
} from 'lucide-vue-next'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
  scopeBadges: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  error: { type: String, default: null },
  reportContent: { type: String, default: '' },
  generatedAt: { type: [String, Date], default: null },
})

const emit = defineEmits(['close', 'retry'])

const copied = ref(false)

function handleClose() {
  emit('close')
}

function copyContent() {
  if (!props.reportContent) return
  navigator.clipboard.writeText(props.reportContent)
  copied.value = true
  setTimeout(() => {
    copied.value = false
  }, 2000)
}

const formattedTime = computed(() => {
  if (!props.generatedAt) return ''
  const d = new Date(props.generatedAt)
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' ' + d.toLocaleDateString('vi-VN')
})
</script>
