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
  <GlassPanel variant="strong" density="none" class="rounded-2xl lg-glass-card-hover">
    <div class="p-4 lg:p-5">
    <div class="flex items-start justify-between gap-3">
      <div>
        <h2 class="text-base font-semibold text-heading">Tài chính & Đăng ký</h2>
        <p class="text-xs font-medium text-body">Học phí & Học phần</p>
      </div>
      <CreditCard :size="18" class="text-(--color-warning-text)" />
    </div>

    <div class="mt-5 rounded-2xl border border-[color-mix(in srgb,var(--color-warning-text) 20%,transparent)] bg-(--color-warning-bg) p-4 shadow-sm backdrop-blur-md">
      <div class="flex items-center justify-between gap-2">
        <p class="text-xs font-semibold text-heading opacity-80">Học phí còn nợ</p>
        <GlassBadge variant="warning" size="sm">{{ tuition.status }}</GlassBadge>
      </div>
      <p class="mt-1 text-2xl font-semibold tracking-tight text-(--color-warning-text)">{{ tuition.remaining }}</p>
      
      <div class="mt-4 grid grid-cols-2 gap-2.5">
        <div class="rounded-xl bg-(--surface-card) p-2.5 shadow-sm border border-card">
          <p class="text-xs font-medium text-body">Tổng HP</p>
          <p class="mt-0.5 text-xs font-bold text-heading">{{ tuition.total }}</p>
        </div>
        <div class="rounded-xl bg-(--surface-card) p-2.5 shadow-sm border border-card">
          <p class="text-xs font-medium text-body">Đã nộp</p>
          <p class="mt-0.5 text-xs font-bold text-(--color-success-text)">{{ tuition.paid }}</p>
        </div>
      </div>

      <div class="mt-4 flex items-center justify-between gap-3 pt-1">
        <p class="text-xs font-semibold text-(--color-warning-text) opacity-70">Hạn: {{ tuition.dueDate }}</p>
        <router-link to="/student/tuition" class="lg-button-primary h-9 rounded-xl px-4 text-xs font-semibold shadow-md">
          Thanh toán
        </router-link>
      </div>
    </div>

    <router-link to="/student/registrations" class="lg-list-item mt-4 flex items-center p-3 shadow-sm border border-card">
      <div class="flex w-full items-center gap-3">
        <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-(--accent-violet-soft) text-(--accent-violet) shadow-sm">
          <ListPlus :size="17" />
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between gap-2">
            <h3 class="truncate text-[13px] font-semibold text-heading leading-tight">{{ registration.title }}</h3>
            <GlassBadge variant="violet" size="sm">{{ registration.status }}</GlassBadge>
          </div>
          <p class="mt-0.5 text-xs font-medium text-body">
            {{ registration.registered }} lớp · {{ registration.closesIn }}
          </p>
        </div>
      </div>
    </router-link>
    </div>
  </GlassPanel>
</template>
