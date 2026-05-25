import { usePopupStore } from '@/stores/popup'

export function usePopup() {
  const store = usePopupStore()
  return store
}
