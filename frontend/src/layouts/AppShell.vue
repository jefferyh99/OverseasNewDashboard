<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useTabsStore } from '@/stores/tabs'
import { settingsApi } from '@/services/api'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const tabsStore = useTabsStore()

interface LeafItem { label: string; path: string; icon: string }
interface GroupItem { label: string; key: string; icon: string; children: Omit<LeafItem, 'icon'>[] }
type MenuItem = LeafItem | GroupItem

interface WarehouseClockConfigItem {
  warehouseId: string
  warehouseName: string
  timeZoneId: string
}

interface WarehouseClockViewItem extends WarehouseClockConfigItem {
  localDateTime: string
  seasonLabel: '冬令时' | '夏令时'
}

const menuGroups: MenuItem[] = [
  { label: '工作看板', path: '/dashboard', icon: 'W' },
  { label: '异常看板', path: '/anomaly-dashboard', icon: 'A' },
  {
    label: '异常管理',
    key: '/anomalies',
    icon: 'M',
    children: [
      { label: '包裹出库异常', path: '/anomalies/outbound' },
      { label: '入库单箱子到仓不齐', path: '/anomalies/inbound' },
      { label: 'SKU上架异常', path: '/anomalies/shelving' },
    ],
  },
  {
    label: '工作管理',
    key: '/workloads',
    icon: 'J',
    children: [
      { label: '出库包裹详情', path: '/workloads/outbound-packages' },
      { label: 'SKU详情', path: '/workloads/skus' },
      { label: '箱子详情', path: '/workloads/cartons' },
      { label: '待到仓货量详情', path: '/workloads/future-inbound-volume' },
    ],
  },
  {
    label: '系统设置',
    key: '/settings',
    icon: 'S',
    children: [
      { label: '提醒配置', path: '/settings/alerts' },
    ],
  },
  {
    label: '原型',
    key: '/prototypes',
    icon: 'P',
    children: [
      { label: '企微通知原型', path: '/prototypes/wecom-notification' },
    ],
  },
]

const warehouseNameMap: Record<string, string> = {
  DE: '德国仓',
  ON: '安大略仓',
}

const clockConfigs = ref<WarehouseClockConfigItem[]>([])
const clockItems = ref<WarehouseClockViewItem[]>([])
let clockTimer: ReturnType<typeof setInterval> | null = null
let clockConfigRefreshTimer: ReturnType<typeof setInterval> | null = null

function isGroup(item: MenuItem): item is GroupItem {
  return 'children' in item
}

const openedKeys = ['/' + route.path.split('/')[1]]

watch(
  () => route.path,
  (path) => {
    const title = (route.meta as Record<string, string>).title ?? path
    if (route.meta.requiresAuth) {
      tabsStore.addTab(path, title)
    }
  },
  { immediate: true },
)

function closeTab(path: string) {
  const target = tabsStore.closeTab(path)
  if (route.path === path) router.push(target)
}

function getBreadcrumbs() {
  const meta = route.meta as Record<string, string>
  const items: { label: string; path?: string }[] = [{ label: '工作看板', path: '/dashboard' }]
  if (meta.groupTitle && route.path !== '/dashboard') items.push({ label: meta.groupTitle })
  if (meta.title && route.path !== '/dashboard') items.push({ label: meta.title, path: route.path })
  return items
}

function handleUserCommand(cmd: string) {
  if (cmd === 'logout') {
    tabsStore.reset()
    authStore.signOut()
    router.push('/login')
  }
}

function getDateTimeInZone(timeZoneId: string, at: Date) {
  const parts = new Intl.DateTimeFormat('en-GB', {
    timeZone: timeZoneId,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  }).formatToParts(at)

  const partMap = Object.fromEntries(parts.map(part => [part.type, part.value]))
  return `${partMap.year}-${partMap.month}-${partMap.day} ${partMap.hour}:${partMap.minute}:${partMap.second}`
}

function getSeasonLabel(timeZoneId: string, at: Date): '冬令时' | '夏令时' {
  const parts = new Intl.DateTimeFormat('en-US', {
    timeZone: timeZoneId,
    timeZoneName: 'long',
  }).formatToParts(at)
  const longName = parts.find(part => part.type === 'timeZoneName')?.value ?? ''
  return /daylight|summer/i.test(longName) ? '夏令时' : '冬令时'
}

function rebuildClockItems(at: Date) {
  clockItems.value = clockConfigs.value.map(item => ({
    ...item,
    localDateTime: getDateTimeInZone(item.timeZoneId, at),
    seasonLabel: getSeasonLabel(item.timeZoneId, at),
  }))
}

