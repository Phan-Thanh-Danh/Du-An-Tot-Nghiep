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
import Embed from '@editorjs/embed'
import CodeTool from '@editorjs/code'
import Delimiter from '@editorjs/delimiter'
import Warning from '@editorjs/warning'

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
      header: Header,
      paragraph: {
        class: Paragraph,
        inlineToolbar: true
      },
      list: {
        class: List,
        inlineToolbar: true
      },
      checklist: {
        class: Checklist,
        inlineToolbar: true
      },
      quote: Quote,
      table: Table,
      image: {
        class: ImageTool,
        config: {
          uploader: {
            uploadByFile(file: File) {
              // Mock image upload with object URL for frontend task
              return Promise.resolve({
                success: 1,
                file: {
                  url: URL.createObjectURL(file)
                }
              })
            }
          }
        }
      },
      embed: Embed,
      code: CodeTool,
      delimiter: Delimiter,
      warning: Warning
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
