<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// 侧边栏菜单结构（支持二级）
interface LeafItem { label: string; path: string }
interface GroupItem { label: string; key: string; children: LeafItem[] }
type MenuItem = LeafItem | GroupItem

const menuGroups: MenuItem[] = [
  { label: '首页', path: '/dashboard' },
  {
    label: '异常管理',
    key: '/anomalies',
    children: [
      { label: '出库异常', path: '/anomalies/outbound' },
      { label: '到仓不齐', path: '/anomalies/inbound' },
      { label: '上架异常', path: '/anomalies/shelving' },
    ],
  },
  {
    label: '系统设置',
    key: '/settings',
    children: [
      { label: '提醒配置', path: '/settings/alerts' },
    ],
  },
]

function isGroup(item: MenuItem): item is GroupItem {
  return 'children' in item
}

// 当前所在分组自动展开
const openedKeys = computed(() => {
  const prefix = '/' + route.path.split('/')[1]
  return [prefix]
})

// 面包屑
interface BreadcrumbItem { label: string; path?: string }
const breadcrumbs = computed((): BreadcrumbItem[] => {
  const meta = route.meta as Record<string, string>
  const items: BreadcrumbItem[] = [{ label: '首页', path: '/dashboard' }]
  if (meta.groupTitle) items.push({ label: meta.groupTitle })
  if (meta.title && route.path !== '/dashboard') items.push({ label: meta.title })
  return items
})

// 用户下拉菜单
function handleUserCommand(cmd: string) {
  if (cmd === 'logout') {
    authStore.signOut()
    router.push('/login')
  }
}
</script>

<template>
  <div class="app-shell">

    <!-- 全宽顶部导航栏 -->
    <header class="navbar">
      <div class="navbar-brand">
        <span class="brand-icon">📊</span>
        <span class="brand-name">Ops Monitor</span>
      </div>

      <nav class="navbar-center">
        <span
          class="nav-tag"
          :class="{ active: route.path === '/dashboard' }"
          @click="router.push('/dashboard')"
        >控制台</span>
        <span
          class="nav-tag"
          :class="{ active: route.path.startsWith('/anomalies') }"
          @click="router.push('/anomalies/outbound')"
        >异常管理</span>
        <span
          class="nav-tag"
          :class="{ active: route.path.startsWith('/settings') }"
          @click="router.push('/settings/alerts')"
        >系统设置</span>
      </nav>

      <div class="navbar-right">
        <el-dropdown trigger="click" @command="handleUserCommand">
          <div class="user-trigger">
            <span class="avatar">{{ authStore.displayName.charAt(0) }}</span>
            <span class="display-name">{{ authStore.displayName }}</span>
            <svg class="caret" viewBox="0 0 10 6" width="10" height="6">
              <path d="M0 0l5 6 5-6z" fill="currentColor" />
            </svg>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item disabled>
                <span style="color:#64748b;font-size:12px">当前账号：{{ authStore.displayName }}</span>
              </el-dropdown-item>
              <el-dropdown-item divided command="logout" style="color:#ef4444">
                退出登录
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </header>

    <!-- 主体区（侧边栏 + 内容） -->
    <div class="body">

      <!-- 侧边栏 -->
      <aside class="sidebar">
        <el-menu
          :default-active="route.path"
          :default-openeds="openedKeys"
          router
          class="sidebar-menu"
        >
          <template v-for="item in menuGroups" :key="item.label">
            <el-menu-item v-if="!isGroup(item)" :index="(item as LeafItem).path">
              {{ item.label }}
            </el-menu-item>
            <el-sub-menu v-else :index="(item as GroupItem).key">
              <template #title>{{ item.label }}</template>
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

      <!-- 内容区 -->
      <section class="content">
        <!-- 面包屑栏 -->
        <div class="breadbar">
          <el-breadcrumb separator="›">
            <el-breadcrumb-item
              v-for="crumb in breadcrumbs"
              :key="crumb.label"
              :to="crumb.path"
            >{{ crumb.label }}</el-breadcrumb-item>
          </el-breadcrumb>
          <span class="page-title">{{ (route.meta as any).title ?? '' }}</span>
        </div>

        <!-- 页面内容 -->
        <main class="page-body">
          <router-view />
        </main>
      </section>

    </div>
  </div>
