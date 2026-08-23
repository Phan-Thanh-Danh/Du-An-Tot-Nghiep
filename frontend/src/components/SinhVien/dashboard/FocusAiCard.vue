<script setup>
import { ref } from 'vue'
import { Bot, CheckCircle2, SendHorizontal, Sparkles } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { useAiAssistant } from '@/composables/useAiAssistant'

const { openWithPrompt } = useAiAssistant()
const inputPrompt = ref('')

function handleSend() {
  const query = inputPrompt.value.trim()
  if (!query) return
  openWithPrompt(query)
  inputPrompt.value = ''
}

function handleQuickAsk(suggestion) {
  openWithPrompt(suggestion)
}
</script>

<template>
  <GlassPanel variant="strong" density="none" class="relative rounded-2xl lg-glass-card-hover">
    <div class="relative flex min-h-[205px] flex-col justify-between gap-3 p-4 lg:p-5">
      <div class="flex items-start justify-between gap-3">
        <div>
          <p class="text-xs font-semibold text-(--accent-violet)">Trợ lý học tập AI</p>
          <h2 class="mt-1 text-lg font-semibold tracking-tight text-heading">Gợi ý học tập hôm nay</h2>
        </div>
        <button
          class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-(--accent-violet) text-white shadow-(--lg-shadow-md) hover:scale-105 active:scale-95 transition-transform"
          title="Mở trợ lý AI"
          @click="openWithPrompt('Tôi muốn xem gợi ý học tập hôm nay.')"
        >
          <Bot :size="16" />
        </button>
      </div>

      <div
        class="lg-readable cursor-pointer rounded-2xl border border-[color-mix(in srgb,var(--accent-violet) 20%,transparent)] px-3.5 py-3 text-[13px] font-medium leading-6 text-heading shadow-sm hover:border-(--accent-violet)/40 transition-colors"
        @click="handleQuickAsk('Hướng dẫn em phương pháp học và làm bài tập Phân tích thuật toán hiệu quả')"
      >
        <div class="flex items-start gap-2">
          <Sparkles :size="14" class="mt-0.5 text-(--accent-violet) flex-shrink-0" />
          <p>Ưu tiên: hoàn thành bài tập <span class="font-semibold text-(--accent-violet)">Phân tích thuật toán</span> trước 23:59.</p>
        </div>
      </div>

      <div class="grid gap-2 sm:grid-cols-2">
        <button
          class="flex items-center gap-2 rounded-2xl bg-(--surface-card) px-3 py-2 text-xs font-medium text-body ring-1 ring-border-card hover:ring-(--accent-violet)/40 transition-all text-left"
          @click="handleQuickAsk('Hãy tóm tắt các kiến thức trọng tâm cần ôn trong môn Cơ sở dữ liệu')"
        >
          <CheckCircle2 :size="14" class="text-(--color-success-text) flex-shrink-0" />
          <span>Ôn 15 phút CSDL</span>
        </button>
        <button
          class="flex items-center gap-2 rounded-2xl bg-(--surface-card) px-3 py-2 text-xs font-medium text-body ring-1 ring-border-card hover:ring-(--accent-violet)/40 transition-all text-left"
          @click="handleQuickAsk('Cách sắp xếp lịch học và rà soát deadline môn Cấu trúc dữ liệu')"
        >
          <CheckCircle2 :size="14" class="text-(--color-success-text) flex-shrink-0" />
          <span>Soát deadline CTDL</span>
        </button>
      </div>

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
