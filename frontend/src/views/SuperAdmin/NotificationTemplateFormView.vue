<template>
  <div class="h-full flex flex-col max-h-[calc(100vh-100px)]">
    <!-- Header -->
    <div class="flex-none p-6 border-b border-default bg-surface-card sticky top-0 z-10">
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div class="flex items-center gap-2 mb-1">
            <button
              @click="$router.push('/super-admin/notifications/templates')"
              class="p-1 -ml-1 text-text-secondary hover:text-text-primary rounded-md transition-colors"
              title="Quay lại danh sách"
            >
              <ArrowLeft class="w-5 h-5" />
            </button>
            <h1 class="text-xl font-semibold text-text-heading">
              {{ isEdit ? 'Chỉnh sửa Mẫu thông báo' : 'Thêm mới Mẫu thông báo' }}
            </h1>
          </div>
          <p class="text-sm text-text-secondary">
            Thiết lập nội dung và quy tắc cho thông báo hệ thống
          </p>
        </div>
        
        <div class="flex items-center gap-2">
          <button
            @click="$router.push('/super-admin/notifications/templates')"
            class="px-4 py-2 rounded-lg text-sm font-medium text-text-secondary hover:bg-slate-100 transition-colors"
          >
            Hủy
          </button>
          <button
            @click="handleSubmit"
            :disabled="isSaving"
            class="px-4 py-2 rounded-lg text-sm font-medium text-white bg-lg-primary hover:opacity-90 transition-all flex items-center gap-2 disabled:opacity-50"
          >
            <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
            <Save v-else class="w-4 h-4" />
            Lưu mẫu thông báo
          </button>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex-1 overflow-y-auto p-6 bg-slate-50">
      <div class="max-w-5xl mx-auto space-y-6">
        <div v-if="loading" class="flex justify-center py-12">
          <Loader2 class="w-8 h-8 text-lg-primary animate-spin" />
        </div>

        <template v-else>
          <!-- Cấu hình cơ bản -->
          <div class="bg-surface-card rounded-xl border border-default p-6 space-y-6 shadow-sm">
            <h2 class="text-base font-semibold text-text-heading flex items-center gap-2 border-b border-default pb-4">
              <Settings class="w-5 h-5 text-lg-primary" />
              Thông tin chung
            </h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Mã mẫu -->
              <div class="space-y-1.5">
                <label class="text-sm font-medium text-text-primary">
                  Mã mẫu <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="form.maMau"
                  type="text"
                  placeholder="Vd: TPL-HOC-PHI"
                  :disabled="isEdit"
                  class="w-full px-3 py-2 text-sm rounded-lg border border-input bg-surface-input text-text-primary focus:outline-none focus:border-lg-primary focus:ring-1 focus:ring-lg-primary transition-colors disabled:bg-slate-100 disabled:text-text-secondary"
                />
              </div>

              <!-- Loại thông báo -->
              <div class="space-y-1.5">
                <label class="text-sm font-medium text-text-primary">
                  Phân loại <span class="text-red-500">*</span>
                </label>
                <select
                  v-model="form.loaiThongBao"
                  class="w-full px-3 py-2 text-sm rounded-lg border border-input bg-surface-input text-text-primary focus:outline-none focus:border-lg-primary focus:ring-1 focus:ring-lg-primary transition-colors"
                >
                  <option value="">-- Chọn phân loại --</option>
                  <option value="hoc_vu">Học vụ</option>
                  <option value="tai_chinh">Tài chính</option>
                  <option value="he_thong">Hệ thống</option>
                  <option value="khac">Khác</option>
                </select>
              </div>

              <!-- Tên mẫu -->
              <div class="space-y-1.5 md:col-span-2">
                <label class="text-sm font-medium text-text-primary">
                  Tên mẫu <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="form.tenMau"
                  type="text"
                  placeholder="Vd: Thông báo nộp học phí kỳ Fall"
                  class="w-full px-3 py-2 text-sm rounded-lg border border-input bg-surface-input text-text-primary focus:outline-none focus:border-lg-primary focus:ring-1 focus:ring-lg-primary transition-colors"
                />
              </div>

              <!-- Tiêu đề hiển thị -->
              <div class="space-y-1.5 md:col-span-2">
                <label class="text-sm font-medium text-text-primary">
                  Tiêu đề hiển thị (khi gửi đi) <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="form.tieuDeMau"
                  type="text"
                  placeholder="Vd: Thông báo đóng học phí - {{Kỳ_Học}}"
                  class="w-full px-3 py-2 text-sm rounded-lg border border-input bg-surface-input text-text-primary focus:outline-none focus:border-lg-primary focus:ring-1 focus:ring-lg-primary transition-colors"
                />
              </div>
              
              <!-- Kênh thông báo -->
              <div class="space-y-1.5">
                <label class="text-sm font-medium text-text-primary">Kênh thông báo</label>
                <select
                  v-model="form.kenhThongBao"
                  class="w-full px-3 py-2 text-sm rounded-lg border border-input bg-surface-input text-text-primary focus:outline-none focus:border-lg-primary focus:ring-1 focus:ring-lg-primary transition-colors"
                >
                  <option value="in_app">In-App (Mặc định)</option>
                  <option value="email">Email</option>
                  <option value="sms">SMS</option>
                </select>
              </div>
              
              <!-- Trạng thái -->
              <div class="space-y-1.5 flex flex-col justify-end">
                <label class="flex items-center gap-2 cursor-pointer p-2 border border-default rounded-lg hover:bg-slate-50 transition-colors">
                  <input
                    type="checkbox"
                    v-model="form.dangHoatDong"
                    class="w-4 h-4 text-lg-primary rounded border-slate-300 focus:ring-lg-primary"
                  />
                  <span class="text-sm font-medium text-text-primary">Kích hoạt sử dụng</span>
                </label>
              </div>
            </div>
          </div>

          <!-- Nội dung (Editor.js) -->
          <div class="bg-surface-card rounded-xl border border-default p-6 space-y-6 shadow-sm">
            <div class="flex items-center justify-between border-b border-default pb-4">
              <h2 class="text-base font-semibold text-text-heading flex items-center gap-2">
                <FileText class="w-5 h-5 text-lg-primary" />
                Nội dung mẫu
              </h2>
              <div class="text-xs text-text-secondary bg-slate-100 px-2 py-1 rounded-md">
                Sử dụng <code class="text-lg-primary">{{ Tên_Biến }}</code> để chèn dữ liệu động
              </div>
            </div>
            
            <div class="min-h-[400px] border border-input rounded-lg bg-surface-input prose max-w-none relative p-4">
              <div id="editorjs" class="min-h-[300px]"></div>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, Save, Loader2, Settings, FileText } from 'lucide-vue-next'
