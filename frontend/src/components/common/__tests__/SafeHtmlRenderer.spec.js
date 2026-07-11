import { mount } from '@vue/test-utils'
import { describe, it, expect } from 'vitest'
import SafeHtmlRenderer from '../SafeHtmlRenderer.vue'

describe('SafeHtmlRenderer', () => {
  it('renders safe html correctly', () => {
    const wrapper = mount(SafeHtmlRenderer, {
      props: {
        html: '<p>Hello <b>World</b></p>'
      }
    })
    expect(wrapper.html()).toContain('<p>Hello <b>World</b></p>')
  })

  it('strips <script> tags', () => {
    const wrapper = mount(SafeHtmlRenderer, {
      props: {
        html: '<p>Test</p><script>alert(1)</script>'
      }
    })
    expect(wrapper.html()).toContain('<p>Test</p>')
    expect(wrapper.html()).not.toContain('script')
  })

  it('strips <img onerror> payloads', () => {
    const wrapper = mount(SafeHtmlRenderer, {
      props: {
        html: '<img src="x" onerror="alert(1)">'
      }
    })
    // img is not in the allowlist, so it should be stripped entirely
    expect(wrapper.html()).not.toContain('img')
    expect(wrapper.html()).not.toContain('onerror')
  })

  it('strips javascript: protocols in links', () => {
    const wrapper = mount(SafeHtmlRenderer, {
      props: {
        html: '<a href="javascript:alert(1)">Click me</a>'
      }
    })
    // DOMPurify typically removes the href or changes it to something safe
    expect(wrapper.html()).not.toContain('javascript:')
  })

  it('strips iframe, object, embed', () => {
    const wrapper = mount(SafeHtmlRenderer, {
      props: {
        html: '<iframe src="test"></iframe><object></object><embed>'
      }
    })
    expect(wrapper.html()).not.toContain('iframe')
    expect(wrapper.html()).not.toContain('object')
    expect(wrapper.html()).not.toContain('embed')
  })

  it('handles malformed tags gracefully', () => {
    const wrapper = mount(SafeHtmlRenderer, {
      props: {
        html: '<p>Test <b'
      }
    })
    // DOMPurify will close the tags or sanitize it
    expect(wrapper.html()).not.toContain('<b<') // just making sure no weird parsing
  })
})
