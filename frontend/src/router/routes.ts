import type { RouteRecordRaw } from 'vue-router'
import AuthShell from '@/layouts/AuthShell.vue'
import AppShell from '@/layouts/AppShell.vue'
import LoginPageView from '@/modules/auth/views/LoginPageView.vue'
import DashboardView from '@/modules/dashboard/views/DashboardView.vue'
import OutboundView from '@/modules/anomalies/views/OutboundView.vue'
import InboundView from '@/modules/anomalies/views/InboundView.vue'
import ShelvingView from '@/modules/anomalies/views/ShelvingView.vue'
import AlertsConfigView from '@/modules/settings/views/AlertsConfigView.vue'
import WorkloadDashboardView from '@/modules/workload/views/WorkloadDashboardView.vue'
import OutboundPackagesDetailView from '@/modules/workload/views/OutboundPackagesDetailView.vue'
import SkuDetailView from '@/modules/workload/views/SkuDetailView.vue'
import CartonDetailView from '@/modules/workload/views/CartonDetailView.vue'
import FutureInboundVolumeDetailView from '@/modules/workload/views/FutureInboundVolumeDetailView.vue'

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
        component: WorkloadDashboardView,
        meta: { title: '工作量看板', permission: 'workload-dashboard', requiresAuth: true },
      },
      {
        path: 'anomaly-dashboard',
        name: 'anomaly-dashboard',
        component: DashboardView,
        meta: { title: '异常看板', permission: 'anomaly-dashboard', requiresAuth: true },
      },
      {
        path: 'workloads/outbound-packages',
        name: 'workload-outbound-detail',
        component: OutboundPackagesDetailView,
        meta: { title: '出库包裹详情页', permission: 'workload-outbound-detail', requiresAuth: true },
      },
      {
        path: 'workloads/skus',
        name: 'workload-sku-detail',
        component: SkuDetailView,
        meta: { title: 'SKU详情页', permission: 'workload-sku-detail', requiresAuth: true },
      },
      {
        path: 'workloads/cartons',
        name: 'workload-carton-detail',
        component: CartonDetailView,
        meta: { title: '箱子详情页', permission: 'workload-carton-detail', requiresAuth: true },
      },
      {
        path: 'workloads/future-inbound-volume',
        name: 'workload-future-inbound-volume',
        component: FutureInboundVolumeDetailView,
        meta: { title: '待到仓货量详情页', permission: 'workload-future-inbound-volume', requiresAuth: true },
      },
      {
        path: 'anomalies/outbound',
        name: 'anomaly-outbound',
        component: OutboundView,
        meta: { title: '箱子出库异常', permission: 'anomaly-outbound', groupTitle: '异常管理', requiresAuth: true },
      },
      {
        path: 'anomalies/inbound',
        name: 'anomaly-inbound',
        component: InboundView,
        meta: { title: '入库单箱子到仓不齐', permission: 'anomaly-inbound', groupTitle: '异常管理', requiresAuth: true },
      },
      {
        path: 'anomalies/shelving',
        name: 'anomaly-shelving',
        component: ShelvingView,
        meta: { title: 'SKU上架异常', permission: 'anomaly-shelving', groupTitle: '异常管理', requiresAuth: true },
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
