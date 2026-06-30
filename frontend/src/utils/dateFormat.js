import dayjs from 'dayjs'
import 'dayjs/locale/vi'

dayjs.locale('vi')
import relativeTime from 'dayjs/plugin/relativeTime'

dayjs.extend(relativeTime)

const EMPTY_DATE_LABEL = 'Chưa có dữ liệu'

function parseDate(value) {
  if (value === null || value === undefined || value === '') return null
  const parsed = dayjs(value)
  return parsed.isValid() ? parsed : null
}

export function formatDate(value, fallback = EMPTY_DATE_LABEL) {
  const parsed = parseDate(value)
  return parsed ? parsed.format('DD/MM/YYYY') : fallback
}

export function formatTime(value, fallback = EMPTY_DATE_LABEL) {
  if (typeof value === 'string' && /^\d{1,2}:\d{2}(:\d{2})?$/.test(value.trim())) {
    return value.trim().slice(0, 5)
  }

  const parsed = parseDate(value)
  return parsed ? parsed.format('HH:mm') : fallback
}

export function formatDateTime(value, fallback = EMPTY_DATE_LABEL) {
  const parsed = parseDate(value)
  return parsed ? parsed.format('DD/MM/YYYY HH:mm') : fallback
}

export function formatDateRange(start, end, fallback = EMPTY_DATE_LABEL) {
  const startLabel = formatDate(start, '')
  const endLabel = formatDate(end, '')

  if (!startLabel && !endLabel) return fallback
  if (startLabel && !endLabel) return `Từ ${startLabel}`
  if (!startLabel && endLabel) return `Đến ${endLabel}`
  return `${startLabel} - ${endLabel}`
}

export function formatTimeRange(start, end, fallback = EMPTY_DATE_LABEL) {
  const startLabel = formatTime(start, '')
  const endLabel = formatTime(end, '')

  if (!startLabel && !endLabel) return fallback
  if (startLabel && !endLabel) return `Từ ${startLabel}`
  if (!startLabel && endLabel) return `Đến ${endLabel}`
  return `${startLabel} - ${endLabel}`
}

export function formatWeekdayDate(value, fallback = EMPTY_DATE_LABEL) {
  const parsed = parseDate(value)
  return parsed ? parsed.format('dddd, DD/MM/YYYY') : fallback
}

export function toDateInputValue(value) {
  const parsed = parseDate(value)
  return parsed ? parsed.format('YYYY-MM-DD') : ''
}

export function isSameDay(left, right) {
  const leftDate = parseDate(left)
  const rightDate = parseDate(right)
  return Boolean(leftDate && rightDate && leftDate.isSame(rightDate, 'day'))
}

export { parseDate }

export function formatTimeAgo(value, fallback = EMPTY_DATE_LABEL) {
  const parsed = parseDate(value)
  return parsed ? parsed.fromNow() : fallback
}
