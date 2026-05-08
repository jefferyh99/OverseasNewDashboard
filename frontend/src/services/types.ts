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

export interface StatusSplit {
  total: number
  processed: number
  pending: number
}

export interface MetricSplit {
  total: number
  processed: number
  pending: number
}

export type TransportMode = 'sea' | 'air' | 'express' | 'truck'

export interface InboundTransportRow {
  transportMode: TransportMode
  cartons: MetricSplit
  weightKg: MetricSplit
  volumeM3: MetricSplit
  units: MetricSplit
  skuCount: MetricSplit
  seaContainerCount: number | null
  truckPalletCount: number | null
}

export interface TomorrowInboundSummary {
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
  seaContainerCount: number
  truckPalletCount: number
}

export interface TomorrowInboundTransportRow {
  transportMode: TransportMode
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
  seaContainerCount: number | null
  truckPalletCount: number | null
}

export interface FutureInboundForecastBar {
  arrivalDate: string
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  seaContainerCount: number
  truckPalletCount: number
}

export interface WorkloadDashboardData {
  warehouse: { code: string; name: string }
  overdueTasks: Array<{ code: string; title: string; total: number }>
  todayOutboundPackages: StatusSplit
  todayShelvingSkus: StatusSplit
  todayInboundSummary: {
    totalCartons: MetricSplit
    totalWeightKg: MetricSplit
    totalVolumeM3: MetricSplit
    totalUnits: MetricSplit
    totalSkuCount: MetricSplit
    seaContainerCount: number
    truckPalletCount: number
  }
  todayInbound: InboundTransportRow[]
  tomorrowInboundSummary: TomorrowInboundSummary
  tomorrowInbound: TomorrowInboundTransportRow[]
  futureInboundForecast: FutureInboundForecastBar[]
}

export interface WorkloadBaseQuery {
  warehouseCode: string
  dateStart: string
  dateEnd: string
}

export interface OutboundPackageDetailItem {
  orderId: string
  customer: string
  logisticsProvider: string
  trackingNo: string
  productService: string
  shippingRule: string
  orderTime: string
  deadlineAt: string
  actualOutboundTime?: string
  processingStatus: 'processed' | 'pending'
}

export interface WorkloadOutboundQuery extends WorkloadBaseQuery {
  processingStatus: 'all' | 'processed' | 'pending'
  keyword?: string
}

export interface OutboundPackageDetailData {
  summary: StatusSplit
  items: OutboundPackageDetailItem[]
}

export interface SkuDetailItem {
  inventoryCode: string
  sku: string
  customer: string
  cartonId: string
  asnId: string
  transportMode: TransportMode
  totalUnits: number
  processedUnits: number
  pendingUnits: number
  arrivalTime: string
  deadlineAt: string
  actualShelvedAt?: string
  processingStatus: 'processed' | 'pending'
}

export interface WorkloadSkuQuery extends WorkloadBaseQuery {
  processingStatus: 'all' | 'processed' | 'pending'
  transportMode?: TransportMode
  keyword?: string
}

export interface SkuDetailData {
  summary: StatusSplit
  items: SkuDetailItem[]
}

export interface CartonDetailItem {
  cartonId: string
  asnId: string
  transportMode: TransportMode
  etaAt: string
  arrivedAt?: string
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
  processingStatus: 'processed' | 'pending'
}

export interface WorkloadCartonQuery extends WorkloadBaseQuery {
  processingStatus: 'all' | 'processed' | 'pending'
  transportMode?: TransportMode
  keyword?: string
  context: 'today' | 'tomorrow'
}

export interface CartonDetailData {
  summary: {
    total: number
    processed: number
    pending: number
    totalWeightKg: number
    totalVolumeM3: number
  }
  items: CartonDetailItem[]
}

export interface FutureInboundVolumeItem {
  arrivalDate: string
  transportMode: TransportMode
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
  truckPalletCount: number | null
  seaContainerCount: number | null
}

export interface FutureInboundVolumeQuery extends WorkloadBaseQuery {
  transportMode?: TransportMode
}

export interface FutureInboundVolumeData {
  summary: {
    totalCartons: number
    totalWeightKg: number
    totalVolumeM3: number
    seaContainerCount: number
    truckPalletCount: number
  }
  items: FutureInboundVolumeItem[]
}

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
  warehouseCode?: string
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
  warehouseCode?: string
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
  warehouseCode?: string
  riskStatus?: string
  arrivalDateStart?: string
  arrivalDateEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}

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
