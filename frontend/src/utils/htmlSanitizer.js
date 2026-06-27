/**
 * htmlSanitizer.js
 * 
 * Utility to sanitize HTML strings using DOMParser to prevent basic XSS attacks
 * in the Preview mode. Removes scripts, iframes, inline event handlers, and
 * javascript: URIs.
 */

export function sanitizeHtml(htmlString) {
  if (!htmlString) return ''

  try {
    const parser = new DOMParser()
    const doc = parser.parseFromString(htmlString, 'text/html')

    // 1. Remove dangerous tags
    const dangerousTags = ['script', 'style', 'link', 'iframe', 'object', 'embed', 'applet', 'meta', 'base']
    dangerousTags.forEach(tag => {
      const elements = doc.getElementsByTagName(tag)
      for (let i = elements.length - 1; i >= 0; i--) {
        elements[i].parentNode.removeChild(elements[i])
      }
    })

    // 2. Remove dangerous attributes from all elements
    const allElements = doc.getElementsByTagName('*')
    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      const attributes = el.attributes

      for (let j = attributes.length - 1; j >= 0; j--) {
        const attrName = attributes[j].name.toLowerCase()
        const attrValue = attributes[j].value.toLowerCase()

        // Remove inline event handlers (on*)
        if (attrName.startsWith('on')) {
          el.removeAttribute(attrName)
        }

        // Remove javascript: URIs in href or src
        if ((attrName === 'href' || attrName === 'src') && attrValue.includes('javascript:')) {
          el.removeAttribute(attrName)
        }
      }
    }

    return doc.body.innerHTML
  } catch (error) {
    console.error('HTML Sanitization error:', error)
    return 'Lỗi khi hiển thị nội dung.'
  }
}
