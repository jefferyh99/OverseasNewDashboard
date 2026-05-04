import type { RouteRecordRaw } from 'vue-router'
import AuthShell from '@/layouts/AuthShell.vue'
import AppShell from '@/layouts/AppShell.vue'
import LoginPageView from '@/modules/auth/views/LoginPageView.vue'
import DashboardView from '@/modules/dashboard/views/DashboardView.vue'
import OutboundView from '@/modules/anomalies/views/OutboundView.vue'
import InboundView from '@/modules/anomalies/views/InboundView.vue'
import ShelvingView from '@/modules/anomalies/views/ShelvingView.vue'
import AlertsConfigView from '@/modules/settings/views/AlertsConfigView.vue'

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
        component: DashboardView,
        meta: { title: '首页', permission: 'dashboard', requiresAuth: true },
      },
      {
        path: 'anomalies/outbound',
        name: 'anomaly-outbound',
        component: OutboundView,
        meta: { title: '出库异常', permission: 'anomaly-outbound', groupTitle: '异常管理', requiresAuth: true },
      },
      {
        path: 'anomalies/inbound',
        name: 'anomaly-inbound',
        component: InboundView,
        meta: { title: '到仓不齐', permission: 'anomaly-inbound', groupTitle: '异常管理', requiresAuth: true },
      },
      {
        path: 'anomalies/shelving',
        name: 'anomaly-shelving',
        component: ShelvingView,
        meta: { title: '上架异常', permission: 'anomaly-shelving', groupTitle: '异常管理', requiresAuth: true },
      },
      {
        path: 'settings/alerts',
        name: 'settings-alerts',
        component: AlertsConfigView,
        meta: { title: '提醒配置', permission: 'settings-alerts', groupTitle: '系统设置', requiresAuth: true },
      },
    ],
  },
]
