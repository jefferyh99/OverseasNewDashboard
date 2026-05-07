import { http } from './http'
import type {
  ApiResponse,
  LoginRequest,
  LoginData,
  ValidateData,
  PermissionsData,
  DashboardData,
  WorkloadDashboardData,
  WorkloadOutboundQuery,
  OutboundPackageDetailData,
  WorkloadSkuQuery,
  SkuDetailData,
  WorkloadCartonQuery,
  CartonDetailData,
  FutureInboundVolumeQuery,
  FutureInboundVolumeData,
  OutboundData,
  OutboundQuery,
  InboundData,
  InboundQuery,
  ShelvingData,
  ShelvingQuery,
  AlertsConfigData,
  DiagnosticsData,
} from './types'

export const authApi = {
  login: (data: LoginRequest) =>
    http.post<ApiResponse<LoginData>>('/auth/login', data),

  validate: () =>
    http.get<ApiResponse<ValidateData>>('/auth/validate'),

  permissions: () =>
    http.get<ApiResponse<PermissionsData>>('/auth/permissions'),
}

export const dashboardApi = {
  get: (warehouseCode?: string) =>
    http.get<ApiResponse<DashboardData>>('/dashboard', {
      params: warehouseCode ? { warehouseCode } : undefined,
    }),
}

export const workloadApi = {
  dashboard: (warehouseCode: string) =>
    http.get<ApiResponse<WorkloadDashboardData>>('/workloads/dashboard', { params: { warehouseCode } }),

  outboundPackages: (params: WorkloadOutboundQuery) =>
    http.get<ApiResponse<OutboundPackageDetailData>>('/workloads/outbound-packages', { params }),

  skus: (params: WorkloadSkuQuery) =>
    http.get<ApiResponse<SkuDetailData>>('/workloads/skus', { params }),

  cartons: (params: WorkloadCartonQuery) =>
    http.get<ApiResponse<CartonDetailData>>('/workloads/cartons', { params }),

  futureInboundVolume: (params: FutureInboundVolumeQuery) =>
    http.get<ApiResponse<FutureInboundVolumeData>>('/workloads/future-inbound-volume', { params }),
}

export const anomaliesApi = {
  outbound: (params: OutboundQuery) =>
    http.get<ApiResponse<OutboundData>>('/anomalies/outbound', { params }),

  inbound: (params: InboundQuery) =>
    http.get<ApiResponse<InboundData>>('/anomalies/inbound', { params }),

  shelving: (params: ShelvingQuery) =>
    http.get<ApiResponse<ShelvingData>>('/anomalies/shelving', { params }),
}

export const settingsApi = {
  getAlerts: () =>
    http.get<ApiResponse<AlertsConfigData>>('/settings/alerts'),

  saveAlerts: (data: Omit<AlertsConfigData, 'updatedAt' | 'updatedBy'>) =>
    http.put<ApiResponse<AlertsConfigData>>('/settings/alerts', data),
}

export const diagnosticsApi = {
  overview: () =>
    http.get<ApiResponse<DiagnosticsData>>('/diagnostics/overview'),
}
