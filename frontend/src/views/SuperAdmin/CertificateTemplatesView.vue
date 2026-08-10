<script setup>
import { computed, onMounted, ref } from 'vue'
import { Code2, Eye, FileCode2, Loader2, Palette, Plus, Power, Save, Search, X } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { certificateTemplateApi } from '@/services/certificateTemplateApi'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

const TOKENS = [
  { key: 'hoTen', label: 'Họ tên sinh viên' },
  { key: 'mssv', label: 'Mã số sinh viên' },
  { key: 'tenHocKy', label: 'Tên học kỳ' },
  { key: 'danhHieu', label: 'Danh hiệu' },
  { key: 'xepHang', label: 'Xếp hạng' },
  { key: 'diemXet', label: 'Điểm xét' },
  { key: 'ngayCap', label: 'Ngày cấp' },
]

const TOKEN_HINT = '{{hoTen}}'

const SAMPLE_DATA = {
  hoTen: 'Nguyễn Văn A',
  mssv: 'SV000001',
  tenHocKy: 'Học kỳ 1 năm học 2026-2027',
  danhHieu: 'Top 100 học kỳ',
  xepHang: '1',
  diemXet: '9.25',
  ngayCap: new Date().toISOString().slice(0, 10),
}

const DEFAULT_HTML = `<div class="certificate">
  <div class="frame">
    <p class="org">TRƯỜNG ĐẠI HỌC LẠC HỒNG</p>
    <h1 class="title">GIẤY KHEN</h1>
    <p class="subtitle">tặng cho sinh viên</p>
    <h2 class="name">{{hoTen}}</h2>
    <p class="mssv">MSSV: {{mssv}}</p>
    <p class="body">đạt danh hiệu <strong>{{danhHieu}}</strong> — hạng <strong>{{xepHang}}</strong></p>
    <p class="body">Học kỳ: {{tenHocKy}} &nbsp;•&nbsp; Điểm xét: {{diemXet}}</p>
    <p class="footer">Ngày cấp: {{ngayCap}}</p>
  </div>
</div>`

const DEFAULT_CSS = `.certificate {
  width: 100%;
  height: 100%;
  box-sizing: border-box;
  font-family: 'Times New Roman', serif;
  display: flex;
  align-items: center;
  justify-content: center;
}
.frame {
  width: 90%;
  height: 86%;
  border: 6px double #b45309;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 24px;
  background:
    radial-gradient(circle at 50% 50%, rgba(251, 191, 36, 0.12), transparent 70%),
    linear-gradient(180deg, #fffdf5, #fef3c7);
}
.org { font-size: 18px; letter-spacing: 4px; color: #92400e; margin: 0 0 8px; }
.title { font-size: 52px; font-weight: 800; color: #b45309; margin: 0 0 12px; letter-spacing: 8px; }
.subtitle { font-size: 16px; color: #78350f; margin: 0 0 8px; font-style: italic; }
.name { font-size: 40px; font-weight: 700; color: #1e293b; margin: 0 0 4px; }
.mssv { font-size: 14px; color: #64748b; margin: 0 0 16px; }
.body { font-size: 18px; color: #334155; margin: 6px 0; }
.footer { font-size: 14px; color: #92400e; margin-top: 28px; }`

const loading = ref(true)
const saving = ref(false)
const templates = ref([])
const searchQuery = ref('')
const modalOpen = ref(false)
const modalMode = ref('create')
const editingId = ref(null)
const confirmDisable = ref(null)
const editorTab = ref('html')
const previewOpen = ref(true)
const formError = ref('')

const form = ref({
  tenMau: '',
  loaiMau: 'TOP_100_HOC_KY',
  fileNenUrl: '',
  chieuRong: 1123,
  chieuCao: 794,
  huongGiay: 'A4_NGANG',
  mode: 'html',
  html: DEFAULT_HTML,
  css: DEFAULT_CSS,
  json: '',
})

const filteredTemplates = computed(() => {
  if (!searchQuery.value) return templates.value
  const q = searchQuery.value.toLowerCase()
  return templates.value.filter((t) => (t.tenMau || '').toLowerCase().includes(q))
})

function parseConfig(json) {
  try {
    return typeof json === 'string' ? JSON.parse(json) : json
  } catch {
    return null
  }
}

