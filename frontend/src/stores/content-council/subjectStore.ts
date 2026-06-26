import { defineStore } from 'pinia'
import { ref } from 'vue'
import { initializeSubjectMockData } from '@/mocks/content-council'
import { ContentCouncilSubject, ContentCouncilSubjectDetail, SubjectContentSettings } from '@/types/content-council/subject'
import { EditorChapter, EditorLesson, EditorContentBlock } from '@/types/content-council/curriculum'

export const useSubjectStore = defineStore('contentCouncilSubject', () => {
  const subjects = ref<ContentCouncilSubject[]>([])
  const subjectDetails = ref<Record<number, ContentCouncilSubjectDetail>>({})
  const subjectSettings = ref<Record<number, SubjectContentSettings>>({})
  const initialized = ref(false)

  function init() {
    if (initialized.value) return
    const mock = initializeSubjectMockData()
    subjects.value = mock.subjects
    subjectDetails.value = mock.subjectDetails
    initialized.value = true
  }

  function reset() {
    initialized.value = false
    init()
  }

  function getSubjectById(id: number) {
    return subjects.value.find(s => s.id === id)
  }

  function getSubjectDetail(id: number) {
    return subjectDetails.value[id]
  }

  function updateSubjectSettings(id: number, settings: Partial<SubjectContentSettings>) {
    if (subjectSettings.value[id]) {
      Object.assign(subjectSettings.value[id], settings)
    } else {
      subjectSettings.value[id] = settings as SubjectContentSettings
    }
  }

  // Chapters & Curriculum
  function getChapters(subjectId: number) {
    return subjectDetails.value[subjectId]?.chapters || []
  }

  function addChapter(subjectId: number, chapter: EditorChapter) {
    const detail = subjectDetails.value[subjectId]
    if (detail) {
      if (!detail.chapters) detail.chapters = []
      detail.chapters.push(chapter)
      detail.chapterCount = detail.chapters.length
      syncSummary(subjectId)
    }
  }

  function updateChapter(subjectId: number, chapterId: number, payload: Partial<EditorChapter>) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const idx = detail.chapters.findIndex(c => c.id === chapterId)
      if (idx !== -1) {
        Object.assign(detail.chapters[idx], payload)
      }
    }
  }

  function deleteChapter(subjectId: number, chapterId: number) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      detail.chapters = detail.chapters.filter(c => c.id !== chapterId)
      detail.chapterCount = detail.chapters.length
      syncSummary(subjectId)
    }
  }

  function reorderChapters(subjectId: number, newOrder: EditorChapter[]) {
    const detail = subjectDetails.value[subjectId]
    if (detail) {
      detail.chapters = newOrder.map((c, i) => ({ ...c, order: i + 1 }))
    }
  }

  // Lesson
  function addLesson(subjectId: number, chapterId: number, lesson: EditorLesson) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter) {
        if (!chapter.lessons) chapter.lessons = []
        chapter.lessons.push(lesson)
        syncSummary(subjectId)
      }
    }
  }

  function updateLesson(subjectId: number, chapterId: number, lessonId: number, payload: Partial<EditorLesson>) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter?.lessons) {
        const idx = chapter.lessons.findIndex(l => l.id === lessonId)
        if (idx !== -1) {
          Object.assign(chapter.lessons[idx], payload)
        }
      }
    }
  }

  function deleteLesson(subjectId: number, chapterId: number, lessonId: number) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter?.lessons) {
        chapter.lessons = chapter.lessons.filter(l => l.id !== lessonId)
        syncSummary(subjectId)
      }
    }
  }

  // Content Block
  function addContent(subjectId: number, chapterId: number, lessonId: number, content: EditorContentBlock) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter?.lessons) {
        const lesson = chapter.lessons.find(l => l.id === lessonId)
        if (lesson) {
          if (!lesson.contents) lesson.contents = []
          lesson.contents.push(content)
          syncSummary(subjectId)
        }
      }
    }
  }

  function updateContent(subjectId: number, chapterId: number, lessonId: number, contentId: number, payload: Partial<EditorContentBlock>) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter?.lessons) {
        const lesson = chapter.lessons.find(l => l.id === lessonId)
        if (lesson?.contents) {
          const idx = lesson.contents.findIndex(c => c.id === contentId)
          if (idx !== -1) {
            Object.assign(lesson.contents[idx], payload)
          }
        }
      }
    }
  }

  function deleteContent(subjectId: number, chapterId: number, lessonId: number, contentId: number) {
    const detail = subjectDetails.value[subjectId]
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter?.lessons) {
        const lesson = chapter.lessons.find(l => l.id === lessonId)
        if (lesson?.contents) {
          lesson.contents = lesson.contents.filter(c => c.id !== contentId)
          syncSummary(subjectId)
        }
      }
    }
  }

  // Internal
  function syncSummary(subjectId: number) {
    const detail = subjectDetails.value[subjectId]
    if (!detail) return
    let cCount = detail.chapters?.length || 0
    let lCount = 0
    let contentCount = 0
    let qCount = 0

    detail.chapters?.forEach(c => {
      lCount += c.lessons?.length || 0
      c.lessons?.forEach(l => {
        contentCount += l.contents?.length || 0
        l.contents?.forEach(cb => {
          if (cb.type === 'trac_nghiem') qCount++
        })
      })
    })

    detail.chapterCount = cCount
    detail.lessonCount = lCount
    detail.contentCount = contentCount
    detail.quizCount = qCount
    
    // sync to subjects summary
    const sub = subjects.value.find(s => s.id === subjectId)
    if (sub) {
      sub.chapterCount = cCount
      sub.lessonCount = lCount
      sub.contentCount = contentCount
      sub.quizCount = qCount
    }
  }

  return {
    subjects,
    subjectDetails,
    subjectSettings,
    initialized,
    init,
    reset,
    getSubjectById,
    getSubjectDetail,
    updateSubjectSettings,
    getChapters,
    addChapter,
    updateChapter,
    deleteChapter,
    reorderChapters,
    addLesson,
    updateLesson,
    deleteLesson,
    addContent,
    updateContent,
    deleteContent
  }
})
