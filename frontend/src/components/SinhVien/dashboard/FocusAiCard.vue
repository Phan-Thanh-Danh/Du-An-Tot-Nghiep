<script setup>
import {
  Bot,
  CheckCircle2,
  SendHorizontal,
  Sparkles,
} from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
</script>

<template>
  <GlassPanel density="compact" class="relative flex min-h-[240px] flex-col justify-between rounded-[32px] overflow-hidden">
    <div class="pointer-events-none absolute -right-12 top-2 h-36 w-36 rounded-full bg-violet-300/12 blur-3xl" />
    
    <div class="relative flex flex-col gap-3.5 p-5 lg:p-6">
      <div class="flex items-start justify-between gap-3">
        <div>
          <p class="text-[10px] font-bold uppercase tracking-[0.16em] text-violet-700">
            Trợ lý học tập
          </p>
          <h2 class="mt-1 text-lg font-bold tracking-tight text-slate-950">
            Việc nên làm tiếp theo
          </h2>
        </div>
        <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-violet-600 text-white shadow-lg shadow-violet-500/20">
          <Bot :size="16" />
        </div>
      </div>

      <div class="lg-readable rounded-2xl px-4 py-3 text-[13px] font-semibold leading-relaxed text-slate-800 shadow-sm border border-violet-100/50">
        <div class="flex items-start gap-2">
          <Sparkles :size="14" class="mt-0.5 text-violet-600 flex-shrink-0" />
          <p>Ưu tiên: hoàn thành bài <span class="text-violet-700 font-bold">Phân tích thuật toán</span> trước 23:59.</p>
        </div>
      </div>

      <div class="flex flex-wrap gap-2">
        <button class="rounded-full border border-white/60 bg-white/70 px-3 py-1.5 text-[10px] font-bold text-slate-700 shadow-sm transition hover:bg-white hover:text-blue-700">
          Vào bài học
        </button>
        <button class="rounded-full border border-white/60 bg-white/70 px-3 py-1.5 text-[10px] font-bold text-slate-700 shadow-sm transition hover:bg-white hover:text-blue-700">
          Ôn tập nhanh
        </button>
        <button class="rounded-full border border-white/60 bg-white/70 px-3 py-1.5 text-[10px] font-bold text-slate-700 shadow-sm transition hover:bg-white hover:text-blue-700">
          Deadline
        </button>
      </div>
    </div>

    <div class="relative px-5 pb-5 lg:px-6 lg:pb-6">
      <div class="lg-input flex items-center gap-2 rounded-2xl px-3.5 py-2 shadow-sm border border-white/60 bg-white/50 backdrop-blur-md">
        <input 
          class="min-w-0 flex-1 bg-transparent text-[12px] font-medium text-slate-700 outline-none placeholder:text-slate-400" 
          placeholder="Hỏi AI về bài học, deadline..."
        />
        <button class="lg-icon-button h-7 w-7 bg-blue-600 text-white shadow-md transition hover:scale-105 active:scale-95" aria-label="Gửi câu hỏi">
          <SendHorizontal :size="14" />
        </button>
      </div>
    </div>
  </GlassPanel>
</template>
