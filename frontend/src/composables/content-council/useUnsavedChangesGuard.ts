import { onBeforeRouteLeave } from 'vue-router'
import { Ref } from 'vue'

export function useUnsavedChangesGuard(isDirty: Ref<boolean>) {
  onBeforeRouteLeave((to, from, next) => {
    if (isDirty.value) {
      const answer = window.confirm('Bạn có thay đổi chưa lưu. Bạn có chắc chắn muốn rời khỏi trang này không?')
      if (answer) {
        next()
      } else {
        next(false)
      }
    } else {
      next()
    }
  })
}
