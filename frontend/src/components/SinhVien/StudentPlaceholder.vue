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
  <div class="space-y-4">
    <LmsCard variant="glass-soft" class="relative">
      <div class="pointer-events-none absolute -right-16 -top-24 h-44 w-44 rounded-full bg-(--page-gradient-a) blur-3xl" />

      <div class="relative flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div class="flex items-start gap-4">
          <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-(--accent-primary-soft) text-(--text-link) shadow-(--lg-shadow-sm)">
            <component :is="Icon" :size="23" :stroke-width="2.1" />
          </div>
          <div>
            <LmsBadge variant="info" size="sm">{{ status }}</LmsBadge>
            <h2 class="mt-2 text-xl font-bold leading-tight text-heading">{{ title }}</h2>
            <p class="mt-2 max-w-2xl text-sm leading-6 text-body">
              {{ subtitle || 'Module này đã có vị trí trong frontend và sẽ được nối API khi backend hoàn thiện controller tương ứng.' }}
            </p>
          </div>
        </div>

        <router-link
          to="/student/dashboard"
          class="inline-flex items-center justify-center rounded-xl border border-card bg-(--surface-card) px-5 py-3 text-sm font-semibold text-heading shadow-sm backdrop-blur-xl transition hover:bg-(--glass-bg-strong) hover:shadow-md focus:outline-none focus:ring-4 focus:ring-(--text-link)/20"
        >
          Về Dashboard
        </router-link>
      </div>
    </LmsCard>

    <div class="grid gap-3 md:grid-cols-3">
      <LmsCard
        v-for="item in items"
        :key="item.title"
        variant="solid"
        class="min-h-[116px]"
      >
        <p class="text-sm font-semibold text-heading">{{ item.title }}</p>
        <p class="mt-2 text-sm leading-6 text-body">{{ item.description }}</p>
      </LmsCard>
    </div>
  </div>
</template>
