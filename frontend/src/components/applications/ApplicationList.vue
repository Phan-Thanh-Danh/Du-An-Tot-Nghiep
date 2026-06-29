<script setup>
import { ref, computed } from 'vue'
import ApplicationCard from './ApplicationCard.vue'

import GlassButton from '@/components/ui/GlassButton.vue'
import { Search, FolderOpen } from 'lucide-vue-next'

const props = defineProps({
  applications: { type: Array, default: () => [] },
  loading: Boolean
})

const emit = defineEmits(['select', 'create'])
const filter = ref('ALL')
const search = ref('')

const filteredList = computed(() => {
  return props.applications.filter(app => {
    if (filter.value !== 'ALL' && app.trangThai !== filter.value) return false
    if (search.value && !app.tieuDe?.toLowerCase().includes(search.value.toLowerCase())) return false
    return true
  })
})
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-col sm:flex-row justify-between gap-4">
      <div class="flex items-center gap-2 overflow-x-auto custom-scrollbar pb-1">
        <GlassButton :variant="filter === 'ALL' ? 'primary' : 'ghost'" size="sm" @click="filter = 'ALL'">Tất cả</GlassButton>
        <GlassButton :variant="filter === 'NHAP' ? 'primary' : 'ghost'" size="sm" @click="filter = 'NHAP'">Nháp</GlassButton>
        <GlassButton :variant="filter === 'DA_NOP' ? 'primary' : 'ghost'" size="sm" @click="filter = 'DA_NOP'">Đã nộp</GlassButton>
        <GlassButton :variant="filter === 'DANG_XEM_XET' ? 'primary' : 'ghost'" size="sm" @click="filter = 'DANG_XEM_XET'">Đang xử lý</GlassButton>
        <GlassButton :variant="filter === 'YEU_CAU_BO_SUNG' ? 'warning' : 'ghost'" size="sm" @click="filter = 'YEU_CAU_BO_SUNG'">Cần bổ sung</GlassButton>
        <GlassButton :variant="filter === 'DA_DUYET' ? 'success' : 'ghost'" size="sm" @click="filter = 'DA_DUYET'">Hoàn thành</GlassButton>
      </div>

      <div class="flex gap-2">
        <div class="relative">
          <Search class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-[var(--text-muted)]" />
          <input 
            v-model="search"
            class="h-9 pl-9 pr-3 text-sm rounded-full bg-[var(--surface-input)] border border-[var(--border-input)] focus:border-[var(--lg-primary)] focus:ring-1 focus:ring-[var(--lg-primary)] outline-none text-[var(--text-body)] transition-all w-full sm:w-64"
            placeholder="Tìm kiếm..."
          />
        </div>
        <GlassButton variant="primary" @click="emit('create')">Tạo đơn</GlassButton>
      </div>
    </div>

    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <div v-for="i in 8" :key="i" class="h-32 bg-[var(--surface-hover)] rounded-xl animate-pulse"></div>
    </div>
    
    <div v-else-if="filteredList.length === 0" class="py-12 flex flex-col items-center justify-center text-[var(--text-muted)] bg-[var(--surface-card)] border border-[var(--border-card)] rounded-xl">
      <FolderOpen class="w-12 h-12 mb-3 opacity-20" />
      <p>Không tìm thấy đơn từ nào.</p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <ApplicationCard 
        v-for="app in filteredList" 
        :key="app.id" 
        :application="app" 
        @click="emit('select', app)"
      />
    </div>
  </div>
</template>
