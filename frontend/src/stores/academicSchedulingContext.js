import { defineStore } from 'pinia'
import { ref } from 'vue'
import academicSchedulingApi from '../services/academicSchedulingApi'

export const useAcademicSchedulingContextStore = defineStore('academicSchedulingContext', () => {
  const isContextLoaded = ref(false)
  const isLoading = ref(false)
  const error = ref(null)

  const today = ref(null)
  const timeZone = ref(null)
  const currentTerm = ref(null)
  const nextTerm = ref(null)
  const schedulableTerm = ref(null)
  const canPrepareSchedule = ref(false)
  const reasonCode = ref('')
  const reasonMessage = ref('')
  const readiness = ref({
    hasCourses: false,
    hasClasses: false,
    hasSubjects: false,
    hasTeachers: false,
    hasRooms: false,
    hasShifts: false,
    hasPublishedSchedule: false,
    hasDraftSchedule: false,
    blockingIssues: []
  })

  async function fetchContext() {
    isLoading.value = true
    error.value = null
    try {
      const response = await academicSchedulingApi.getContext()
      
      // Update state with API response
      today.value = response.today
      timeZone.value = response.timeZone
      currentTerm.value = response.currentTerm
      nextTerm.value = response.nextTerm
      schedulableTerm.value = response.schedulableTerm
      canPrepareSchedule.value = response.canPrepareSchedule
      reasonCode.value = response.reasonCode
      reasonMessage.value = response.reasonMessage
      readiness.value = response.readiness

      isContextLoaded.value = true
    } catch (err) {
      error.value = err
      isContextLoaded.value = false
    } finally {
      isLoading.value = false
    }
  }

  const selectedMajorId = ref(null)
  const selectedSpecializationId = ref(null)
  const selectedClassId = ref(null)

  function clearContext() {
    isContextLoaded.value = false
    error.value = null
    today.value = null
    timeZone.value = null
    currentTerm.value = null
    nextTerm.value = null
    schedulableTerm.value = null
    canPrepareSchedule.value = false
    reasonCode.value = ''
    reasonMessage.value = ''
    readiness.value = {
      blockingIssues: []
    }
  }

  function clearSelection() {
    selectedMajorId.value = null
    selectedSpecializationId.value = null
    selectedClassId.value = null
  }

  return {
    isContextLoaded,
    isLoading,
    error,
    today,
    timeZone,
    currentTerm,
    nextTerm,
    schedulableTerm,
    canPrepareSchedule,
    reasonCode,
    reasonMessage,
    readiness,
    selectedMajorId,
    selectedSpecializationId,
    selectedClassId,
    fetchContext,
    clearContext,
    clearSelection
  }
})
