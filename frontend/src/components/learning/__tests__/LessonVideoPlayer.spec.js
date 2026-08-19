import { createPinia } from 'pinia'
import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import LessonVideoPlayer from '../LessonVideoPlayer.vue'

describe('LessonVideoPlayer progress completion', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.spyOn(HTMLMediaElement.prototype, 'pause').mockImplementation(() => {})
    vi.spyOn(HTMLMediaElement.prototype, 'load').mockImplementation(() => {})
  })

  it('keeps 80 percent as partial progress and only completes when the video ends', async () => {
    const wrapper = mount(LessonVideoPlayer, {
      global: { plugins: [createPinia()] },
      props: {
        lesson: {
          id: 'l101',
          videoUrl: 'https://media.example/video.mp4',
          durationSeconds: 100,
          progressPercent: 0,
          minWatchPercentToComplete: 80,
          allowSeek: true,
        },
      },
    })

    const video = wrapper.get('video')
    Object.defineProperty(video.element, 'duration', { configurable: true, value: 100 })
    await video.trigger('loadedmetadata')
    video.element.currentTime = 80
    await video.trigger('timeupdate')

    const partialPayload = wrapper.emitted('progress').at(-1)[0]
    expect(partialPayload.progressPercent).toBe(80)
    expect(partialPayload.completed).toBe(false)
    expect(wrapper.emitted('completed')).toBeUndefined()

    wrapper.unmount()

    const reopenedWrapper = mount(LessonVideoPlayer, {
      global: { plugins: [createPinia()] },
      props: {
        lesson: {
          id: 'l101',
          videoUrl: 'https://media.example/video.mp4',
          durationSeconds: 100,
          progressPercent: 80,
          minWatchPercentToComplete: 80,
          allowSeek: true,
        },
      },
    })
    const reopenedVideo = reopenedWrapper.get('video')
    Object.defineProperty(reopenedVideo.element, 'duration', { configurable: true, value: 100 })
    await reopenedVideo.trigger('loadedmetadata')
    await reopenedVideo.trigger('timeupdate')

    const restoredPayload = reopenedWrapper.emitted('progress').at(-1)[0]
    expect(reopenedVideo.element.currentTime).toBe(80)
    expect(restoredPayload.progressPercent).toBe(80)
    expect(restoredPayload.completed).toBe(false)
    expect(reopenedWrapper.emitted('completed')).toBeUndefined()

    reopenedVideo.element.currentTime = 100
    await reopenedVideo.trigger('ended')

    const completedPayload = reopenedWrapper.emitted('completed').at(-1)[0]
    expect(completedPayload.progressPercent).toBe(100)
    expect(completedPayload.completed).toBe(true)

    reopenedWrapper.unmount()
  })

  it.each([
    { progressPercent: 5, watchedSeconds: 72 },
    { progressPercent: 26, watchedSeconds: 374 },
    { progressPercent: 50, watchedSeconds: 720 },
    { progressPercent: 89, watchedSeconds: 1282 },
  ])('does not compound $progressPercent percent after repeated remounts', async ({ progressPercent, watchedSeconds }) => {
    const lessonId = `stable-${progressPercent}`

    for (let cycle = 0; cycle < 3; cycle += 1) {
      const wrapper = mount(LessonVideoPlayer, {
        global: { plugins: [createPinia()] },
        props: {
          lesson: {
            id: lessonId,
            videoUrl: 'https://media.example/video.mp4',
            // Cố ý truyền duration DTO sai để mô phỏng lỗi cũ 1200 giây.
            durationSeconds: 1200,
            progressPercent,
            watchedSeconds,
            maxWatchedSeconds: watchedSeconds,
            allowSeek: true,
          },
        },
      })

      const video = wrapper.get('video')
      Object.defineProperty(video.element, 'duration', { configurable: true, value: 1440 })
      await video.trigger('loadedmetadata')
      await video.trigger('timeupdate')

      const payload = wrapper.emitted('progress').at(-1)[0]
      expect(payload.currentTimeSeconds).toBe(watchedSeconds)
      expect(payload.progressPercent).toBe(progressPercent)
      expect(payload.completed).toBe(false)

      wrapper.unmount()
    }
  })

  it('ignores a synced local cache and restores progress from the DB value', async () => {
    localStorage.setItem('lms_offline_progress_guest_offline-repair', JSON.stringify({
      currentTimeSeconds: 720,
      maxWatchedSeconds: 720,
      progressPercent: 54,
    }))

    const wrapper = mount(LessonVideoPlayer, {
      global: { plugins: [createPinia()] },
      props: {
        lesson: {
          id: 'offline-repair',
          videoUrl: 'https://media.example/video.mp4',
          durationSeconds: 1200,
          progressPercent: 50,
          allowSeek: true,
        },
      },
    })

    const video = wrapper.get('video')
    Object.defineProperty(video.element, 'duration', { configurable: true, value: 1440 })
    await video.trigger('loadedmetadata')
    await video.trigger('timeupdate')

    const restored = wrapper.emitted('progress').at(-1)[0]
    expect(restored.currentTimeSeconds).toBe(720)
    expect(restored.progressPercent).toBe(50)
    expect(restored.forceSave).toBe(false)

    wrapper.unmount()
  })

  it('does not turn an unviewed lesson into 100 percent from a stale cache', async () => {
    localStorage.setItem('lms_offline_progress_guest_unviewed', JSON.stringify({
      currentTimeSeconds: 100,
      maxWatchedSeconds: 100,
      progressPercent: 100,
      pendingSync: false,
    }))

    const wrapper = mount(LessonVideoPlayer, {
      global: { plugins: [createPinia()] },
      props: {
        lesson: {
          id: 'unviewed',
          videoUrl: 'https://media.example/video.mp4',
          durationSeconds: 100,
          progressPercent: 0,
          allowSeek: true,
        },
      },
    })

    const video = wrapper.get('video')
    Object.defineProperty(video.element, 'duration', { configurable: true, value: 100 })
    await video.trigger('loadedmetadata')
    await video.trigger('timeupdate')

    const payload = wrapper.emitted('progress').at(-1)[0]
    expect(video.element.currentTime).toBe(0)
    expect(payload.progressPercent).toBe(0)
    expect(payload.completed).toBe(false)

    wrapper.unmount()
  })
})
