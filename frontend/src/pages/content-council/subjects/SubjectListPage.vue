<script setup>
import { onMounted, computed } from 'vue'
import { RefreshCw, ChevronLeft, ChevronRight } from 'lucide-vue-next'
import { useSubjectFilters } from '@/composables/content-council/useSubjectFilters'

import SubjectFilterBar from '@/components/content-council/subjects/SubjectFilterBar.vue'
import SubjectCard from '@/components/content-council/subjects/SubjectCard.vue'
import SubjectTable from '@/components/content-council/subjects/SubjectTable.vue'
import SubjectListSkeleton from '@/components/content-council/subjects/SubjectListSkeleton.vue'
import SubjectEmptyState from '@/components/content-council/subjects/SubjectEmptyState.vue'

const {
  isLoading,
  searchQuery,
  statusFilter,
  sortOption,
  viewMode,
  selectedMajor,
  selectedSpecialization,
  majors,
  specializations,
  pageIndex,
  pageSize,
  totalSubjects,
  filteredSubjects,
  hasActiveFilters,
  fetchSubjects,
  clearFilters,
  subjects,
  error
} = useSubjectFilters()

const totalPages = computed(() => {
  return Math.max(1, Math.ceil(totalSubjects.value / pageSize.value))
})

const prevPage = () => {
  if (pageIndex.value > 1) {
    pageIndex.value--
  }
}

const nextPage = () => {
  if (pageIndex.value < totalPages.value) {
    pageIndex.value++
  }
}

onMounted(() => {
  fetchSubjects()
})
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Page Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl sm:text-3xl font-bold text-slate-900 mb-1">Nội dung môn học</h1>
        <p class="text-slate-500">Chọn một môn học để xây dựng chương, bài học, video, tài liệu và Quiz.</p>
      </div>
      <button 
        @click="fetchSubjects"
        :disabled="isLoading"
        class="inline-flex items-center justify-center gap-2 px-4 py-2 bg-white border border-slate-200 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shrink-0"
      >
        <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': isLoading }" />
        Làm mới
      </button>
    </div>

    <!-- Error State -->
    <div v-if="error" class="flex flex-col items-center justify-center h-full py-12 bg-red-50 rounded-xl border border-red-100 mb-6">
      <div class="text-red-500 mb-4">
        <svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
      </div>
      <h3 class="text-lg font-bold text-red-800 mb-2">Đã xảy ra lỗi</h3>
      <p class="text-red-600 mb-6 text-center max-w-md">{{ error }}</p>
      <button 
        @click="fetchSubjects()"
        class="inline-flex items-center justify-center px-4 py-2 border border-transparent rounded-lg text-sm font-medium text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
      >
        <RefreshCw class="w-4 h-4 mr-2" />
        Thử lại
      </button>
    </div>

    <template v-else>
      <!-- Filter Bar -->
      <SubjectFilterBar 
        v-model:searchQuery="searchQuery"
        v-model:statusFilter="statusFilter"
        v-model:sortOption="sortOption"
        v-model:viewMode="viewMode"
        v-model:selectedMajor="selectedMajor"
        v-model:selectedSpecialization="selectedSpecialization"
        :majors="majors"
        :specializations="specializations"
        :hasActiveFilters="hasActiveFilters"
        @clearFilters="clearFilters"
      />

      <!-- Main Content Area -->
      <div class="flex-1">
        <!-- Loading State -->
        <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
          <SubjectListSkeleton v-for="i in 6" :key="i" />
        </div>

        <!-- Content -->
        <template v-else>
          <template v-if="filteredSubjects.length > 0">
            <!-- View Modes -->
            <div v-if="viewMode === 'grid'" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
              <SubjectCard 
                v-for="subject in filteredSubjects" 
                :key="subject.id" 
                :subject="subject" 
              />
            </div>
            <SubjectTable 
              v-else 
              :subjects="filteredSubjects" 
            />

            <!-- Pagination -->
            <div v-if="totalPages > 1" class="mt-8 flex items-center justify-between border-t border-slate-200 pt-6 pb-4">
              <div class="text-sm text-slate-500">
                Hiển thị từ <span class="font-medium text-slate-900">{{ (pageIndex - 1) * pageSize + 1 }}</span> đến <span class="font-medium text-slate-900">{{ Math.min(pageIndex * pageSize, totalSubjects) }}</span> trong tổng số <span class="font-medium text-slate-900">{{ totalSubjects }}</span> kết quả
              </div>
              
              <div class="flex items-center gap-2">
                <button 
                  @click="prevPage" 
                  :disabled="pageIndex === 1"
                  class="inline-flex items-center justify-center p-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  <ChevronLeft class="w-4 h-4" />
                </button>
                
                <div class="flex items-center gap-1">
                  <button 
                    v-for="page in totalPages" 
                    :key="page"
                    @click="pageIndex = page"
                    :class="[
                      'inline-flex items-center justify-center w-8 h-8 rounded-lg text-sm font-medium transition-colors',
                      pageIndex === page 
                        ? 'bg-blue-600 text-white border border-transparent' 
                        : 'border border-slate-200 text-slate-600 hover:bg-slate-50'
                    ]"
                  >
                    {{ page }}
                  </button>
                </div>

                <button 
                  @click="nextPage" 
                  :disabled="pageIndex === totalPages"
                  class="inline-flex items-center justify-center p-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  <ChevronRight class="w-4 h-4" />
                </button>
              </div>
            </div>
          </template>

          <!-- Empty States -->
          <SubjectEmptyState 
            v-else-if="subjects.length === 0 && !hasActiveFilters" 
            type="empty" 
          />
          
          <SubjectEmptyState 
            v-else 
            type="no-results" 
            @clearFilters="clearFilters" 
          />
        </template>
      </div>
    </template>
  </div>
</template>
