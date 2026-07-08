import { ref, computed, watch, onMounted } from 'vue'
import { useSubjectStore } from '@/stores/content-council/subjectStore'
import { contentCouncilApi } from '@/services/contentCouncilApi'

export function useSubjectFilters() {
  const store = useSubjectStore()
  
  const subjects = computed(() => store.subjects)
  const totalSubjects = computed(() => store.totalSubjects)
  const isLoading = ref(true)
  const error = computed(() => store.error)
  
  const searchQuery = ref('')
  const statusFilter = ref<'all' | 'active' | 'inactive' | 'archived'>('all')
  const sortOption = ref<'updatedAt' | 'nameAsc' | 'nameDesc' | 'codeAsc'>('updatedAt')
  
  const viewMode = ref<'grid' | 'table'>('grid')
  
  const selectedMajor = ref<number | null>(null)
  const selectedSpecialization = ref<number | null>(null)
  
  const majors = ref<any[]>([])
  const specializations = ref<any[]>([])
  
  const pageIndex = ref(1)
  const pageSize = ref(12) // Show 12 items per page for grid
  
  const fetchMajors = async () => {
    try {
      const res = await contentCouncilApi.getMajors()
      let payloadData = res?.data ?? res?.Data ?? res ?? []
      let itemsList = []
      
      if (payloadData && !Array.isArray(payloadData)) {
         if (Array.isArray(payloadData.items)) {
             itemsList = payloadData.items
         } else if (Array.isArray(payloadData.Items)) {
             itemsList = payloadData.Items
         }
      } else if (Array.isArray(payloadData)) {
         itemsList = payloadData
      }
      majors.value = itemsList
    } catch (e) {
      console.error('Failed to load majors', e)
    }
  }
  
  const fetchSpecializations = async (majorId: number) => {
    try {
      const res = await contentCouncilApi.getSpecializations({ majorId })
      let payloadData = res?.data ?? res?.Data ?? res ?? []
      let itemsList = []
      
      if (payloadData && !Array.isArray(payloadData)) {
         if (Array.isArray(payloadData.items)) {
             itemsList = payloadData.items
         } else if (Array.isArray(payloadData.Items)) {
             itemsList = payloadData.Items
         }
      } else if (Array.isArray(payloadData)) {
         itemsList = payloadData
      }
      specializations.value = itemsList
    } catch (e) {
      console.error('Failed to load specializations', e)
    }
  }
  
  watch(selectedMajor, async (newVal) => {
    selectedSpecialization.value = null
    specializations.value = []
    if (newVal) {
      await fetchSpecializations(newVal)
    }
    // Automatically trigger search when major changes
    pageIndex.value = 1
    await fetchSubjects()
  })
  
  watch(selectedSpecialization, async () => {
    pageIndex.value = 1
    await fetchSubjects()
  })
  
  watch(pageIndex, async () => {
    await fetchSubjects()
  })
  
  // Expose fetchSubjects directly
  const fetchSubjects = async () => {
    isLoading.value = true
    try {
      await store.fetchSubjects({
        keyword: searchQuery.value,
        pageIndex: pageIndex.value,
        pageSize: pageSize.value,
        majorId: selectedMajor.value,
        specializationId: selectedSpecialization.value
      })
    } finally {
      isLoading.value = false
    }
  }

  // Handle client-side filters (Status, Sort) since backend doesn't support sorting in API yet
  const filteredSubjects = computed(() => {
    let result = subjects.value

    if (statusFilter.value !== 'all') {
      result = result.filter(s => s.status === statusFilter.value)
    }

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
    return searchQuery.value.trim() !== '' || statusFilter.value !== 'all' || selectedMajor.value !== null
  })

  const clearFilters = () => {
    searchQuery.value = ''
    statusFilter.value = 'all'
    selectedMajor.value = null
    selectedSpecialization.value = null
    pageIndex.value = 1
  }
  
  onMounted(async () => {
    await fetchMajors()
  })

  return {
    subjects,
    totalSubjects,
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
    error,
    filteredSubjects,
    hasActiveFilters,
    fetchSubjects,
    clearFilters
  }
}
