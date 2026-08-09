<script setup>
import { computed, ref, watch } from 'vue'
import {
  AlignLeft,
  Calendar,
  CheckSquare,
  ChevronDown,
  ChevronUp,
  Database,
  GripVertical,
  Hash,
  ListChecks,
  Mail,
  Pencil,
  Phone,
  Plus,
  Trash2,
  Type,
  User,
  UserCheck,
} from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'

const props = defineProps({
  modelValue: { type: Object, default: () => ({ fields: [] }) },
})

const emit = defineEmits(['update:modelValue'])

const PALETTE = [
  { type: 'studentInfo', label: 'Thông tin sinh viên', icon: User, badge: 'Tự động' },
  { type: 'text', label: 'Ô văn bản', icon: Type, badge: 'text' },
  { type: 'textarea', label: 'Đoạn văn bản dài', icon: AlignLeft, badge: 'textarea' },
  { type: 'number', label: 'Số', icon: Hash, badge: 'number' },
  { type: 'date', label: 'Ngày', icon: Calendar, badge: 'date' },
  { type: 'email', label: 'Email', icon: Mail, badge: 'email' },
  { type: 'tel', label: 'Số điện thoại', icon: Phone, badge: 'tel' },
  { type: 'select', label: 'Chọn 1 lựa chọn', icon: ListChecks, badge: 'select' },
  { type: 'multiselect', label: 'Chọn nhiều lựa chọn', icon: CheckSquare, badge: 'multiselect' },
  { type: 'boolean', label: 'Có / Không', icon: CheckSquare, badge: 'boolean' },
  { type: 'select', label: 'Dropdown từ CSDL', icon: Database, badge: 'autoFill', dynamic: true },
  { type: 'text', label: 'Thông tin hiện tại (tự điền)', icon: UserCheck, badge: 'prefill', prefill: true },
]

const DATA_SOURCES = [
  { value: 'campuses', label: 'Cơ sở', kind: 'dropdown', relatedEntity: 'don_vi' },
  { value: 'majors', label: 'Ngành đào tạo', kind: 'dropdown', relatedEntity: 'nganh' },
  { value: 'specializationsByMajor', label: 'Chuyên ngành theo ngành', kind: 'dropdown', needsDependsOn: true },
  { value: 'studentSemesters', label: 'Học kỳ của sinh viên', kind: 'dropdown' },
  { value: 'availableSemesters', label: 'Học kỳ áp dụng', kind: 'dropdown' },
  { value: 'availableRetakeSubjects', label: 'Khóa học có thể thi lại', kind: 'dropdown', relatedEntity: 'khoa_hoc' },
  { value: 'availableRegradeScores', label: 'Điểm có thể phúc tra', kind: 'dropdown' },
  { value: 'availableExamSessions', label: 'Ca thi (theo khóa học)', kind: 'dropdown', needsDependsOn: true },
  { value: 'studentEmail', label: 'Email sinh viên', kind: 'prefill' },
  { value: 'currentCampus', label: 'Cơ sở hiện tại', kind: 'prefill' },
  { value: 'currentMajor', label: 'Ngành hiện tại', kind: 'prefill' },
  { value: 'currentClass', label: 'Lớp hành chính hiện tại', kind: 'prefill' },
]

const UNKNOWN_TYPE_LABELS = {
  datetime: 'Ngày giờ',
  related_entity: 'Tham chiếu dữ liệu',
}

