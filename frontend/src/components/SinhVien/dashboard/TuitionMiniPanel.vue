<script setup>
import { CreditCard, ListPlus } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  tuition: {
    type: Object,
    required: true,
  },
  registration: {
    type: Object,
    required: true,
  },
})
</script>

<template>
  <GlassPanel class="rounded-[28px] overflow-hidden" padding="p-5 lg:p-6">
    <div class="flex items-start justify-between gap-3">
      <div>
        <h2 class="text-base font-bold text-slate-950">Tài chính & Đăng ký</h2>
        <p class="text-[11px] font-bold text-slate-500 uppercase tracking-wider">Học phí & Học phần</p>
      </div>
      <CreditCard :size="18" class="text-amber-700" />
    </div>

    <div class="mt-5 rounded-[24px] border border-amber-200/50 bg-amber-50/60 p-4 shadow-sm backdrop-blur-md">
      <div class="flex items-center justify-between gap-2">
        <p class="text-[10px] font-bold uppercase tracking-wider text-slate-800 opacity-80">Học phí còn nợ</p>
        <GlassBadge variant="warning" size="sm">{{ tuition.status }}</GlassBadge>
      </div>
      <p class="mt-1 text-2xl font-bold tracking-tight text-amber-700">{{ tuition.remaining }}</p>
      
      <div class="mt-4 grid grid-cols-2 gap-2.5">
        <div class="rounded-xl bg-white/70 p-2.5 shadow-sm border border-white/60">
          <p class="text-[9px] font-bold uppercase tracking-widest text-slate-400">Tổng HP</p>
          <p class="mt-0.5 text-xs font-bold text-slate-700">{{ tuition.total }}</p>
        </div>
        <div class="rounded-xl bg-white/70 p-2.5 shadow-sm border border-white/60">
          <p class="text-[9px] font-bold uppercase tracking-widest text-slate-400">Đã nộp</p>
          <p class="mt-0.5 text-xs font-bold text-emerald-600">{{ tuition.paid }}</p>
        </div>
      </div>

      <div class="mt-4 flex items-center justify-between gap-3 pt-1">
        <p class="text-[10px] font-bold text-amber-800 opacity-70">Hạn: {{ tuition.dueDate }}</p>
        <router-link to="/student/tuition" class="lg-button-primary h-8 px-4 text-[10px] font-bold uppercase tracking-wide shadow-md">
          Thanh toán
        </router-link>
      </div>
    </div>

    <div class="lg-list-item mt-4 flex items-center p-3 shadow-sm border border-white/50">
      <div class="flex w-full items-center gap-3">
        <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-violet-50 text-violet-700 shadow-sm">
          <ListPlus :size="17" />
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between gap-2">
            <h3 class="truncate text-[13px] font-bold text-slate-950 leading-tight">{{ registration.title }}</h3>
            <GlassBadge variant="violet" size="sm">{{ registration.status }}</GlassBadge>
          </div>
          <p class="mt-0.5 text-[10px] font-bold text-slate-400 uppercase tracking-wide">
            {{ registration.registered }} lớp · {{ registration.closesIn }}
          </p>
        </div>
      </div>
    </div>
  </GlassPanel>
</template>
