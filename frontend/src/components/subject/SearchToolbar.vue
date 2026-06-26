<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Search, ChevronDown, RotateCw } from 'lucide-vue-next'
import type { SubjectStatus, SortKey } from '@/types/subject'

const props = defineProps<{
  searchQuery: string
  statusFilter: SubjectStatus | 'all'
  sortKey: SortKey
}>()

const emit = defineEmits<{
  'update:searchQuery': [value: string]
  'update:statusFilter': [value: SubjectStatus | 'all']
  'update:sortKey': [value: SortKey]
  refresh: []
}>()

const statusOpen = ref(false)
const sortOpen = ref(false)

const statusOptions: { label: string; value: SubjectStatus | 'all' }[] = [
  { label: 'Tất cả', value: 'all' },
  { label: 'Đang hoạt động', value: 'active' },
  { label: 'Nháp', value: 'draft' },
  { label: 'Đã khóa', value: 'locked' },
]

const sortOptions: { label: string; value: SortKey }[] = [
  { label: 'Ngày cập nhật mới nhất', value: 'newest' },
  { label: 'Ngày cập nhật cũ nhất', value: 'oldest' },
  { label: 'Tên A-Z', value: 'nameAsc' },
  { label: 'Tên Z-A', value: 'nameDesc' },
]

const selectedStatusLabel = computed(() => statusOptions.find(o => o.value === props.statusFilter)?.label ?? 'Tất cả')
const selectedSortLabel = computed(() => sortOptions.find(o => o.value === props.sortKey)?.label ?? 'Ngày cập nhật mới nhất')

function handleClickOutside(e: MouseEvent) {
  const target = e.target as HTMLElement
  if (!target.closest('[data-dropdown-status]')) statusOpen.value = false
  if (!target.closest('[data-dropdown-sort]')) sortOpen.value = false
}

onMounted(() => document.addEventListener('click', handleClickOutside))
onUnmounted(() => document.removeEventListener('click', handleClickOutside))
</script>

<template>
  <div class="flex flex-wrap items-center gap-3">
    <div class="relative flex-1 min-w-[200px] max-w-md">
      <label for="subject-search" class="sr-only">Tìm kiếm môn học</label>
      <Search
        :size="16"
        class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 dark:text-slate-500 pointer-events-none"
        aria-hidden="true"
      />
      <input
        id="subject-search"
        :value="searchQuery"
        @input="emit('update:searchQuery', ($event.target as HTMLInputElement).value)"
        type="text"
        placeholder="Tìm theo tên môn hoặc mã môn..."
        class="w-full h-10 pl-9 pr-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-sm text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 dark:focus:border-blue-400 transition-all duration-200"
      />
    </div>

    <div class="relative" data-dropdown-status>
      <label for="status-filter-btn" class="sr-only">Lọc trạng thái</label>
      <button
        id="status-filter-btn"
        type="button"
        :aria-label="'Lọc trạng thái: ' + selectedStatusLabel"
        :aria-expanded="statusOpen"
        class="inline-flex items-center gap-2 h-10 px-3.5 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-sm font-medium text-slate-600 dark:text-slate-300 shadow-sm transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 min-w-[130px]"
        @click="statusOpen = !statusOpen"
      >
        <span class="flex-1 text-left">{{ selectedStatusLabel }}</span>
        <ChevronDown :size="14" class="text-slate-400 transition-transform duration-200" :class="statusOpen ? 'rotate-180' : ''" aria-hidden="true" />
      </button>
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 scale-95 -translate-y-1"
        enter-to-class="opacity-100 scale-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 scale-100 translate-y-0"
        leave-to-class="opacity-0 scale-95 -translate-y-1"
      >
        <div v-if="statusOpen" class="absolute top-full mt-1.5 left-0 z-20 min-w-[180px] rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-lg py-1.5" role="listbox" :aria-label="'Chọn trạng thái'">
          <button
            v-for="opt in statusOptions"
            :key="opt.value"
            type="button"
            role="option"
            :aria-selected="opt.value === statusFilter"
            :class="[
              'w-full text-left px-3.5 py-2 text-sm transition-colors duration-150 focus-visible:outline-none focus-visible:bg-blue-50 dark:focus-visible:bg-blue-500/10',
              opt.value === statusFilter
                ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/10 font-semibold'
                : 'text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700',
            ]"
            @click="emit('update:statusFilter', opt.value); statusOpen = false"
          >
            {{ opt.label }}
          </button>
        </div>
      </Transition>
    </div>

    <div class="relative" data-dropdown-sort>
      <label for="sort-btn" class="sr-only">Sắp xếp</label>
      <button
        id="sort-btn"
        type="button"
        :aria-label="'Sắp xếp: ' + selectedSortLabel"
        :aria-expanded="sortOpen"
        class="inline-flex items-center gap-2 h-10 px-3.5 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-sm font-medium text-slate-600 dark:text-slate-300 shadow-sm transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 min-w-[130px]"
        @click="sortOpen = !sortOpen"
      >
        <span class="flex-1 text-left truncate max-w-[120px]">{{ selectedSortLabel }}</span>
        <ChevronDown :size="14" class="text-slate-400 transition-transform duration-200" :class="sortOpen ? 'rotate-180' : ''" aria-hidden="true" />
      </button>
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 scale-95 -translate-y-1"
        enter-to-class="opacity-100 scale-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 scale-100 translate-y-0"
        leave-to-class="opacity-0 scale-95 -translate-y-1"
      >
        <div v-if="sortOpen" class="absolute top-full mt-1.5 left-0 z-20 min-w-[200px] rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-lg py-1.5" role="listbox" :aria-label="'Chọn cách sắp xếp'">
          <button
            v-for="opt in sortOptions"
            :key="opt.value"
            type="button"
            role="option"
            :aria-selected="opt.value === sortKey"
            :class="[
              'w-full text-left px-3.5 py-2 text-sm transition-colors duration-150 focus-visible:outline-none focus-visible:bg-blue-50 dark:focus-visible:bg-blue-500/10',
              opt.value === sortKey
                ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/10 font-semibold'
                : 'text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700',
            ]"
            @click="emit('update:sortKey', opt.value); sortOpen = false"
          >
            {{ opt.label }}
          </button>
        </div>
      </Transition>
    </div>

    <button
      type="button"
      :aria-label="'Làm mới danh sách'"
      class="inline-flex items-center justify-center h-10 w-10 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-500 dark:text-slate-400 shadow-sm transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 hover:text-slate-700 dark:hover:text-slate-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
      @click="$emit('refresh')"
    >
      <RotateCw :size="16" aria-hidden="true" />
    </button>
  </div>
</template>
