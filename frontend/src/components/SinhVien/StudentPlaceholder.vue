<script setup>
import { computed } from 'vue'
import * as LucideIcons from 'lucide-vue-next'
import LmsBadge from '@/components/LmsBadge.vue'
import LmsCard from '@/components/LmsCard.vue'

const props = defineProps({
  icon: {
    type: String,
    default: 'Sparkles',
  },
  title: {
    type: String,
    required: true,
  },
  subtitle: {
    type: String,
    default: '',
  },
  status: {
    type: String,
    default: 'dự kiến',
  },
  items: {
    type: Array,
    default: () => [],
  },
})

const Icon = computed(() => LucideIcons[props.icon] || LucideIcons.Sparkles)
</script>

<template>
  <div class="space-y-5">
    <LmsCard variant="glass" class="relative overflow-hidden">
      <div class="pointer-events-none absolute -right-20 -top-24 h-52 w-52 rounded-full bg-cyan-300/30 blur-3xl" />
      <div class="pointer-events-none absolute -bottom-28 left-1/4 h-56 w-56 rounded-full bg-violet-300/20 blur-3xl" />

      <div class="relative flex flex-col gap-5 sm:flex-row sm:items-center sm:justify-between">
        <div class="flex items-start gap-4">
          <div class="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-blue-900 text-white shadow-lg shadow-blue-900/20">
            <component :is="Icon" :size="26" :stroke-width="2.1" />
          </div>
          <div>
            <LmsBadge variant="info" size="sm">{{ status }}</LmsBadge>
            <h2 class="mt-3 text-xl font-bold leading-tight text-slate-950">{{ title }}</h2>
            <p class="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              {{ subtitle || 'Module này đã có vị trí trong frontend và sẽ được nối API khi backend hoàn thiện controller tương ứng.' }}
            </p>
          </div>
        </div>

        <router-link
          to="/student/dashboard"
          class="inline-flex items-center justify-center rounded-xl border border-white/60 bg-white/70 px-5 py-3 text-sm font-semibold text-slate-800 shadow-sm backdrop-blur-xl transition hover:bg-white/90 hover:shadow-md focus:outline-none focus:ring-4 focus:ring-blue-500/20"
        >
          Về Dashboard
        </router-link>
      </div>
    </LmsCard>

    <div class="grid gap-4 md:grid-cols-3">
      <LmsCard
        v-for="item in items"
        :key="item.title"
        variant="solid"
        class="min-h-[132px]"
      >
        <p class="text-sm font-semibold text-slate-900">{{ item.title }}</p>
        <p class="mt-2 text-sm leading-6 text-slate-600">{{ item.description }}</p>
      </LmsCard>
    </div>
  </div>
</template>
