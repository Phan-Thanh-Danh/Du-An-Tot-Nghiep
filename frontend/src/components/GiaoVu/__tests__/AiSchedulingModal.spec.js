import { mount, flushPromises } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import AiSchedulingModal from '../AiSchedulingModal.vue'
import { aiApi } from '@/services/aiApi'
import { scheduleApi } from '@/services/scheduleApi'

vi.mock('vue-router', () => ({ useRouter: () => ({ push: vi.fn() }) }))
vi.mock('@/services/aiApi', () => ({ aiApi: { interpretSchedulingIntent: vi.fn(), explainSchedulingReadiness: vi.fn(), explainSchedulingDraft: vi.fn() } }))
vi.mock('@/services/scheduleApi', () => ({ scheduleApi: { generateDraft: vi.fn(), getGenerationProgress: vi.fn() } }))
const create = () => mount(AiSchedulingModal, { props: { isOpen: true, campusId: 14, termId: 15 }, global: { stubs: { Teleport: true } } })
const send = async (wrapper, text) => {
  await wrapper.get('textarea').setValue(text)
  await wrapper.findAll('button').find(x => x.text().includes('GỬI CHO TRỢ LÝ')).trigger('click')
  await flushPromises()
}

describe('Scheduling prompt conversation', () => {
  beforeEach(() => vi.clearAllMocks())
  it('answers a question without rendering a generation confirmation', async () => {
    aiApi.interpretSchedulingIntent.mockResolvedValue({ intent: 'query_schedule', summary: 'Có 2 ca tối mỗi tuần.', canPrepareSchedule: false })
    const wrapper = create()
    await send(wrapper, 'Lịch có ca tối nào không?')
    expect(wrapper.text()).toContain('Có 2 ca tối mỗi tuần.')
    expect(wrapper.text()).not.toContain('XÁC NHẬN & BẮT ĐẦU')
    expect(scheduleApi.generateDraft).not.toHaveBeenCalled()
    wrapper.unmount()
  })
  it('sends prior conversation and confirmed exclusions, once only', async () => {
    aiApi.interpretSchedulingIntent.mockResolvedValueOnce({ intent: 'query_schedule', summary: 'Có 1 ca tối.', canPrepareSchedule: false })
      .mockResolvedValueOnce({ intent: 'prepare_schedule', summary: 'Bỏ ca tối', excludeEvening: true, canPrepareSchedule: true, requestedPreferences: [] })
    let finish
    scheduleApi.generateDraft.mockReturnValue(new Promise(resolve => { finish = resolve }))
    const wrapper = create()
    await send(wrapper, 'Lịch có ca tối không?')
    await send(wrapper, 'Vậy bỏ ca tối đi')
    expect(aiApi.interpretSchedulingIntent.mock.calls[1][0].history).toHaveLength(2)
    const confirm = wrapper.findAll('button').find(x => x.text().includes('XÁC NHẬN & BẮT ĐẦU'))
    await confirm.trigger('click')
    expect(scheduleApi.generateDraft).toHaveBeenCalledTimes(1)
    expect(scheduleApi.generateDraft.mock.calls[0][0]).toMatchObject({ excludeEvening: true, maHocKy: 15, maDonVi: 14 })
    finish({ draftId: 'test-draft' })
    await flushPromises()
    wrapper.unmount()
  })
  it('invalidates an old plan immediately when the prompt changes', async () => {
    aiApi.interpretSchedulingIntent.mockResolvedValue({ intent: 'prepare_schedule', summary: 'Tạo lịch', canPrepareSchedule: true, requestedPreferences: [] })
    const wrapper = create()
    await send(wrapper, 'Tạo lịch')
    await wrapper.get('textarea').setValue('Chỉ xem lịch')
    expect(wrapper.text()).not.toContain('XÁC NHẬN & BẮT ĐẦU')
    wrapper.unmount()
  })
  it('discards a response to an older prompt still in flight', async () => {
    let finish
    aiApi.interpretSchedulingIntent.mockReturnValue(new Promise(resolve => { finish = resolve }))
    const wrapper = create()
    await send(wrapper, 'Tạo lịch')
    await wrapper.get('textarea').setValue('Có ca tối không?')
    finish({ intent: 'prepare_schedule', summary: 'Old plan', canPrepareSchedule: true })
    await flushPromises()
    expect(wrapper.text()).not.toContain('Old plan')
    wrapper.unmount()
  })
})
