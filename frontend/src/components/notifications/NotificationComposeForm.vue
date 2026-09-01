<script setup>
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { Send, Eye, Users } from 'lucide-vue-next'
import { superAdminApi } from '@/services/superAdminApi'
import EditorJS from '@editorjs/editorjs'
import Paragraph from '@editorjs/paragraph'
import Header from '@editorjs/header'
import List from '@editorjs/list'
import Checklist from '@editorjs/checklist'
import Quote from '@editorjs/quote'
import Marker from '@editorjs/marker'
import InlineCode from '@editorjs/inline-code'
import Underline from '@editorjs/underline'
import Embed from '@editorjs/embed'
import SimpleImage from '@editorjs/simple-image'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'

const props = defineProps({
  loading: { type: Boolean, default: false },
})
const emit = defineEmits(['preview', 'submit'])

const popupStore = usePopupStore()
const confirmAction = ref(null)

const form = ref({
  title: '',
  excerpt: '',
  body: '',
  bodyHtml: '',
  category: 'hoc_vu',
  priority: 'thong_tin',
  scope: 'all'
})

const templates = ref([])
const selectedTemplateId = ref('')
let editor = null

const updateFormBody = async () => {
  if (editor) {
    const outputData = await editor.save()
    form.value.editorData = outputData
    const tempEl = document.createElement('div')
    let html = ''
    
    outputData.blocks.forEach(b => {
      // Plain text extraction
      if (b.data && b.data.text) tempEl.innerHTML += b.data.text + ' '
      else if (b.data && b.data.items) {
        b.data.items.forEach(i => {
           const t = (typeof i === 'string') ? i : (i.content || i.text || '')
           tempEl.innerHTML += t + ' '
        })
      }
      
      // HTML generation for preview
      if (b.type === 'paragraph') {
        html += `<p class="mb-2">${b.data.text}</p>`
      } else if (b.type === 'header') {
        html += `<h${b.data.level} class="font-bold my-2" style="font-size: ${1.5 - (b.data.level * 0.1)}rem">${b.data.text}</h${b.data.level}>`
      } else if (b.type === 'list') {
        const tag = b.data.style === 'ordered' ? 'ol' : 'ul'
        const listClass = b.data.style === 'ordered' ? 'list-decimal pl-5 mb-2' : 'list-disc pl-5 mb-2'
        html += `<${tag} class="${listClass}">`
        b.data.items.forEach(i => {
           const text = (typeof i === 'string') ? i : (i.content || i.text || '')
           html += `<li>${text}</li>`
        })
        html += `</${tag}>`
      } else if (b.type === 'checklist') {
        html += `<div class="mb-2">`
        b.data.items.forEach(i => {
          html += `<div class="flex items-start gap-2"><input type="checkbox" ${i.checked ? 'checked' : ''} disabled class="mt-1"><span>${i.text}</span></div>`
        })
        html += `</div>`
      } else if (b.type === 'quote') {
        html += `<blockquote class="border-l-4 border-gray-300 pl-4 italic my-2">${b.data.text}</blockquote>`
      } else if (b.type === 'image') {
        html += `<img src="${b.data.url}" class="max-w-full rounded-lg my-2" />`
      }
    })
    
    form.value.body = tempEl.textContent || tempEl.innerText || ''
    form.value.bodyHtml = html
  }
}

const initEditor = (initialData) => {
  if (editor) {
    editor.destroy()
    editor = null
  }
  
  if (typeof initialData === 'string' && initialData.startsWith('{')) {
    try {
      initialData = JSON.parse(initialData)
    } catch(e){}
  } else if (!initialData) {
    initialData = { blocks: [] }
  }

  editor = new EditorJS({
    holder: 'editorjs-compose',
    placeholder: 'Soạn nội dung thông báo...',
    data: initialData,
    onChange: () => {
      updateFormBody()
    },
    tools: {
      paragraph: { class: Paragraph, inlineToolbar: true },
      header: {
        class: Header,
        inlineToolbar: ['link', 'marker', 'underline', 'inlineCode'],
        config: { placeholder: 'Nhập tiêu đề...', levels: [1, 2, 3, 4, 5, 6], defaultLevel: 2 }
      },
      list: { class: List, inlineToolbar: true },
      checklist: { class: Checklist, inlineToolbar: true },
      quote: { class: Quote, inlineToolbar: true },
      marker: { class: Marker },
      inlineCode: { class: InlineCode },
      underline: { class: Underline },
      embed: { class: Embed, inlineToolbar: true },
      image: { class: SimpleImage }
    }
  })
}

onMounted(async () => {
  try {
    const res = await superAdminApi.getNotificationTemplates({ Status: 'active', PageSize: 100 })
    if (res.success && res.data) {
      templates.value = res.data.items || []
    }
    await nextTick()
    setTimeout(() => {
      initEditor('')
    }, 100)
  } catch (err) {
    console.error('Failed to load templates', err)
  }
})

onBeforeUnmount(() => {
  if (editor) {
    editor.destroy()
    editor = null
  }
})

