import { defineStore } from 'pinia'
import { ref } from 'vue'
import { initializeSubjectMockData } from '@/mocks/content-council'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type { ContentCouncilSubject, ContentCouncilSubjectDetail, SubjectContentSettings } from '@/types/content-council/subject'
import type { EditorChapter, EditorLesson, EditorContentBlock } from '@/types/content-council/curriculum'

const ENABLE_MOCK_API =
  import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const normalizeLessonStatus = (status?: string) => {
  switch (status) {
    case 'nhap':
      return 'draft'
    case 'da_xuat_ban':
      return 'published'
    default:
      return status || 'draft'
  }
}

const toLessonApiStatus = (status?: string) => {
  switch (status) {
    case 'draft':
      return 'nhap'
    case 'published':
      return 'da_xuat_ban'
    default:
      return status
  }
}

const normalizeLessonType = (type?: string) => {
  if (type === 'document') return 'pdf'
  return type || 'van_ban'
}

const mapLessonFromApi = (l: any) => ({
  id: l.maBaiHoc || l.id,
  chapterId: l.maChuong || l.chapterId,
  title: l.tieuDe || l.title,
  type: normalizeLessonType(l.loaiBaiHoc || l.type),
  status: normalizeLessonStatus(l.trangThai || l.status),
  order: l.thuTu || l.order,
  contentCount: l.soLuongNoiDung || l.soNoiDung || l.contentCount || 0,
  contents: (l.noiDungs || l.contents || []).map((cb: any) => {
    const dataObj = cb.duLieuJson ? JSON.parse(cb.duLieuJson) : (cb.noiDungJson ? JSON.parse(cb.noiDungJson) : (cb.data || {}));
    return {
      id: cb.maNoiDung || cb.id,
      lessonId: cb.maBaiHoc || cb.lessonId,
      type: cb.loaiNoiDung || cb.type,
      title: cb.tieuDe || cb.title,
      status: normalizeLessonStatus(cb.trangThai || cb.status),
      order: cb.thuTu || cb.order,
      ...dataObj
    }
  })
})

