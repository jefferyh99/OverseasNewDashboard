import type { RouteRecordRaw } from 'vue-router'
import AuthShell from '@/layouts/AuthShell.vue'
import AppShell from '@/layouts/AppShell.vue'
import LoginPageView from '@/modules/auth/views/LoginPageView.vue'
import ScaffoldPageView from '@/shared/views/ScaffoldPageView.vue'

export const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    component: AuthShell,
    children: [
      {
        path: '',
        name: 'login',
        component: LoginPageView,
        meta: { title: '登录' },
      },
    ],
  },
  {
    path: '/',
    component: AppShell,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        redirect: '/dashboard',
      },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: ScaffoldPageView,
        meta: { title: '首页', permission: 'dashboard' },
      },
      {
        path: 'anomalies/outbound',
        name: 'anomaly-outbound',
        component: ScaffoldPageView,
        meta: { title: '出库异常', permission: 'anomaly-outbound' },
      },
      {
        path: 'anomalies/inbound',
        name: 'anomaly-inbound',
        component: ScaffoldPageView,
        meta: { title: '到仓不齐', permission: 'anomaly-inbound' },
      },
      {
        path: 'anomalies/shelving',
        name: 'anomaly-shelving',
        component: ScaffoldPageView,
        meta: { title: '上架异常', permission: 'anomaly-shelving' },
      },
      {
        path: 'settings/alerts',
        name: 'settings-alerts',
        component: ScaffoldPageView,
        meta: { title: '提醒配置', permission: 'settings-alerts' },
      },
    ],
  },
]
