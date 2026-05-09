export const menuPermissions = {
  dashboard: 'dashboard',
  workloadDashboard: 'workload-dashboard',
  anomalyDashboard: 'anomaly-dashboard',
  workloadOutboundDetail: 'workload-outbound-detail',
  workloadSkuDetail: 'workload-sku-detail',
  workloadCartonDetail: 'workload-carton-detail',
  workloadFutureInboundVolume: 'workload-future-inbound-volume',
  anomalyOutbound: 'anomaly-outbound',
  anomalyInbound: 'anomaly-inbound',
  anomalyShelving: 'anomaly-shelving',
  settingsAlerts: 'settings-alerts',
  prototypeWecomNotification: 'prototype-wecom-notification',
} as const

export const buttonPermissions = {
  anomaliesExport: 'anomalies-export',
  settingsAlertsSave: 'settings-alerts-save',
} as const
