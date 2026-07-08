<script setup>
import { Search, X, LayoutGrid, List } from 'lucide-vue-next'

const props = defineProps({
  searchQuery: { type: String, default: '' },
  statusFilter: { type: String, default: 'all' },
  sortOption: { type: String, default: 'updatedAt' },
  hasActiveFilters: { type: Boolean, default: false },
  viewMode: { type: String, default: 'grid' },
  selectedMajor: { type: Number, default: null },
  selectedSpecialization: { type: Number, default: null },
  majors: { type: Array, default: () => [] },
  specializations: { type: Array, default: () => [] }
})

const emit = defineEmits([
  'update:searchQuery',
  'update:statusFilter',
  'update:sortOption',
  'update:viewMode',
  'update:selectedMajor',
  'update:selectedSpecialization',
  'clearFilters'
])
</script>

<template>
  <div class="flex flex-col gap-3 mb-6 bg-white p-3 rounded-xl border border-slate-200">
    <!-- Top row: Search and View Toggle -->
    <div class="flex flex-col md:flex-row gap-3">
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

      <div class="flex bg-slate-100 p-1 rounded-lg self-start md:self-auto">
        <button
          @click="emit('update:viewMode', 'grid')"
          :class="['p-1.5 rounded-md flex items-center transition-colors', viewMode === 'grid' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500 hover:text-slate-700']"
          title="Chế độ thẻ"
        >
          <LayoutGrid class="w-4 h-4" />
        </button>
        <button
          @click="emit('update:viewMode', 'table')"
          :class="['p-1.5 rounded-md flex items-center transition-colors', viewMode === 'table' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500 hover:text-slate-700']"
          title="Chế độ bảng"
        >
          <List class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Bottom row: Filters -->
    <div class="flex flex-col sm:flex-row flex-wrap gap-3">
      <!-- Major Filter -->
      <select
        :value="selectedMajor || ''"
        @change="e => emit('update:selectedMajor', e.target.value ? Number(e.target.value) : null)"
        class="block w-full sm:w-48 pl-3 pr-8 py-2 border border-slate-200 rounded-lg text-sm text-slate-700 bg-slate-50 hover:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none cursor-pointer"
      >
        <option value="">Tất cả ngành</option>
        <option v-for="major in majors" :key="major.maNganh" :value="major.maNganh">
          {{ major.tenNganh }}
        </option>
      </select>

      <!-- Specialization Filter -->
      <select
        :value="selectedSpecialization || ''"
        @change="e => emit('update:selectedSpecialization', e.target.value ? Number(e.target.value) : null)"
        :disabled="!selectedMajor || specializations.length === 0"
        class="block w-full sm:w-48 pl-3 pr-8 py-2 border border-slate-200 rounded-lg text-sm text-slate-700 bg-slate-50 hover:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <option value="">Tất cả chuyên ngành</option>
        <option v-for="spec in specializations" :key="spec.maChuyenNganh" :value="spec.maChuyenNganh">
          {{ spec.tenChuyenNganh }}
        </option>
      </select>

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
        Xóa lọc
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
