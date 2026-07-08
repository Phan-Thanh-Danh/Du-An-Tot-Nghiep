import { ref, computed } from 'vue'
import dayjs from 'dayjs'
import isoWeek from 'dayjs/plugin/isoWeek'
import 'dayjs/locale/vi'

dayjs.extend(isoWeek)
dayjs.locale('vi')

const events = ref([])
const currentMonth = ref(dayjs())

export function useSchedule() {
  // ── Month view ─────────────────────────────────────
  const monthGrid = computed(() => {
    const startOfMonth = currentMonth.value.startOf('month')
    const endOfMonth = currentMonth.value.endOf('month')
    const startOfWeek = startOfMonth.startOf('isoWeek')
    const endOfWeek = endOfMonth.endOf('isoWeek')
    const weeks = []
    let current = startOfWeek
    while (current.isBefore(endOfWeek) || current.isSame(endOfWeek, 'day')) {
      const week = []
      for (let i = 0; i < 7; i++) {
        const d = current.add(i, 'day')
        const iso = d.format('YYYY-MM-DD')
        week.push({
          date: d,
          isoDate: iso,
          dayNum: d.format('D'),
          isToday: d.isSame(dayjs(), 'day'),
          isCurrentMonth: d.month() === currentMonth.value.month(),
          events: events.value.filter(e => dayjs(e.start).format('YYYY-MM-DD') === iso),
        })
      }
      weeks.push(week)
      current = current.add(7, 'day')
    }
    return weeks
  })

  const monthLabel = computed(() =>
    `Tháng ${currentMonth.value.format('M')}, ${currentMonth.value.format('YYYY')}`
  )

  // ── Navigation ──────────────────────────────────────
  function nextMonth() { currentMonth.value = currentMonth.value.add(1, 'month') }
  function prevMonth() { currentMonth.value = currentMonth.value.add(-1, 'month') }
  function goToToday() { currentMonth.value = dayjs() }

  // ── CRUD ────────────────────────────────────────────
  let nextId = Math.max(0, ...events.value.map(e => e.id)) + 1

  function createEvent(eventData) {
    const newEvent = {
      id: nextId++,
      title: eventData.title || '',
      teacher: eventData.teacher || '',
      room: eventData.room || '',
      start: eventData.start || dayjs().format('YYYY-MM-DDTHH:mm:ss'),
      end: eventData.end || dayjs().add(1, 'hour').format('YYYY-MM-DDTHH:mm:ss'),
      status: eventData.status || 'draft',
      color: eventData.color || '#6366f1',
      type: eventData.type || 'class',
    }
    events.value.push(newEvent)
    return newEvent
  }

  function updateEvent(id, data) {
    const event = events.value.find(e => e.id === id)
    if (!event) return null
    Object.assign(event, data)
    return event
  }

  function deleteEvent(id) {
    const idx = events.value.findIndex(e => e.id === id)
    if (idx === -1) return false
    events.value.splice(idx, 1)
    return true
  }

  function getEventById(id) { return events.value.find(e => e.id === id) }

  // ── Stats ──────────────────────────────────────────
  const totalEvents = computed(() => events.value.length)
  const publishedEvents = computed(() => events.value.filter(e => e.status === 'published').length)
  const pendingEvents = computed(() => events.value.filter(e => e.status === 'pending').length)
  const draftEvents = computed(() => events.value.filter(e => e.status === 'draft').length)

  // ── Filter ─────────────────────────────────────────
  function filterEventsByLecturer(lecturerName) {
    if (!lecturerName || lecturerName === 'Tất cả giảng viên') return events.value
    return events.value.filter(e => e.teacher === lecturerName)
  }

  function filterEventsByQuery(query, eventList) {
    const source = eventList || events.value
    if (!query || !query.trim()) return source
    const q = query.toLowerCase()
    return source.filter(e =>
      e.title.toLowerCase().includes(q) ||
      e.teacher.toLowerCase().includes(q) ||
      e.room.toLowerCase().includes(q)
    )
  }

  // ── Export ──────────────────────────────────────────
  function exportToExcel(filteredEvents, _semester) {
    return filteredEvents.map(s => ({
      'Môn học': s.title,
      'Giảng viên': s.teacher,
      'Phòng': s.room,
      'Bắt đầu': dayjs(s.start).format('DD/MM/YYYY HH:mm'),
      'Kết thúc': dayjs(s.end).format('DD/MM/YYYY HH:mm'),
      'Trạng thái': s.status === 'published' ? 'Đã công bố' : s.status === 'pending' ? 'Chờ duyệt' : 'Bản nháp',
    }))
  }

  return {
    currentMonth, monthGrid, monthLabel,
    nextMonth, prevMonth, goToToday,
    createEvent, updateEvent, deleteEvent, getEventById,
    totalEvents, publishedEvents, pendingEvents, draftEvents,
    filterEventsByLecturer, filterEventsByQuery, exportToExcel,
  }
}
