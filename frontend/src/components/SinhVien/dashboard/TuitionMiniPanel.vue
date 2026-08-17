<script setup>
import { computed } from 'vue'
import { CreditCard, ListPlus } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const props = defineProps({
  tuition: {
    type: Object,
    default: () => ({}),
  },
  registration: {
    type: Object,
    default: () => ({}),
  },
})

const remaining = computed(() => props.tuition?.remaining || props.tuition?.totalDue || '0 đ')
const total = computed(() => props.tuition?.total || props.tuition?.totalDue || '0 đ')
const paid = computed(() => props.tuition?.paid || '0 đ')
const dueDate = computed(() => props.tuition?.dueDate || props.tuition?.deadline || 'Không có')
const tuitionStatus = computed(() => props.tuition?.status || 'Đã hoàn thành')
const tuitionVariant = computed(() => props.tuition?.statusVariant || 'success')

const regTitle = computed(() => props.registration?.title || `Đăng ký môn học ${props.registration?.semester || ''}`)
const regStatus = computed(() => props.registration?.status || 'Đang mở')
const regRegistered = computed(() => props.registration?.registered ?? '0')
const regClosesIn = computed(() => props.registration?.closesIn || 'Đã xếp lịch')
</script>

<template>
  <GlassPanel variant="strong" density="none" class="rounded-2xl lg-glass-card-hover h-full flex flex-col justify-between">
    <div class="p-4 lg:p-5 flex-1 flex flex-col justify-between">
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
          <GlassBadge :variant="tuitionVariant" size="sm">{{ tuitionStatus }}</GlassBadge>
        </div>
        <p class="mt-1 text-2xl font-semibold tracking-tight text-(--color-warning-text)">{{ remaining }}</p>
        
        <div class="mt-4 grid grid-cols-2 gap-2.5">
          <div class="rounded-xl bg-(--surface-card) p-2.5 shadow-sm border border-card">
            <p class="text-xs font-medium text-body">Tổng HP</p>
            <p class="mt-0.5 text-xs font-bold text-heading">{{ total }}</p>
          </div>
          <div class="rounded-xl bg-(--surface-card) p-2.5 shadow-sm border border-card">
            <p class="text-xs font-medium text-body">Đã nộp</p>
            <p class="mt-0.5 text-xs font-bold text-(--color-success-text)">{{ paid }}</p>
          </div>
        </div>

        <div class="mt-4 flex items-center justify-between gap-3 pt-1">
          <p class="text-xs font-semibold text-(--color-warning-text) opacity-70">Hạn: {{ dueDate }}</p>
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
              <h3 class="truncate text-[13px] font-semibold text-heading leading-tight">{{ regTitle }}</h3>
              <GlassBadge variant="violet" size="sm">{{ regStatus }}</GlassBadge>
            </div>
            <p class="mt-0.5 text-xs font-medium text-body">
              {{ regRegistered }} lớp · {{ regClosesIn }}
            </p>
          </div>
        </div>
      </router-link>
    </div>
  </GlassPanel>
</template>
