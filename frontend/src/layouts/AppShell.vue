<script setup lang="ts">
import { watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useTabsStore } from '@/stores/tabs'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const tabsStore = useTabsStore()

interface LeafItem { label: string; path: string; icon: string }
interface GroupItem { label: string; key: string; icon: string; children: Omit<LeafItem, 'icon'>[] }
type MenuItem = LeafItem | GroupItem

const menuGroups: MenuItem[] = [
  { label: '工作量看板', path: '/dashboard', icon: 'menu-workload' },
  { label: '异常看板', path: '/anomaly-dashboard', icon: 'menu-overview' },
  {
    label: '异常管理',
    key: '/anomalies',
    icon: 'menu-anomaly',
    children: [
      { label: '箱子出库异常', path: '/anomalies/outbound' },
      { label: '入库单箱子到仓不齐', path: '/anomalies/inbound' },
      { label: 'SKU上架异常', path: '/anomalies/shelving' },
    ],
  },
  {
    label: '系统设置',
    key: '/settings',
    icon: 'menu-settings',
    children: [
      { label: '提醒配置', path: '/settings/alerts' },
    ],
  },
]

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
  const items: { label: string; path?: string }[] = [{ label: '工作量看板', path: '/dashboard' }]
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
</script>

<template>
  <div class="app-shell">
    <aside class="sidebar">
      <div class="brand">
        <span class="brand-icon">
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <use href="/dc-icons.svg#logo-grid" />
          </svg>
        </span>
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
              <span class="menu-icon">
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <use :href="`/dc-icons.svg#${(item as LeafItem).icon}`" />
                </svg>
              </span>
              {{ item.label }}
            </template>
          </el-menu-item>

          <el-sub-menu v-else :index="(item as GroupItem).key">
            <template #title>
              <span class="menu-icon">
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <use :href="`/dc-icons.svg#${(item as GroupItem).icon}`" />
                </svg>
              </span>
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
                  退出登录
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
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
  grid-template-columns: 236px 1fr;
  background:
    radial-gradient(circle at 0 0, rgba(58, 126, 197, 0.16), transparent 34%),
    radial-gradient(circle at 100% 0, rgba(49, 116, 185, 0.11), transparent 38%),
    linear-gradient(180deg, #071321 0%, #0a1828 100%);
}

.sidebar {
  background:
    linear-gradient(180deg, rgba(6, 24, 44, 0.96) 0%, rgba(8, 30, 54, 0.97) 72%),
    radial-gradient(circle at 20% 0%, rgba(76, 155, 233, 0.2), transparent 52%);
  display: flex;
  flex-direction: column;
  position: sticky;
  top: 0;
  height: 100vh;
  overflow-y: auto;
  border-right: 1px solid rgba(188, 216, 245, 0.18);
  box-shadow: inset -1px 0 0 rgba(255, 255, 255, 0.04);
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  height: 60px;
  padding: 0 18px;
  border-bottom: 1px solid rgba(111, 166, 219, 0.24);
  flex-shrink: 0;
}

