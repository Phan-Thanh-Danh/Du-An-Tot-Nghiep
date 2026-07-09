import { ref, computed } from 'vue'
import { useSubjectStore } from '@/stores/content-council/subjectStore'
import { EditorChapter, EditorLesson, EditorContentBlock } from '@/types/content-council/curriculum'
import { ContentBlockType } from '@/types/content-council/common'

export const useCurriculumEditor = (subjectId: number) => {
  const store = useSubjectStore()
  
  // Ensure store is initialized
  store.init()

  const chapters = computed(() => store.getChapters(subjectId))

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
  const selectedContentType = ref<ContentBlockType>('video')
  
  const deleteTarget = ref<{ type: 'chapter' | 'lesson' | 'content', id: number, parentId?: number } | null>(null)

  // Computed
  const selectedLesson = computed(() => {
    if (!selectedLessonId.value) return null
    for (const c of chapters.value) {
      const lesson = c.lessons?.find(l => l.id === selectedLessonId.value)
      if (lesson) return lesson
    }
    return null
  })

  const selectedChapter = computed(() => {
    if (!selectedChapterId.value && !selectedLesson.value) return null
    const cid = selectedChapterId.value || chapters.value.find(c => c.lessons?.some(l => l.id === selectedLessonId.value))?.id
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
    const parentChap = chapters.value.find(c => c.lessons?.some(l => l.id === id))
    if (parentChap && !expandedChapterIds.value.includes(parentChap.id)) {
      expandedChapterIds.value.push(parentChap.id)
    }
  }

  // --- CRUD Operations delegating to store ---

  const saveChapter = async (chapter: any) => {
    if (editingChapter.value) {
      await store.updateChapter(subjectId, editingChapter.value.id, chapter)
    } else {
      const newChap = {
        ...chapter,
        id: Date.now(),
        order: chapters.value.length + 1,
        lessons: []
      }
      await store.addChapter(subjectId, newChap)
    }
    isChapterModalOpen.value = false
    editingChapter.value = null
  }

  const saveLesson = async (lesson: any) => {
    const editingChapId = editingLesson.value
      ? chapters.value.find(c => c.lessons?.some(l => l.id === editingLesson.value?.id))?.id
      : null
    const chapId = editingChapId || selectedChapterId.value || selectedChapter.value?.id
    if (!chapId) return

    if (editingLesson.value) {
      await store.updateLesson(subjectId, chapId, editingLesson.value.id, lesson)
    } else {
      const chapter = chapters.value.find(c => c.id === chapId)
      const newLesson = {
        ...lesson,
        id: Date.now(),
        chapterId: chapId,
        order: (chapter?.lessons?.length || 0) + 1,
        contents: []
      }
      await store.addLesson(subjectId, chapId, newLesson)
      selectLesson(newLesson.id)
      expandedChapterIds.value.push(chapId)
      
      // Auto-open content drawer based on the selected lesson type
      if (lesson.type) {
        // Map lesson type to content type if needed, e.g. 'van_ban' -> 'text'
        const mappedType = lesson.type === 'van_ban' ? 'text' : lesson.type === 'pdf' ? 'document' : lesson.type === 'trac_nghiem' ? 'quiz' : lesson.type;
        selectedContentType.value = mappedType;
        // Small timeout to allow lesson to be fully selected and DOM to update before opening drawer
        setTimeout(() => {
          isContentDrawerOpen.value = true;
        }, 50);
      }
    }
    isLessonModalOpen.value = false
    editingLesson.value = null
  }

  const saveContent = async (content: any) => {
    const chapId = selectedChapter.value?.id
    const lessonId = selectedLessonId.value
    if (!chapId || !lessonId) return

    if (editingContent.value) {
      await store.updateContent(subjectId, chapId, lessonId, editingContent.value.id, content)
    } else {
      const lesson = selectedLesson.value
      const newContent = {
        ...content,
        id: Date.now(),
        lessonId: lessonId,
        order: (lesson?.contents?.length || 0) + 1,
      }
      await store.addContent(subjectId, chapId, lessonId, newContent)
    }
    isContentDrawerOpen.value = false
    editingContent.value = null
  }

  const confirmDelete = () => {
    if (!deleteTarget.value) return
    const { type, id, parentId } = deleteTarget.value

    if (type === 'chapter') {
      store.deleteChapter(subjectId, id)
      if (selectedChapterId.value === id) selectedChapterId.value = null
    } else if (type === 'lesson') {
      const chapId = parentId || chapters.value.find(c => c.lessons?.some(l => l.id === id))?.id
      if (chapId) store.deleteLesson(subjectId, chapId, id)
      if (selectedLessonId.value === id) selectedLessonId.value = null
    } else if (type === 'content') {
      const chapId = selectedChapter.value?.id
      const lessonId = parentId || selectedLessonId.value
      if (chapId && lessonId) store.deleteContent(subjectId, chapId, lessonId, id)
    }

    isDeleteDialogOpen.value = false
    deleteTarget.value = null
  }

  // --- Handlers ---
  const openAddChapter = () => {
    editingChapter.value = null
    isChapterModalOpen.value = true
  }

  const openEditChapter = (chapter: EditorChapter) => {
    editingChapter.value = { ...chapter }
    isChapterModalOpen.value = true
  }

  const openAddLesson = (chapterId: number) => {
    selectedChapterId.value = chapterId
    editingLesson.value = null
    isLessonModalOpen.value = true
  }

  const openEditLesson = (lesson: EditorLesson) => {
    editingLesson.value = { ...lesson }
    isLessonModalOpen.value = true
  }

  const openAddContent = (type: ContentBlockType) => {
    selectedContentType.value = type
    editingContent.value = null
    isContentDrawerOpen.value = true
  }

  const openEditContent = (content: EditorContentBlock) => {
    selectedContentType.value = content.type
    editingContent.value = { ...content }
    isContentDrawerOpen.value = true
  }

  const requestDelete = (type: 'chapter' | 'lesson' | 'content', id: number, parentId?: number) => {
    deleteTarget.value = { type, id, parentId }
    isDeleteDialogOpen.value = true
  }

  const reorderChapters = (newChapters: EditorChapter[]) => {
    store.reorderChapters(subjectId, newChapters)
  }

  const reorderLessons = (chapterId: number, newLessons: EditorLesson[]) => {
    const detail = store.getSubjectDetail(subjectId)
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.id === chapterId)
      if (chapter) {
        chapter.lessons = newLessons.map((l, i) => ({ ...l, order: i + 1 }))
        store.syncSummary(subjectId) // call sync just in case
      }
    }
  }

  const reorderContents = (lessonId: number, newContents: EditorContentBlock[]) => {
    const detail = store.getSubjectDetail(subjectId)
    if (detail?.chapters) {
      const chapter = detail.chapters.find(c => c.lessons?.some(l => l.id === lessonId))
      if (chapter?.lessons) {
        const lesson = chapter.lessons.find(l => l.id === lessonId)
        if (lesson) {
          lesson.contents = newContents.map((c, i) => ({ ...c, order: i + 1 }))
        }
      }
    }
  }

  // Duplicate
  const duplicateContent = (content: EditorContentBlock) => {
    const chapId = selectedChapter.value?.id
    const lessonId = selectedLessonId.value
    if (!chapId || !lessonId) return

    const lesson = selectedLesson.value
    const cloned = JSON.parse(JSON.stringify(content))
    cloned.id = Date.now()
    cloned.title = `${cloned.title} (Bản sao)`
    cloned.order = (lesson?.contents?.length || 0) + 1
    
    store.addContent(subjectId, chapId, lessonId, cloned)
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
    selectedLesson,
    selectedChapter,
    deleteTarget,

    toggleChapter,
    selectLesson,
    saveChapter,
    saveLesson,
    saveContent,
    confirmDelete,

    openAddChapter,
    openEditChapter,
    openAddLesson,
    openEditLesson,
    openAddContent,
    openEditContent,
    requestDelete,

    reorderChapters,
    reorderLessons,
    reorderContents,
    duplicateContent
  }
}