const TYPE_OPTIONS = [
  {
    group: 'Nhập liệu',
    options: [
      { value: 'text', label: 'Ô văn bản', desc: 'Nhập một dòng ngắn' },
      { value: 'textarea', label: 'Đoạn văn bản', desc: 'Nhập nội dung dài nhiều dòng' },
      { value: 'number', label: 'Số', desc: 'Nhập số' },
      { value: 'date', label: 'Ngày', desc: 'Chọn ngày từ lịch' },
      { value: 'email', label: 'Email', desc: 'Nhập địa chỉ email' },
      { value: 'tel', label: 'Số điện thoại', desc: 'Nhập số điện thoại' },
    ],
  },
  {
    group: 'Lựa chọn (danh sách tự nhập)',
    options: [
      { value: 'select', label: 'Dropdown', desc: 'Chọn 1 trong danh sách bạn nhập tay' },
      { value: 'multiselect', label: 'Chọn nhiều', desc: 'Chọn nhiều trong danh sách bạn nhập tay' },
      { value: 'boolean', label: 'Có / Không', desc: 'Lựa chọn đúng / sai' },
    ],
  },
  {
    group: 'Tự động điền thông tin',
    options: [
      { value: 'studentInfo', label: 'Thông tin sinh viên', desc: 'Tự động hiển thị MSSV, họ tên, lớp, ngành' },
      { value: 'source:studentEmail', label: 'Email sinh viên', desc: 'Tự động điền email của sinh viên' },
      { value: 'source:currentCampus', label: 'Cơ sở hiện tại', desc: 'Tự động điền cơ sở của sinh viên' },
      { value: 'source:currentMajor', label: 'Ngành hiện tại', desc: 'Tự động điền ngành đang học' },
      { value: 'source:currentClass', label: 'Lớp hành chính', desc: 'Tự động điền lớp hành chính' },
    ],
  },
  {
    group: 'Dropdown lấy dữ liệu từ CSDL',
    options: [
      { value: 'source:campuses', label: 'Cơ sở đào tạo', desc: 'Tải danh sách cơ sở từ hệ thống' },
      { value: 'source:majors', label: 'Ngành đào tạo', desc: 'Tải danh sách ngành từ hệ thống' },
      { value: 'source:specializationsByMajor', label: 'Chuyên ngành theo ngành', desc: 'Cần chọn field ngành trước' },
      { value: 'source:studentSemesters', label: 'Học kỳ của sinh viên', desc: 'Các học kỳ sinh viên đã học' },
      { value: 'source:availableSemesters', label: 'Học kỳ áp dụng', desc: 'Các học kỳ đang mở đăng ký' },
      { value: 'source:availableRetakeSubjects', label: 'Khóa học thi lại', desc: 'Khóa học rớt có ca thi mở' },
      { value: 'source:availableRegradeScores', label: 'Điểm phúc tra', desc: 'Các môn có điểm phúc tra được' },
      { value: 'source:availableExamSessions', label: 'Ca thi', desc: 'Ca thi mở, cần chọn khóa học trước' },
    ],
  },
]

let idSeq = 0
const fields = ref(Array.isArray(props.modelValue.fields) ? props.modelValue.fields.map(cloneField) : [])
const selectedId = ref(null)
const dragItem = ref(null)
const dragType = ref(null)
const dragFieldId = ref(null)
const dragOverIndex = ref(null)
const addDialogOpen = ref(false)
const addStep = ref(1)
const addPendingType = ref(null)
const addTitle = ref('')
const editDialogOpen = ref(false)
const editFieldId = ref(null)
const editTitle = ref('')
const editTypeValue = ref('text')

watch(
  () => props.modelValue,
  (value) => {
    const incoming = Array.isArray(value?.fields) ? value.fields : []
    const currentJson = JSON.stringify(fields.value)
    const incomingJson = JSON.stringify(incoming)
    if (currentJson !== incomingJson) {
      fields.value = incoming.map(cloneField)
      idSeq = Math.max(...fields.value.map((f) => f.__id ?? 0), 0)
    }
  },
  { deep: true },
)

watch(
  () => JSON.stringify(fields.value),
  () => emit('update:modelValue', { fields: fields.value.map(stripInternal) }),
)

const selectedField = computed(() => fields.value.find((f) => f.__id === selectedId.value) ?? null)

const fieldsWithOptions = computed(() =>
  selectedField.value && ['select', 'multiselect', 'related_entity'].includes(selectedField.value.type),
)

const sourceOptions = computed(() => {
  if (!selectedField.value) return []
  const allowedKinds = ['select', 'multiselect', 'related_entity'].includes(selectedField.value.type)
    ? ['dropdown']
    : ['prefill']
  return [
    { value: '', label: 'Tự nhập (options tĩnh)', kind: 'static' },
    ...DATA_SOURCES.filter((source) => allowedKinds.includes(source.kind)),
  ]
})

const otherFieldKeys = computed(() =>
  fields.value.filter((f) => f.__id !== selectedId.value).map((f) => f.key),
)

const needsDependsOn = computed(() => {
  const source = DATA_SOURCES.find((s) => s.value === selectedField.value?.autoFill)
  return Boolean(source?.needsDependsOn)
})

const editField = computed(() => fields.value.find((f) => f.__id === editFieldId.value) ?? null)

