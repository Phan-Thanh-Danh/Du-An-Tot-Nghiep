<script setup>
import { computed } from 'vue'
import { Eye, EyeOff, TrendingUp, TrendingDown, ArrowRight } from 'lucide-vue-next'
import GlassPanel from './GlassPanel.vue'
import GlassButton from './GlassButton.vue'

const props = defineProps({
  balance: {
    type: Number,
    default: 0,
  },
  currency: {
    type: String,
    default: 'VNĐ',
  },
  label: {
    type: String,
    default: 'Tổng số dư',
  },
  trend: {
    type: Number,
    default: 0,
  },
  trendLabel: {
    type: String,
    default: 'so với tháng trước',
  },
  accountNumber: {
    type: String,
    default: '',
  },
  hidden: {
    type: Boolean,
    default: false,
  },
  accent: {
    type: String,
    default: 'blue',
    validator: (value) => ['blue', 'violet', 'teal', 'amber'].includes(value),
  },
})

const emit = defineEmits(['toggle-visibility', 'action'])

const formattedBalance = computed(() => {
  if (props.hidden) return '•••••••'
  return new Intl.NumberFormat('vi-VN').format(props.balance)
})

const trendIcon = computed(() => (props.trend >= 0 ? TrendingUp : TrendingDown))
const trendColor = computed(() =>
  props.trend >= 0
    ? 'text-emerald-600 dark:text-emerald-400'
    : 'text-red-600 dark:text-red-400',
)

const glowAccent = computed(() => `shadow-[0_0_40px_rgba(37,99,235,0.12)] dark:shadow-[0_0_60px_rgba(37,99,235,0.2)]`)
</script>

<template>
  <GlassPanel
    glow
    density="spacious"
    class="relative overflow-hidden"
    :class="glowAccent"
  >
    <div class="pointer-events-none absolute -right-16 -top-16 h-40 w-40 rounded-full bg-blue-500/10 dark:bg-blue-500/15 blur-[64px]" />
    <div class="pointer-events-none absolute -left-8 bottom-0 h-24 w-24 rounded-full bg-cyan-400/10 dark:bg-cyan-400/15 blur-[48px]" />

    <div class="relative z-10 space-y-5">
      <div class="flex items-center justify-between">
        <div>
          <p class="text-xs font-semibold tracking-wide text-slate-500 dark:text-slate-400 uppercase">
            {{ label }}
          </p>
        </div>
        <button
          class="flex h-8 w-8 items-center justify-center rounded-xl bg-white/60 dark:bg-white/5 border border-white/50 dark:border-white/10 backdrop-blur-xl text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 transition-all hover:scale-105"
          @click="emit('toggle-visibility')"
        >
          <Eye v-if="!hidden" :size="15" />
          <EyeOff v-else :size="15" />
        </button>
      </div>

      <div class="space-y-1">
        <p class="text-4xl font-black tracking-tight text-slate-900 dark:text-white">
          <span v-if="!hidden" class="text-2xl font-semibold text-slate-400 dark:text-slate-500 align-top mr-1">₫</span>
          <span class="lg-text-gradient">{{ formattedBalance }}</span>
        </p>
        <p v-if="accountNumber && !hidden" class="text-xs font-medium text-slate-400 dark:text-slate-500 tracking-wider">
          {{ accountNumber }}
        </p>
      </div>

      <div v-if="trend !== 0" class="flex items-center gap-2">
        <div class="flex items-center gap-1 rounded-full bg-white/60 dark:bg-white/5 border border-white/50 dark:border-white/10 backdrop-blur-xl px-2.5 py-1">
          <component :is="trendIcon" :size="14" :class="trendColor" />
          <span :class="['text-xs font-bold', trendColor]">
            {{ trend >= 0 ? '+' : '' }}{{ trend }}%
          </span>
        </div>
        <span class="text-xs font-medium text-slate-400 dark:text-slate-500">{{ trendLabel }}</span>
      </div>

      <div class="pt-2">
        <GlassButton variant="primary" size="sm" glow @click="emit('action')">
          Quản lý số dư
          <ArrowRight :size="15" />
        </GlassButton>
      </div>
    </div>
  </GlassPanel>
</template>