import { usePopupStore } from '@/stores/popup'
import { superAdminApi } from '@/services/superAdminApi'
import EditorJS from '@editorjs/editorjs'
import Paragraph from '@editorjs/paragraph'
import Header from '@editorjs/header'
import List from '@editorjs/list'
import Checklist from '@editorjs/checklist'
import Quote from '@editorjs/quote'
import Table from '@editorjs/table'
import Delimiter from '@editorjs/delimiter'
import Warning from '@editorjs/warning'
import CodeTool from '@editorjs/code'
import Marker from '@editorjs/marker'
import InlineCode from '@editorjs/inline-code'
import Underline from '@editorjs/underline'
import Embed from '@editorjs/embed'
import SimpleImage from '@editorjs/simple-image'
import RawTool from '@editorjs/raw'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const isEdit = ref(route.params.id ? true : false)
const templateId = ref(route.params.id)
const loading = ref(isEdit.value)
const isSaving = ref(false)

const form = reactive({
  maMau: '',
  tenMau: '',
  loaiThongBao: '',
  tieuDeMau: '',
  kenhThongBao: 'in_app',
  dangHoatDong: true,
  noiDungMau: ''
})

let editorInstance = null

const initEditor = (initialDataStr) => {
  let initialData = {}
  try {
    if (initialDataStr && initialDataStr.startsWith('{')) {
      initialData = JSON.parse(initialDataStr)
    } else if (initialDataStr) {
      // Fallback if not JSON
      initialData = {
        blocks: [
          { type: 'paragraph', data: { text: initialDataStr } }
        ]
      }
    }
  } catch (e) {
    console.warn('Lỗi parse EditorJS JSON, dùng dữ liệu trắng', e)
  }

  editorInstance = new EditorJS({
    holder: 'editorjs',
    placeholder: 'Bắt đầu soạn thảo nội dung...',
    data: initialData,
    tools: {
      paragraph: {
        class: Paragraph,
        inlineToolbar: true,
      },
      header: {
        class: Header,
        inlineToolbar: ['link', 'marker', 'underline', 'inlineCode'],
        config: {
          placeholder: 'Nhập tiêu đề...',
          levels: [1, 2, 3, 4, 5, 6],
          defaultLevel: 2
        }
      },
      list: {
        class: List,
        inlineToolbar: true
      },
      checklist: {
        class: Checklist,
        inlineToolbar: true
      },
      quote: {
        class: Quote,
        inlineToolbar: true
      },
      table: {
        class: Table,
        inlineToolbar: true
      },
      delimiter: Delimiter,
      warning: {
        class: Warning,
        inlineToolbar: true
      },
      code: CodeTool,
      embed: {
        class: Embed,
        inlineToolbar: true
      },
      image: SimpleImage,
      raw: RawTool,
      marker: Marker,
      inlineCode: InlineCode,
      underline: Underline
    }
  })
}