const addTypeLabel = computed(() => {
  const flat = TYPE_OPTIONS.flatMap((g) => g.options)
  return flat.find((o) => o.value === addPendingType.value)?.label ?? 'Trường'
})

function fieldTypeLabel(field) {
  if (field.type === 'studentInfo') return 'Tự động: thông tin SV'
  if (field.autoFill) {
    const source = DATA_SOURCES.find((s) => s.value === field.autoFill)
    if (source?.kind === 'prefill') return 'Tự động điền'
    return `CSDL: ${source?.label ?? field.autoFill}`
  }
  if (field.type === 'select') return 'Dropdown'
  if (field.type === 'multiselect') return 'Chọn nhiều'
  if (field.type === 'boolean') return 'Có / Không'
  if (['text', 'textarea', 'number', 'date', 'email', 'tel'].includes(field.type)) return 'Nhập liệu'
  return field.type
}

function applyTypeToField(field, selection) {
  if (selection === 'studentInfo') {
    field.type = 'studentInfo'
    field.readonly = true
    delete field.autoFill
    delete field.relatedEntity
    delete field.dependsOn
    delete field.options
    return
  }

  const source = selection.startsWith('source:')
    ? DATA_SOURCES.find((s) => s.value === selection.slice(7))
    : null
  if (source) {
    if (source.kind === 'prefill') {
      field.type = 'text'
      field.readonly = true
      delete field.options
    } else {
      field.type = 'select'
      delete field.options
    }
    field.autoFill = source.value
    if (source.relatedEntity) field.relatedEntity = source.relatedEntity
    else delete field.relatedEntity
    if (source.needsDependsOn && !field.dependsOn) field.dependsOn = ''
    if (!source.needsDependsOn) delete field.dependsOn
    return
  }

  delete field.autoFill
  delete field.relatedEntity
  delete field.dependsOn
  field.readonly = false
  field.type = selection
  if (selection === 'select' || selection === 'multiselect') {
    if (!Array.isArray(field.options) || !field.options.length) {
      field.options = [
        { value: 'lua_chon_1', label: 'Lựa chọn 1' },
        { value: 'lua_chon_2', label: 'Lựa chọn 2' },
      ]
    }
  } else {
    delete field.options
  }
}

function editTypeValueFor(field) {
  if (field.type === 'studentInfo') return 'studentInfo'
  if (field.autoFill) return `source:${field.autoFill}`
  return field.type
}

function openAddDialog() {
  addPendingType.value = null
  addTitle.value = ''
  addStep.value = 1
  addDialogOpen.value = true
}

function openAddWithType(item) {
  let value = item.type
  if (item.dynamic) value = 'source:campuses'
  else if (item.prefill) value = 'source:currentCampus'
  addPendingType.value = value
  addTitle.value = item.label
  addStep.value = 2
  addDialogOpen.value = true
}

function selectAddType(option) {
  addPendingType.value = option.value
  addTitle.value = option.label
  addStep.value = 2
}

function closeAddDialog() {
  addDialogOpen.value = false
  addStep.value = 1
  addPendingType.value = null
  addTitle.value = ''
}

function confirmAdd() {
  if (!addPendingType.value) return
  const field = { __id: ++idSeq, type: 'text', key: '', label: '', required: false }
  applyTypeToField(field, addPendingType.value)
  const title = addTitle.value.trim()
  if (title) {
    field.label = title
    field.key = slugify(title)
  } else {
    field.label = addTypeLabel.value
    field.key = slugify(addTypeLabel.value)
  }
  fields.value.push(field)
  selectedId.value = field.__id
  closeAddDialog()
}

function openEditDialog(field) {
  editFieldId.value = field.__id
  editTitle.value = field.label
  editTypeValue.value = editTypeValueFor(field)
  editDialogOpen.value = true
}

function closeEditDialog() {
  editDialogOpen.value = false
  editFieldId.value = null
}

function confirmEdit() {
  const field = editField.value
  if (!field) return
  const title = editTitle.value.trim()
  if (title) field.label = title
  if (editTypeValue.value !== editTypeValueFor(field)) {
    applyTypeToField(field, editTypeValue.value)
  }
  closeEditDialog()
}

function cloneField(field) {
  return { ...field, options: field.options ? field.options.map((o) => ({ ...o })) : undefined, __id: ++idSeq }
}

function stripInternal(field) {
  const rest = { ...field }
  delete rest.__id
  return rest
}

