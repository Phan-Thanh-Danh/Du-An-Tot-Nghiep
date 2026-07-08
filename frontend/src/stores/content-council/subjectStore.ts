import { defineStore } from 'pinia'
import { ref, shallowRef } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type { ContentCouncilSubject, ContentCouncilSubjectDetail, SubjectContentSettings } from '@/types/content-council/subject'
import type { EditorChapter, EditorLesson, EditorContentBlock } from '@/types/content-council/curriculum'

export const useSubjectStore = defineStore('contentCouncilSubject', () => {
  const subjects = ref<ContentCouncilSubject[]>([])
  const subjectDetails = shallowRef<Record<number, ContentCouncilSubjectDetail>>({})
  const subjectSettings = ref<Record<number, SubjectContentSettings>>({})
  const initialized = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function init() {
    if (initialized.value) return
    loading.value = true
    error.value = null
    try {
      const res = await contentCouncilApi.getSubjects()
      const data = res?.data ?? res?.items ?? res ?? []
      subjects.value = Array.isArray(data) ? data : []
      initialized.value = true
    } catch (e: any) {
      error.value = e?.message || 'Không thể tải danh sách môn học'
    } finally {
      loading.value = false
    }
  }

  async function loadSubjectDetail(id: number) {
    if (subjectDetails.value[id]?.chapters) return subjectDetails.value[id]
    try {
      const [subRes, chaptersRes] = await Promise.all([
        contentCouncilApi.getSubjectById(id),
        contentCouncilApi.getChapters(id),
      ])
      const sub = subRes?.data ?? subRes
      const rawChapters = chaptersRes?.data ?? chaptersRes?.items ?? chaptersRes ?? []
      
      const mappedChapters: EditorChapter[] = Array.isArray(rawChapters) ? rawChapters.map((c: any) => ({
         id: c.maChuong || c.id,
         subjectId: c.maMonHoc || id,
         title: c.tieuDe || c.title || '',
         order: c.thuTu || c.order || 0,
         hidden: c.daAn || c.hidden || false,
         lessons: (c.baiHocs || c.lessons || []).map((l: any) => ({
            id: l.maBaiHoc || l.id,
            chapterId: l.maChuong || c.maChuong,
            title: l.tieuDe || l.title || '',
            order: l.thuTu || l.order || 0,
            type: l.loaiBaiHoc || l.type || 'van_ban',
            status: l.trangThai || l.status || 'draft',
            contents: (l.noiDungs || l.contents || []).map((n: any) => ({
               id: n.maNoiDung || n.id,
               lessonId: n.maBaiHoc || l.maBaiHoc,
               type: n.loaiNoiDung || n.type || 'van_ban',
               title: n.tieuDe || n.title || '',
               order: n.thuTu || n.order || 0,
               status: n.trangThai || n.status || 'draft',
               data: n.noiDungJson || n.noiDungHtml || n.data || ''
            }))
         }))
      })) : []

      subjectDetails.value = {
        ...subjectDetails.value,
        [id]: { ...sub, chapters: mappedChapters },
      }
      return subjectDetails.value[id]
    } catch (e: any) {
      error.value = e?.message || 'Không thể tải chi tiết môn học'
      return null
    }
  }

  function reset() {
    initialized.value = false
    subjects.value = []
    subjectDetails.value = {}
    subjectSettings.value = {}
    error.value = null
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

  function getChapters(subjectId: number) {
    return subjectDetails.value[subjectId]?.chapters || []
  }

  async function addChapter(subjectId: number, chapter: EditorChapter) {
    const detail = subjectDetails.value[subjectId]
    if (!detail) return
    try {
      const res = await contentCouncilApi.createChapter({
        subjectId,
        title: chapter.title,
        order: (detail.chapters?.length || 0) + 1,
      })
      const newChapter = res.data ?? res;
      if (!detail.chapters) detail.chapters = []
      detail.chapters.push({
         id: newChapter.maChuong || newChapter.id || Date.now(),
         subjectId,
         title: newChapter.tieuDe || newChapter.title || chapter.title,
         order: newChapter.thuTu || newChapter.order || chapter.order,
         hidden: newChapter.daAn || newChapter.hidden || false,
         lessons: []
      })
      detail.chapterCount = detail.chapters.length
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm chương'
    }
  }

  async function updateChapter(subjectId: number, chapterId: number, payload: Partial<EditorChapter>) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    try {
      await contentCouncilApi.updateChapter(chapterId, payload)
      const idx = detail.chapters.findIndex(c => c.id === chapterId)
      if (idx !== -1) Object.assign(detail.chapters[idx], payload)
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật chương'
    }
  }

  async function deleteChapter(subjectId: number, chapterId: number) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    try {
      await contentCouncilApi.deleteChapter(chapterId)
      detail.chapters = detail.chapters.filter(c => c.id !== chapterId)
      detail.chapterCount = detail.chapters.length
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa chương'
    }
  }

  async function reorderChapters(subjectId: number, newOrder: EditorChapter[]) {
    const detail = subjectDetails.value[subjectId]
    if (!detail) return
    try {
      await contentCouncilApi.reorderChapters(subjectId, { order: newOrder.map((c, i) => ({ id: c.id, order: i + 1 })) })
      detail.chapters = newOrder.map((c, i) => ({ ...c, order: i + 1 }))
    } catch (e: any) {
      error.value = e?.message || 'Không thể sắp xếp chương'
    }
  }

  async function addLesson(subjectId: number, chapterId: number, lesson: EditorLesson) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter) return
    try {
      const res = await contentCouncilApi.createLesson({
        chapterId,
        title: lesson.title,
        type: lesson.type,
        order: (chapter.lessons?.length || 0) + 1,
      })
      const newLesson = res.data ?? res;
      if (!chapter.lessons) chapter.lessons = []
      chapter.lessons.push({
         id: newLesson.maBaiHoc || newLesson.id || Date.now(),
         chapterId,
         title: newLesson.tieuDe || newLesson.title || lesson.title,
         order: newLesson.thuTu || newLesson.order || lesson.order,
         type: newLesson.loaiBaiHoc || newLesson.type || lesson.type,
         status: newLesson.trangThai || newLesson.status || 'draft',
         contents: []
      })
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm bài học'
    }
  }

  async function updateLesson(subjectId: number, chapterId: number, lessonId: number, payload: Partial<EditorLesson>) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    try {
      await contentCouncilApi.updateLesson(lessonId, payload)
      const idx = chapter.lessons.findIndex(l => l.id === lessonId)
      if (idx !== -1) Object.assign(chapter.lessons[idx], payload)
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật bài học'
    }
  }

  async function deleteLesson(subjectId: number, chapterId: number, lessonId: number) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    try {
      await contentCouncilApi.deleteLesson(lessonId)
      chapter.lessons = chapter.lessons.filter(l => l.id !== lessonId)
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa bài học'
    }
  }

  async function addContent(subjectId: number, chapterId: number, lessonId: number, content: EditorContentBlock) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    const lesson = chapter.lessons.find(l => l.id === lessonId)
    if (!lesson) return
    try {
      const res = await contentCouncilApi.createContent({
        lessonId,
        type: content.type,
        data: content.data,
        order: (lesson.contents?.length || 0) + 1,
      })
      const newContent = res.data ?? res;
      if (!lesson.contents) lesson.contents = []
      lesson.contents.push({
         id: newContent.maNoiDung || newContent.id || Date.now(),
         lessonId,
         type: newContent.loaiNoiDung || newContent.type || content.type,
         title: newContent.tieuDe || newContent.title || content.title,
         order: newContent.thuTu || newContent.order || content.order,
         status: newContent.trangThai || newContent.status || 'draft',
         data: newContent.noiDungJson || newContent.noiDungHtml || newContent.data || content.data
      })
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm nội dung'
    }
  }

  async function updateContent(subjectId: number, chapterId: number, lessonId: number, contentId: number, payload: Partial<EditorContentBlock>) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    const lesson = chapter.lessons.find(l => l.id === lessonId)
    if (!lesson?.contents) return
    try {
      await contentCouncilApi.updateContent(contentId, payload)
      const idx = lesson.contents.findIndex(c => c.id === contentId)
      if (idx !== -1) Object.assign(lesson.contents[idx], payload)
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật nội dung'
    }
  }

  async function deleteContent(subjectId: number, chapterId: number, lessonId: number, contentId: number) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    const lesson = chapter.lessons.find(l => l.id === lessonId)
    if (!lesson?.contents) return
    try {
      await contentCouncilApi.deleteContent(contentId)
      lesson.contents = lesson.contents.filter(c => c.id !== contentId)
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa nội dung'
    }
  }

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
    loading,
    error,
    init,
    loadSubjectDetail,
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
    deleteContent,
  }
})