const onTemplateSelect = async () => {
  if (!selectedTemplateId.value) {
    form.value.title = ''
    form.value.category = 'hoc_vu'
    initEditor('')
    return
  }
  try {
    const res = await superAdminApi.getNotificationTemplateDetail(selectedTemplateId.value)
    if (res.success && res.data) {
      const data = res.data
      form.value.title = data.tieuDeMau || data.tenMau
      form.value.category = data.loaiThongBao || 'hoc_vu'
      form.value.priority = data.mucDoUuTien || 'thong_tin'
      
      await nextTick()
      initEditor(data.noiDungMau)
      
      // Delay to allow editor to render before saving
      setTimeout(async () => {
        await updateFormBody()
      }, 300)
    }
  } catch (err) {
    popupStore.error('Lỗi', 'Không thể tải chi tiết mẫu')
  }
}

const categories = [
  { value: 'hoc_vu', label: 'Học vụ' },
  { value: 'hoc_phi', label: 'Học phí' },
  { value: 'system', label: 'Hệ thống' }
]

const priorities = [
  { value: 'thong_tin', label: 'Bình thường' },
  { value: 'khan_cap', label: 'Khẩn cấp' }
]

const scopes = [
  { value: 'all', label: 'Tất cả sinh viên và giảng viên' },
  { value: 'students', label: 'Chỉ sinh viên' },
  { value: 'teachers', label: 'Chỉ giảng viên' }
]

const recipientScopeLabel = computed(() => scopes.find((item) => item.value === form.value.scope)?.label || 'Chưa chọn')

function buildPayload() {
  const scopeMap = {
    all: { phamViGui: 'vai_tro', roleCodes: ['hoc_sinh', 'giao_vien'] },
    students: { phamViGui: 'vai_tro', roleCodes: ['hoc_sinh', 'Student'] },
    teachers: { phamViGui: 'vai_tro', roleCodes: ['giao_vien', 'Teacher'] },
  }
  const scope = scopeMap[form.value.scope] || scopeMap.all
  return {
    tieuDe: form.value.title.trim(),
    tomTat: form.value.excerpt.trim() || null,
    tomTatNoiDung: form.value.excerpt.trim() || null,
    noiDungText: form.value.body,
    noiDungJson: JSON.stringify(form.value.editorData || { blocks: [] }),
    mucDo: form.value.priority,
    loaiThongBao: form.value.category,
    phamViGui: scope.phamViGui,
    targetType: scope.phamViGui,
    roleCodes: scope.roleCodes,
    targetIds: [],
  }
}

async function previewRecipients() {
  await updateFormBody()
  emit('preview', buildPayload())
}

const submitForm = async () => {
  if (!form.value.title.trim()) {
    popupStore.error('Lỗi', 'Vui lòng nhập tiêu đề thông báo')
    return
  }
  
  await updateFormBody()

  if (!form.value.body.trim()) {
    popupStore.error('Lỗi', 'Vui lòng nhập nội dung thông báo')
    return
  }

  const payload = buildPayload()
  confirmAction.value = {
    title: 'Gửi thông báo?',
    message: `Thông báo "${form.value.title}" sẽ được gửi đến nhóm: ${recipientScopeLabel.value}.`,
    label: 'Gửi đi',
    variant: 'primary',
    run: () => {
      emit('submit', payload)
      confirmAction.value = null
      form.value.title = ''
      form.value.excerpt = ''
      form.value.body = ''
    }
  }
}
</script>