function addField(type, index = fields.value.length, paletteItemOverride = null) {
  const paletteItem = paletteItemOverride ?? PALETTE.find((item) => item.type === type)
  const field = {
    __id: ++idSeq,
    type,
    key: slugify(paletteItem?.label ?? type),
    label: paletteItem?.label ?? (UNKNOWN_TYPE_LABELS[type] ?? type),
    required: false,
  }
  if (type === 'studentInfo') field.readonly = true
  if (paletteItem?.dynamic) {
    field.autoFill = 'campuses'
    field.relatedEntity = 'don_vi'
  }
  if (paletteItem?.prefill) {
    field.autoFill = 'currentCampus'
    field.readonly = true
  }
  if (['select', 'multiselect'].includes(type) && !paletteItem?.dynamic) {
    field.options = [
      { value: 'lua_chon_1', label: 'Lựa chọn 1' },
      { value: 'lua_chon_2', label: 'Lựa chọn 2' },
    ]
  }
  fields.value.splice(index, 0, field)
  selectedId.value = field.__id
}

function onSourceChange() {
  const field = selectedField.value
  if (!field) return
  const source = DATA_SOURCES.find((s) => s.value === field.autoFill)
  if (!source) {
    delete field.autoFill
    delete field.relatedEntity
    delete field.dependsOn
    return
  }
  if (source.kind === 'prefill') {
    field.type = 'text'
    field.readonly = true
    delete field.options
  } else if (['select', 'multiselect'].includes(field.type)) {
    delete field.options
  }
  if (source.relatedEntity) field.relatedEntity = source.relatedEntity
  else delete field.relatedEntity
  if (source.needsDependsOn && !field.dependsOn) field.dependsOn = otherFieldKeys.value[0] ?? ''
  if (!source.needsDependsOn) delete field.dependsOn
}

function duplicateField(field) {
  const copy = cloneField({ ...field, key: `${field.key}_copy` })
  const index = fields.value.findIndex((f) => f.__id === field.__id)
  fields.value.splice(index + 1, 0, copy)
  selectedId.value = copy.__id
}

function removeField(field) {
  fields.value = fields.value.filter((f) => f.__id !== field.__id)
  if (selectedId.value === field.__id) selectedId.value = null
}

function moveField(field, direction) {
  const index = fields.value.findIndex((f) => f.__id === field.__id)
  const target = index + direction
  if (target < 0 || target >= fields.value.length) return
  const [moved] = fields.value.splice(index, 1)
  fields.value.splice(target, 0, moved)
}

function onPaletteDragStart(event, item) {
  dragItem.value = item
  dragType.value = item.type
  event.dataTransfer.effectAllowed = 'copy'
  event.dataTransfer.setData('text/plain', item.type)
}

function onFieldDragStart(event, field) {
  dragFieldId.value = field.__id
  event.dataTransfer.effectAllowed = 'move'
  event.dataTransfer.setData('text/plain', field.__id.toString())
}

function onDragEnd() {
  dragItem.value = null
  dragType.value = null
  dragFieldId.value = null
  dragOverIndex.value = null
}

function onDrop(event, index) {
  event.preventDefault()
  const data = event.dataTransfer.getData('text/plain')
  if (dragItem.value) {
    addField(dragItem.value.type, index, dragItem.value)
  } else if (dragFieldId.value != null && data === dragFieldId.value.toString()) {
    const from = fields.value.findIndex((f) => f.__id === dragFieldId.value)
    if (from === -1 || from === index) return
    const [moved] = fields.value.splice(from, 1)
    const adjusted = index > from ? index - 1 : index
    fields.value.splice(adjusted, 0, moved)
  }
  onDragEnd()
}

function slugify(label) {
  return label
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9]+/g, '_')
    .replace(/^_+|_+$/g, '')
    .slice(0, 80)
}

function addOption() {
  if (!selectedField.value?.options) return
  const index = selectedField.value.options.length + 1
  selectedField.value.options.push({ value: `lua_chon_${index}`, label: `Lựa chọn ${index}` })
}

function removeOption(index) {
  selectedField.value?.options?.splice(index, 1)
}
defineExpose({
  fields,
  openEditDialog,
  removeField,
  openAddDialog,
})
</script>

