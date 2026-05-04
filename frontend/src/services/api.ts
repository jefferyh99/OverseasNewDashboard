import { http } from './http'
import type {
  ApiResponse,
  LoginRequest, LoginData, ValidateData, PermissionsData,
  DashboardData,
  OutboundData, OutboundQuery,
  InboundData, InboundQuery,
  ShelvingData, ShelvingQuery,
  AlertsConfigData,
  DiagnosticsData,
} from './types'

// ── Auth ─────────────────────────────────────────────────────────

export const authApi = {
  login: (data: LoginRequest) =>
    http.post<ApiResponse<LoginData>>('/auth/login', data),

  validate: () =>
    http.get<ApiResponse<ValidateData>>('/auth/validate'),

  permissions: () =>
    http.get<ApiResponse<PermissionsData>>('/auth/permissions'),
}

// ── Dashboard ────────────────────────────────────────────────────

export const dashboardApi = {
  get: () =>
    http.get<ApiResponse<DashboardData>>('/dashboard'),
}

// ── Anomalies ────────────────────────────────────────────────────

export const anomaliesApi = {
  outbound: (params: OutboundQuery) =>
    http.get<ApiResponse<OutboundData>>('/anomalies/outbound', { params }),

  inbound: (params: InboundQuery) =>
    http.get<ApiResponse<InboundData>>('/anomalies/inbound', { params }),

  shelving: (params: ShelvingQuery) =>
    http.get<ApiResponse<ShelvingData>>('/anomalies/shelving', { params }),
}

// ── Settings ─────────────────────────────────────────────────────

export const settingsApi = {
  getAlerts: () =>
    http.get<ApiResponse<AlertsConfigData>>('/settings/alerts'),

  saveAlerts: (data: Omit<AlertsConfigData, 'updatedAt' | 'updatedBy'>) =>
    http.put<ApiResponse<AlertsConfigData>>('/settings/alerts', data),
}

// ── Diagnostics ──────────────────────────────────────────────────

export const diagnosticsApi = {
  overview: () =>
    http.get<ApiResponse<DiagnosticsData>>('/diagnostics/overview'),
}
