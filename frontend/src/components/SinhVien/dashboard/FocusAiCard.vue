<script setup>
import { Bot, CheckCircle2, SendHorizontal, Sparkles } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
</script>

<template>
  <GlassPanel density="none" class="relative rounded-[28px]">
    <div class="pointer-events-none absolute -right-12 top-2 h-36 w-36 rounded-full bg-violet-300/12 blur-3xl" />

    <div class="relative flex min-h-[205px] flex-col justify-between gap-3 p-4 lg:p-5">
      <div class="flex items-start justify-between gap-3">
        <div>
          <p class="text-xs font-semibold text-violet-700">Trợ lý học tập</p>
          <h2 class="mt-1 text-lg font-bold tracking-tight text-slate-950">Gợi ý tiếp theo</h2>
        </div>
        <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-violet-600 text-white shadow-lg shadow-violet-500/20">
          <Bot :size="16" />
        </div>
      </div>

      <div class="lg-readable rounded-2xl border border-violet-100/50 px-3.5 py-3 text-[13px] font-medium leading-6 text-slate-800 shadow-sm">
        <div class="flex items-start gap-2">
          <Sparkles :size="14" class="mt-0.5 text-violet-600 flex-shrink-0" />
          <p>Ưu tiên phụ: hoàn thành bài <span class="font-semibold text-violet-700">Phân tích thuật toán</span> trước 23:59.</p>
        </div>
      </div>

      <div class="grid gap-2 sm:grid-cols-2">
        <div class="flex items-center gap-2 rounded-2xl bg-white/70 px-3 py-2 text-xs font-medium text-slate-700 ring-1 ring-white/70">
          <CheckCircle2 :size="14" class="text-emerald-600" />
          Ôn 15 phút CSDL
        </div>
        <div class="flex items-center gap-2 rounded-2xl bg-white/70 px-3 py-2 text-xs font-medium text-slate-700 ring-1 ring-white/70">
          <CheckCircle2 :size="14" class="text-emerald-600" />
          Soát deadline CTDL
        </div>
      </div>

      <div class="lg-input flex items-center gap-2 rounded-2xl border border-white/60 bg-white/50 px-3.5 py-2 shadow-sm backdrop-blur-md">
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