</template>

<style scoped>
/* 根容器 */
.app-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* 全宽顶部导航栏 */
.navbar {
  height: 56px;
  flex-shrink: 0;
  background: #16324f;
  display: flex;
  align-items: center;
  padding: 0 24px;
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.18);
}

.navbar-brand {
  display: flex;
  align-items: center;
  gap: 9px;
  min-width: 180px;
}

.brand-icon { font-size: 20px; }

.brand-name {
  font-size: 16px;
  font-weight: 700;
  color: #ffffff;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.navbar-center {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 0 16px;
}

.nav-tag {
  padding: 5px 14px;
  border-radius: 6px;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.65);
  cursor: pointer;
  transition: all 0.15s;
  user-select: none;
}

.nav-tag:hover {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.1);
}

.nav-tag.active {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.15);
  font-weight: 600;
}

.navbar-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 8px;
  transition: background 0.15s;
}

.user-trigger:hover { background: rgba(255, 255, 255, 0.1); }

.avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  color: #ffffff;
  font-size: 12px;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1.5px solid rgba(255, 255, 255, 0.35);
}

.display-name {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.85);
}

.caret {
  color: rgba(255, 255, 255, 0.45);
  margin-top: 1px;
}

/* 主体区 */
.body {
  flex: 1;
  display: grid;
  grid-template-columns: 200px 1fr;
  min-height: 0;
}

/* 侧边栏 */
.sidebar {
  background: #1e3a54;
  overflow-y: auto;
}

.sidebar-menu {
  background-color: transparent !important;
  border-right: none !important;
  padding: 10px 8px;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  color: rgba(255, 255, 255, 0.6) !important;
  border-radius: 8px;
  margin-bottom: 2px;
  height: 40px;
  line-height: 40px;
  font-size: 13.5px;
}

:deep(.el-menu-item:hover),
:deep(.el-sub-menu__title:hover) {
  background-color: rgba(255, 255, 255, 0.08) !important;
  color: #ffffff !important;
}

:deep(.el-menu-item.is-active) {
  background-color: rgba(255, 255, 255, 0.14) !important;
  color: #ffffff !important;
  font-weight: 600;
}

:deep(.el-sub-menu .el-menu) {
  background-color: transparent !important;
}

:deep(.el-sub-menu .el-menu .el-menu-item) {
  padding-left: 36px !important;
  font-size: 13px;
  height: 36px;
  line-height: 36px;
}

:deep(.el-sub-menu__icon-arrow) {
  color: rgba(255, 255, 255, 0.35) !important;
}

:deep(.el-sub-menu.is-opened > .el-sub-menu__title) {
  color: rgba(255, 255, 255, 0.9) !important;
}

/* 内容区 */
.content {
  background: #f4f6fb;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

/* 面包屑栏 */
.breadbar {
  height: 48px;
  padding: 0 24px;
  background: #ffffff;
  border-bottom: 1px solid #e5eaf3;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.03);
}

:deep(.el-breadcrumb__inner) {
  color: #64748b !important;
  font-weight: 400;
  font-size: 13px;
}

:deep(.el-breadcrumb__inner.is-link:hover) {
  color: #16324f !important;
}

:deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: #1e293b !important;
  font-weight: 500;
}

:deep(.el-breadcrumb__separator) {
  color: #cbd5e1 !important;
  margin: 0 6px;
}

.page-title {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
}

/* 页面内容 */
.page-body {
  flex: 1;
  padding: 20px 24px;
  overflow-y: auto;
}
</style>
