<script setup>
import { Activity, Filter, ArrowRight } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
</script>
<template>
  <div class="h-full flex flex-col space-y-4">
    <div class="flex items-start justify-between">
      <div>
        <div class="flex items-center gap-2">
          <Activity class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Thay đổi & Dạy bù</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Giám sát các biến động bất thường so với thời khóa biểu gốc.</p>
      </div>
      <GlassButton variant="secondary"><Filter :size="15" class="mr-1" /> Bộ lọc</GlassButton>
    </div>

    <div class="flex-1 surface-card border border-(--border-card) rounded-2xl overflow-auto p-4 flex flex-col gap-3">
       <div v-for="i in 3" :key="i" class="border border-(--border-default) rounded-xl p-4 flex items-center justify-between group hover:border-(--lg-primary) transition-colors bg-(--surface-card)">
          <div class="flex items-center gap-4">
             <div class="w-10 h-10 rounded-full bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 flex items-center justify-center">
                <Activity :size="20"/>
             </div>
             <div>
                <div class="flex items-center gap-2">
                   <h3 class="font-bold text-(--text-heading)">Lập trình Web (SE104.N11)</h3>
                   <GlassBadge variant="info" size="xs">Dạy bù</GlassBadge>
                </div>
                <div class="text-sm text-(--text-body) mt-1 flex items-center gap-2">
                   <span class="line-through text-(--text-muted)">T3, Ca 1, A1.01</span>
                   <ArrowRight :size="14" class="text-(--text-muted)"/>
                   <span class="font-bold">CN, Ca 2, B2.03</span>
                </div>
             </div>
          </div>
          <div class="text-right">
             <p class="text-xs text-(--text-muted)">Giảng viên: Lê Văn C</p>
             <p class="text-[10px] text-(--text-muted) mt-1">Cập nhật: 10 phút trước</p>
          </div>
       </div>
    </div>
  </div>
</template>
