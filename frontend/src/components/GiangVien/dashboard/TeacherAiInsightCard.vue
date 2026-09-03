<script setup>
import { ref, onMounted } from 'vue'
import {
  Sparkles,
  Bot,
  RotateCcw,
  AlertTriangle,
  Clock,
  FileEdit,
  ChevronRight,
  GraduationCap
} from 'lucide-vue-next'
import { aiApi } from '@/services/aiApi'
import { useAiAssistant } from '@/composables/useAiAssistant'

const { openWithPrompt } = useAiAssistant()

const insight = ref(null)
const loading = ref(true)
const isRefreshing = ref(false)
const error = ref(null)

async function fetchInsight(force = false) {
  try {
    if (force) isRefreshing.value = true
    else loading.value = true
    error.value = null

    const data = await aiApi.getDashboardInsight({ forceRefresh: force })
    insight.value = data
  } catch (err) {
    console.error('Failed to load teacher AI insight', err)
    error.value = 'Không thể tải phân tích AI lúc này.'
  } finally {
    loading.value = false
    isRefreshing.value = false
  }
}

function handleActionClick(prompt) {
  if (!prompt) return
  openWithPrompt(prompt)
}

function getSeverityClasses(severity) {
  switch (severity) {
    case 'danger':
      return 'border-rose-500/30 bg-rose-500/10 text-rose-600 dark:text-rose-400'
    case 'warning':
      return 'border-amber-500/30 bg-amber-500/10 text-amber-600 dark:text-amber-400'
    case 'success':
      return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
    default:
      return 'border-blue-500/30 bg-blue-500/10 text-blue-600 dark:text-blue-400'
  }
}

function getActionIcon(title = '') {
  const lower = title.toLowerCase()
  if (lower.includes('chấm') || lower.includes('bài tập')) return FileEdit
  if (lower.includes('nguy cơ') || lower.includes('hỗ trợ') || lower.includes('cảnh báo')) return AlertTriangle
  if (lower.includes('ca dạy') || lower.includes('lịch')) return Clock
  return GraduationCap
}

onMounted(() => {
  fetchInsight(false)
})
</script>

<template>
  <div class="lg-glass-soft rounded-2xl p-5 border border-card shadow-sm transition-all hover:shadow-md">
    <!-- Header -->
    <div class="flex items-center justify-between border-b border-card pb-3.5 mb-4">
      <div class="flex items-center gap-2.5">
        <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-600/10 text-blue-600 dark:text-blue-400 border border-blue-600/20">
          <Bot :size="18" />
        </div>
        <div>
          <div class="flex items-center gap-1.5">
            <h3 class="text-sm font-bold text-heading">Trợ lý Sư phạm AI</h3>
            <span class="inline-flex items-center gap-0.5 rounded-full bg-blue-500/10 px-1.5 py-0.5 text-[10px] font-semibold text-blue-600 dark:text-blue-300">
              <Sparkles :size="10" /> AI
            </span>
          </div>
          <p class="text-[11px] text-muted">Nhận định học thuật & hỗ trợ giảng dạy tức thì</p>
        </div>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-1 rounded-lg px-2.5 py-1 text-[11px] font-semibold text-muted hover:text-heading hover:bg-(--surface-input) border border-card transition-all"
        :disabled="loading || isRefreshing"
        title="Làm mới nhận định AI"
        @click="fetchInsight(true)"
      >
        <RotateCcw :size="12" :class="{ 'animate-spin text-blue-600': isRefreshing }" />
        <span>{{ isRefreshing ? 'Đang phân tích...' : 'Làm mới' }}</span>
      </button>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="loading" class="space-y-3 py-1">
      <div class="h-4 w-5/6 rounded-md bg-slate-200/60 dark:bg-slate-700/50 animate-pulse"></div>
      <div class="h-4 w-4/6 rounded-md bg-slate-200/60 dark:bg-slate-700/50 animate-pulse"></div>
      <div class="mt-4 grid gap-2">
        <div class="h-14 rounded-xl bg-slate-200/50 dark:bg-slate-700/40 animate-pulse"></div>
        <div class="h-14 rounded-xl bg-slate-200/50 dark:bg-slate-700/40 animate-pulse"></div>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="py-4 text-center">
      <p class="text-xs text-rose-500">{{ error }}</p>
      <button
        type="button"
        class="mt-2 text-xs font-bold text-blue-600 hover:underline"
        @click="fetchInsight(true)"
      >
        Thử lại
      </button>
    </div>

    <!-- Main Content -->
    <div v-else class="space-y-4">
      <!-- Executive Summary -->
      <div class="rounded-xl border border-blue-500/20 bg-blue-500/5 p-3.5 text-xs text-body leading-relaxed">
        <p class="font-medium text-heading">
          {{ insight?.executiveSummary || 'Hiện tại không có ghi chú đặc biệt cho các lớp học phần của bạn.' }}
        </p>
      </div>

      <!-- Action Items Grid -->
      <div v-if="insight?.actionItems?.length" class="space-y-2">
        <p class="text-[11px] font-bold uppercase tracking-wider text-muted">Hành động khuyến nghị</p>
        <div class="grid gap-2">
          <div
            v-for="(item, idx) in insight.actionItems"
            :key="idx"
            class="group flex items-center justify-between rounded-xl border border-card surface-card p-2.5 transition-all hover:border-blue-500/40 hover:shadow-xs cursor-pointer"
            @click="handleActionClick(item.actionPrompt)"
          >
            <div class="flex items-center gap-2.5 min-w-0 flex-1">
              <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-lg border', getSeverityClasses(item.severity)]">
                <component :is="getActionIcon(item.title)" :size="15" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-xs font-bold text-heading truncate group-hover:text-blue-600 transition-colors">
                  {{ item.title }}
                </p>
                <p class="text-[11px] text-muted truncate mt-0.5">
                  {{ item.description }}
                </p>
              </div>
            </div>

            <div class="flex items-center gap-1 text-[11px] font-semibold text-blue-600 opacity-80 group-hover:opacity-100 flex-shrink-0 ml-2">
              <span>Hỏi AI</span>
              <ChevronRight :size="13" class="transition-transform group-hover:translate-x-0.5" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
