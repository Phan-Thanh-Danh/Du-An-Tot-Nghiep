<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, Eye, FileCode2, Loader2, Palette, Save } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import { certificateTemplateApi } from '@/services/certificateTemplateApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
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

const editingId = computed(() => {
  const id = Number(route.params.id)
  return Number.isFinite(id) && id > 0 ? id : null
})

const isEdit = computed(() => Boolean(editingId.value))

const loading = ref(false)
const saving = ref(false)
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

function parseConfig(json) {
  try {
    return typeof json === 'string' ? JSON.parse(json) : json
  } catch {
    return null
  }
}

async function loadTemplate() {
  if (!isEdit.value) return
  loading.value = true
  try {
    const template = await certificateTemplateApi.getTemplate(editingId.value)
    const config = parseConfig(template?.cauHinhJson) || {}
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
      json: typeof template?.cauHinhJson === 'string' ? template.cauHinhJson : JSON.stringify(template?.cauHinhJson || {}, null, 2),
    }
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không tải được mẫu giấy khen.')
    router.replace('/super-admin/awards/certificate-templates')
  } finally {
    loading.value = false
  }
}

function goBack() {
  router.push('/super-admin/awards/certificate-templates')
}

function buildConfigPayload() {
  if (form.value.mode === 'html') {
    return {
      mode: 'html',
      html: form.value.html,
      css: form.value.css || '',
    }
  }
  return JSON.parse(form.value.json)
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
    if (isEdit.value) {
      await certificateTemplateApi.updateTemplate(editingId.value, payload)
    } else {
      await certificateTemplateApi.createTemplate(payload)
    }
    popupStore.success('Thành công', isEdit.value ? 'Đã cập nhật mẫu giấy khen.' : 'Đã tạo mẫu giấy khen.')
    goBack()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Lưu mẫu giấy khen thất bại.')
  } finally {
    saving.value = false
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

onMounted(loadTemplate)
</script>

<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div class="flex items-center gap-3">
        <GlassButton variant="ghost" size="sm" @click="goBack">
          <template #leading><ArrowLeft :size="16" /></template>
          Quay lại
        </GlassButton>
        <div>
          <div class="flex flex-wrap items-center gap-2">
            <h2 class="text-heading text-lg font-bold">{{ isEdit ? 'Sửa mẫu giấy khen' : 'Tạo mẫu giấy khen mới' }}</h2>
            <GlassBadge variant="secondary">mẫu bằng khen</GlassBadge>
          </div>
          <p class="text-label mt-0.5 text-sm">Viết HTML/CSS tùy biến, dùng token {{ TOKEN_HINT }} để chèn dữ liệu sinh viên.</p>
        </div>
      </div>
    </div>

    <LoadingSkeleton v-if="loading" :lines="8" />

    <form v-else class="space-y-4" @submit.prevent="saveTemplate">
      <div class="rounded-2xl border border-(--border-card) p-5">
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
      </div>

      <div class="rounded-2xl border border-(--border-card) p-5">
        <div class="mb-4 flex flex-wrap items-center justify-between gap-2">
          <div class="flex overflow-hidden rounded-lg border border-(--border-card)">
            <button
              :class="form.mode === 'html' ? 'bg-(--color-info-bg) text-(--color-info-text)' : 'text-(--text-placeholder)'"
              class="px-3 py-1.5 text-xs font-medium transition-colors"
              type="button"
              @click="form.mode = 'html'"
            >
              HTML/CSS
            </button>
            <button
              :class="form.mode === 'fields' ? 'bg-(--color-info-bg) text-(--color-info-text)' : 'text-(--text-placeholder)'"
              class="border-l border-(--border-card) px-3 py-1.5 text-xs font-medium transition-colors"
              type="button"
              @click="form.mode = 'fields'"
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

        <div v-else>
          <label class="text-label mb-1 block text-sm font-medium">Cấu hình JSON (fields)</label>
          <textarea
            v-model="form.json"
            rows="14"
            spellcheck="false"
            class="w-full resize-y rounded-lg border border-(--border-input) bg-(--surface-input) px-3 py-2 font-mono text-xs text-(--text-body) outline-none transition-shadow focus:ring-2 focus:ring-(--border-focus)"
          ></textarea>
          <p class="text-placeholder mt-1 text-xs">Cấu trúc cũ: root.fields là mảng với key, x, y, fontSize, align, color, bold.</p>
        </div>

        <div v-if="previewOpen && form.mode === 'html'" class="mt-4 overflow-hidden rounded-xl border border-(--border-card)">
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

        <p v-if="formError" class="mt-3 text-sm text-(--color-danger-text)">{{ formError }}</p>
      </div>

      <div class="flex justify-end gap-3">
        <GlassButton variant="secondary" @click="goBack">Hủy</GlassButton>
        <GlassButton variant="primary" :disabled="saving" @click="saveTemplate">
          <template #leading><Save v-if="!saving" :size="16" /><Loader2 v-else class="h-4 w-4 animate-spin" /></template>
          {{ saving ? 'Đang lưu...' : 'Lưu mẫu' }}
        </GlassButton>
      </div>
    </form>
  </div>
</template>
