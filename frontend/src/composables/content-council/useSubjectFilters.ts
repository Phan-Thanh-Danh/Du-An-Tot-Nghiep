import { ref, computed } from 'vue'
import { useSubjectStore } from '@/stores/content-council/subjectStore'

export function useSubjectFilters() {
  const store = useSubjectStore()
  
  const subjects = computed(() => store.subjects)
  const isLoading = ref(true)
  const searchQuery = ref('')
  const statusFilter = ref<'all' | 'active' | 'inactive' | 'archived'>('all')
  const sortOption = ref<'updatedAt' | 'nameAsc' | 'nameDesc' | 'codeAsc'>('updatedAt')

  const fetchSubjects = async () => {
    isLoading.value = true
    try {
      store.init()
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 800))
    } finally {
      isLoading.value = false
    }
  }

  const filteredSubjects = computed(() => {
    let result = subjects.value

    // Lọc theo search (theo tên hoặc mã)
    if (searchQuery.value.trim()) {
      const q = searchQuery.value.toLowerCase().trim()
      result = result.filter(
        s => s.name.toLowerCase().includes(q) || s.code.toLowerCase().includes(q)
      )
    }

    // Lọc theo trạng thái
    if (statusFilter.value !== 'all') {
      result = result.filter(s => s.status === statusFilter.value)
    }

    // Sắp xếp
    result = [...result].sort((a, b) => {
      switch (sortOption.value) {
        case 'nameAsc':
          return a.name.localeCompare(b.name)
        case 'nameDesc':
          return b.name.localeCompare(a.name)
        case 'codeAsc':
          return a.code.localeCompare(b.code)
        case 'updatedAt':
        default:
          return new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
      }
    })

    return result
  })

  const hasActiveFilters = computed(() => {
    return searchQuery.value.trim() !== '' || statusFilter.value !== 'all'
  })

  const clearFilters = () => {
    searchQuery.value = ''
    statusFilter.value = 'all'
    // sortOption.value = 'updatedAt' // Option này có thể giữ nguyên theo yêu cầu
  }

  return {
    subjects,
    isLoading,
    searchQuery,
    statusFilter,
    sortOption,
    filteredSubjects,
    hasActiveFilters,
    fetchSubjects,
    clearFilters
  }
}
