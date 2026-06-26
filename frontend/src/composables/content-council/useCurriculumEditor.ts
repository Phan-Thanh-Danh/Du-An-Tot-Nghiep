import { ref, computed } from 'vue'
import type { EditorChapter, EditorLesson, EditorContentBlock } from '@/types/content-council/curriculumEditor'
import { mockSubjectDetails } from '@/mocks/contentCouncilSubjectDetails'

// Simple Toast mock since project toast is not explicitly defined in requirements
// In a real app we would use the app's notification system
export const useCurriculumEditor = (subjectId: number) => {
  const originalSubject = mockSubjectDetails.find(s => s.id === subjectId)
  
  const chapters = ref<EditorChapter[]>([])
  
  // Clone mock data to local state
  if (originalSubject) {
    chapters.value = JSON.parse(JSON.stringify(originalSubject.chapters))
    // Ensure all lessons have a contents array
    chapters.value.forEach(c => {
      c.lessons.forEach(l => {
        if (!l.contents) l.contents = []
      })
    })
  }

  const selectedChapterId = ref<number | null>(null)
  const selectedLessonId = ref<number | null>(null)
  const expandedChapterIds = ref<number[]>(chapters.value.map(c => c.id))
  
  const isChapterModalOpen = ref(false)
  const isLessonModalOpen = ref(false)
  const isContentDrawerOpen = ref(false)
  const isDeleteDialogOpen = ref(false)
  
  const editingChapter = ref<EditorChapter | null>(null)
  const editingLesson = ref<EditorLesson | null>(null)
  const editingContent = ref<EditorContentBlock | null>(null)
  const selectedContentType = ref<string>('video')
  
  const deleteTarget = ref<{ type: 'chapter' | 'lesson' | 'content', id: number, parentId?: number } | null>(null)

  // Computed
  const selectedLesson = computed(() => {
    if (!selectedLessonId.value) return null
    for (const c of chapters.value) {
      const lesson = c.lessons.find(l => l.id === selectedLessonId.value)
      if (lesson) return lesson
    }
    return null
  })

  const selectedChapter = computed(() => {
    if (!selectedChapterId.value && !selectedLesson.value) return null
    const cid = selectedChapterId.value || chapters.value.find(c => c.lessons.some(l => l.id === selectedLessonId.value))?.id
    return chapters.value.find(c => c.id === cid) || null
  })

  // Actions
  const toggleChapter = (id: number) => {
    const idx = expandedChapterIds.value.indexOf(id)
    if (idx > -1) {
      expandedChapterIds.value.splice(idx, 1)
    } else {
      expandedChapterIds.value.push(id)
    }
  }

  const selectLesson = (id: number) => {
    selectedLessonId.value = id
    // Ensure parent chapter is expanded
    const parentChap = chapters.value.find(c => c.lessons.some(l => l.id === id))
    if (parentChap && !expandedChapterIds.value.includes(parentChap.id)) {
      expandedChapterIds.value.push(parentChap.id)
    }
  }

  // --- Chapter Operations ---
  const saveChapter = (chapterData: Partial<EditorChapter>) => {
    const targetId = editingChapter.value?.id || chapterData.id
    if (targetId) {
      // Update
      const idx = chapters.value.findIndex(c => c.id === targetId)
      if (idx > -1) {
        chapters.value[idx] = { ...chapters.value[idx], ...chapterData } as EditorChapter
      }
    } else {
      // Create
      const newChap: EditorChapter = {
        id: Date.now(),
        subjectId,
        title: chapterData.title || '',
        order: chapterData.order || chapters.value.length + 1,
        hidden: chapterData.hidden || false,
        lessons: []
      }
      chapters.value.push(newChap)
      chapters.value.sort((a, b) => a.order - b.order)
      if (!expandedChapterIds.value.includes(newChap.id)) expandedChapterIds.value.push(newChap.id)
    }
    isChapterModalOpen.value = false
    editingChapter.value = null
  }

  // --- Lesson Operations ---
  const saveLesson = (lessonData: Partial<EditorLesson>) => {
    const targetId = editingLesson.value?.id || lessonData.id
    if (!selectedChapter.value && !targetId) return // need chapter to create
    
    if (targetId) {
      // Update
      for (const c of chapters.value) {
        const idx = c.lessons.findIndex(l => l.id === targetId)
        if (idx > -1) {
          c.lessons[idx] = { ...c.lessons[idx], ...lessonData } as EditorLesson
          c.lessons.sort((a, b) => a.order - b.order)
          break
        }
      }
    } else if (selectedChapter.value) {
      // Create
      const chapterId = selectedChapter.value.id
      const newLesson: EditorLesson = {
        id: Date.now(),
        chapterId,
        title: lessonData.title || '',
        order: lessonData.order || selectedChapter.value.lessons.length + 1,
        type: lessonData.type || 'video',
        status: lessonData.status || 'draft',
        contents: []
      }
      const c = chapters.value.find(c => c.id === chapterId)
      if (c) {
        c.lessons.push(newLesson)
        c.lessons.sort((a, b) => a.order - b.order)
      }
      selectLesson(newLesson.id)
    }
    isLessonModalOpen.value = false
    editingLesson.value = null
  }

  // --- Content Operations ---
  const saveContent = (contentData: Partial<EditorContentBlock>) => {
    if (!selectedLesson.value) return
    const lesson = selectedLesson.value
    
    const targetId = editingContent.value?.id || contentData.id
    if (targetId) {
      // Update
      const idx = lesson.contents.findIndex(c => c.id === targetId)
      if (idx > -1) {
        lesson.contents[idx] = { ...lesson.contents[idx], ...contentData } as EditorContentBlock
        lesson.contents.sort((a, b) => a.order - b.order)
      }
    } else {
      // Create
      const newContent: EditorContentBlock = {
        ...contentData,
        id: Date.now(),
        lessonId: lesson.id,
        type: contentData.type || 'video',
        title: contentData.title || '',
        order: contentData.order || lesson.contents.length + 1,
        status: contentData.status || 'draft'
      } as EditorContentBlock
      
      lesson.contents.push(newContent)
      lesson.contents.sort((a, b) => a.order - b.order)
    }
    isContentDrawerOpen.value = false
    editingContent.value = null
  }

  const deleteItem = () => {
    if (!deleteTarget.value) return
    const { type, id, parentId } = deleteTarget.value
    
    if (type === 'chapter') {
      const idx = chapters.value.findIndex(c => c.id === id)
      if (idx > -1) chapters.value.splice(idx, 1)
      if (selectedChapterId.value === id) selectedChapterId.value = null
    } else if (type === 'lesson') {
      for (const c of chapters.value) {
        const idx = c.lessons.findIndex(l => l.id === id)
        if (idx > -1) {
          c.lessons.splice(idx, 1)
          break
        }
      }
      if (selectedLessonId.value === id) selectedLessonId.value = null
    } else if (type === 'content' && parentId) {
      for (const c of chapters.value) {
        const lesson = c.lessons.find(l => l.id === parentId)
        if (lesson) {
          const idx = lesson.contents.findIndex(ct => ct.id === id)
          if (idx > -1) lesson.contents.splice(idx, 1)
          break
        }
      }
    }
    
    isDeleteDialogOpen.value = false
    deleteTarget.value = null
  }

  return {
    chapters,
    selectedChapterId,
    selectedLessonId,
    expandedChapterIds,
    isChapterModalOpen,
    isLessonModalOpen,
    isContentDrawerOpen,
    isDeleteDialogOpen,
    editingChapter,
    editingLesson,
    editingContent,
    selectedContentType,
    deleteTarget,
    selectedLesson,
    selectedChapter,
    toggleChapter,
    selectLesson,
    saveChapter,
    saveLesson,
    saveContent,
    deleteItem
  }
}
