<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { BookOpen, CircleCheck, Clock, Lock } from 'lucide-vue-next'
import { useSubjectStore } from '@/stores/useSubjectStore'
import { storeToRefs } from 'pinia'

import PageHeader from '@/components/shared/PageHeader.vue'
import EmptyState from '@/components/shared/EmptyState.vue'
import LoadingSkeleton from '@/components/shared/LoadingSkeleton.vue'
import Pagination from '@/components/shared/Pagination.vue'
import Toast, { type Toast as ToastType } from '@/components/shared/Toast.vue'
import Modal from '@/components/shared/Modal.vue'

import StatsCard from '@/components/subject/StatsCard.vue'
import SearchToolbar from '@/components/subject/SearchToolbar.vue'
import SubjectCard from '@/components/subject/SubjectCard.vue'

const store = useSubjectStore()
const {
  searchQuery,
  statusFilter,
  sortKey,
  currentPage,
  pageSize,
  totalItems,
  totalPages,
  paginatedSubjects,
  stats,
  loading,
  hasSubjects,
  hasFilteredResults,
  paginationRange,
} = storeToRefs(store)

const toasts = ref<ToastType[]>([])
let toastId = 0

const modal = ref({
  open: false,
  type: 'info' as 'confirm' | 'delete' | 'info',
  title: '',
  message: '',
  confirmLabel: '',
})

function addToast(type: ToastType['type'], message: string) {
  const id = ++toastId
  toasts.value.push({ id, type, message })
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }, 4000)
}

function dismissToast(id: number) {
  toasts.value = toasts.value.filter(t => t.id !== id)
}

function showModal(type: 'confirm' | 'delete' | 'info', title: string, message: string, confirmLabel?: string) {
  modal.value = { open: true, type, title, message, confirmLabel: confirmLabel || '' }
}

function closeModal() {
  modal.value.open = false
}

onMounted(() => {
  store.loadSubjects()
})

function handleRefresh() {
  store.loadSubjects()
  addToast('info', 'Đang làm mới dữ liệu...')
}

function handleAdd() {
  addToast('info', 'Chức năng "Thêm môn học" sẽ được phát triển sau.')
}

function handleViewDetail(id: string) {
  addToast('info', `Xem chi tiết môn học: ${id}`)
}

function handleEdit(id: string) {
  addToast('info', `Chỉnh sửa môn học: ${id}`)
}

function handlePreview(id: string) {
  addToast('info', `Xem Preview môn học: ${id}`)
}

function handleCopy(id: string) {
  addToast('success', `Đã sao chép môn học: ${id}`)
}

function handleArchive(id: string) {
  showModal('delete', 'Lưu trữ môn học', `Bạn có chắc muốn lưu trữ môn học này? Dữ liệu sẽ không bị xóa nhưng sẽ ẩn khỏi danh sách chính.`, 'Lưu trữ')
}

function handleModalConfirm() {
  closeModal()
  addToast('success', 'Thao tác thành công!')
}

const pageDescription = 'Quản lý danh mục môn học, nội dung giảng dạy và cấu trúc chương trình đào tạo.'
</script>

<template>
  <Toast :toasts="toasts" @dismiss="dismissToast" />

  <Modal
    :open="modal.open"
    :type="modal.type as any"
    :title="modal.title"
    :message="modal.message"
    :confirm-label="modal.confirmLabel || undefined"
    @confirm="handleModalConfirm"
    @cancel="closeModal"
  />

  <div class="pb-8 space-y-6">
    <PageHeader
      title="Quản lý Môn học"
      :description="pageDescription"
      :is-loading="loading"
      @refresh="handleRefresh"
      @add="handleAdd"
    />

    <template v-if="loading">
      <LoadingSkeleton variant="stats" />
      <LoadingSkeleton variant="toolbar" />
      <LoadingSkeleton variant="grid" :count="6" />
    </template>

    <template v-else-if="!hasSubjects">
      <EmptyState
        title="Chưa có môn học"
        description="Hiện chưa có môn học nào trong hệ thống. Hãy tạo môn học đầu tiên để bắt đầu xây dựng nội dung giảng dạy."
        action-label="Tạo môn học"
        @action="handleAdd"
      />
    </template>

    <template v-else>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatsCard label="Tổng số môn" :value="stats.total" :icon="BookOpen" variant="default" :index="0" />
        <StatsCard label="Đang hoạt động" :value="stats.active" :icon="CircleCheck" variant="success" :index="1" />
        <StatsCard label="Nháp" :value="stats.draft" :icon="Clock" variant="warning" :index="2" />
        <StatsCard label="Đã khóa" :value="stats.locked" :icon="Lock" variant="danger" :index="3" />
      </div>

      <div class="sticky top-0 z-10 py-3 -mx-4 px-4 bg-slate-50/90 dark:bg-slate-950/90 backdrop-blur-md border-b border-transparent transition-colors">
        <SearchToolbar
          :search-query="searchQuery"
          :status-filter="statusFilter"
          :sort-key="sortKey"
          @update:search-query="store.setSearchQuery"
          @update:status-filter="store.setStatusFilter"
          @update:sort-key="store.setSortKey"
          @refresh="handleRefresh"
        />
      </div>

      <div v-if="!hasFilteredResults" class="flex flex-col items-center justify-center py-16">
        <div class="relative mb-4">
          <div class="relative flex h-16 w-16 items-center justify-center rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-sm">
            <BookOpen :size="24" class="text-slate-300 dark:text-slate-600" aria-hidden="true" />
          </div>
        </div>
        <h3 class="text-base font-bold text-slate-900 dark:text-white">Không tìm thấy kết quả</h3>
        <p class="mt-1.5 text-sm text-slate-500 dark:text-slate-400">Thử thay đổi từ khóa hoặc bộ lọc tìm kiếm.</p>
        <button
          type="button"
          class="mt-4 text-sm font-semibold text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 rounded-lg px-2 py-1"
          @click="store.resetFilters"
        >
          Xóa bộ lọc
        </button>
      </div>

      <TransitionGroup
        v-else
        tag="div"
        class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5"
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-y-4"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 translate-y-4"
      >
        <SubjectCard
          v-for="subject in paginatedSubjects"
          :key="subject.id"
          :subject="subject"
          @view="handleViewDetail"
          @edit="handleEdit"
          @preview="handlePreview"
          @copy="handleCopy"
          @archive="handleArchive"
        />
      </TransitionGroup>

      <Pagination
        v-if="totalPages > 1"
        :current-page="currentPage"
        :total-pages="totalPages"
        :total-items="totalItems"
        :page-size="pageSize"
        :range="paginationRange"
        @update:page="store.setPage"
        @update:page-size="store.setPageSize"
      />
    </template>
  </div>
</template>
