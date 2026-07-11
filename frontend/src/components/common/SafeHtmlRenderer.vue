<template>
  <component :is="tag" ref="container" v-bind="$attrs"></component>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import DOMPurify from 'dompurify'

const props = defineProps({
  html: {
    type: String,
    default: ''
  },
  tag: {
    type: String,
    default: 'div'
  }
})

const container = ref(null)

DOMPurify.addHook('afterSanitizeAttributes', function (node) {
  if ('target' in node && node.getAttribute('target') === '_blank') {
    node.setAttribute('rel', 'noopener noreferrer')
  }
})

function sanitize(dirtyHtml) {
  if (!dirtyHtml) return ''
  return DOMPurify.sanitize(dirtyHtml, {
    ALLOWED_TAGS: [
      'b', 'i', 'em', 'strong', 'a', 'p', 'br', 'ul', 'ol', 'li',
      'span', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'figure', 'figcaption', 'footer'
    ],
    ALLOWED_ATTR: ['href', 'title', 'class', 'target'],
    ALLOW_DATA_ATTR: false,
    RETURN_DOM: false,
    RETURN_DOM_FRAGMENT: false,
    RETURN_DOM_IMPORT: false,
    SANITIZE_DOM: true,
    KEEP_CONTENT: true,
    IN_PLACE: false,
    FORBID_TAGS: ['script', 'style', 'iframe', 'object', 'embed', 'svg', 'math', 'base', 'form'],
    FORBID_ATTR: ['onerror', 'onload', 'onmouseover', 'onfocus', 'onblur', 'onclick', 'style']
  })
}

function updateContent() {
  if (container.value) {
    container.value.innerHTML = sanitize(props.html)
  }
}

onMounted(() => {
  updateContent()
})

watch(() => props.html, () => {
  updateContent()
})
</script>
