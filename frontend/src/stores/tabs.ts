import { ref } from 'vue'
import { defineStore } from 'pinia'

export interface TabItem {
  path: string
  title: string
  closeable: boolean
}

const HOME_PATH = '/dashboard'
const HOME_TITLE = '工作量看板'

export const useTabsStore = defineStore('tabs', () => {
  const tabs = ref<TabItem[]>([
    { path: HOME_PATH, title: HOME_TITLE, closeable: false },
  ])

  function addTab(path: string, title: string) {
    if (tabs.value.find((t) => t.path === path)) return
    tabs.value.push({ path, title, closeable: path !== HOME_PATH })
  }

  function closeTab(path: string): string {
    const idx = tabs.value.findIndex((t) => t.path === path)
    if (idx === -1) return path
    tabs.value.splice(idx, 1)
    const next = tabs.value[idx] ?? tabs.value[idx - 1] ?? tabs.value[0]
    return next?.path ?? HOME_PATH
  }

  function reset() {
    tabs.value = [{ path: HOME_PATH, title: HOME_TITLE, closeable: false }]
  }

  return { tabs, addTab, closeTab, reset }
})
