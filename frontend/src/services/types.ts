// ── 通用包装 ──────────────────────────────────────────────────────

export interface ApiResponse<T> {
  success: boolean
  code: string
  message: string
  traceId?: string
  data?: T
  errors?: string[]
}

export interface PagedList<T> {
  pageNo: number
  pageSize: number
  total: number
  items: T[]
}

// ── Auth ─────────────────────────────────────────────────────────

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginData {
  token: string
  tokenType: string
  expiresIn: number
  userId: string
  userName: string
  displayName: string
}

export interface ValidateData {
  valid: boolean
  userId: string
  userName: string
  expiresAt: string
}

export interface PermissionsData {
  menuPermissions: string[]
  buttonPermissions: string[]
}

// ── Dashboard ────────────────────────────────────────────────────

export interface AlertChannelStatus {
  channelCode: string
  channelName: string
  status: string
}

export interface AnomalyPreviewItem {
  orderId?: string
  asnId?: string
  cartonId?: string
  customerOrChannel?: string
  orderTime?: string
  firstArrivalTime?: string
  arrivalTime?: string
  deadlineAt: string
  currentStatus?: string
  riskStatus: string
  timeStatus: string
  timeValueMinutes: number
  timeValueLabel: string
  plannedCartonCount?: number
  arrivedCartonCount?: number
  missingCartonCount?: number
  skuCount?: number
  unshelvedSkuCount?: number
}

export interface AnomalyPreviewGroup {
  total: number
  imminentCount: number
  overdueCount: number
  items: AnomalyPreviewItem[]
}

export interface DashboardData {
  warehouse: { warehouseId: string; warehouseName: string }
  baseStatus: {
    lastSyncTime: string
    syncStatus: string
    todayReminderCount: number
    todayOverdueCount: number
    alertChannels: AlertChannelStatus[]
    delayedDataFlag: boolean
    delayedReason: string | null
  }
  todayOverview: {
    outboundRiskCount: number
    inboundRiskCount: number
    shelvingRiskCount: number
    todayVolumePressureLevel: string
    operationTip: string
  }
  anomalyPreview: {
    outbound: AnomalyPreviewGroup
    inbound: AnomalyPreviewGroup
    shelving: AnomalyPreviewGroup
  }
  forecast7Days: Array<{
    date: string
    cartonCount: number
    weight: number
    volume: number
    isPeakDay: boolean
  }>
}

// ── Anomalies ────────────────────────────────────────────────────

export interface AnomalySummary {
  total: number
  imminentCount: number
  overdueCount: number
}

export interface OutboundItem {
  orderId: string
  customerOrChannel: string
  orderTime: string
  deadlineAt: string
  timeStatus: string
  timeValueMinutes: number
  timeValueLabel: string
  currentStatus: string
  shipped: boolean
  riskStatus: string
}

export interface OutboundData {
  summary: AnomalySummary
  filterOptions: { channels: string[]; customers: string[] }
  list: PagedList<OutboundItem>
}

export interface OutboundQuery {
  riskStatus?: string
  channel?: string
  customer?: string
  orderTimeStart?: string
  orderTimeEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}

export interface InboundItem {
  asnId: string
  etaDate: string
  plannedCartonCount: number
  arrivedCartonCount: number
  missingCartonCount: number
  firstArrivalTime: string
  deadlineAt: string
  timeStatus: string
  timeValueMinutes: number
  timeValueLabel: string
  riskStatus: string
}

export interface InboundData {
  summary: AnomalySummary
  list: PagedList<InboundItem>
}

export interface InboundQuery {
  riskStatus?: string
  etaDateStart?: string
  etaDateEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}

export interface ShelvingItem {
  cartonId: string
  asnId: string
  arrivalTime: string
  skuCount: number
  unshelvedSkuCount: number
  deadlineAt: string
  timeStatus: string
  timeValueMinutes: number
  timeValueLabel: string
  riskStatus: string
}

export interface ShelvingData {
  summary: AnomalySummary
  list: PagedList<ShelvingItem>
}

export interface ShelvingQuery {
  riskStatus?: string
  arrivalDateStart?: string
  arrivalDateEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}

// ── Settings ─────────────────────────────────────────────────────

export interface LeadTimeConfig {
  monitorType: string
  leadTimeHours: number
}

export interface SeverityThreshold {
  monitorType: string
  metricCode: string
  thresholdValue: number
}

export interface AlertReceivers {
  userIds: string[]
  groupIds: string[]
  emails: string[]
}

export interface AlertChannelConfig {
  channelCode: string
  enabled: boolean
}

export interface AlertsConfigData {
  warehouseId: string
  leadTimes: LeadTimeConfig[]
  severityThresholds: SeverityThreshold[]
  receivers: AlertReceivers
  channels: AlertChannelConfig[]
  updatedAt: string
  updatedBy: string
}

// ── Diagnostics ──────────────────────────────────────────────────

export interface DiagnosticsData {
  warehouse: { warehouseId: string; warehouseName: string }
  sync: {
    lastSyncTime: string
    syncStatus: string
    delayedDataFlag: boolean
    delayedReason: string | null
  }
  alerts: {
    channels: Array<{ channelCode: string; status: string }>
    todayReminderCount: number
  }
  dataAnomalies: {
    count: number
    topExamples: string[]
  }
}