export const useSubjectStore = defineStore('contentCouncilSubject', () => {
  const subjects = ref<ContentCouncilSubject[]>([])
  const subjectDetails = ref<Record<number, ContentCouncilSubjectDetail>>({})
  const subjectSettings = ref<Record<number, SubjectContentSettings>>({})
  const initialized = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const totalSubjects = ref(0)
  const currentPage = ref(1)

  async function fetchSubjects(params: any = {}) {
    loading.value = true
    error.value = null
    try {
      if (ENABLE_MOCK_API) {
        const mock = initializeSubjectMockData()
        subjects.value = mock.subjects
        totalSubjects.value = mock.subjects.length
        initialized.value = true
        return
      }
      const res = await contentCouncilApi.getSubjects(params)
      let payloadData = res?.data ?? res?.Data ?? res ?? []
      let itemsList = []
      
      if (payloadData && !Array.isArray(payloadData)) {
         if (Array.isArray(payloadData.items)) {
             itemsList = payloadData.items
             totalSubjects.value = payloadData.totalCount ?? payloadData.totalItems ?? payloadData.items.length
         } else if (Array.isArray(payloadData.Items)) {
             itemsList = payloadData.Items
             totalSubjects.value = payloadData.TotalCount ?? payloadData.TotalItems ?? payloadData.Items.length
         }
      } else if (Array.isArray(payloadData)) {
         itemsList = payloadData
         totalSubjects.value = itemsList.length
      }

      subjects.value = itemsList.map(s => {
        // Map backend DTO to frontend format if necessary
        const id = s.maMonHoc ?? s.MaMonHoc
        if (id) {
          return {
            id: id,
            code: s.maCodeMonHoc ?? s.MaCodeMonHoc,
            name: s.tenMonHoc ?? s.TenMonHoc,
            status: (s.conHoatDong ?? s.ConHoatDong) ? 'draft' : 'empty', // Fallback status
            chapterCount: 0,
            lessonCount: 0,
            contentCount: 0,
            quizCount: 0,
            updatedAt: new Date().toISOString()
          }
        }
        return s
      })
      currentPage.value = params.pageIndex || 1
      initialized.value = true
    } catch (e: any) {
      if (ENABLE_MOCK_API) {
        const mock = initializeSubjectMockData()
        subjects.value = mock.subjects
        totalSubjects.value = mock.subjects.length
        initialized.value = true
        return
      }
      error.value = e?.message || 'Không thể tải danh sách môn học'
    } finally {
      loading.value = false
    }
  }

  async function init() {
    if (initialized.value) return
    await fetchSubjects({ pageIndex: 1, pageSize: 20 })
  }

  async function loadSubjectDetail(id: number) {
    if (subjectDetails.value[id]?.chapters) return subjectDetails.value[id]
    try {
      if (ENABLE_MOCK_API) {
        const mock = initializeSubjectMockData()
        if (mock.subjectDetails[id]) {
          subjectDetails.value = { ...subjectDetails.value, [id]: mock.subjectDetails[id] }
        }
        return subjectDetails.value[id]
      }
      const [subRes, chaptersRes] = await Promise.all([
        contentCouncilApi.getSubjectById(id),
        contentCouncilApi.getChapters(id),
      ])
      let sub = subRes?.data ?? subRes
      if (sub && sub.maMonHoc) {
        sub = {
          id: sub.maMonHoc,
          code: sub.maCodeMonHoc,
          name: sub.tenMonHoc,
          status: sub.conHoatDong ? 'draft' : 'empty',
          chapterCount: 0,
          lessonCount: 0,
          contentCount: 0,
          quizCount: 0,
          description: '',
          instructorCount: 0,
          studentCount: 0,
          publishedContentRatio: 0,
          isPublished: sub.conHoatDong,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString()
        }
      }
      
      let chapters = chaptersRes?.data ?? chaptersRes?.items ?? chaptersRes ?? []
      if (chapters && !Array.isArray(chapters) && Array.isArray(chapters.items)) {
        chapters = chapters.items
      } else if (chapters && !Array.isArray(chapters) && Array.isArray(chapters.Items)) {
        chapters = chapters.Items
      }
      
      // Map backend DTO to frontend format
      chapters = Array.isArray(chapters) ? chapters.map((c: any) => ({
        id: c.maChuong || c.id,
        title: c.tieuDe || c.title,
        order: c.thuTu || c.order,
        hidden: c.daAn || c.hidden || false,
        lessons: (c.baiHocs || c.lessons || []).map(mapLessonFromApi)
      })) : []
      
      subjectDetails.value = {
        ...subjectDetails.value,
        [id]: { ...sub, chapters },
      }
      return subjectDetails.value[id]
    } catch (e: any) {
      if (ENABLE_MOCK_API) {
        const mock = initializeSubjectMockData()
        if (mock.subjectDetails[id]) {
          subjectDetails.value = { ...subjectDetails.value, [id]: mock.subjectDetails[id] }
        }
        return subjectDetails.value[id]
      }
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
      if (!ENABLE_MOCK_API) {
        await contentCouncilApi.createChapter({
          maMonHoc: subjectId,
          tieuDe: chapter.title,
          thuTu: (detail.chapters?.length || 0) + 1,
        })
      }
      if (!detail.chapters) detail.chapters = []
      detail.chapters.push(chapter)
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
      if (!ENABLE_MOCK_API) {
        const updatePayload: any = {}
        if (payload.title !== undefined) updatePayload.tieuDe = payload.title
        if (payload.hidden !== undefined) updatePayload.daAn = payload.hidden
        if (payload.order !== undefined) updatePayload.thuTu = payload.order
        await contentCouncilApi.updateChapter(chapterId, updatePayload)
      }
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
      if (!ENABLE_MOCK_API) {
        await contentCouncilApi.deleteChapter(chapterId)
      }
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
      if (!ENABLE_MOCK_API) {
        await contentCouncilApi.reorderChapters(subjectId, { order: newOrder.map((c, i) => ({ id: c.id, order: i + 1 })) })
      }
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
      if (!ENABLE_MOCK_API) {
        const res = await contentCouncilApi.createLesson({
          maChuong: chapterId,
          tieuDe: lesson.title,
          loaiBaiHoc: normalizeLessonType(lesson.type),
          thuTu: (chapter.lessons?.length || 0) + 1,
        })
        const createdLesson = res?.data ?? res
        if (createdLesson && createdLesson.maBaiHoc) {
          lesson.id = createdLesson.maBaiHoc
        }
      }
      if (!chapter.lessons) chapter.lessons = []
      chapter.lessons.push(lesson)
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
      if (!ENABLE_MOCK_API) {
        const updatePayload: any = {}
        if (payload.title !== undefined) updatePayload.tieuDe = payload.title
        if (payload.type !== undefined) updatePayload.loaiBaiHoc = normalizeLessonType(payload.type)
        if (payload.status !== undefined) updatePayload.trangThai = toLessonApiStatus(payload.status)
        if (payload.order !== undefined && payload.order !== null) updatePayload.thuTu = payload.order
        const updated = await contentCouncilApi.updateLesson(lessonId, updatePayload)
        const updatedPayload = updated?.data ?? updated
        const idx = chapter.lessons.findIndex(l => l.id === lessonId)
        if (idx !== -1 && updatedPayload) {
          chapter.lessons[idx] = mapLessonFromApi(updatedPayload)
          return
        }
      }
      const idx = chapter.lessons.findIndex(l => l.id === lessonId)
      if (idx !== -1) Object.assign(chapter.lessons[idx], payload)
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật bài học'
      throw e
    }
  }

  async function deleteLesson(subjectId: number, chapterId: number, lessonId: number) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    try {
      if (!ENABLE_MOCK_API) {
        await contentCouncilApi.deleteLesson(lessonId)
      }
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
      if (!ENABLE_MOCK_API) {
        const { id: _id, lessonId: _l, type: _t, status: _s, order: _o, data, ...rest } = content as any;
        const duLieu = data ? data : rest;
        await contentCouncilApi.createContent({
          maBaiHoc: lessonId,
          loaiNoiDung: content.type,
          noiDungJson: JSON.stringify(duLieu),
          thuTu: (lesson.contents?.length || 0) + 1,
        })
      }
      if (!lesson.contents) lesson.contents = []
      lesson.contents.push(content)
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
      if (!ENABLE_MOCK_API) {
        const updatePayload: any = {}
        const { id: _id, lessonId: _l, type, status, order, data, ...rest } = payload as any;
        if (type !== undefined) updatePayload.loaiNoiDung = type
        if (status !== undefined) updatePayload.trangThai = status
        if (order !== undefined) updatePayload.thuTu = order
        
        const duLieu = data ? data : rest;
        if (Object.keys(duLieu).length > 0) {
          updatePayload.noiDungJson = JSON.stringify(duLieu)
        }
        await contentCouncilApi.updateContent(contentId, updatePayload)
      }
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
      if (!ENABLE_MOCK_API) {
        await contentCouncilApi.deleteContent(contentId)
      }
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
    totalSubjects,
    currentPage,
    init,
    fetchSubjects,
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
    deleteContent
  }
})
