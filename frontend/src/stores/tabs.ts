import { ref } from 'vue'
import { defineStore } from 'pinia'

export interface TabItem {
  path: string
  title: string
  closeable: boolean
}

export const useTabsStore = defineStore('tabs', () => {
  const tabs = ref<TabItem[]>([
    { path: '/dashboard', title: '首页', closeable: false },
  ])

  function addTab(path: string, title: string) {
    if (tabs.value.find(t => t.path === path)) return
    tabs.value.push({ path, title, closeable: path !== '/dashboard' })
  }

  function closeTab(path: string): string {
    const idx = tabs.value.findIndex(t => t.path === path)
    if (idx === -1) return path
    tabs.value.splice(idx, 1)
    // 返回关闭后应跳转到的路径（后一个 → 前一个 → 第一个）
    const next = tabs.value[idx] ?? tabs.value[idx - 1] ?? tabs.value[0]
    return next?.path ?? '/dashboard'
  }

  function reset() {
    tabs.value = [{ path: '/dashboard', title: '首页', closeable: false }]
  }

  return { tabs, addTab, closeTab, reset }
})