const loadData = async () => {
  try {
    loading.value = true
    const res = await superAdminApi.getNotificationTemplateDetail(templateId.value)
    if (res.success && res.data) {
      const data = res.data
      form.maMau = data.maMau || data.maMauThongBao.toString()
      form.tenMau = data.tenMau
      form.loaiThongBao = data.loaiThongBao
      form.tieuDeMau = data.tieuDeMau
      form.kenhThongBao = data.kenhThongBao || 'in_app'
      form.dangHoatDong = data.dangHoatDong
      
      loading.value = false
      await nextTick()
      initEditor(data.noiDungMau)
    } else {
      popupStore.error('Lỗi', 'Không tải được chi tiết mẫu thông báo')
      router.push('/super-admin/notifications/templates')
    }
  } catch (error) {
    console.error('Fetch template detail error:', error)
    popupStore.error('Lỗi', 'Lỗi khi tải chi tiết')
    router.push('/super-admin/notifications/templates')
  }
}

onMounted(() => {
  if (isEdit.value) {
    loadData()
  } else {
    // Khởi tạo EditorJS trống ngay
    loading.value = false
    setTimeout(() => {
      initEditor('')
    }, 100)
  }
})

onBeforeUnmount(() => {
  if (editorInstance && typeof editorInstance.destroy === 'function') {
    editorInstance.destroy()
  }
})

const validateForm = () => {
  if (!form.maMau.trim()) return 'Vui lòng nhập mã mẫu'
  if (!form.tenMau.trim()) return 'Vui lòng nhập tên mẫu'
  if (!form.loaiThongBao) return 'Vui lòng chọn loại thông báo'
  if (!form.tieuDeMau.trim()) return 'Vui lòng nhập tiêu đề mẫu'
  return null
}

const handleSubmit = async () => {
  const errorMsg = validateForm()
  if (errorMsg) {
    popupStore.warning('Cảnh báo', errorMsg)
    return
  }

  if (!editorInstance) {
    popupStore.error('Lỗi', 'Trình soạn thảo chưa được tải')
    return
  }

  try {
    isSaving.value = true
    
    // Save editor data
    const editorData = await editorInstance.save()
    form.noiDungMau = JSON.stringify(editorData)

    if (isEdit.value) {
      await superAdminApi.updateNotificationTemplate(templateId.value, form)
      popupStore.success('Thành công', 'Cập nhật mẫu thông báo thành công')
    } else {
      await superAdminApi.createNotificationTemplate(form)
      popupStore.success('Thành công', 'Tạo mẫu thông báo thành công')
    }
    router.push('/super-admin/notifications/templates')
  } catch (error) {
    console.error('Save template error:', error)
    popupStore.error('Lỗi', error.response?.data?.message || 'Lỗi khi lưu mẫu thông báo')
  } finally {
    isSaving.value = false
  }
}
</script>

<style scoped>
/* Customize Editor.js styling to match design system */
:deep(.codex-editor) {
  padding: 0 !important;
}
:deep(.ce-block__content) {
  max-width: 100%;
  font-size: 17px;
  line-height: 1.6;
}
:deep(.ce-toolbar__content) {
  max-width: 100%;
}
</style>
