<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import EditorJS from '@editorjs/editorjs'
import Header from '@editorjs/header'
import Paragraph from '@editorjs/paragraph'
import List from '@editorjs/list'
import Checklist from '@editorjs/checklist'
import Quote from '@editorjs/quote'
import Table from '@editorjs/table'
import ImageTool from '@editorjs/image'
import SimpleImage from '@editorjs/simple-image'
import Embed from '@editorjs/embed'
import CodeTool from '@editorjs/code'
import Delimiter from '@editorjs/delimiter'
import Warning from '@editorjs/warning'
import RawTool from '@editorjs/raw'
import Marker from '@editorjs/marker'
import InlineCode from '@editorjs/inline-code'
import Underline from '@editorjs/underline'
import AttachesTool from '@editorjs/attaches'
import { storageApi } from '@/services/apiClient'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue'])

const editorHolder = ref<HTMLElement | null>(null)
let editorInstance: EditorJS | null = null

const initEditor = () => {
  if (!editorHolder.value) return

  let initialData = {}
  try {
    if (props.modelValue) {
      initialData = JSON.parse(props.modelValue)
    }
  } catch (e) {
    console.error('Invalid JSON for Editor.js:', e)
  }

  editorInstance = new EditorJS({
    holder: editorHolder.value,
    placeholder: 'Nhấn Tab hoặc nút + để thêm nội dung...',
    data: initialData,
    tools: {
      header: {
        class: Header,
        inlineToolbar: ['link', 'marker', 'underline', 'inlineCode'],
        config: {
          placeholder: 'Nhập tiêu đề...',
          levels: [1, 2, 3, 4, 5, 6],
          defaultLevel: 2
        }
      },
      paragraph: {
        class: Paragraph,
        inlineToolbar: ['link', 'marker', 'underline', 'inlineCode']
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
        inlineToolbar: true,
        config: {
          quotePlaceholder: 'Nhập nội dung trích dẫn...',
          captionPlaceholder: 'Tác giả / Nguồn trích dẫn'
        }
      },
      table: {
        class: Table,
        inlineToolbar: true,
        config: {
          rows: 3,
          cols: 3
        }
      },
      image: {
        class: ImageTool,
        config: {
          uploader: {
            async uploadByFile(file: File) {
              try {
                const response = await storageApi.upload(file, 'slides')
                if (response && response.success && response.data) {
                  const result = Array.isArray(response.data) ? response.data[0] : response.data
                  const imageUrl = result.url || result.Url
                  return {
                    success: 1,
                    file: {
                      url: imageUrl
                    }
                  }
                }
                throw new Error('Upload failed')
              } catch (error) {
                console.error('Editor.js image upload error:', error)
                return {
                  success: 0,
                  message: 'Không thể upload hình ảnh'
                }
              }
            }
          }
        }
      },
      simpleImage: {
        class: SimpleImage,
        inlineToolbar: true
      },
      attaches: {
        class: AttachesTool,
        config: {
          uploader: {
            async uploadByFile(file: File) {
              try {
                const response = await storageApi.upload(file, 'slides')
                if (response && response.success && response.data) {
                  const result = Array.isArray(response.data) ? response.data[0] : response.data
                  return {
                    success: 1,
                    file: {
                      url: result.url || result.Url,
                      size: file.size,
                      name: file.name,
                      extension: file.name.split('.').pop()
                    }
                  }
                }
                throw new Error('Upload failed')
              } catch (error) {
                return { success: 0, message: 'Upload file thất bại' }
              }
            }
          }
        }
      },
      embed: {
        class: Embed,
        inlineToolbar: true
      },
      code: {
        class: CodeTool,
        config: {
          placeholder: 'Nhập mã nguồn / code...'
        }
      },
      raw: {
        class: RawTool,
        config: {
          placeholder: 'Chèn mã HTML thô...'
        }
      },
      delimiter: Delimiter,
      warning: {
        class: Warning,
        inlineToolbar: true,
        config: {
          titlePlaceholder: 'Tiêu đề cảnh báo',
          messagePlaceholder: 'Nội dung thông điệp chú ý...'
        }
      },
      marker: {
        class: Marker,
        shortcut: 'CMD+SHIFT+M'
      },
      inlineCode: {
        class: InlineCode,
        shortcut: 'CMD+SHIFT+C'
      },
      underline: {
        class: Underline,
        shortcut: 'CMD+U'
      }
    },
    onChange: async () => {
      if (editorInstance) {
        try {
          const savedData = await editorInstance.save()
          emit('update:modelValue', JSON.stringify(savedData))
        } catch (error) {
          console.error('Failed to save Editor.js data:', error)
        }
      }
    }
  })
}

onMounted(() => {
  initEditor()
})

onBeforeUnmount(() => {
  if (editorInstance) {
    editorInstance.destroy()
    editorInstance = null
  }
})

