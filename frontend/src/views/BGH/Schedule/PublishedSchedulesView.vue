<script setup>
import { ref, computed } from 'vue'
import { Eye, Search, Building2, CalendarDays, Clock, Users } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()
const searchQuery = ref('')

const publishedData = ref([
  { id: 'TKB-001', dept: 'Khoa Công nghệ Thông tin', term: 'Học kỳ 1 - 2026', type: 'Chính quy', classes: 120, hours: 3600, campus: 'Cơ sở chính', status: 'active' },
  { id: 'TKB-002', dept: 'Khoa Kinh tế & QT', term: 'Học kỳ 1 - 2026', type: 'Chính quy', classes: 80, hours: 2400, campus: 'Cơ sở 2', status: 'active' },
  { id: 'TKB-003', dept: 'Khoa Ngôn ngữ Anh', term: 'Học kỳ 1 - 2026', type: 'Chất lượng cao', classes: 65, hours: 1950, campus: 'Cơ sở chính', status: 'active' },
  { id: 'TKB-004', dept: 'Khoa Thiết kế', term: 'Học kỳ 2 - 2025', type: 'Chính quy', classes: 45, hours: 1350, campus: 'Cơ sở chính', status: 'archived' },
  { id: 'TKB-005', dept: 'Khoa CNTT', term: 'Học kỳ 2 - 2025', type: 'Chất lượng cao', classes: 55, hours: 1650, campus: 'Cơ sở chính', status: 'archived' },
])

const filteredData = computed(() => {
  if (!searchQuery.value) return publishedData.value
  const q = searchQuery.value.toLowerCase()
  return publishedData.value.filter(s => s.dept.toLowerCase().includes(q) || s.id.toLowerCase().includes(q) || s.term.toLowerCase().includes(q))
})

function viewSchedule(item) {
  popup.info(`Lịch biểu: ${item.id}`, `${item.dept}\nHọc kỳ: ${item.term}\nLoại: ${item.type}\nSố lớp: ${item.classes}\nTổng giờ: ${item.hours}h\nCơ sở: ${item.campus}`)
}
</script>

<template>
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
    <div class="col-span-full flex items-center gap-3 mb-2">
      <div class="relative">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
        <input v-model="searchQuery" type="text" placeholder="Tìm khoa, mã lớp..." class="w-56 surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
      </div>
      <span class="text-xs font-bold text-muted">{{ filteredData.length }} bộ TKB</span>
    </div>
      <div v-for="item in filteredData" :key="item.id" class="surface-card border border-card rounded-2xl p-5 hover:shadow-md transition-all relative overflow-hidden group">
         <div :class="['absolute top-0 left-0 right-0 h-1', item.status === 'active' ? 'bg-(--color-success-text)' : 'bg-(--text-placeholder)']"></div>
         <div class="flex justify-between items-start mb-3">
           <div class="flex items-center gap-2">
             <Building2 :size="18" class="text-muted" />
             <h3 class="font-bold text-heading">{{ item.dept }}</h3>
           </div>
           <GlassBadge :variant="item.status === 'active' ? 'success' : 'default'" size="xs">
             {{ item.status === 'active' ? 'Đang áp dụng' : 'Lưu trữ' }}
           </GlassBadge>
         </div>
         <div class="flex items-center gap-2 text-xs text-muted mb-4">
           <CalendarDays :size="14" />
           <span class="font-semibold">{{ item.term }} — {{ item.type }}</span>
         </div>
         <div class="grid grid-cols-2 gap-3 py-3 border-y border-default">
           <div class="flex items-center gap-2">
             <Users :size="14" class="text-placeholder" />
             <div>
               <p class="text-[10px] text-muted uppercase tracking-wider">Lớp HP</p>
               <p class="text-sm font-bold text-heading">{{ item.classes }}</p>
             </div>
           </div>
           <div class="flex items-center gap-2">
             <Clock :size="14" class="text-placeholder" />
             <div>
               <p class="text-[10px] text-muted uppercase tracking-wider">Tổng giờ</p>
               <p class="text-sm font-bold text-heading">{{ item.hours }}h</p>
             </div>
           </div>
         </div>
         <div class="mt-4 flex items-center justify-between">
           <span class="text-[10px] font-bold text-muted">{{ item.campus }}</span>
           <GlassButton variant="secondary" size="sm" @click="viewSchedule(item)">
             <Eye :size="14" class="mr-1" /> Xem lịch biểu
           </GlassButton>
         </div>
      </div>
    </div>

    <div v-if="filteredData.length === 0" class="py-16 text-center surface-card border border-card rounded-2xl">
      <CalendarDays :size="48" class="text-placeholder mx-auto mb-3" />
      <p class="text-sm font-semibold text-muted">Không có thời khóa biểu phù hợp</p>
  </div>
</template>
