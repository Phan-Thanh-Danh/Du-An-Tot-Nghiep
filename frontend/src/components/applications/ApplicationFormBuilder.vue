<script setup>
import { computed, ref, watch } from 'vue'
import {
  AlignLeft,
  Calendar,
  CheckSquare,
  ChevronDown,
  ChevronUp,
  GripVertical,
  Hash,
  ListChecks,
  Mail,
  Phone,
  Plus,
  Trash2,
  Type,
  User,
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
]

const UNKNOWN_TYPE_LABELS = {
  datetime: 'Ngày giờ',
  related_entity: 'Tham chiếu dữ liệu',
}

const fields = ref(Array.isArray(props.modelValue.fields) ? props.modelValue.fields.map(cloneField) : [])
const selectedId = ref(null)
const dragType = ref(null)
const dragFieldId = ref(null)
const dragOverIndex = ref(null)
let idSeq = 0

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
  selectedField.value && ['select', 'multiselect'].includes(selectedField.value.type),
)

function cloneField(field) {
  return { ...field, options: field.options ? field.options.map((o) => ({ ...o })) : undefined, __id: ++idSeq }
}

function stripInternal(field) {
  const rest = { ...field }
  delete rest.__id
  return rest
}

function addField(type, index = fields.value.length) {
  const paletteItem = PALETTE.find((item) => item.type === type)
  const field = {
    __id: ++idSeq,
    type,
    key: slugify(paletteItem?.label ?? type),
    label: paletteItem?.label ?? (UNKNOWN_TYPE_LABELS[type] ?? type),
    required: false,
  }
  if (type === 'studentInfo') field.readonly = true
  if (['select', 'multiselect'].includes(type)) {
    field.options = [
      { value: 'lua_chon_1', label: 'Lựa chọn 1' },
      { value: 'lua_chon_2', label: 'Lựa chọn 2' },
    ]
  }
  fields.value.splice(index, 0, field)
  selectedId.value = field.__id
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

function onPaletteDragStart(event, type) {
  dragType.value = type
  event.dataTransfer.effectAllowed = 'copy'
  event.dataTransfer.setData('text/plain', type)
}

function onFieldDragStart(event, field) {
  dragFieldId.value = field.__id
  event.dataTransfer.effectAllowed = 'move'
  event.dataTransfer.setData('text/plain', field.__id.toString())
}

function onDragEnd() {
  dragType.value = null
  dragFieldId.value = null
  dragOverIndex.value = null
}

function onDrop(event, index) {
  event.preventDefault()
  const data = event.dataTransfer.getData('text/plain')
  if (dragType.value) {
    addField(dragType.value, index)
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
          :key="item.type"
          draggable="true"
          class="cursor-grab rounded-lg border border-(--border-card) p-2.5 transition-all hover:border-(--lg-primary) hover:bg-(--color-info-bg)"
          @dragstart="onPaletteDragStart($event, item.type)"
          @dragend="onDragEnd"
        >
          <div class="flex items-center gap-2">
            <component :is="item.icon" class="h-4 w-4 text-(--text-label)" />
            <span class="text-body text-sm font-medium">{{ item.label }}</span>
          </div>
          <span class="mt-1 inline-block rounded bg-(--color-info-bg) px-1.5 py-0.5 text-[10px] text-(--color-info-text)">
            {{ item.badge }}
          </span>
        </div>
        <p class="text-placeholder px-1 pt-1 text-xs">Kéo vào vùng biểu mẫu bên phải để thêm.</p>
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
              {{ field.type }}
            </span>
            <span v-if="field.required" class="text-sm text-(--color-danger-text)">*</span>
            <span v-if="field.readonly" class="text-[10px] text-(--color-info-text)">tự động</span>
            <div class="flex items-center gap-0.5 opacity-0 transition-opacity group-hover:opacity-100">
              <button class="p-1 text-(--text-placeholder) hover:text-(--text-body)" @click.stop="moveField(field, -1)">
                <ChevronUp class="h-3.5 w-3.5" />
              </button>
              <button class="p-1 text-(--text-placeholder) hover:text-(--text-body)" @click.stop="moveField(field, 1)">
                <ChevronDown class="h-3.5 w-3.5" />
              </button>
              <button class="p-1 text-(--color-danger-text)" @click.stop="removeField(field)">
                <Trash2 class="h-3.5 w-3.5" />
              </button>
            </div>
          </div>
        </div>
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
          <div v-if="fieldsWithOptions">
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
</template>
