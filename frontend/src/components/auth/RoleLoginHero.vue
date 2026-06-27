<script setup>
import { computed } from 'vue'
import {
  AlertTriangle,
  Award,
  BarChart,
  Bell,
  BookOpenCheck,
  Calendar,
  CheckCheck,
  ClipboardList,
  FileSearch,
  FileText,
  GraduationCap,
  HeartHandshake,
  Landmark,
  Presentation,
  ShieldCheck,
  Users,
  Wallet,
} from 'lucide-vue-next'

const props = defineProps({
  portal: { type: Object, required: true },
})

const iconMap = {
  GraduationCap,
  Presentation,
  HeartHandshake,
  ClipboardList,
  Landmark,
  BookOpenCheck,
  ShieldCheck,
}

const featureIconMap = {
  AlertTriangle,
  Award,
  BarChart,
  Bell,
  Calendar,
  CheckCheck,
  ClipboardList,
  FileSearch,
  FileText,
  GraduationCap,
  HeartHandshake,
  Landmark,
  Presentation,
  ShieldCheck,
  Users,
  Wallet,
}

const resolvedIcon = computed(() => iconMap[props.portal.icon] || GraduationCap)

const featureItems = computed(() => props.portal.featureDetails || props.portal.features.map((f) => ({ label: f, icon: 'ShieldCheck' })))

function resolveFeatureIcon(iconName) {
  return featureIconMap[iconName] || ShieldCheck
}

const accentColors = {
  blue: { primary: '#00236f', light: 'rgba(0, 35, 111, 0.08)', badge: 'rgba(0, 35, 111, 0.1)', dot: '#2563eb' },
  indigo: { primary: '#3730a3', light: 'rgba(55, 48, 163, 0.08)', badge: 'rgba(55, 48, 163, 0.1)', dot: '#4f46e5' },
  cyan: { primary: '#155e75', light: 'rgba(21, 94, 117, 0.08)', badge: 'rgba(21, 94, 117, 0.1)', dot: '#0891b2' },
  teal: { primary: '#115e59', light: 'rgba(17, 94, 89, 0.08)', badge: 'rgba(17, 94, 89, 0.1)', dot: '#0d9488' },
  navy: { primary: '#1e3a8a', light: 'rgba(30, 58, 138, 0.08)', badge: 'rgba(30, 58, 138, 0.1)', dot: '#1e40af' },
  violet: { primary: '#4c1d95', light: 'rgba(76, 29, 149, 0.08)', badge: 'rgba(76, 29, 149, 0.1)', dot: '#6d28d9' },
  slate: { primary: '#334155', light: 'rgba(51, 65, 85, 0.08)', badge: 'rgba(51, 65, 85, 0.1)', dot: '#475569' },
}

const colors = computed(() => accentColors[props.portal.accent] || accentColors.blue)
</script>

<template>
  <div
    class="hidden lg:flex flex-col w-1/2 p-8 relative border-r"
    :style="{
      background: `linear-gradient(135deg, ${colors.light} 0%, transparent 100%)`,
      borderColor: colors.light,
    }"
  >
    <div
      class="absolute inset-0 pointer-events-none opacity-[0.04]"
      :style="{
        backgroundImage: `radial-gradient(${colors.primary} 1px, transparent 1px)`,
        backgroundSize: '16px 16px',
        maskImage: 'linear-gradient(to bottom right, rgba(0,0,0,0.75), transparent 78%)',
        WebkitMaskImage: 'linear-gradient(to bottom right, rgba(0,0,0,0.75), transparent 78%)',
      }"
    ></div>

    <div class="relative z-10 space-y-2">
      <div class="flex items-center gap-2" :style="{ color: colors.primary }">
        <GraduationCap class="w-8 h-8" aria-hidden="true" />
        <div class="flex flex-col">
          <span class="font-bold text-[24px] leading-tight">EduLMS</span>
          <span class="text-[13px] font-medium text-[#444651]">Cơ sở Đồng Nai</span>
        </div>
      </div>
      <div
        class="inline-block px-3 py-1 rounded-full font-semibold text-[13px] mt-4"
        :style="{
          background: colors.badge,
          color: colors.primary,
        }"
      >
        {{ portal.eyebrow }}
      </div>
    </div>

    <div class="relative z-10 mt-8 mb-4 space-y-0 flex flex-col">
      <div class="space-y-[10px]">
        <h1
          class="tracking-tight leading-tight font-bold"
          :style="{ color: colors.primary, fontSize: '38px', lineHeight: '1.1' }"
        >
          {{ portal.headline }}
        </h1>
        <p class="text-lg text-[#444651] max-w-md" style="font-size: 18px; line-height: 28px; font-weight: 400;">
          {{ portal.description }}
        </p>
      </div>

      <div class="grid grid-cols-3 gap-4 mt-6">
        <div
          v-for="feature in featureItems"
          :key="feature.label"
          class="bg-white/60 backdrop-blur-md rounded-xl p-4 border border-white/50 shadow-sm flex flex-col items-start gap-1"
          :class="{ 'pb-3': feature.description }"
        >
          <div
            class="w-[36px] h-[36px] rounded-lg flex items-center justify-center mb-1"
            :style="{ background: colors.light, color: colors.primary }"
          >
            <component :is="resolveFeatureIcon(feature.icon)" class="w-[18px] h-[18px]" aria-hidden="true" />
          </div>
          <span class="font-semibold text-[13px] text-[#191c1e]">{{ feature.label }}</span>
          <span
            v-if="feature.description"
            class="text-[12px] text-[#444651] font-medium leading-[1.45]"
          >{{ feature.description }}</span>
        </div>
      </div>

      <div class="mt-6 flex items-center gap-2">
        <div
          class="flex-shrink-0 p-1 rounded-full"
          :style="{ background: colors.light }"
        >
          <div
            class="w-2 h-2 rounded-full"
            :style="{ background: colors.dot }"
          ></div>
        </div>
        <span class="text-[13px] font-medium text-[#191c1e]">
          {{ portal.audience }}
        </span>
      </div>
    </div>

    <div
      class="relative z-10 flex items-center gap-2 mt-auto text-[#191c1e]"
    >
      <component :is="resolvedIcon" class="w-5 h-5" :style="{ color: colors.primary }" aria-hidden="true" />
      <span class="text-[13px] font-medium">{{ portal.label }}</span>
    </div>
  </div>
</template>
