<template>
  <div class="surface-card p-4 rounded-xl border border-slate-200 mb-6 flex flex-col md:flex-row gap-4 items-end">
    <div class="flex-1 w-full">
      <label class="block text-sm font-medium text-slate-700 mb-1">Ngành đào tạo</label>
      <SearchableSelect
        v-model="context.selectedMajorId"
        :options="majorOptions"
        placeholder="-- Chọn Ngành đào tạo --"
        @change="onMajorChange"
      />
    </div>

    <div class="flex-1 w-full">
      <label class="block text-sm font-medium text-slate-700 mb-1">Chuyên ngành</label>
      <SearchableSelect
        v-model="context.selectedSpecializationId"
        :options="specializationOptions"
        :disabled="!context.selectedMajorId"
        placeholder="-- Chọn Chuyên ngành --"
        @change="onSpecializationChange"
      />
    </div>

    <div class="flex-1 w-full">
      <label class="block text-sm font-medium text-slate-700 mb-1">Lớp hành chính</label>
      <SearchableSelect
        v-model="context.selectedClassId"
        :options="classOptions"
        :disabled="!context.selectedSpecializationId"
        placeholder="-- Chọn Lớp hành chính --"
        @change="onClassChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useAcademicSchedulingContextStore } from '@/stores/academicSchedulingContext'
import academicSchedulingApi from '@/services/academicSchedulingApi'
import SearchableSelect from '@/components/common/SearchableSelect.vue'

const context = useAcademicSchedulingContextStore()

const majors = ref([])
const specializations = ref([])
const classes = ref([])

const majorOptions = computed(() => majors.value.map(m => ({
  value: m.maNganh,
  label: `${m.tenNganh} (${m.maCodeNganh})`
})))

const specializationOptions = computed(() => specializations.value.map(s => ({
  value: s.maChuyenNganh,
  label: s.tenChuyenNganh
})))

const classOptions = computed(() => classes.value.map(c => ({
  value: c.maLop,
  label: `${c.tenLop} (${c.maCodeLop})`
})))

const emit = defineEmits(['class-selected'])

onMounted(async () => {
  try {
    majors.value = await academicSchedulingApi.getMajors()
    if (context.selectedMajorId) {
      specializations.value = await academicSchedulingApi.getSpecializations(context.selectedMajorId)
      if (context.selectedSpecializationId) {
        classes.value = await academicSchedulingApi.getClassesBySpecialization(context.selectedSpecializationId)
      }
    }
  } catch (error) {
    console.error('Lỗi khi tải danh sách bộ lọc:', error)
  }
})

const onMajorChange = async (val) => {
  context.selectedMajorId = val
  context.selectedSpecializationId = null
  context.selectedClassId = null
  specializations.value = []
  classes.value = []
  
  if (context.selectedMajorId) {
    specializations.value = await academicSchedulingApi.getSpecializations(context.selectedMajorId)
  }
}

const onSpecializationChange = async (val) => {
  context.selectedSpecializationId = val
  context.selectedClassId = null
  classes.value = []
  
  if (context.selectedSpecializationId) {
    classes.value = await academicSchedulingApi.getClassesBySpecialization(context.selectedSpecializationId)
  }
}

const onClassChange = (val) => {
  context.selectedClassId = val
  emit('class-selected', context.selectedClassId)
}
</script>
