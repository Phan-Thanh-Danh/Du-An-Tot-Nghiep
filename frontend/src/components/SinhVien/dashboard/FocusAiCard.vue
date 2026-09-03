<script setup>
import { ref, onMounted } from 'vue'
import { Bot, CheckCircle2, SendHorizontal, Sparkles, RotateCcw } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { useAiAssistant } from '@/composables/useAiAssistant'
import { aiApi } from '@/services/aiApi'

const { openWithPrompt } = useAiAssistant()
const inputPrompt = ref('')
const insight = ref(null)
const loading = ref(true)
const isRefreshing = ref(false)

async function fetchInsight(force = false) {
  try {
    if (force) isRefreshing.value = true
    else loading.value = true

    const data = await aiApi.getDashboardInsight({ forceRefresh: force })
    insight.value = data
  } catch (err) {
    console.error('Failed to load student AI focus insight', err)
  } finally {
    loading.value = false
    isRefreshing.value = false
  }
}

function handleSend() {
  const query = inputPrompt.value.trim()
  if (!query) return
  openWithPrompt(query)
  inputPrompt.value = ''
}

function handleQuickAsk(suggestion) {
  if (!suggestion) return
  openWithPrompt(suggestion)
}

onMounted(() => {
  fetchInsight(false)
})
</script>

<template>
  <GlassPanel variant="strong" density="none" class="relative rounded-2xl lg-glass-card-hover">
    <div class="relative flex min-h-[205px] flex-col justify-between gap-3 p-4 lg:p-5">
      <!-- Header -->
      <div class="flex items-start justify-between gap-3">
        <div>
          <div class="flex items-center gap-1.5">
            <p class="text-xs font-semibold text-(--accent-violet)">Trợ lý học tập AI</p>
            <span class="inline-flex items-center gap-0.5 rounded-full bg-purple-500/10 px-1.5 py-0.5 text-[9px] font-semibold text-purple-600 dark:text-purple-300">
              <Sparkles :size="9" /> Qwen
            </span>
          </div>
          <h2 class="mt-1 text-lg font-semibold tracking-tight text-heading">Gợi ý học tập hôm nay</h2>
        </div>
        <div class="flex items-center gap-1.5">
          <button
            type="button"
            class="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-xl bg-(--surface-card) text-muted hover:text-heading ring-1 ring-border-card transition-all"
            title="Làm mới nhận định AI"
            :disabled="loading || isRefreshing"
            @click="fetchInsight(true)"
          >
            <RotateCcw :size="13" :class="{ 'animate-spin text-purple-600': isRefreshing }" />
          </button>
          <button
            class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-(--accent-violet) text-white shadow-(--lg-shadow-md) hover:scale-105 active:scale-95 transition-transform"
            title="Mở trợ lý AI"
            @click="openWithPrompt('Tôi muốn xem gợi ý và kế hoạch học tập hôm nay.')"
          >
            <Bot :size="16" />
          </button>
        </div>
      </div>

      <!-- Loading Skeleton -->
      <div v-if="loading" class="space-y-2.5 py-2">
        <div class="h-10 rounded-2xl bg-slate-200/50 dark:bg-slate-700/40 animate-pulse"></div>
        <div class="grid gap-2 sm:grid-cols-2">
          <div class="h-8 rounded-2xl bg-slate-200/40 dark:bg-slate-700/30 animate-pulse"></div>
          <div class="h-8 rounded-2xl bg-slate-200/40 dark:bg-slate-700/30 animate-pulse"></div>
        </div>
      </div>

      <!-- Real Dynamic Summary & Suggestions -->
      <template v-else>
        <!-- Primary Focus Box -->
        <div
          class="lg-readable cursor-pointer rounded-2xl border border-[color-mix(in srgb,var(--accent-violet) 20%,transparent)] px-3.5 py-3 text-[13px] font-medium leading-6 text-heading shadow-sm hover:border-(--accent-violet)/40 transition-colors"
          @click="handleQuickAsk(insight?.actionItems?.[0]?.actionPrompt || 'Hướng dẫn phương pháp học tập hiệu quả hôm nay')"
        >
          <div class="flex items-start gap-2">
            <Sparkles :size="14" class="mt-0.5 text-(--accent-violet) flex-shrink-0" />
            <p>
              {{ insight?.executiveSummary || 'Hãy duy trì phong độ học tập và hoàn thành đầy đủ bài tập trong kỳ nhé!' }}
            </p>
          </div>
        </div>

        <!-- Dynamic Action Chips -->
        <div v-if="insight?.actionItems?.length" class="grid gap-2 sm:grid-cols-2">
          <button
            v-for="(item, idx) in insight.actionItems.slice(0, 2)"
            :key="idx"
            type="button"
            class="flex items-center gap-2 rounded-2xl bg-(--surface-card) px-3 py-2 text-xs font-medium text-body ring-1 ring-border-card hover:ring-(--accent-violet)/40 transition-all text-left truncate"
            @click="handleQuickAsk(item.actionPrompt)"
          >
            <CheckCircle2 :size="14" class="text-(--color-success-text) flex-shrink-0" />
            <span class="truncate">{{ item.title }}</span>
          </button>
        </div>
        <div v-else class="grid gap-2 sm:grid-cols-2">
          <button
            type="button"
            class="flex items-center gap-2 rounded-2xl bg-(--surface-card) px-3 py-2 text-xs font-medium text-body ring-1 ring-border-card hover:ring-(--accent-violet)/40 transition-all text-left"
            @click="handleQuickAsk('Hướng dẫn em cách ôn tập thi kết thúc học phần hiệu quả')"
          >
            <CheckCircle2 :size="14" class="text-(--color-success-text) flex-shrink-0" />
            <span>Kế hoạch ôn thi kết thúc môn</span>
          </button>
          <button
            type="button"
            class="flex items-center gap-2 rounded-2xl bg-(--surface-card) px-3 py-2 text-xs font-medium text-body ring-1 ring-border-card hover:ring-(--accent-violet)/40 transition-all text-left"
            @click="handleQuickAsk('Kiểm tra điều kiện dự thi và quy chế vắng học tối đa')"
          >
            <CheckCircle2 :size="14" class="text-(--color-success-text) flex-shrink-0" />
            <span>Quy chế điểm danh & thi</span>
          </button>
        </div>
      </template>

      <!-- Bottom Chat Input -->
      <div class="lg-input flex items-center gap-2 rounded-2xl border border-card bg-(--surface-card) px-3.5 py-2 shadow-sm backdrop-blur-md">
        <input
          v-model="inputPrompt"
          class="min-w-0 flex-1 bg-transparent text-[12px] font-medium text-body outline-none placeholder:text-placeholder"
          placeholder="Hỏi AI về bài học, deadline, quy chế..."
          @keydown.enter="handleSend"
        />
        <button
          class="lg-icon-button h-7 w-7 bg-(--text-link) text-white shadow-md transition hover:scale-105 active:scale-95 disabled:opacity-40"
          :disabled="!inputPrompt.trim()"
          aria-label="Gửi câu hỏi"
          @click="handleSend"
        >
          <SendHorizontal :size="14" />
        </button>
      </div>
    </div>
  </GlassPanel>
</template>
