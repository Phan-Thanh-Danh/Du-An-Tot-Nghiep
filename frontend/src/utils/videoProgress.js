export function mergeMonotonicLessonProgress({
  lessonId,
  incomingPercent,
  currentLesson,
  lessonDraft,
}) {
  const incoming = Math.max(0, Math.min(100, Number(incomingPercent) || 0))
  const currentLessonPercent = String(currentLesson?.id) === String(lessonId)
    ? (Number(currentLesson?.progressPercent) || 0)
    : 0
  const draftPercent = Number(lessonDraft?.progressPercent) || 0

  return Math.max(incoming, currentLessonPercent, draftPercent)
}