const saveData = async () => {
  if (editorInstance) {
    try {
      const data = await editorInstance.save()
      const jsonStr = JSON.stringify(data)
      emit('update:modelValue', jsonStr)
      return jsonStr
    } catch (e) {
      console.error('Lỗi khi lưu dữ liệu EditorJS:', e)
    }
  }
  return props.modelValue
}

defineExpose({
  saveData
})
</script>

<template>
  <div class="editor-container border border-input rounded-xl p-4 surface-input transition-colors">
    <div ref="editorHolder" class="min-h-[300px] text-body"></div>
  </div>
</template>

<style>
/* Adjust Editor.js default styles slightly to match design */
.editor-container .ce-block__content,
.editor-container .ce-toolbar__content {
  max-width: 100%;
}

/* ── Heading Styles (Fix Tailwind Preflight font-size reset) ── */
.editor-container h1.ce-header {
  font-size: 2.25rem !important;
  line-height: 1.25 !important;
  font-weight: 800 !important;
  margin-top: 1.5rem;
  margin-bottom: 0.75rem;
  color: var(--text-heading);
}

.editor-container h2.ce-header {
  font-size: 1.75rem !important;
  line-height: 1.3 !important;
  font-weight: 700 !important;
  margin-top: 1.25rem;
  margin-bottom: 0.625rem;
  color: var(--text-heading);
}

.editor-container h3.ce-header {
  font-size: 1.375rem !important;
  line-height: 1.35 !important;
  font-weight: 600 !important;
  margin-top: 1rem;
  margin-bottom: 0.5rem;
  color: var(--text-heading);
}

.editor-container h4.ce-header {
  font-size: 1.15rem !important;
  line-height: 1.4 !important;
  font-weight: 600 !important;
  margin-top: 0.75rem;
  margin-bottom: 0.375rem;
  color: var(--text-heading);
}

.editor-container h5.ce-header {
  font-size: 1rem !important;
  line-height: 1.45 !important;
  font-weight: 600 !important;
  margin-top: 0.5rem;
  margin-bottom: 0.25rem;
  color: var(--text-heading);
}

.editor-container h6.ce-header {
  font-size: 0.875rem !important;
  line-height: 1.5 !important;
  font-weight: 600 !important;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-top: 0.5rem;
  margin-bottom: 0.25rem;
  color: var(--text-muted);
}

.editor-container .ce-paragraph {
  font-size: 1rem;
  line-height: 1.65;
  margin-bottom: 0.5rem;
}

/* Make the Add (+) and Settings buttons slightly larger */
.editor-container .ce-toolbar__plus,
.editor-container .ce-toolbar__settings-btn {
  transform: scale(1.25);
  transform-origin: center;
}

.editor-container .ce-toolbar__plus:hover,
.editor-container .ce-toolbar__settings-btn:hover {
  transform: scale(1.35);
}

/* ── Editor.js Liquid Glass Theme Integration ── */

.editor-container .ce-popover,
.editor-container .ce-inline-toolbar,
.editor-container .ce-conversion-toolbar {
  background-color: var(--surface-dropdown);
  border: 1px solid var(--border-card);
  box-shadow: var(--lg-shadow-md);
  color: var(--text-body);
}

.editor-container .ce-popover__item:hover,
.editor-container .ce-inline-tool:hover,
.editor-container .ce-inline-toolbar__dropdown:hover,
.editor-container .ce-conversion-tool:hover {
  background-color: var(--surface-table-row-hover);
  color: var(--text-heading);
}

.editor-container .ce-popover__item-icon,
.editor-container .ce-conversion-tool__icon {
  background-color: var(--surface-input);
  color: var(--text-body);
  border: 1px solid var(--border-card);
  box-shadow: none;
}

.editor-container .cdx-search-field {
  background-color: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}

.editor-container .ce-toolbar__plus,
.editor-container .ce-toolbar__settings-btn {
  color: var(--text-muted);
}

.editor-container .ce-toolbar__plus:hover,
.editor-container .ce-toolbar__settings-btn:hover {
  background-color: var(--surface-card-hover);
  color: var(--text-heading);
}

.editor-container .ce-block--selected .ce-block__content {
  background-color: var(--focus-ring);
  border-radius: 4px;
}

.editor-container [data-placeholder]:empty::before {
  color: var(--text-placeholder);
}

.editor-container .ce-code__textarea {
  background-color: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-body);
}

.editor-container .tc-popover {
  background-color: var(--surface-dropdown);
  border-color: var(--border-card);
}

.editor-container .tc-popover__item:hover {
  background-color: var(--surface-table-row-hover);
}

.ce-popover__item-separator {
  background-color: var(--border-card) !important;
}

.dark .editor-container svg {
  fill: currentColor;
}
</style>
