<script setup>
import { computed } from 'vue'
import * as LucideIcons from 'lucide-vue-next'
import LmsBadge from '@/components/LmsBadge.vue'
import LmsCard from '@/components/LmsCard.vue'
import EmptyState from '@/components/ui/EmptyState.vue'

const props = defineProps({
  icon: {
    type: String,
    default: 'LayoutDashboard',
  },
  eyebrow: {
    type: String,
    default: 'Student module',
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
  metrics: {
    type: Array,
    default: () => [],
  },
  primaryTitle: {
    type: String,
    default: 'Nội dung chính',
  },
  primaryDescription: {
    type: String,
    default: '',
  },
  rows: {
    type: Array,
    default: () => [],
  },
  timelineTitle: {
    type: String,
    default: 'Việc cần chú ý',
  },
  timeline: {
    type: Array,
    default: () => [],
  },
  actions: {
    type: Array,
    default: () => [],
  },
  note: {
    type: String,
    default: 'Dữ liệu trên màn này là dữ liệu FE demo/dự kiến. Chỉ nối API khi backend có controller và contract tương ứng.',
  },
})

const HeroIcon = computed(() => LucideIcons[props.icon] || LucideIcons.LayoutDashboard)

function resolveIcon(name, fallback = 'Circle') {
  return LucideIcons[name] || LucideIcons[fallback] || LucideIcons.Circle
}

const badgeVariantByTone = {
  blue: 'info',
  cyan: 'info',
  teal: 'success',
  green: 'success',
  amber: 'warning',
  orange: 'warning',
  red: 'error',
  violet: 'primary',
  slate: 'primary',
}

function badgeVariant(tone) {
  return badgeVariantByTone[tone] || 'info'
}

function toneClass(tone, part) {
  if (part === 'icon') return 'bg-(--accent-primary-soft) text-(--text-link) shadow-(--lg-shadow-sm)'
  if (part === 'bar') return 'bg-(--text-link)'
  if (part === 'dot') return 'bg-(--text-link)'
  return ''
}
</script>

<template>
  <div class="lg-page-enter space-y-4">
    <LmsCard variant="glass-soft" class="relative">
      <div class="pointer-events-none absolute -right-16 -top-24 h-48 w-48 rounded-full bg-(--page-gradient-a) blur-3xl" />

      <div class="relative grid gap-4 lg:grid-cols-[1fr_auto] lg:items-center">
        <div class="flex min-w-0 gap-4">
          <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-(--accent-primary-soft) text-(--text-link) shadow-(--lg-shadow-sm)">
            <component :is="HeroIcon" :size="24" :stroke-width="2.15" />
          </div>
          <div class="min-w-0">
            <div class="flex flex-wrap items-center gap-2">
              <span class="text-xs font-semibold text-(--color-success-text)">{{ eyebrow }}</span>
              <LmsBadge variant="info" size="sm">{{ status }}</LmsBadge>
            </div>
            <h2 class="mt-2 text-2xl font-bold leading-tight text-slate-950">
              {{ title }}
            </h2>
            <p class="mt-2 max-w-3xl text-sm leading-6 text-slate-600">
              {{ subtitle }}
            </p>
          </div>
        </div>

        <div v-if="actions.length" class="flex flex-wrap gap-2 lg:justify-end">
          <router-link
            v-for="action in actions"
            :key="action.label"
            :to="action.to || '#'"
            :class="[
              'inline-flex items-center justify-center rounded-2xl px-4 py-2.5 text-sm font-semibold transition-all duration-200 ease-out active:scale-[0.98] focus:outline-none focus:ring-4',
              action.primary
                ? 'lg-button-primary focus:ring-(--text-link)/25'
                : 'lg-button-secondary focus:ring-(--text-link)/20',
            ]"
          >
            {{ action.label }}
          </router-link>
        </div>
      </div>
    </LmsCard>

    <div v-if="metrics.length" class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <LmsCard
        v-for="metric in metrics"
        :key="metric.label"
        variant="glass-soft"
        interactive
        class="min-h-[116px]"
      >
        <div class="flex items-start justify-between gap-3">
          <div>
            <p class="text-xs font-semibold text-slate-500">{{ metric.label }}</p>
            <div class="mt-2 flex items-baseline gap-1">
              <span class="text-2xl font-bold text-slate-950">{{ metric.value }}</span>
              <span v-if="metric.unit" class="text-xs font-semibold text-slate-500">{{ metric.unit }}</span>
            </div>
          </div>
          <div :class="['flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl ring-1', toneClass(metric.tone, 'icon')]">
            <component :is="resolveIcon(metric.icon || 'Activity')" :size="19" />
          </div>
        </div>
        <div v-if="metric.progress !== undefined" class="mt-4">
          <div class="h-2 overflow-hidden rounded-full bg-(--surface-input)">
            <div
              :class="['h-full rounded-full transition-all duration-700', toneClass(metric.tone, 'bar')]"
              :style="{ width: `${metric.progress}%` }"
            />
          </div>
        </div>
        <p v-if="metric.hint" class="mt-3 text-xs leading-5 text-slate-500">{{ metric.hint }}</p>
      </LmsCard>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_360px]">
      <LmsCard variant="solid" padding="0" class="overflow-hidden">
        <div class="border-b border-white/55 bg-white/45 px-5 py-4">
          <h3 class="text-base font-bold text-slate-950">{{ primaryTitle }}</h3>
          <p v-if="primaryDescription" class="mt-1 text-sm leading-6 text-slate-600">
            {{ primaryDescription }}
          </p>
        </div>

        <div v-if="rows.length" class="divide-y divide-slate-100">
          <div
            v-for="row in rows"
            :key="row.title"
            @click="row.to ? $router.push(row.to) : null"
            :class="[
              'grid gap-3 px-5 py-4 transition md:grid-cols-[minmax(0,1fr)_auto] md:items-center',
              row.to ? 'cursor-pointer hover:bg-white/80 active:bg-white/60' : 'hover:bg-white/60'
            ]"
          >
            <div class="flex min-w-0 gap-3">
              <div :class="['mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl ring-1', toneClass(row.tone, 'icon')]">
                <component :is="resolveIcon(row.icon || 'FileText')" :size="18" />
              </div>
              <div class="min-w-0">
                <div class="flex flex-wrap items-center gap-2">
                  <h4 class="text-sm font-bold text-slate-900">{{ row.title }}</h4>
                  <LmsBadge v-if="row.badge" :variant="badgeVariant(row.tone)" size="sm">
                    {{ row.badge }}
                  </LmsBadge>
                </div>
                <p v-if="row.description" class="mt-1 text-sm leading-6 text-slate-600">
                  {{ row.description }}
                </p>
                <div v-if="row.meta?.length" class="mt-2 flex flex-wrap gap-2">
                  <span
                    v-for="meta in row.meta"
                    :key="meta"
                    class="rounded-full border border-white/60 bg-white/60 px-2.5 py-1 text-xs font-medium text-slate-500 backdrop-blur-md"
                  >
                    {{ meta }}
                  </span>
                </div>
              </div>
            </div>

            <div v-if="row.value || row.to" class="flex items-center justify-between gap-3 md:justify-end">
              <div v-if="row.value" class="text-left md:text-right">
                <p class="text-sm font-bold text-slate-950">{{ row.value }}</p>
                <p v-if="row.valueHint" class="text-xs text-slate-500">{{ row.valueHint }}</p>
              </div>
              <span
                v-if="row.to"
                class="lg-button-secondary inline-flex h-9 items-center px-3 text-xs font-semibold"
              >
                Mở
              </span>
            </div>
          </div>
        </div>

        <EmptyState
          v-else
          class="m-5"
          title="Chưa có dữ liệu"
          description="Module đang chờ API hoặc dữ liệu học vụ thật. UI đã sẵn trạng thái empty để nối store sau."
        />
      </LmsCard>

      <div class="space-y-4">
        <LmsCard variant="glass" padding="0" class="overflow-hidden">
          <div class="border-b border-white/50 px-5 py-4">
            <h3 class="text-base font-bold text-slate-950">{{ timelineTitle }}</h3>
          </div>
          <div v-if="timeline.length" class="space-y-0 px-5 py-3">
            <div
              v-for="item in timeline"
              :key="item.title"
              class="relative border-l border-white/60 py-3 pl-5 first:pt-1 last:pb-1"
            >
              <span :class="['absolute -left-[5px] top-5 h-2.5 w-2.5 rounded-full ring-4 ring-white', toneClass(item.tone, 'dot')]" />
              <p class="text-sm font-bold text-slate-900">{{ item.title }}</p>
              <p v-if="item.description" class="mt-1 text-sm leading-6 text-slate-600">
                {{ item.description }}
              </p>
              <p v-if="item.time" class="mt-1 text-xs font-semibold text-slate-400">{{ item.time }}</p>
            </div>
          </div>
          <div v-else class="px-5 py-8 text-sm text-slate-500">
            Chưa có mốc cần chú ý.
          </div>
        </LmsCard>

        <LmsCard variant="glass-soft">
          <div class="flex gap-3">
            <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-2xl bg-(--accent-primary-soft) text-(--text-link)">
              <component :is="resolveIcon('Info')" :size="18" />
            </div>
            <div>
              <p class="text-sm font-bold text-slate-900">Ghi chú triển khai</p>
              <p class="mt-1 text-sm leading-6 text-slate-600">{{ note }}</p>
            </div>
          </div>
        </LmsCard>
      </div>
    </div>
  </div>
</template>
