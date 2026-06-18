<script setup>
import { ArrowRight, Award, Code, Database, BookOpen, Calculator } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  grades: {
    type: Array,
    default: () => [],
  },
})

// Tính xếp loại chữ từ điểm số hệ 10
function getLetterGrade(scoreStr) {
  const score = Number.parseFloat(scoreStr)
  if (isNaN(score)) return ''
  if (score >= 9.0) return 'A+'
  if (score >= 8.0) return 'A'
  if (score >= 7.0) return 'B'
  if (score >= 6.0) return 'C'
  return 'D'
}

// Lấy icon tương ứng với môn học
function getIconComponent(courseName) {
  const name = courseName.toLowerCase()
  if (name.includes('web')) return Code
  if (name.includes('dữ liệu') || name.includes('csdl') || name.includes('hệ quản trị')) return Database
  if (name.includes('toán') || name.includes('math')) return Calculator
  return BookOpen
}

// Lấy stroke style dùng CSS variables từ theme
function getStrokeStyle(scoreStr) {
  const score = Number.parseFloat(scoreStr)
  if (isNaN(score)) return { stroke: 'var(--text-placeholder)' }
  if (score >= 8.0) return { stroke: 'var(--color-success-text)' }
  if (score >= 6.5) return { stroke: 'var(--color-warning-text)' }
  return { stroke: 'var(--color-danger-text)' }
}

// Lấy màu text dùng CSS variables từ theme
function getTextStyle(scoreStr) {
  const score = Number.parseFloat(scoreStr)
  if (isNaN(score)) return { color: 'var(--text-placeholder)' }
  if (score >= 8.0) return { color: 'var(--color-success-text)' }
  if (score >= 6.5) return { color: 'var(--color-warning-text)' }
  return { color: 'var(--color-danger-text)' }
}
</script>

<template>
  <GlassPanel variant="strong" density="none" class="rounded-2xl border border-card overflow-hidden shadow-xl hover:shadow-2xl transition-all duration-300">
    <!-- Header -->
    <div class="flex items-center justify-between gap-3 border-b border-card px-4 py-3.5">
      <div class="flex items-center gap-2.5">
        <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-[color-mix(in_srgb,var(--accent-violet)_10%,transparent)] text-[var(--accent-violet)] shadow-sm">
          <Award :size="18" class="animate-pulse" />
        </div>
        <div>
          <h2 class="text-sm font-bold text-heading">Điểm gần đây</h2>
          <div class="flex items-center gap-1.5 mt-0.5">
            <span class="relative flex h-1.5 w-1.5">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
              <span class="relative inline-flex rounded-full h-1.5 w-1.5 bg-emerald-500"></span>
            </span>
            <p class="text-[11px] font-semibold text-body">Hệ thống vừa công bố</p>
          </div>
        </div>
      </div>
      <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-[color-mix(in_srgb,var(--accent-violet)_10%,transparent)] text-[var(--accent-violet)] border border-[color-mix(in_srgb,var(--accent-violet)_20%,transparent)]">
        Học kỳ mới
      </span>
    </div>

    <!-- Grades List -->
    <div class="space-y-2.5 p-4">
      <router-link
        v-for="grade in grades"
        :key="grade.id"
        to="/student/grades"
        class="group flex items-center justify-between gap-4 p-3 rounded-xl border border-default hover:border-primary/30 bg-[var(--surface-card)] hover:bg-[color-mix(in_srgb,var(--sidebar-accent)_4%,var(--surface-card))] transition-all duration-300 shadow-sm hover:shadow-md"
      >
        <!-- Left: Icon & Info -->
        <div class="flex items-center gap-3 min-w-0">
          <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-[var(--surface-input)] border border-default text-label group-hover:text-primary group-hover:border-primary/20 transition-all duration-300">
            <component :is="getIconComponent(grade.course)" class="h-4.5 w-4.5 transition-transform duration-300 group-hover:scale-110" />
          </div>
          
          <div class="min-w-0">
            <h3 class="truncate text-[13.5px] font-bold text-heading group-hover:text-primary transition-colors leading-tight">
              {{ grade.course }}
            </h3>
            <div class="flex items-center gap-2 mt-1">
              <span class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold bg-[var(--surface-input)] text-body border border-default">
                {{ grade.type }}
              </span>
              <span v-if="grade.note" class="truncate text-[11px] text-placeholder italic">
                • {{ grade.note }}
              </span>
            </div>
          </div>
        </div>

        <!-- Right: Progress Ring -->
        <div class="relative flex items-center justify-center h-11 w-11 flex-shrink-0 bg-[var(--surface-input)] rounded-full p-0.5 border border-default shadow-inner">
          <svg class="absolute inset-0 h-full w-full -rotate-90 transform" viewBox="0 0 36 36">
            <!-- Background circle -->
            <circle
              cx="18"
              cy="18"
              r="15.5"
              fill="none"
              stroke="currentColor"
              class="text-[var(--border-default)] opacity-40"
              stroke-width="2.5"
            />
            <!-- Progress circle -->
            <circle
              cx="18"
              cy="18"
              r="15.5"
              fill="none"
              stroke-width="2.5"
              stroke-linecap="round"
              stroke-dasharray="97.39"
              :stroke-dashoffset="97.39 - (Number(grade.score) / 10) * 97.39"
              :style="getStrokeStyle(grade.score)"
              class="transition-all duration-700 ease-out"
            />
          </svg>
          
          <!-- Score & Grade in center -->
          <div class="flex flex-col items-center justify-center z-10">
            <span class="text-[12px] font-black text-heading tracking-tight mt-[0.5px]">
              {{ grade.score }}
            </span>
            <span :style="getTextStyle(grade.score)" class="text-[8px] font-black uppercase leading-none mt-0.5 tracking-wider">
              {{ getLetterGrade(grade.score) }}
            </span>
          </div>
        </div>
      </router-link>
    </div>

    <!-- Footer -->
    <div class="border-t border-card px-4 py-3 bg-[var(--surface-card)]">
      <router-link
        to="/student/grades"
        class="flex items-center justify-center gap-2 rounded-xl border border-default bg-[var(--surface-input)] hover:bg-[var(--border-default)] px-4 py-2.5 text-xs font-bold text-heading transition-all duration-300 hover:gap-3"
      >
        <span>Xem toàn bộ bảng điểm chi tiết</span>
        <ArrowRight :size="13" class="text-link" />
      </router-link>
    </div>
  </GlassPanel>
</template>

