import { defineStore } from 'pinia'
import { ref } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type {
  ContentCouncilSubject,
  ContentCouncilSubjectDetail,
  SubjectContentSettings,
} from '@/types/content-council/subject'
import type { EditorChapter, EditorLesson, EditorContentBlock } from '@/types/content-council/curriculum'

function unwrapList(payload: any) {
  const data = payload?.data ?? payload?.Data ?? payload ?? []
  if (Array.isArray(data)) return { items: data, total: data.length }
  if (Array.isArray(data?.items)) return { items: data.items, total: data.totalCount ?? data.totalItems ?? data.items.length }
  if (Array.isArray(data?.Items)) return { items: data.Items, total: data.TotalCount ?? data.TotalItems ?? data.Items.length }
  return { items: [], total: 0 }
}

function unwrapData(payload: any) {
  return payload?.data ?? payload?.Data ?? payload
}

function parseContentData(value: any) {
  if (!value) return ''
  if (typeof value !== 'string') return value
  try {
    return JSON.parse(value)
  } catch {
    return value
  }
}

function mapSubject(source: any): ContentCouncilSubject {
  const id = source.maMonHoc ?? source.MaMonHoc ?? source.id
  const isActive = source.conHoatDong ?? source.ConHoatDong
  return {
    id,
    code: source.maCodeMonHoc ?? source.MaCodeMonHoc ?? source.code ?? '',
    name: source.tenMonHoc ?? source.TenMonHoc ?? source.name ?? '',
    thumbnailUrl: source.thumbnailUrl ?? source.ThumbnailUrl ?? '',
    status: source.status ?? (isActive ? 'draft' : 'empty'),
    chapterCount: source.chapterCount ?? source.soChuong ?? source.SoChuong ?? 0,
    lessonCount: source.lessonCount ?? source.soBaiHoc ?? source.SoBaiHoc ?? 0,
    contentCount: source.contentCount ?? source.soNoiDung ?? source.SoNoiDung ?? 0,
    quizCount: source.quizCount ?? source.soDeThi ?? source.SoDeThi ?? 0,
    updatedAt: source.updatedAt ?? source.ngayCapNhat ?? source.NgayCapNhat ?? new Date().toISOString(),
  }
}

function mapContentBlock(source: any): EditorContentBlock {
  const jsonData = source.noiDungJson ?? source.NoiDungJson ?? source.duLieuJson ?? source.data
  return {
    id: source.maNoiDung ?? source.MaNoiDung ?? source.id,
    lessonId: source.maBaiHoc ?? source.MaBaiHoc ?? source.lessonId,
    type: source.loaiNoiDung ?? source.LoaiNoiDung ?? source.type ?? 'van_ban',
    title: source.tieuDe ?? source.TieuDe ?? source.title ?? '',
    order: source.thuTu ?? source.ThuTu ?? source.order ?? 0,
    status: source.trangThai ?? source.TrangThai ?? source.status ?? 'draft',
    html: source.noiDungHtml ?? source.NoiDungHtml ?? source.html,
    fileUrl: source.urlTapTin ?? source.UrlTapTin ?? source.fileUrl,
    fileSize: source.kichThuocByte ?? source.KichThuocByte ?? source.fileSize,
    durationSeconds: source.thoiLuongGiay ?? source.ThoiLuongGiay ?? source.durationSeconds,
    quizId: source.maDeKiemTra ?? source.MaDeKiemTra ?? source.quizId,
    data: parseContentData(jsonData),
  } as EditorContentBlock
}

function mapLesson(source: any): EditorLesson {
  const contents = source.noiDungs ?? source.NoiDungs ?? source.contents ?? []
  return {
    id: source.maBaiHoc ?? source.MaBaiHoc ?? source.id,
    chapterId: source.maChuong ?? source.MaChuong ?? source.chapterId,
    title: source.tieuDe ?? source.TieuDe ?? source.title ?? '',
    order: source.thuTu ?? source.ThuTu ?? source.order ?? 0,
    type: source.loaiBaiHoc ?? source.LoaiBaiHoc ?? source.type ?? 'van_ban',
    status: source.trangThai ?? source.TrangThai ?? source.status ?? 'draft',
    contents: Array.isArray(contents) ? contents.map(mapContentBlock) : [],
  }
}

function mapChapter(source: any, subjectId: number): EditorChapter {
  const lessons = source.baiHocs ?? source.BaiHocs ?? source.lessons ?? []
  return {
    id: source.maChuong ?? source.MaChuong ?? source.id,
    subjectId: source.maMonHoc ?? source.MaMonHoc ?? source.subjectId ?? subjectId,
    title: source.tieuDe ?? source.TieuDe ?? source.title ?? '',
    order: source.thuTu ?? source.ThuTu ?? source.order ?? 0,
    hidden: source.daAn ?? source.DaAn ?? source.hidden ?? false,
    lessons: Array.isArray(lessons) ? lessons.map(mapLesson) : [],
  }
}

