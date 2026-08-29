import { describe, expect, it } from 'vitest'
import { mergeMonotonicLessonProgress } from '../videoProgress'

describe('mergeMonotonicLessonProgress', () => {
  it('never borrows progress from the newly selected lesson', () => {
    const result = mergeMonotonicLessonProgress({
      lessonId: 'old-video',
      incomingPercent: 0,
      currentLesson: { id: 'new-video', progressPercent: 100 },
      lessonDraft: null,
    })

    expect(result).toBe(0)
  })

  it('keeps progress monotonic for the same lesson', () => {
    const result = mergeMonotonicLessonProgress({
      lessonId: 'video-1',
      incomingPercent: 26,
      currentLesson: { id: 'video-1', progressPercent: 30 },
      lessonDraft: { progressPercent: 28 },
    })

    expect(result).toBe(30)
  })
})
