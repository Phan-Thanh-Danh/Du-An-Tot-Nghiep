import { mount, flushPromises } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import AiCertificateAssistantModal from '../AiCertificateAssistantModal.vue'
import { aiApi } from '@/services/aiApi'
vi.mock('@/services/aiApi', () => ({ aiApi: { editCertificateTemplate: vi.fn() } }))
const create = () => mount(AiCertificateAssistantModal, { props: { isOpen: true, currentHtml: '<div>{{hoTen}}</div>', currentCss: 'div{color:red}' }, global: { stubs: { Teleport: true } } })
const send = async (wrapper, text) => {
  await wrapper.get('textarea').setValue(text)
  await wrapper.findAll('button').find(x => x.text().includes('THỰC THI')).trigger('click')
  await flushPromises()
}
describe('Certificate prompt editing', () => {
  beforeEach(() => vi.clearAllMocks())
  it('previews and edits the latest proposed design before applying', async () => {
    const design = { updatedHtml: '<div>{{hoTen}}</div>', updatedCss: 'div{background:yellow;border:1px solid black}', explanation: 'Nền vàng viền đen', changesSummary: [] }
    aiApi.editCertificateTemplate.mockResolvedValue(design)
    const wrapper = create()
    await send(wrapper, 'nền vàng viền đen')
    expect(wrapper.get('iframe').attributes('sandbox')).toBe('')
    expect(wrapper.get('iframe').attributes('srcdoc')).toContain('background:yellow')
    await send(wrapper, 'giữ nền, đổi viền xanh lá')
    expect(aiApi.editCertificateTemplate.mock.calls[1][0].currentCss).toBe(design.updatedCss)
    expect(wrapper.emitted('apply')).toBeUndefined()
    wrapper.unmount()
  })
  it('never offers apply after an AI failure', async () => {
    aiApi.editCertificateTemplate.mockRejectedValue(new Error('AI chưa sẵn sàng'))
    const wrapper = create()
    await send(wrapper, 'nền vàng')
    expect(wrapper.text()).toContain('AI chưa sẵn sàng')
    expect(wrapper.find('iframe').exists()).toBe(false)
    expect(wrapper.text()).not.toContain('ÁP DỤNG VÀO MẪU')
    wrapper.unmount()
  })
})