async function loadClockConfigs() {
  try {
    const response = await settingsApi.getAllAlerts()
    const rows = response.data.data ?? []
    clockConfigs.value = rows.map(row => ({
      warehouseId: row.warehouseId,
      warehouseName: warehouseNameMap[row.warehouseId] ?? row.warehouseId,
      timeZoneId: row.timeZoneId,
    }))
    rebuildClockItems(new Date())
  } catch {
    // Keep UI functional even if clock data cannot be loaded.
    clockConfigs.value = []
    clockItems.value = []
  }
}

onMounted(async () => {
  await loadClockConfigs()

  clockTimer = setInterval(() => {
    rebuildClockItems(new Date())
  }, 1000)

  clockConfigRefreshTimer = setInterval(() => {
    void loadClockConfigs()
  }, 60_000)
})

onUnmounted(() => {
  if (clockTimer) {
    clearInterval(clockTimer)
    clockTimer = null
  }
  if (clockConfigRefreshTimer) {
    clearInterval(clockConfigRefreshTimer)
    clockConfigRefreshTimer = null
  }
})
</script>

<template>
  <div class="app-shell">
    <aside class="sidebar">
      <div class="brand">
        <span class="brand-icon">O</span>
        <span class="brand-name">Ops Monitor</span>
      </div>

      <el-menu
        :default-active="route.path"
        :default-openeds="openedKeys"
        router
        class="sidebar-menu"
      >
        <template v-for="item in menuGroups" :key="item.label">
          <el-menu-item v-if="!isGroup(item)" :index="(item as LeafItem).path">
            <template #title>
              <span class="menu-icon">{{ (item as LeafItem).icon }}</span>
              {{ item.label }}
            </template>
          </el-menu-item>

          <el-sub-menu v-else :index="(item as GroupItem).key">
            <template #title>
              <span class="menu-icon">{{ (item as GroupItem).icon }}</span>
              {{ item.label }}
            </template>
            <el-menu-item
              v-for="child in (item as GroupItem).children"
              :key="child.path"
              :index="child.path"
            >
              {{ child.label }}
            </el-menu-item>
          </el-sub-menu>
        </template>
      </el-menu>
    </aside>

    <div class="main">
      <header class="topbar">
        <div class="topbar-main">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item
              v-for="crumb in getBreadcrumbs()"
              :key="crumb.label"
              :to="crumb.path"
            >{{ crumb.label }}</el-breadcrumb-item>
          </el-breadcrumb>

          <div class="user-area">
            <el-dropdown trigger="click" @command="handleUserCommand">
              <div class="user-btn">
                <span class="avatar">{{ authStore.displayName.charAt(0) }}</span>
                <span class="username">{{ authStore.displayName }}</span>
                <svg viewBox="0 0 10 6" width="9" height="9" style="margin-left:2px;color:#94a3b8">
                  <path d="M0 0l5 6 5-6z" fill="currentColor" />
                </svg>
              </div>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item disabled style="font-size:12px;color:#94a3b8">
                    {{ authStore.displayName }}
                  </el-dropdown-item>
                  <el-dropdown-item divided command="logout" style="color:#ef4444">
                    閫€鍑虹櫥褰?
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>
      </header>

      <nav class="tab-bar">
        <div
          v-for="tab in tabsStore.tabs"
          :key="tab.path"
          class="tab-item"
          :class="{ 'tab-active': route.path === tab.path }"
          @click="router.push(tab.path)"
        >
          <span v-if="route.path === tab.path" class="tab-dot">•</span>
          <span class="tab-label">{{ tab.title }}</span>
          <button
            v-if="tab.closeable"
            class="tab-close"
            @click.stop="closeTab(tab.path)"
          >×</button>
        </div>
      </nav>
      <div v-if="clockItems.length > 0" class="world-clock-row">
          <div v-for="item in clockItems" :key="item.warehouseId" class="world-clock-item">
            <span class="clock-warehouse">{{ item.warehouseName }}</span>
            <span class="clock-time">{{ item.localDateTime }}</span>
            <span class="clock-zone">{{ item.timeZoneId }}</span>
            <span class="clock-season">{{ item.seasonLabel }}</span>
          </div>
        </div>
      <main class="page-body">
      <router-view />
      </main>
    </div>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 180px 1fr;
}

.sidebar {
  background: #2d3a4a;
  display: flex;
  flex-direction: column;
  position: sticky;
  top: 0;
  height: 100vh;
  overflow-y: auto;
}

.brand {
  display: flex;
  align-items: center;
  gap: 8px;
  height: 50px;
  padding: 0 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  flex-shrink: 0;
}

.brand-icon { font-size: 18px; }

.brand-name {
  font-size: 14px;
  font-weight: 700;
  color: #e2e8f0;
  letter-spacing: 0.3px;
}

