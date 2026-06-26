import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Subject, SubjectStatus, SortKey, SubjectStats } from '@/types/subject'
import { mockSubjects } from '@/mock/subjects'

export const useSubjectStore = defineStore('subject', () => {
  const subjects = ref<Subject[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const searchQuery = ref('')
  const statusFilter = ref<SubjectStatus | 'all'>('all')
  const sortKey = ref<SortKey>('newest')

  const currentPage = ref(1)
  const pageSize = ref(12)

  const filteredSubjects = computed(() => {
    let result = [...subjects.value]

    const q = searchQuery.value.toLowerCase().trim()
    if (q) {
      result = result.filter(
        s => s.name.toLowerCase().includes(q) || s.code.toLowerCase().includes(q),
      )
    }

    if (statusFilter.value !== 'all') {
      result = result.filter(s => s.status === statusFilter.value)
    }

    switch (sortKey.value) {
      case 'newest':
        result.sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
        break
      case 'oldest':
        result.sort((a, b) => new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime())
        break
      case 'nameAsc':
        result.sort((a, b) => a.name.localeCompare(b.name, 'vi'))
        break
      case 'nameDesc':
        result.sort((a, b) => b.name.localeCompare(a.name, 'vi'))
        break
    }

    return result
  })

  const totalItems = computed(() => filteredSubjects.value.length)

  const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / pageSize.value)))

  const paginatedSubjects = computed(() => {
    const start = (currentPage.value - 1) * pageSize.value
    const end = start + pageSize.value
    return filteredSubjects.value.slice(start, end)
  })

  const stats = computed<SubjectStats>(() => ({
    total: subjects.value.length,
    active: subjects.value.filter(s => s.status === 'active').length,
    draft: subjects.value.filter(s => s.status === 'draft').length,
    locked: subjects.value.filter(s => s.status === 'locked').length,
  }))

  const hasSubjects = computed(() => subjects.value.length > 0)
  const hasFilteredResults = computed(() => filteredSubjects.value.length > 0)

  const paginationRange = computed(() => {
    const start = (currentPage.value - 1) * pageSize.value + 1
    const end = Math.min(currentPage.value * pageSize.value, totalItems.value)
    return { start, end }
  })

  function loadSubjects() {
    loading.value = true
    error.value = null
    setTimeout(() => {
      subjects.value = [...mockSubjects]
      loading.value = false
    }, 600)
  }

  function setSearchQuery(value: string) {
    searchQuery.value = value
    currentPage.value = 1
  }

  function setStatusFilter(value: SubjectStatus | 'all') {
    statusFilter.value = value
    currentPage.value = 1
  }

  function setSortKey(value: SortKey) {
    sortKey.value = value
    currentPage.value = 1
  }

  function setPage(page: number) {
    if (page >= 1 && page <= totalPages.value) {
      currentPage.value = page
    }
  }

  function setPageSize(size: number) {
    pageSize.value = size
    currentPage.value = 1
  }

  function resetFilters() {
    searchQuery.value = ''
    statusFilter.value = 'all'
    sortKey.value = 'newest'
    currentPage.value = 1
  }

  return {
    subjects,
    loading,
    error,
    searchQuery,
    statusFilter,
    sortKey,
    currentPage,
    pageSize,
    filteredSubjects,
    totalItems,
    totalPages,
    paginatedSubjects,
    stats,
    hasSubjects,
    hasFilteredResults,
    paginationRange,
    loadSubjects,
    setSearchQuery,
    setStatusFilter,
    setSortKey,
    setPage,
    setPageSize,
    resetFilters,
  }
})