<template>
  <div class="notification-compose max-w-7xl mx-auto space-y-6">
    <div class="page-header mb-6">
      <h1 class="text-2xl font-bold text-(--text-heading)">Gửi thông báo</h1>
      <p class="text-(--text-body)">Soạn thảo và gửi thông báo hệ thống với chế độ xem trước.</p>
    </div>

    <div class="compose-layout">
      <div class="compose-form space-y-5">
        <GlassPanel variant="flat">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-semibold text-(--text-heading)">Nội dung</h2>
            <div class="w-64">
              <select v-model="selectedTemplateId" @change="onTemplateSelect" class="lg-control w-full text-sm">
                <option value="">-- Soạn thủ công --</option>
                <option v-for="t in templates" :key="t.maMauThongBao" :value="t.maMauThongBao">
                  Mẫu: {{ t.tenMau }}
                </option>
              </select>
            </div>
          </div>
          <div class="space-y-4">
            <GlassInput v-model="form.title" label="Tiêu đề" placeholder="Nhập tiêu đề thông báo" required />
            <GlassInput v-model="form.excerpt" label="Mô tả ngắn (Hiển thị ở inbox)" placeholder="Tóm tắt ngắn gọn 1-2 câu" />
            <div class="block">
              <span class="block text-sm font-medium text-(--text-label) mb-1">Nội dung chi tiết *</span>
              <div class="p-4 rounded-xl border border-(--border-default) bg-(--surface-card) min-h-[300px]">
                <div id="editorjs-compose" class="prose max-w-none text-[17px] leading-[1.6]"></div>
              </div>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <label class="block">
                <span class="block text-sm font-medium text-(--text-label) mb-1">Danh mục</span>
                <select v-model="form.category" class="lg-control w-full">
                  <option v-for="c in categories" :key="c.value" :value="c.value">{{ c.label }}</option>
                </select>
              </label>
              <label class="block">
                <span class="block text-sm font-medium text-(--text-label) mb-1">Độ ưu tiên</span>
                <select v-model="form.priority" class="lg-control w-full">
                  <option v-for="p in priorities" :key="p.value" :value="p.value">{{ p.label }}</option>
                </select>
              </label>
            </div>
          </div>
        </GlassPanel>

        <GlassPanel variant="flat">
          <h2 class="text-lg font-semibold mb-4 text-(--text-heading)">Phạm vi gửi</h2>
          <div class="space-y-4">
            <label class="block">
              <span class="block text-sm font-medium text-(--text-label) mb-1">Nhóm người nhận</span>
              <select v-model="form.scope" class="lg-control w-full">
                <option v-for="s in scopes" :key="s.value" :value="s.value">{{ s.label }}</option>
              </select>
            </label>
            <div class="p-4 bg-(--surface-modal) rounded-lg flex items-center justify-between">
              <span class="flex items-center gap-2 text-(--text-heading)">
                <Users :size="18" /> Phạm vi gửi
              </span>
              <strong class="text-lg">{{ recipientScopeLabel }}</strong>
            </div>
          </div>
        </GlassPanel>

        <div class="flex justify-end gap-3">
          <GlassButton variant="secondary" :disabled="props.loading" @click="previewRecipients">Xem trước người nhận</GlassButton>
          <GlassButton variant="primary" :disabled="props.loading" @click="submitForm">
            <template #leading><Send :size="16" /></template>
            {{ props.loading ? 'Đang gửi...' : 'Gửi thông báo' }}
          </GlassButton>
        </div>
      </div>

      <div class="preview-panel">
        <GlassPanel variant="readable" class="sticky top-6">
          <h2 class="text-lg font-semibold mb-4 text-(--text-heading) flex items-center gap-2">
            <Eye :size="18" /> Xem trước (Preview)
          </h2>
          <div class="preview-card border border-(--border-default) rounded-xl p-5 space-y-4 bg-(--surface-card)">
            <div class="flex gap-2">
              <GlassBadge v-if="form.priority === 'khan_cap'" variant="danger" size="sm">Khẩn cấp</GlassBadge>
              <GlassBadge variant="info" size="sm">{{ categories.find(c => c.value === form.category)?.label }}</GlassBadge>
            </div>
            <h3 class="text-xl font-bold text-(--text-heading) leading-tight">
              {{ form.title || 'Tiêu đề thông báo' }}
            </h3>
            <p class="text-(--text-muted) text-sm">Người gửi: Ban Giám Hiệu • Vừa xong</p>
            <div class="text-(--text-body) text-sm border-l-4 border-(--border-focus) pl-3 italic">
              {{ form.excerpt || 'Mô tả ngắn hiển thị ở inbox' }}
            </div>
            <div class="text-(--text-body) text-base prose prose-sm max-w-none">
              <template v-if="form.bodyHtml">
                <div v-html="form.bodyHtml"></div>
              </template>
              <template v-else>Nội dung chi tiết...</template>
            </div>
          </div>
        </GlassPanel>
      </div>
    </div>

    <ConfirmActionDialog
      v-if="confirmAction"
      :modelValue="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
  </div>
</template>

<style scoped>
.compose-layout {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 1.5rem;
  align-items: start;
}
@media (max-width: 1024px) {
  .compose-layout {
    grid-template-columns: 1fr;
  }
}
:deep(h1.ce-header) {
  font-size: 2.25rem !important;
  line-height: 1.25 !important;
  font-weight: 800 !important;
  margin-top: 1.5rem;
  margin-bottom: 0.75rem;
  color: var(--text-heading);
}
:deep(h2.ce-header) {
  font-size: 1.75rem !important;
  line-height: 1.3 !important;
  font-weight: 700 !important;
  margin-top: 1.25rem;
  margin-bottom: 0.625rem;
  color: var(--text-heading);
}
:deep(h3.ce-header) {
  font-size: 1.375rem !important;
  line-height: 1.35 !important;
  font-weight: 600 !important;
  margin-top: 1rem;
  margin-bottom: 0.5rem;
  color: var(--text-heading);
}
:deep(h4.ce-header) {
  font-size: 1.15rem !important;
  line-height: 1.4 !important;
  font-weight: 600 !important;
  margin-top: 0.75rem;
  margin-bottom: 0.375rem;
  color: var(--text-heading);
}
:deep(h5.ce-header) {
  font-size: 1rem !important;
  line-height: 1.45 !important;
  font-weight: 600 !important;
  margin-top: 0.5rem;
  margin-bottom: 0.25rem;
  color: var(--text-heading);
}
:deep(h6.ce-header) {
  font-size: 0.875rem !important;
  line-height: 1.5 !important;
  font-weight: 600 !important;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-top: 0.5rem;
  margin-bottom: 0.25rem;
  color: var(--text-muted);
}
:deep(.ce-paragraph) {
  font-size: 1rem;
  line-height: 1.65;
  margin-bottom: 0.5rem;
}
</style>