async function loadTemplates() {
  loading.value = true
  try {
    const data = await certificateTemplateApi.getTemplates({ pageIndex: 1, pageSize: 100 })
    templates.value = data?.items ?? data?.Items ?? []
  } catch (err) {
    templates.value = []
    popupStore.error('Lỗi', err?.message || 'Không tải được danh sách mẫu giấy khen.')
  } finally {
    loading.value = false
  }
}

function openCreate() {
  modalMode.value = 'create'
  editingId.value = null
  form.value = {
    tenMau: '',
    loaiMau: 'TOP_100_HOC_KY',
    fileNenUrl: '',
    chieuRong: 1123,
    chieuCao: 794,
    huongGiay: 'A4_NGANG',
    mode: 'html',
    html: DEFAULT_HTML,
    css: DEFAULT_CSS,
    json: '',
  }
  editorTab.value = 'html'
  formError.value = ''
  modalOpen.value = true
}

async function openEdit(template) {
  modalMode.value = 'edit'
  editingId.value = template.maMauBangKhen
  const config = parseConfig(template.cauHinhJson) || {}
  const isHtml = String(config.mode || '').toLowerCase() === 'html'
  form.value = {
    tenMau: template.tenMau || '',
    loaiMau: template.loaiMau || 'TOP_100_HOC_KY',
    fileNenUrl: template.fileNenUrl || '',
    chieuRong: template.chieuRong || 1123,
    chieuCao: template.chieuCao || 794,
    huongGiay: template.huongGiay || 'A4_NGANG',
    mode: isHtml ? 'html' : 'fields',
    html: isHtml ? config.html : DEFAULT_HTML,
    css: isHtml ? config.css || '' : DEFAULT_CSS,
    json: template.cauHinhJson || '',
  }
  editorTab.value = isHtml ? 'html' : 'json'
  formError.value = ''
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
}

function buildConfigPayload() {
  if (form.value.mode === 'html') {
    return {
      mode: 'html',
      html: form.value.html,
      css: form.value.css || '',
    }
  }
  const parsed = JSON.parse(form.value.json)
  return parsed
}

async function saveTemplate() {
  formError.value = ''
  if (!form.value.tenMau.trim()) {
    formError.value = 'Tên mẫu không được rỗng.'
    return
  }
  if (form.value.mode === 'fields') {
    try {
      buildConfigPayload()
    } catch {
      formError.value = 'Cấu hình JSON không hợp lệ.'
      return
    }
  }

  const payload = {
    tenMau: form.value.tenMau.trim(),
    loaiMau: form.value.loaiMau,
    fileNenUrl: form.value.fileNenUrl.trim(),
    chieuRong: Number(form.value.chieuRong) || 1123,
    chieuCao: Number(form.value.chieuCao) || 794,
    huongGiay: form.value.huongGiay,
    cauHinhJson: JSON.stringify(buildConfigPayload()),
  }

  saving.value = true
  try {
    if (modalMode.value === 'create') {
      await certificateTemplateApi.createTemplate(payload)
    } else {
      await certificateTemplateApi.updateTemplate(editingId.value, payload)
    }
    popupStore.success('Thành công', modalMode.value === 'create' ? 'Đã tạo mẫu giấy khen.' : 'Đã cập nhật mẫu giấy khen.')
    closeModal()
    await loadTemplates()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Lưu mẫu giấy khen thất bại.')
  } finally {
    saving.value = false
  }
}

async function toggleActive(template) {
  try {
    await certificateTemplateApi.disableTemplate(template.maMauBangKhen)
    popupStore.success('Thành công', 'Đã vô hiệu hóa mẫu giấy khen.')
    await loadTemplates()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Vô hiệu hóa mẫu thất bại.')
  }
}

const previewDoc = computed(() => {
  const html = form.value.html.replace(/\{\{\s*([\w]+)\s*\}\}/g, (_, key) => SAMPLE_DATA[key] ?? `{{${key}}}`)
  return `<!DOCTYPE html><html lang="vi"><head><meta charset="utf-8"><style>*{box-sizing:border-box;margin:0;padding:0}html,body{width:100%;height:100%}${form.value.css}</style></head><body>${html}</body></html>`
})

const previewScale = computed(() => {
  if (!form.value.chieuRong) return 1
  return Math.min(1, 860 / form.value.chieuRong)
})