<template>
  <div class="grid grid-cols-1 gap-4 md:grid-cols-[220px_1fr_260px]">
    <div class="border-card rounded-xl border bg-(--surface-card)">
      <p class="text-label border-(--border-card) border-b px-3 py-2.5 text-xs font-bold uppercase tracking-wide">
        Trường dữ liệu
      </p>
      <div class="space-y-2 p-3">
        <div
          v-for="item in PALETTE"
          :key="item.label"
          draggable="true"
          class="cursor-grab rounded-lg border border-(--border-card) p-2.5 transition-all hover:border-(--lg-primary) hover:bg-(--color-info-bg)"
          @dragstart="onPaletteDragStart($event, item)"
          @dragend="onDragEnd"
        >
          <div class="flex items-center gap-2">
            <component :is="item.icon" class="h-4 w-4 text-(--text-label)" />
            <span class="text-body flex-1 text-sm font-medium">{{ item.label }}</span>
            <button
              type="button"
              class="rounded-md p-1 text-(--text-placeholder) transition-colors hover:bg-(--lg-primary)/10 hover:text-(--lg-primary)"
              title="Thêm vào biểu mẫu"
              @click.stop="openAddWithType(item)"
            >
              <Plus :size="14" />
            </button>
          </div>
          <span class="mt-1 inline-block rounded bg-(--color-info-bg) px-1.5 py-0.5 text-[10px] text-(--color-info-text)">
            {{ item.badge }}
          </span>
        </div>
        <p class="text-placeholder px-1 pt-1 text-xs">Nhấn + hoặc kéo vào vùng biểu mẫu bên phải để thêm.</p>
      </div>
    </div>

    <div
      class="border-card rounded-xl border bg-(--surface-card) p-3"
      @dragover.prevent
      @drop="onDrop($event, fields.length)"
    >
      <div class="space-y-2">
        <div
          v-if="!fields.length"
          class="rounded-xl border border-dashed border-(--border-card) p-8 text-center text-sm text-(--text-placeholder)"
        >
          Chưa có trường nào — kéo một trường từ cột bên trái vào đây.
        </div>
        <div
          v-for="(field, index) in fields"
          :key="field.__id"
          :class="[
            'group rounded-xl border p-3 transition-all',
            selectedId === field.__id ? 'border-(--lg-primary) ring-2 ring-(--lg-primary)/20' : 'border-(--border-card)',
          ]"
          :draggable="dragFieldId === field.__id"
          @dragover.prevent="dragOverIndex = index"
          @dragleave="dragOverIndex === index && (dragOverIndex = null)"
          @drop="onDrop($event, index)"
          @click="selectedId = field.__id"
        >
          <div class="flex items-center gap-2">
            <GripVertical
              class="h-4 w-4 shrink-0 cursor-grab text-(--text-placeholder) hover:text-(--text-body)"
              @mousedown.prevent
              @dragstart="onFieldDragStart($event, field)"
            />
            <div class="min-w-0 flex-1">
              <p class="text-heading truncate text-sm font-medium">{{ field.label }}</p>
              <p class="text-placeholder truncate text-xs">{{ field.key }}</p>
            </div>
            <span class="rounded bg-(--color-info-bg) px-1.5 py-0.5 text-[10px] text-(--color-info-text)">
              {{ fieldTypeLabel(field) }}
            </span>
            <span v-if="field.required" class="text-sm text-(--color-danger-text)">*</span>
            <span v-if="field.readonly" class="text-[10px] text-(--color-info-text)">tự động</span>
            <div class="flex items-center gap-0.5">
              <button
                class="rounded p-1 text-(--text-placeholder) hover:bg-(--lg-primary)/10 hover:text-(--lg-primary)"
                title="Sửa tiêu đề / loại trường"
                @click.stop="openEditDialog(field)"
              >
                <Pencil class="h-3.5 w-3.5" />
              </button>
              <button class="rounded p-1 text-(--text-placeholder) hover:bg-(--color-info-bg) hover:text-(--text-body)" @click.stop="moveField(field, -1)">
                <ChevronUp class="h-3.5 w-3.5" />
              </button>
              <button class="rounded p-1 text-(--text-placeholder) hover:bg-(--color-info-bg) hover:text-(--text-body)" @click.stop="moveField(field, 1)">
                <ChevronDown class="h-3.5 w-3.5" />
              </button>
              <button class="rounded p-1 text-(--color-danger-text) hover:bg-(--color-danger-bg)" @click.stop="removeField(field)">
                <Trash2 class="h-3.5 w-3.5" />
              </button>
            </div>
          </div>
        </div>
        <button
          type="button"
          class="flex w-full items-center justify-center gap-2 rounded-xl border border-dashed border-(--border-card) p-3 text-sm text-(--text-placeholder) transition-all hover:border-(--lg-primary) hover:text-(--lg-primary)"
          @click="openAddDialog"
        >
          <Plus :size="15" />
          Thêm trường mới
        </button>
      </div>
    </div>

    <div class="border-card rounded-xl border bg-(--surface-card) p-4">
      <p class="text-label mb-3 text-xs font-bold uppercase tracking-wide">Cấu hình trường</p>
      <template v-if="selectedField">
        <div class="space-y-3">
          <div>
            <label class="text-label mb-1 block text-xs font-medium">Tên hiển thị</label>
            <GlassInput v-model="selectedField.label" placeholder="VD: Lý do xin nghỉ" />
          </div>
          <div>
            <label class="text-label mb-1 block text-xs font-medium">Khóa (key)</label>
            <GlassInput v-model="selectedField.key" :disabled="selectedField.type === 'studentInfo'" placeholder="VD: ly_do" />
          </div>
          <label v-if="!selectedField.readonly" class="flex items-center gap-2 text-sm text-(--text-body)">
            <input v-model="selectedField.required" type="checkbox" class="accent-(--lg-primary)" />
            Bắt buộc nhập
          </label>
          <div v-if="['text', 'textarea', 'select'].includes(selectedField.type)">
            <label class="text-label mb-1 block text-xs font-medium">Gợi ý nhập (placeholder)</label>
            <GlassInput v-model="selectedField.placeholder" placeholder="VD: Nhập lý do..." />
          </div>
          <div v-if="selectedField.type === 'textarea'">
            <label class="text-label mb-1 block text-xs font-medium">Độ dài tối đa (ký tự)</label>
            <GlassInput v-model.number="selectedField.maxLength" type="number" min="1" max="5000" />
          </div>
          <div v-if="['select', 'multiselect', 'text'].includes(selectedField.type)">
            <label class="text-label mb-1 block text-xs font-medium">Nguồn dữ liệu</label>
            <select
              v-model="selectedField.autoFill"
              class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all"
              @change="onSourceChange"
            >
              <option v-for="source in sourceOptions" :key="source.value" :value="source.value">
                {{ source.label }}
              </option>
            </select>
            <p v-if="selectedField.autoFill" class="text-placeholder mt-1 text-xs">
              {{
                DATA_SOURCES.find((s) => s.value === selectedField.autoFill)?.kind === 'prefill'
                  ? 'Giá trị tự động điền cho sinh viên, không thể chỉnh sửa.'
                  : 'Danh sách lựa chọn được tải tự động từ hệ thống khi sinh viên mở biểu mẫu.'
              }}
            </p>
          </div>

          <div v-if="needsDependsOn && selectedField.autoFill">
            <label class="text-label mb-1 block text-xs font-medium">Field cha (phụ thuộc)</label>
            <select
              v-model="selectedField.dependsOn"
              class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all"
            >
              <option v-for="key in otherFieldKeys" :key="key" :value="key">{{ key }}</option>
            </select>
            <p class="text-placeholder mt-1 text-xs">Danh sách này tải lại theo giá trị của field cha.</p>
          </div>

          <div v-if="fieldsWithOptions && !selectedField.autoFill">
            <p class="text-label mb-2 text-xs font-medium">Danh sách lựa chọn</p>
            <div v-for="(option, index) in selectedField.options" :key="index" class="mb-2 flex items-center gap-2">
              <GlassInput v-model="option.label" placeholder="Nhãn" class="flex-1" />
              <GlassInput v-model="option.value" placeholder="Giá trị" class="flex-1" />
              <button class="p-1 text-(--color-danger-text)" @click="removeOption(index)">
                <Trash2 class="h-3.5 w-3.5" />
              </button>
            </div>
            <GlassButton variant="secondary" size="sm" @click="addOption">
              <template #leading><Plus :size="14" /></template>
              Thêm lựa chọn
            </GlassButton>
          </div>
          <p v-if="selectedField.type === 'studentInfo'" class="text-placeholder text-xs">
            Trường tự động hiển thị thông tin sinh viên, không thể chỉnh sửa.
          </p>
        </div>
        <div class="mt-4 border-(--border-card) border-t pt-3">
          <GlassButton variant="secondary" size="sm" @click="duplicateField(selectedField)">
            <template #leading><Plus :size="14" /></template>
            Nhân bản trường
          </GlassButton>
        </div>
      </template>
      <p v-else class="text-placeholder text-sm">Chọn một trường trong biểu mẫu để cấu hình.</p>
    </div>
  </div>

  <div
    v-if="addDialogOpen"
    class="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 p-4"
    @click.self="closeAddDialog"
  >
    <div class="lg-glass-strong border-card max-h-[85vh] w-full max-w-lg overflow-y-auto rounded-2xl border p-5 shadow-2xl">
      <h4 class="text-heading mb-1 text-base font-bold">Thêm trường mới</h4>
      <p class="text-placeholder mb-4 text-xs">Chọn loại trường, sau đó đặt tiêu đề cho trường.</p>

      <template v-if="addStep === 1">
        <div class="space-y-4">
          <div v-for="group in TYPE_OPTIONS" :key="group.group">
            <p class="text-label mb-1.5 text-xs font-semibold uppercase tracking-wide">{{ group.group }}</p>
            <div class="grid grid-cols-2 gap-2">
              <button
                v-for="opt in group.options"
                :key="opt.value"
                type="button"
                class="rounded-lg border border-(--border-card) p-2.5 text-left transition-all hover:border-(--lg-primary) hover:bg-(--color-info-bg)"
                @click="selectAddType(opt)"
              >
                <span class="text-body block text-sm font-medium">{{ opt.label }}</span>
                <span class="text-placeholder mt-0.5 block text-[11px]">{{ opt.desc }}</span>
              </button>
            </div>
          </div>
        </div>
      </template>

      <template v-else>
        <div class="space-y-3">
          <div>
            <label class="text-label mb-1 block text-xs font-medium">Loại trường</label>
            <GlassInput :model-value="addTypeLabel" disabled />
          </div>
          <div>
            <label class="text-label mb-1 block text-xs font-medium">Tiêu đề trường *</label>
            <GlassInput v-model="addTitle" placeholder="VD: Lý do xin nghỉ" @keyup.enter="confirmAdd" />
            <p class="text-placeholder mt-1 text-xs">Khóa (key) của trường tự sinh từ tiêu đề.</p>
          </div>
          <div class="flex justify-end gap-2 pt-1">
            <GlassButton variant="secondary" size="sm" @click="addStep = 1">Quay lại</GlassButton>
            <GlassButton variant="primary" size="sm" @click="confirmAdd">Thêm trường</GlassButton>
          </div>
        </div>
      </template>
    </div>
  </div>

  <div
    v-if="editDialogOpen && editField"
    class="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 p-4"
    @click.self="closeEditDialog"
  >
    <div class="lg-glass-strong border-card w-full max-w-md rounded-2xl border p-5 shadow-2xl">
      <h4 class="text-heading mb-1 text-base font-bold">Sửa trường</h4>
      <p class="text-placeholder mb-4 text-xs">Đổi tiêu đề hoặc thay loại dữ liệu của trường.</p>
      <div class="space-y-3">
        <div>
          <label class="text-label mb-1 block text-xs font-medium">Tiêu đề trường *</label>
          <GlassInput v-model="editTitle" placeholder="VD: Lý do xin nghỉ" @keyup.enter="confirmEdit" />
        </div>
        <div>
          <label class="text-label mb-1 block text-xs font-medium">Loại dữ liệu</label>
          <select
            v-model="editTypeValue"
            class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all"
          >
            <optgroup v-for="group in TYPE_OPTIONS" :key="group.group" :label="group.group">
              <option v-for="opt in group.options" :key="opt.value" :value="opt.value">
                {{ opt.label }} — {{ opt.desc }}
              </option>
            </optgroup>
          </select>
          <p v-if="editTypeValue.startsWith('source:')" class="text-placeholder mt-1 text-xs">
            Dữ liệu được tải tự động từ hệ thống khi sinh viên mở biểu mẫu.
          </p>
        </div>
        <div class="flex justify-end gap-2 pt-1">
          <GlassButton variant="secondary" size="sm" @click="closeEditDialog">Hủy</GlassButton>
          <GlassButton variant="primary" size="sm" @click="confirmEdit">Lưu</GlassButton>
        </div>
      </div>
    </div>
  </div>
</template>
