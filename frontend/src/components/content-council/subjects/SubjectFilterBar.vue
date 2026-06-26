<script setup>
import { Search, X } from 'lucide-vue-next'

const props = defineProps({
  searchQuery: { type: String, default: '' },
  statusFilter: { type: String, default: 'all' },
  sortOption: { type: String, default: 'updatedAt' },
  hasActiveFilters: { type: Boolean, default: false }
})

const emit = defineEmits([
  'update:searchQuery',
  'update:statusFilter',
  'update:sortOption',
  'clearFilters'
])
</script>

<template>
  <div class="flex flex-col md:flex-row gap-3 mb-6 bg-white p-3 rounded-xl border border-slate-200">
    <!-- Search Input -->
    <div class="relative flex-grow">
      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
        <Search class="h-5 w-5 text-slate-400" />
      </div>
      <input
        type="text"
        :value="searchQuery"
        @input="e => emit('update:searchQuery', e.target.value)"
        class="block w-full pl-10 pr-3 py-2 border border-slate-200 rounded-lg text-sm text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-slate-50 hover:bg-white transition-colors"
        placeholder="Tìm theo mã hoặc tên môn học..."
      />
    </div>

    <div class="flex flex-col sm:flex-row gap-3">
      <!-- Status Filter -->
      <select
        :value="statusFilter"
        @change="e => emit('update:statusFilter', e.target.value)"
        class="block w-full sm:w-48 pl-3 pr-8 py-2 border border-slate-200 rounded-lg text-sm text-slate-700 bg-slate-50 hover:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none cursor-pointer"
      >
        <option value="all">Tất cả trạng thái</option>
        <option value="empty">Chưa có nội dung</option>
        <option value="draft">Đang biên soạn</option>
        <option value="completed">Đã hoàn thiện</option>
      </select>

      <!-- Sort Filter -->
      <select
        :value="sortOption"
        @change="e => emit('update:sortOption', e.target.value)"
        class="block w-full sm:w-48 pl-3 pr-8 py-2 border border-slate-200 rounded-lg text-sm text-slate-700 bg-slate-50 hover:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none cursor-pointer"
      >
        <option value="updatedAt">Cập nhật gần nhất</option>
        <option value="nameAsc">Tên môn A-Z</option>
        <option value="nameDesc">Tên môn Z-A</option>
        <option value="codeAsc">Mã môn A-Z</option>
      </select>
      
      <!-- Clear Filters -->
      <button
        v-if="hasActiveFilters"
        @click="emit('clearFilters')"
        class="flex items-center justify-center gap-1.5 px-3 py-2 text-sm font-medium text-slate-600 hover:text-red-600 hover:bg-red-50 border border-transparent rounded-lg transition-colors whitespace-nowrap"
      >
        <X class="w-4 h-4" />
        Xóa bộ lọc
      </button>
    </div>
  </div>
</template>

<style scoped>
select {
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M6 8l4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 0.5rem center;
  background-repeat: no-repeat;
  background-size: 1.5em 1.5em;
}
</style>