function insertToken(token) {
  form.value.html += `{{${token}}}`
}

function tokenText(token) {
  return `{{${token}}}`
}

onMounted(loadTemplates)
</script>

<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <div class="flex flex-wrap items-center gap-2">
          <h2 class="text-heading text-lg font-bold">Cấu hình giấy khen</h2>
          <GlassBadge variant="secondary">mẫu bằng khen</GlassBadge>
        </div>
        <p class="text-label mt-0.5 text-sm">Custom mẫu giấy khen bằng HTML/CSS, xem trước trực tiếp và cấp phát chứng nhận theo mẫu.</p>
      </div>
      <GlassButton variant="primary" @click="openCreate">
        <template #leading><Plus :size="16" /></template>
        Tạo mẫu mới
      </GlassButton>
    </div>

    <div class="flex flex-wrap gap-3">
      <label class="flex h-10 flex-1 min-w-[220px] items-center gap-2 rounded-lg border border-(--border-input) bg-(--surface-input) px-3 transition-shadow focus-within:ring-2 focus-within:ring-(--border-focus)">
        <Search class="h-4 w-4 text-(--text-muted)" />
        <input v-model="searchQuery" type="text" placeholder="Tìm theo tên mẫu..." class="w-full bg-transparent text-sm text-(--text-body) outline-none" />
      </label>
    </div>

    <LoadingSkeleton v-if="loading" :lines="8" />

    <div v-else-if="filteredTemplates.length === 0" class="border border-dashed border-(--border-card) rounded-2xl">
      <EmptyState
        title="Chưa có mẫu giấy khen"
        description="Tạo mẫu đầu tiên bằng HTML/CSS để tùy biến giấy khen theo ý muốn."
      >
        <GlassButton variant="primary" size="sm" @click="openCreate">
          <template #leading><Plus :size="14" /></template>
          Tạo mẫu mới
        </GlassButton>
      </EmptyState>
    </div>

    <div v-else class="overflow-hidden rounded-2xl border border-(--border-card) shadow-sm">
      <table class="w-full text-left text-sm">
        <thead class="bg-slate-50 text-xs font-bold uppercase text-(--text-muted) dark:bg-slate-800/50">
          <tr>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Tên mẫu</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Loại mẫu</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Chế độ</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Kích thước</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Trạng thái</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Cập nhật</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card) text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-(--border-card)">
          <tr v-for="t in filteredTemplates" :key="t.maMauBangKhen" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
            <td class="px-4 py-3">
              <div class="font-bold text-(--text-heading)">{{ t.tenMau }}</div>
              <div v-if="t.tenNguoiTao" class="text-xs text-(--text-placeholder)">tạo bởi {{ t.tenNguoiTao }}</div>
            </td>
            <td class="px-4 py-3 text-xs font-mono text-(--text-muted)">{{ t.loaiMau }}</td>
            <td class="px-4 py-3">
              <GlassBadge :variant="t.mode === 'html' ? 'info' : 'secondary'" size="sm">
                {{ t.mode === 'html' ? 'HTML/CSS' : 'Vị trí field' }}
              </GlassBadge>
            </td>
            <td class="px-4 py-3 text-xs text-(--text-muted)">
              {{ t.chieuRong }}×{{ t.chieuCao }}px
              <div>{{ t.huongGiay === 'A4_NGANG' ? 'A4 ngang' : 'A4 dọc' }}</div>
            </td>
            <td class="px-4 py-3">
              <GlassBadge :variant="t.conHoatDong ? 'success' : 'secondary'" size="sm">
                {{ t.conHoatDong ? 'Đang hoạt động' : 'Tạm ẩn' }}
              </GlassBadge>
            </td>
            <td class="px-4 py-3 text-xs text-(--text-muted)">{{ t.ngayCapNhat || t.ngayTao ? (t.ngayCapNhat || t.ngayTao).slice(0, 10) : '—' }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center justify-end gap-2">
                <GlassButton variant="secondary" size="sm" @click="openEdit(t)">
                  <template #leading><Code2 :size="13" /></template>
                  Sửa
                </GlassButton>
                <GlassButton v-if="t.conHoatDong" variant="ghost" size="sm" @click="confirmDisable = t">
                  <template #leading><Power :size="13" /></template>
                  Tạm ẩn
                </GlassButton>
                <GlassButton v-else variant="secondary" size="sm" @click="confirmDisable = t">
                  <template #leading><Power :size="13" /></template>
                  Kích hoạt
                </GlassButton>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="modalOpen" class="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 p-4" @click.self="closeModal">
      <div class="lg-glass-strong border-card flex max-h-[94vh] w-full max-w-6xl flex-col rounded-2xl border shadow-2xl">
        <div class="flex items-start justify-between gap-4 border-b border-(--border-card) p-5">
          <div>
            <h3 class="text-heading text-lg font-bold">{{ modalMode === 'create' ? 'Tạo mẫu giấy khen mới' : 'Sửa mẫu giấy khen' }}</h3>
            <p class="text-label mt-1 text-sm">Viết HTML/CSS tùy biến, dùng token {{ TOKEN_HINT }} để chèn dữ liệu sinh viên</p>
          </div>
          <button class="text-(--text-placeholder) hover:text-(--text-heading)" @click="closeModal">
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="overflow-y-auto p-5">
          <form class="space-y-4" @submit.prevent="saveTemplate">
            <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Tên mẫu *</label>
                <GlassInput v-model="form.tenMau" placeholder="VD: Giấy khen Top 100 học kỳ" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Loại mẫu</label>
                <GlassInput v-model="form.loaiMau" disabled />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Ảnh nền (URL)</label>
                <GlassInput v-model="form.fileNenUrl" placeholder="https://... (không bắt buộc)" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Chiều rộng (px)</label>
                <GlassInput v-model.number="form.chieuRong" type="number" min="100" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Chiều cao (px)</label>
                <GlassInput v-model.number="form.chieuCao" type="number" min="100" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Hướng giấy</label>
                <select v-model="form.huongGiay" class="h-10 w-full rounded-lg border border-(--border-input) bg-(--surface-input) px-3 text-sm text-(--text-body) outline-none transition-shadow focus:ring-2 focus:ring-(--border-focus)">
                  <option value="A4_NGANG">A4 ngang (297×210mm)</option>
                  <option value="A4_DOC">A4 dọc (210×297mm)</option>
                </select>
              </div>
            </div>

            <div class="flex flex-wrap items-center justify-between gap-2">
              <div class="flex overflow-hidden rounded-lg border border-(--border-card)">
                <button
                  :class="form.mode === 'html' ? 'bg-(--color-info-bg) text-(--color-info-text)' : 'text-(--text-placeholder)'"
                  class="px-3 py-1.5 text-xs font-medium transition-colors"
                  type="button"
                  @click="form.mode = 'html'; editorTab = 'html'"
                >
                  HTML/CSS
                </button>
                <button
                  :class="form.mode === 'fields' ? 'bg-(--color-info-bg) text-(--color-info-text)' : 'text-(--text-placeholder)'"
                  class="border-l border-(--border-card) px-3 py-1.5 text-xs font-medium transition-colors"
                  type="button"
                  @click="form.mode = 'fields'; editorTab = 'json'"
                >
                  JSON (fields)
                </button>
              </div>
              <GlassButton variant="ghost" size="sm" type="button" @click="previewOpen = !previewOpen">
                <template #leading><Eye :size="14" /></template>
                {{ previewOpen ? 'Ẩn xem trước' : 'Xem trước' }}
              </GlassButton>
            </div>

            <div v-if="form.mode === 'html'">
              <div class="mb-3 flex flex-wrap items-center gap-2">
                <span class="text-label text-xs font-semibold uppercase">Token có sẵn:</span>
                <button
                  v-for="token in TOKENS"
                  :key="token.key"
                  type="button"
                  class="rounded-md border border-(--border-card) bg-(--surface-input) px-2 py-1 font-mono text-[11px] text-(--color-info-text) transition-colors hover:bg-(--color-info-bg)"
                  :title="token.label"
                  @click="insertToken(token.key)"
                >
                  {{ tokenText(token.key) }}
                </button>
              </div>

              <div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
                <div>
                  <div class="mb-1 flex items-center justify-between">
                    <label class="text-label flex items-center gap-1.5 text-sm font-medium">
                      <FileCode2 :size="14" /> HTML
                    </label>
                    <button type="button" class="text-[11px] text-(--color-info-text) underline" @click="form.html = DEFAULT_HTML">Khôi phục mẫu</button>
                  </div>
                  <textarea
                    v-model="form.html"
                    rows="14"
                    spellcheck="false"
                    class="w-full resize-y rounded-lg border border-(--border-input) bg-(--surface-input) px-3 py-2 font-mono text-xs text-(--text-body) outline-none transition-shadow focus:ring-2 focus:ring-(--border-focus)"
                  ></textarea>
                </div>
                <div>
                  <label class="text-label mb-1 block text-sm font-medium">CSS</label>
                  <textarea
                    v-model="form.css"
                    rows="14"
                    spellcheck="false"
                    class="w-full resize-y rounded-lg border border-(--border-input) bg-(--surface-input) px-3 py-2 font-mono text-xs text-(--text-body) outline-none transition-shadow focus:ring-2 focus:ring-(--border-focus)"
                  ></textarea>
                </div>
              </div>
            </div>

            <template v-else>
              <label class="text-label mb-1 block text-sm font-medium">Cấu hình JSON (fields)</label>
              <textarea
                v-model="form.json"
                rows="14"
                spellcheck="false"
                class="w-full resize-y rounded-lg border border-(--border-input) bg-(--surface-input) px-3 py-2 font-mono text-xs text-(--text-body) outline-none transition-shadow focus:ring-2 focus:ring-(--border-focus)"
              ></textarea>
              <p class="text-placeholder mt-1 text-xs">Cấu trúc cũ: root.fields là mảng với key, x, y, fontSize, align, color, bold.</p>
            </template>

            <div v-if="previewOpen && form.mode === 'html'" class="overflow-hidden rounded-xl border border-(--border-card)">
              <div class="flex items-center justify-between border-b border-(--border-card) bg-slate-50 px-4 py-2 dark:bg-slate-800/50">
                <p class="text-label flex items-center gap-1.5 text-xs font-semibold uppercase">
                  <Palette :size="13" /> Xem trước (dữ liệu mẫu)
                </p>
                <span class="text-[11px] text-(--text-placeholder)">{{ form.chieuRong }}×{{ form.chieuCao }}px • {{ form.huongGiay === 'A4_NGANG' ? 'A4 ngang' : 'A4 dọc' }}</span>
              </div>
              <div class="flex justify-center overflow-auto bg-slate-200/70 p-4 dark:bg-slate-900/50">
                <div
                  :style="{
                    width: `${previewScale * form.chieuRong}px`,
                    height: `${previewScale * form.chieuCao}px`,
                    overflow: 'hidden',
                  }"
                >
                  <div
                    :style="{
                      width: `${form.chieuRong}px`,
                      height: `${form.chieuCao}px`,
                      transform: `scale(${previewScale})`,
                      transformOrigin: 'top left',
                      boxShadow: '0 10px 30px rgba(0,0,0,0.25)',
                    }"
                  >
                    <iframe :srcdoc="previewDoc" class="h-full w-full border-0 bg-white" title="Xem trước giấy khen" />
                  </div>
                </div>
              </div>
            </div>

            <p v-if="formError" class="text-sm text-(--color-danger-text)">{{ formError }}</p>

            <div class="flex justify-end gap-3 pt-1">
              <GlassButton variant="secondary" @click="closeModal">Hủy</GlassButton>
              <GlassButton variant="primary" :disabled="saving" @click="saveTemplate">
                <template #leading><Save v-if="!saving" :size="16" /><Loader2 v-else class="h-4 w-4 animate-spin" /></template>
                {{ saving ? 'Đang lưu...' : 'Lưu mẫu' }}
              </GlassButton>
            </div>
          </form>
        </div>
      </div>
    </div>

    <ConfirmActionDialog
      v-if="confirmDisable"
      :model-value="true"
      title="Tạm ẩn mẫu giấy khen"
      :message="`Bạn có chắc muốn tạm ẩn mẫu &quot;${confirmDisable.tenMau}&quot;? Mẫu bị tạm ẩn sẽ không dùng để cấp phát chứng nhận được.`"
      confirm-label="Tạm ẩn"
      variant="danger"
      @confirm="toggleActive(confirmDisable)"
      @cancel="confirmDisable = null"
    />
  </div>
</template>
