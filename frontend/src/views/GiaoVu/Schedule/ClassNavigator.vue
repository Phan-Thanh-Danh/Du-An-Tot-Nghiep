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
      <p
        v-if="noClassNotice"
        class="mt-1.5 text-xs font-medium text-(--color-danger-text) bg-(--color-danger-bg) border border-(--color-danger-border) rounded-lg px-2.5 py-1.5"
      >
        Chuyên ngành này chưa có lớp hành chính. Vui lòng chọn chuyên ngành khác.
      </p>
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
const noClassNotice = ref(false)

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

async function loadClasses(maChuyenNganh) {
  if (!maChuyenNganh) {
    classes.value = []
    noClassNotice.value = false
    return []
  }
  let list
  try {
    list = await academicSchedulingApi.getClassesBySpecialization(maChuyenNganh)
  } catch {
    list = []
  }
  classes.value = list
  noClassNotice.value = list.length === 0
  return list
}

let initialPreselecting = false

async function preselectFirstClassableSpecialization() {
  if (initialPreselecting) return
  initialPreselecting = true
  try {
    for (const spec of specializations.value) {
      const list = await loadClasses(spec.maChuyenNganh)
      if (list.length > 0) {
        context.selectedSpecializationId = spec.maChuyenNganh
        context.selectedClassId = list[0].maLop
        emit('class-selected', context.selectedClassId)
        return
      }
    }
    noClassNotice.value = specializations.value.length > 0
  } finally {
    initialPreselecting = false
  }
}

onMounted(async () => {
  try {
    majors.value = await academicSchedulingApi.getMajors()
    if (majors.value.length === 0) return

    if (!context.selectedMajorId) {
      context.selectedMajorId = majors.value[0].maNganh
    }

    if (context.selectedMajorId) {
      specializations.value = await academicSchedulingApi.getSpecializations(context.selectedMajorId)
      const currentSpecValid = specializations.value.some(s => s.maChuyenNganh === context.selectedSpecializationId)
      if (context.selectedSpecializationId && currentSpecValid) {
        const list = await loadClasses(context.selectedSpecializationId)
        if (list.length > 0 && context.selectedClassId) {
          emit('class-selected', context.selectedClassId)
          return
        }
        if (list.length > 0 && !context.selectedClassId) {
          context.selectedClassId = list[0].maLop
          emit('class-selected', context.selectedClassId)
          return
        }
        if (list.length === 0) await preselectFirstClassableSpecialization()
      } else {
        context.selectedSpecializationId = null
        context.selectedClassId = null
        classes.value = []
        await preselectFirstClassableSpecialization()
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
  noClassNotice.value = false

  if (context.selectedMajorId) {
    specializations.value = await academicSchedulingApi.getSpecializations(context.selectedMajorId)
    await preselectFirstClassableSpecialization()
  }
}

const onSpecializationChange = async (val) => {
  context.selectedSpecializationId = val
  context.selectedClassId = null
  const list = await loadClasses(val)
  noClassNotice.value = list.length === 0
  if (list.length > 0) {
    context.selectedClassId = list[0].maLop
    emit('class-selected', context.selectedClassId)
  }
}

const onClassChange = (val) => {
  context.selectedClassId = val
  emit('class-selected', context.selectedClassId)
}
</script>