.brand-icon {
  width: 30px;
  height: 30px;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(140deg, #2188de 0%, #0f4f8f 100%);
  color: #d7edff;
  box-shadow: 0 8px 22px rgba(14, 95, 167, 0.42);
}

.brand-icon svg {
  width: 16px;
  height: 16px;
}

.brand-name {
  font-size: 15px;
  font-weight: 700;
  color: #edf6ff;
  letter-spacing: 0.02em;
}

.sidebar-menu {
  background: transparent !important;
  border-right: none !important;
  flex: 1;
  padding: 12px 10px;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  color: rgba(216, 232, 248, 0.76) !important;
  border-radius: 10px;
  margin-bottom: 4px;
  height: 42px;
  line-height: 42px;
  font-size: 13px;
  font-weight: 500;
}

:deep(.el-menu-item:hover),
:deep(.el-sub-menu__title:hover) {
  background: rgba(88, 166, 239, 0.24) !important;
  color: #f7fbff !important;
}

:deep(.el-menu-item.is-active) {
  background: linear-gradient(90deg, rgba(37, 131, 216, 0.4) 0%, rgba(25, 122, 211, 0.18) 100%) !important;
  color: #f7fbff !important;
  font-weight: 600;
  box-shadow: inset 3px 0 0 #7ec2ff;
}

:deep(.el-sub-menu .el-menu) {
  background: transparent !important;
}

:deep(.el-sub-menu .el-menu .el-menu-item) {
  padding-left: 44px !important;
  font-size: 12.5px;
  height: 34px;
  line-height: 34px;
  color: rgba(214, 231, 247, 0.65) !important;
}

:deep(.el-sub-menu .el-menu .el-menu-item.is-active) {
  color: #f7fbff !important;
  background: rgba(25, 122, 211, 0.22) !important;
}

:deep(.el-sub-menu__icon-arrow) {
  color: rgba(201, 223, 246, 0.5) !important;
}

:deep(.el-sub-menu.is-opened > .el-sub-menu__title) {
  color: #f2f9ff !important;
}

.menu-icon {
  margin-right: 8px;
  width: 16px;
  height: 16px;
  text-align: center;
  opacity: 0.96;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.menu-icon svg {
  width: 14px;
  height: 14px;
}

.main {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background: transparent;
}

.topbar {
  height: 60px;
  background: rgba(9, 26, 45, 0.92);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid rgba(61, 99, 135, 0.42);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  flex-shrink: 0;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.24);
}

:deep(.el-breadcrumb__inner) {
  color: #89a7c7 !important;
  font-size: 12.5px;
  font-weight: 500;
}

:deep(.el-breadcrumb__inner.is-link:hover) {
  color: #6fc1ff !important;
}

:deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: #d7ebff !important;
  font-weight: 700;
}

:deep(.el-breadcrumb__separator) {
  color: #6287ad !important;
}

.user-area { display: flex; align-items: center; }

.user-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 5px 10px;
  border-radius: 999px;
  border: 1px solid transparent;
  transition: all 0.16s ease;
}

.user-btn:hover {
  background: rgba(82, 137, 191, 0.16);
  border-color: rgba(112, 169, 224, 0.3);
}

.avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: linear-gradient(135deg, #2388e1 0%, #10599f 100%);
  color: #fff;
  font-size: 12px;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.username {
  font-size: 13px;
  color: #cae4ff;
  font-weight: 600;
}

.tab-bar {
  height: 44px;
  background: rgba(8, 23, 40, 0.92);
  border-bottom: 1px solid rgba(61, 99, 135, 0.4);
  display: flex;
  align-items: center;
  padding: 0 10px;
  gap: 4px;
  flex-shrink: 0;
  overflow-x: auto;
  scrollbar-width: none;
}

.tab-bar::-webkit-scrollbar { display: none; }

.tab-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 0 13px;
  height: 31px;
  border-radius: 8px;
  font-size: 12.5px;
  color: #8ba8c6;
  cursor: pointer;
  border: 1px solid transparent;
  white-space: nowrap;
  transition: all 0.16s ease;
  flex-shrink: 0;
  user-select: none;
}

.tab-item:hover {
  background: rgba(83, 145, 206, 0.16);
  color: #d7ebff;
  border-color: rgba(117, 180, 236, 0.28);
}

.tab-active {
  background: linear-gradient(180deg, rgba(58, 145, 221, 0.3) 0%, rgba(33, 109, 179, 0.2) 100%);
  color: #def0ff;
  border-color: rgba(120, 186, 244, 0.42);
  font-weight: 700;
  box-shadow: 0 6px 14px rgba(9, 58, 104, 0.32);
}

.tab-dot {
  font-size: 8px;
  color: #1fba68;
  line-height: 1;
}

.tab-label { line-height: 1; }

.tab-close {
  border: none;
  background: transparent;
  color: #90acc9;
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
  background: rgba(98, 158, 218, 0.22);
  color: #d5ecff;
}

.page-body {
  flex: 1;
  padding: 18px;
  overflow-y: auto;
  background:
    radial-gradient(circle at 100% 0, rgba(45, 104, 165, 0.1), transparent 40%),
    radial-gradient(circle at 0 100%, rgba(52, 120, 190, 0.08), transparent 35%),
    linear-gradient(180deg, #f7fbff 0%, #eef4fb 100%);
}

@media (max-width: 1024px) {
  .app-shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    position: static;
    height: auto;
    max-height: 320px;
  }

  .brand {
    height: 56px;
  }

  .sidebar-menu {
    max-height: 260px;
  }

  .topbar {
    padding: 0 14px;
  }

  .username {
    display: none;
  }

  .page-body {
    padding: 12px;
  }
}
</style>