function mapSubjectDetail(source: any): ContentCouncilSubjectDetail {
  const subject = mapSubject(source)
  return {
    ...subject,
    description: source.description ?? source.moTa ?? source.MoTa ?? '',
    instructorCount: source.instructorCount ?? source.soGiangVien ?? source.SoGiangVien ?? 0,
    studentCount: source.studentCount ?? source.soHocSinh ?? source.SoHocSinh ?? 0,
    publishedContentRatio: source.publishedContentRatio ?? 0,
    createdAt: source.createdAt ?? source.ngayTao ?? source.NgayTao ?? new Date().toISOString(),
    isPublished: source.isPublished ?? source.conHoatDong ?? source.ConHoatDong ?? false,
  }
}

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
      const res = await contentCouncilApi.getSubjects(params)
      const { items, total } = unwrapList(res)
      subjects.value = items.map(mapSubject)
      totalSubjects.value = total
      currentPage.value = params.pageIndex || 1
      initialized.value = true
    } catch (e: any) {
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
      const [subRes, chaptersRes] = await Promise.all([
        contentCouncilApi.getSubjectById(id),
        contentCouncilApi.getChapters(id),
      ])

      const sub = mapSubjectDetail(unwrapData(subRes))
      const { items: rawChapters } = unwrapList(chaptersRes)
      const chapters = rawChapters.map((chapter: any) => mapChapter(chapter, id))

      subjectDetails.value = {
        ...subjectDetails.value,
        [id]: { ...sub, chapters },
      }
      syncSummary(id)
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
        maMonHoc: subjectId,
        tieuDe: chapter.title,
        thuTu: (detail.chapters?.length || 0) + 1,
      })
      const newChapter = mapChapter(unwrapData(res), subjectId)
      if (!detail.chapters) detail.chapters = []
      detail.chapters.push(newChapter)
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
      await contentCouncilApi.updateChapter(chapterId, {
        tieuDe: payload.title ?? '',
        daAn: payload.hidden,
      })
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
      await contentCouncilApi.reorderChapters(subjectId, {
        items: newOrder.map((chapter, index) => ({ id: chapter.id, thuTu: index + 1 })),
      })
      detail.chapters = newOrder.map((chapter, index) => ({ ...chapter, order: index + 1 }))
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
        maChuong: chapterId,
        tieuDe: lesson.title,
        loaiBaiHoc: lesson.type,
        thuTu: (chapter.lessons?.length || 0) + 1,
      })
      const newLesson = mapLesson(unwrapData(res))
      if (!chapter.lessons) chapter.lessons = []
      chapter.lessons.push(newLesson)
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
      await contentCouncilApi.updateLesson(lessonId, {
        tieuDe: payload.title,
        loaiBaiHoc: payload.type,
        trangThai: payload.status,
      })
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
        maBaiHoc: lessonId,
        loaiNoiDung: content.type,
        noiDungHtml: content.html,
        noiDungJson: content.data ? JSON.stringify(content.data) : undefined,
        urlTapTin: content.fileUrl,
        kichThuocByte: content.fileSize,
        thoiLuongGiay: content.durationSeconds,
        maDeKiemTra: content.quizId,
        thuTu: (lesson.contents?.length || 0) + 1,
      })
      const newContent = mapContentBlock(unwrapData(res))
      if (!lesson.contents) lesson.contents = []
      lesson.contents.push({
        ...newContent,
        title: newContent.title || content.title,
      })
      syncSummary(subjectId)
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm nội dung'
    }
  }

  async function updateContent(
    subjectId: number,
    chapterId: number,
    lessonId: number,
    contentId: number,
    payload: Partial<EditorContentBlock>,
  ) {
    const detail = subjectDetails.value[subjectId]
    if (!detail?.chapters) return
    const chapter = detail.chapters.find(c => c.id === chapterId)
    if (!chapter?.lessons) return
    const lesson = chapter.lessons.find(l => l.id === lessonId)
    if (!lesson?.contents) return
    try {
      await contentCouncilApi.updateContent(contentId, {
        noiDungHtml: payload.html,
        noiDungJson: payload.data ? JSON.stringify(payload.data) : undefined,
        urlTapTin: payload.fileUrl,
        storageKey: payload.fileName,
        kichThuocByte: payload.fileSize,
        thoiLuongGiay: payload.durationSeconds,
        maDeKiemTra: payload.quizId,
        trangThai: payload.status,
      })
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
    const chapterCount = detail.chapters?.length || 0
    let lessonCount = 0
    let contentCount = 0
    let quizCount = 0
    detail.chapters?.forEach(chapter => {
      lessonCount += chapter.lessons?.length || 0
      chapter.lessons?.forEach(lesson => {
        contentCount += lesson.contents?.length || 0
        lesson.contents?.forEach(block => {
          if (block.type === 'trac_nghiem') quizCount++
        })
      })
    })
    detail.chapterCount = chapterCount
    detail.lessonCount = lessonCount
    detail.contentCount = contentCount
    detail.quizCount = quizCount
    const subject = subjects.value.find(s => s.id === subjectId)
    if (subject) {
      subject.chapterCount = chapterCount
      subject.lessonCount = lessonCount
      subject.contentCount = contentCount
      subject.quizCount = quizCount
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
    deleteContent,
  }
})
