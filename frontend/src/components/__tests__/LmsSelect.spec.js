import { h } from 'vue'
import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import LmsSelect from '../LmsSelect.vue'

describe('LmsSelect', () => {
  it('renders and selects options supplied by the Dashboard API format', async () => {
    const wrapper = mount(LmsSelect, {
      props: {
        modelValue: 'all',
        options: [
          { value: 'all', label: 'Tất cả' },
          { value: 'cntt', label: 'Công nghệ thông tin' },
        ],
      },
    })

    expect(wrapper.text()).toContain('Tất cả')
    await wrapper.get('.lg-input').trigger('click')
    await wrapper.findAll('li')[1].trigger('click')

    expect(wrapper.emitted('update:modelValue')).toEqual([['cntt']])
    expect(wrapper.emitted('change')).toEqual([['cntt']])
  })

  it('normalizes existing option slots used by BGH views', async () => {
    const wrapper = mount(LmsSelect, {
      props: { modelValue: '' },
      slots: {
        default: () => [
          h('option', { value: '' }, 'Tất cả vai trò'),
          h('option', { value: 'giao_vien' }, 'Giảng viên'),
        ],
      },
    })

    expect(wrapper.get('.lg-input').text()).toContain('Tất cả vai trò')
    await wrapper.get('.lg-input').trigger('click')
    expect(wrapper.findAll('li').map((item) => item.text())).toEqual([
      'Tất cả vai trò',
      'Giảng viên',
    ])
  })

  it('does not emit a disabled placeholder option', async () => {
    const wrapper = mount(LmsSelect, {
      props: { modelValue: '' },
      slots: {
        default: () => [
          h('option', { value: '', disabled: true }, 'Chọn vai trò'),
          h('option', { value: 'admin' }, 'Quản trị viên'),
        ],
      },
    })

    await wrapper.get('.lg-input').trigger('click')
    await wrapper.findAll('li')[0].trigger('click')

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })
})
