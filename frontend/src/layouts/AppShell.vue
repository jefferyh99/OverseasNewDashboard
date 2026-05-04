<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const menuItems = computed(() => [
  { label: '首页', path: '/dashboard' },
  { label: '出库异常', path: '/anomalies/outbound' },
  { label: '到仓不齐', path: '/anomalies/inbound' },
  { label: '上架异常', path: '/anomalies/shelving' },
  { label: '提醒配置', path: '/settings/alerts' },
])

function logout() {
  authStore.signOut()
  router.push('/login')
}
</script>

<template>
  <div class="app-shell">
    <aside class="sidebar">
      <div class="brand">Ops Monitor</div>
      <button
        v-for="item in menuItems"
        :key="item.path"
        class="menu-item"
        :class="{ active: route.path === item.path }"
        @click="router.push(item.path)"
      >
        {{ item.label }}
      </button>
    </aside>

    <section class="content">
      <header class="header">
        <div>
          <h1>{{ route.meta.title ?? 'Ops Monitor' }}</h1>
          <p>Reusable monitoring-system template shell</p>
        </div>
        <button class="logout" @click="logout">退出</button>
      </header>

      <main class="page-body">
        <router-view />
      </main>
    </section>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 240px 1fr;
}

.sidebar {
  padding: 24px;
  background: #16324f;
  color: #ffffff;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.brand {
  font-size: 20px;
  font-weight: 700;
  margin-bottom: 12px;
}

.menu-item {
  border: 0;
  padding: 12px 14px;
  border-radius: 10px;
  text-align: left;
  background: rgba(255, 255, 255, 0.12);
  color: inherit;
  cursor: pointer;
}

.menu-item.active {
  background: #ffffff;
  color: #16324f;
}

.content {
  background: #f3f6fa;
}

.header {
  padding: 24px 32px;
  background: #ffffff;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #d9e2ec;
}

.page-body {
  padding: 24px 32px;
}

.logout {
  border: 0;
  padding: 10px 14px;
  border-radius: 8px;
  background: #16324f;
  color: #ffffff;
  cursor: pointer;
}
</style>
