<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// ── 侧边栏菜单结构（支持二级） ──────────────────────────────────
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

// ── 面包屑 ──────────────────────────────────────────────────────
const breadcrumbs = computed(() => {
  const meta = route.meta as Record<string, string>
  const items: string[] = []
  if (meta.groupTitle) items.push(meta.groupTitle)
  if (meta.title) items.push(meta.title)
  return items
})

// ── 退出 ─────────────────────────────────────────────────────────
function logout() {
  authStore.signOut()
  router.push('/login')
}
</script>

<template>
  <div class="app-shell">
    <!-- ── 侧边栏 ── -->
    <aside class="sidebar">
      <div class="brand">
        <span class="brand-icon">📊</span>
        <span class="brand-name">Ops Monitor</span>
      </div>

      <el-menu
        :default-active="route.path"
        :default-openeds="openedKeys"
        router
        class="sidebar-menu"
      >
        <template v-for="item in menuGroups" :key="item.label">
          <!-- 一级菜单 -->
          <el-menu-item v-if="!isGroup(item)" :index="item.path">
            {{ item.label }}
          </el-menu-item>

          <!-- 带子菜单的分组 -->
          <el-sub-menu v-else :index="item.key">
            <template #title>{{ item.label }}</template>
            <el-menu-item
              v-for="child in item.children"
              :key="child.path"
              :index="child.path"
            >
              {{ child.label }}
            </el-menu-item>
          </el-sub-menu>
        </template>
      </el-menu>
    </aside>

    <!-- ── 主内容区 ── -->
    <section class="content">
      <!-- 顶部栏 -->
      <header class="topbar">
        <el-breadcrumb separator="›" class="breadcrumb">
          <el-breadcrumb-item
            v-for="bc in breadcrumbs"
            :key="bc"
          >{{ bc }}</el-breadcrumb-item>
        </el-breadcrumb>

        <div class="user-bar">
          <span class="avatar">{{ authStore.displayName.charAt(0) }}</span>
          <span class="display-name">{{ authStore.displayName }}</span>
          <el-divider direction="vertical" />
          <el-button link @click="logout" class="logout-btn">退出登录</el-button>
        </div>
      </header>

      <!-- 页面内容 -->
      <main class="page-body">
        <router-view />
      </main>
    </section>
  </div>
</template>

<style scoped>
/* ── 整体布局 ── */
.app-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 220px 1fr;
}

/* ── 侧边栏 ── */
.sidebar {
  background: #16324f;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 20px 20px 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.brand-icon { font-size: 22px; }

.brand-name {
  font-size: 16px;
  font-weight: 700;
  color: #ffffff;
  letter-spacing: 0.5px;
}

/* el-menu 深色主题覆盖 */
.sidebar-menu {
  background-color: transparent !important;
  border-right: none !important;
  flex: 1;
  padding: 8px 12px;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  color: rgba(255, 255, 255, 0.65) !important;
  border-radius: 8px;
  margin-bottom: 2px;
  height: 42px;
  line-height: 42px;
}

:deep(.el-menu-item:hover),
:deep(.el-sub-menu__title:hover) {
  background-color: rgba(255, 255, 255, 0.08) !important;
  color: #ffffff !important;
}

:deep(.el-menu-item.is-active) {
  background-color: rgba(255, 255, 255, 0.15) !important;
  color: #ffffff !important;
  font-weight: 600;
}

:deep(.el-sub-menu .el-menu) {
  background-color: transparent !important;
}

:deep(.el-sub-menu .el-menu .el-menu-item) {
  padding-left: 36px !important;
  font-size: 13px;
}

:deep(.el-sub-menu__icon-arrow) {
  color: rgba(255, 255, 255, 0.4) !important;
}

:deep(.el-sub-menu.is-opened > .el-sub-menu__title) {
  color: #ffffff !important;
}

/* ── 主内容 ── */
.content {
  background: #f4f6fb;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

/* ── 顶部栏 ── */
.topbar {
  height: 56px;
  padding: 0 24px;
  background: #ffffff;
  border-bottom: 1px solid #e5eaf3;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.breadcrumb {
  font-size: 14px;
}

:deep(.el-breadcrumb__inner) {
  color: #64748b;
  font-weight: 400;
}

:deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: #1e293b;
  font-weight: 600;
}

:deep(.el-breadcrumb__separator) {
  color: #94a3b8;
  margin: 0 8px;
}

.user-bar {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: #16324f;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.display-name {
  font-size: 14px;
  color: #374151;
}

.logout-btn {
  font-size: 13px;
  color: #64748b !important;
}

.logout-btn:hover {
  color: #ef4444 !important;
}

/* ── 页面内容 ── */
.page-body {
  flex: 1;
  padding: 24px;
}
</style>