.sidebar-menu {
  background: transparent !important;
  border-right: none !important;
  flex: 1;
  padding: 6px 8px;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  color: rgba(255, 255, 255, 0.55) !important;
  border-radius: 6px;
  margin-bottom: 1px;
  height: 40px;
  line-height: 40px;
  font-size: 13px;
}

:deep(.el-menu-item:hover),
:deep(.el-sub-menu__title:hover) {
  background: rgba(255, 255, 255, 0.07) !important;
  color: rgba(255, 255, 255, 0.9) !important;
}

:deep(.el-menu-item.is-active) {
  background: rgba(64, 158, 255, 0.18) !important;
  color: #7ec8f8 !important;
  font-weight: 600;
}

:deep(.el-sub-menu .el-menu) {
  background: transparent !important;
}

:deep(.el-sub-menu .el-menu .el-menu-item) {
  padding-left: 40px !important;
  font-size: 12.5px;
  height: 36px;
  line-height: 36px;
  color: rgba(255, 255, 255, 0.45) !important;
}

:deep(.el-sub-menu .el-menu .el-menu-item.is-active) {
  color: #7ec8f8 !important;
  background: rgba(64, 158, 255, 0.14) !important;
}

:deep(.el-sub-menu__icon-arrow) {
  color: rgba(255, 255, 255, 0.3) !important;
}

:deep(.el-sub-menu.is-opened > .el-sub-menu__title) {
  color: rgba(255, 255, 255, 0.85) !important;
}

.menu-icon {
  margin-right: 8px;
  font-style: normal;
  font-size: 13px;
  opacity: 0.7;
}

.main {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background: #f0f2f5;
}

.topbar {
  background: #ffffff;
  border-bottom: 1px solid #e8edf3;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 6px;
  padding: 8px 20px;
  flex-shrink: 0;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.topbar-main {
  min-height: 34px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.world-clock-row {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  background-color: white;
  padding:10px 20px;
  border-bottom: 1px solid #F0F2F5;
}

.world-clock-item {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 3px 10px;
  border: 1px solid #e2e8f0;
  border-radius: 999px;
  background: #f8fafc;
  color: #334155;
  font-size: 12px;
  line-height: 1.2;
}

.clock-warehouse { font-weight: 600; color: #0f172a; }
.clock-time { font-variant-numeric: tabular-nums; }
.clock-zone { color: #64748b; }
.clock-season {
  color: #0369a1;
  background: #e0f2fe;
  border-radius: 10px;
  padding: 1px 6px;
}

:deep(.el-breadcrumb__inner) {
  color: #64748b !important;
  font-size: 13px;
}

:deep(.el-breadcrumb__inner.is-link:hover) {
  color: #409eff !important;
}

:deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: #1e293b !important;
  font-weight: 500;
}

:deep(.el-breadcrumb__separator) {
  color: #cbd5e1 !important;
}

.user-area { display: flex; align-items: center; }

.user-btn {
  display: flex;
  align-items: center;
  gap: 7px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 20px;
  transition: background 0.15s;
}

.user-btn:hover { background: #f1f5f9; }

.avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, #36cfc9, #1890ff);
  color: #fff;
  font-size: 12px;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.username {
  font-size: 13px;
  color: #374151;
}

.tab-bar {
  height: 40px;
  background: #ffffff;
  border-bottom: 1px solid #e8edf3;
  display: flex;
  align-items: center;
  padding: 0 6px;
  gap: 2px;
  flex-shrink: 0;
  overflow-x: auto;
  scrollbar-width: none;
}

.tab-bar::-webkit-scrollbar { display: none; }

.tab-item {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 0 12px;
  height: 30px;
  border-radius: 4px;
  font-size: 13px;
  color: #64748b;
  cursor: pointer;
  border: 1px solid transparent;
  white-space: nowrap;
  transition: all 0.15s;
  flex-shrink: 0;
  user-select: none;
}

.tab-item:hover {
  background: #f8fafc;
  color: #374151;
  border-color: #e2e8f0;
}

.tab-active {
  background: #f0f9ff;
  color: #0284c7;
  border-color: #bae6fd;
  font-weight: 500;
}

.tab-dot {
  font-size: 8px;
  color: #10b981;
  line-height: 1;
}

.tab-label { line-height: 1; }

.tab-close {
  border: none;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
  font-size: 14px;
  line-height: 1;
  padding: 0;
  width: 16px;
  height: 16px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 3px;
  margin-left: 2px;
  transition: all 0.15s;
}

.tab-close:hover {
  background: #e2e8f0;
  color: #475569;
}

.page-body {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}
</style>


