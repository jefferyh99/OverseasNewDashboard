<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '@/services/api'
import type { DashboardData, AnomalyPreviewItem } from '@/services/types'

const router = useRouter()
const loading = ref(true)
const data = ref<DashboardData | null>(null)

onMounted(async () => {
  try {
    const res = await dashboardApi.get()
    data.value = res.data.data ?? null
  } finally {
    loading.value = false
  }
})

function syncTagType(status: string) {
  return status === 'success' ? 'success' : status === 'delayed' ? 'warning' : 'danger'
}
function channelTagType(status: string) {
  return status === 'healthy' ? 'success' : status === 'degraded' ? 'warning' : 'danger'
}
function pressureLabel(level: string) {
  const m: Record<string, string> = { low: '低', medium: '中', high: '高' }
  return m[level] ?? level
}
function pressureType(level: string) {
  const m: Record<string, string> = { low: 'success', medium: 'warning', high: 'danger' }
  return m[level] ?? 'info'
}
function riskLabel(s: string) { return s === 'overdue' ? '已超时' : '即将超时' }
function riskTagType(s: string) { return s === 'overdue' ? 'danger' : 'warning' }
function timeTagType(s: string)  { return s === 'overdue' ? 'danger' : 'success' }
function fmt(d: string) { return new Date(d).toLocaleString('zh-CN') }
</script>

<template>
  <div v-if="loading" class="page-loading">
    <el-skeleton :rows="8" animated />
  </div>

  <div v-else-if="data" class="dashboard">

    <!-- 数据延迟告警 -->
    <el-alert
      v-if="data.baseStatus.delayedDataFlag"
      type="warning"
      :closable="false"
      show-icon
      style="border-radius:8px"
    >
      <template #title>
        数据可能延迟 — {{ data.baseStatus.delayedReason ?? '业务系统同步超时' }}，当前展示数据可能不是最新状态，请谨慎决策。
      </template>
    </el-alert>

    <!-- 顶部状态栏 -->
    <div class="status-bar">
      <div class="status-item">
        <span class="status-label">仓库</span>
        <strong>{{ data.warehouse.warehouseName }}</strong>
      </div>
      <div class="status-item">
        <span class="status-label">同步状态</span>
        <el-tag :type="syncTagType(data.baseStatus.syncStatus)" size="small">
          {{ data.baseStatus.syncStatus === 'success' ? '正常' : data.baseStatus.syncStatus === 'delayed' ? '延迟' : '异常' }}
        </el-tag>
        <span class="status-time">{{ new Date(data.baseStatus.lastSyncTime).toLocaleTimeString('zh-CN') }}</span>
      </div>
      <div class="status-item">
        <span class="status-label">今日提醒</span>
        <strong class="count-blue">{{ data.baseStatus.todayReminderCount }}</strong>
      </div>
      <div class="status-item">
        <span class="status-label">今日超时</span>
        <strong class="count-red">{{ data.baseStatus.todayOverdueCount }}</strong>
      </div>
      <div class="status-item">
        <span class="status-label">推送渠道</span>
        <el-tag
          v-for="ch in data.baseStatus.alertChannels"
          :key="ch.channelCode"
          :type="channelTagType(ch.status)"
          size="small"
          style="margin-left:4px"
        >{{ ch.channelName }}</el-tag>
      </div>
    </div>

    <!-- 风险概览卡片 + 运营建议 -->
    <div class="card-row">
      <div class="risk-card" @click="router.push('/anomalies/outbound')">
        <div class="risk-icon">📦</div>
        <div class="risk-info">
          <div class="risk-title">出库异常</div>
          <div class="risk-total">{{ data.todayOverview.outboundRiskCount }}</div>
          <div class="risk-sub">
            <el-tag type="warning" size="small">即将 {{ data.anomalyPreview.outbound.imminentCount }}</el-tag>
            <el-tag type="danger"  size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.outbound.overdueCount }}</el-tag>
          </div>
        </div>
      </div>
      <div class="risk-card" @click="router.push('/anomalies/inbound')">
        <div class="risk-icon">🚚</div>
        <div class="risk-info">
          <div class="risk-title">到仓不齐</div>
          <div class="risk-total">{{ data.todayOverview.inboundRiskCount }}</div>
          <div class="risk-sub">
            <el-tag type="warning" size="small">即将 {{ data.anomalyPreview.inbound.imminentCount }}</el-tag>
            <el-tag type="danger"  size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.inbound.overdueCount }}</el-tag>
          </div>
        </div>
      </div>
      <div class="risk-card" @click="router.push('/anomalies/shelving')">
        <div class="risk-icon">🗄️</div>
        <div class="risk-info">
          <div class="risk-title">上架异常</div>
          <div class="risk-total">{{ data.todayOverview.shelvingRiskCount }}</div>
          <div class="risk-sub">
            <el-tag type="warning" size="small">即将 {{ data.anomalyPreview.shelving.imminentCount }}</el-tag>
            <el-tag type="danger"  size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.shelving.overdueCount }}</el-tag>
          </div>
        </div>
      </div>
      <div class="tip-card">
        <div class="tip-header">
          <span>运营建议</span>
          <el-tag :type="pressureType(data.todayOverview.todayVolumePressureLevel)" size="small">
            货量压力：{{ pressureLabel(data.todayOverview.todayVolumePressureLevel) }}
          </el-tag>
        </div>
        <p class="tip-text">{{ data.todayOverview.operationTip }}</p>
      </div>
    </div>

    <!-- 出库异常清单 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-title">📦 出库异常清单</span>
        <span class="section-summary">
          共 {{ data.anomalyPreview.outbound.total }} 条
          <el-tag type="warning" size="small" style="margin-left:6px">即将 {{ data.anomalyPreview.outbound.imminentCount }}</el-tag>
          <el-tag type="danger"  size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.outbound.overdueCount }}</el-tag>
        </span>
        <el-button type="primary" link size="small" @click="router.push('/anomalies/outbound')">查看全部 →</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.outbound.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="订单号"    width="150" :formatter="(r: AnomalyPreviewItem) => r.orderId ?? ''" />
        <el-table-column label="客户/渠道" width="160" :formatter="(r: AnomalyPreviewItem) => r.customerOrChannel ?? ''" />
        <el-table-column label="当前状态"  width="90"  :formatter="(r: AnomalyPreviewItem) => r.currentStatus ?? ''" />
        <el-table-column label="风险"      width="90">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ riskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ row.timeValueLabel }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="165">
          <template #default="{ row }">{{ fmt(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 到仓不齐清单 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-title">🚚 到仓不齐清单</span>
        <span class="section-summary">
          共 {{ data.anomalyPreview.inbound.total }} 条
          <el-tag type="warning" size="small" style="margin-left:6px">即将 {{ data.anomalyPreview.inbound.imminentCount }}</el-tag>
          <el-tag type="danger"  size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.inbound.overdueCount }}</el-tag>
        </span>
        <el-button type="primary" link size="small" @click="router.push('/anomalies/inbound')">查看全部 →</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.inbound.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="ASN 号"    width="155" :formatter="(r: AnomalyPreviewItem) => r.asnId ?? ''" />
        <el-table-column label="计划/已到/缺" width="130">
          <template #default="{ row }">
            {{ row.plannedCartonCount }} / {{ row.arrivedCartonCount }} /
            <span style="color:#dc2626;font-weight:600">{{ row.missingCartonCount }}</span>
          </template>
        </el-table-column>
        <el-table-column label="风险" width="90">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ riskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ row.timeValueLabel }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="165">
          <template #default="{ row }">{{ fmt(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 上架异常清单 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-title">🗄️ 上架异常清单</span>
        <span class="section-summary">
          共 {{ data.anomalyPreview.shelving.total }} 条
          <el-tag type="warning" size="small" style="margin-left:6px">即将 {{ data.anomalyPreview.shelving.imminentCount }}</el-tag>
          <el-tag type="danger"  size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.shelving.overdueCount }}</el-tag>
        </span>
        <el-button type="primary" link size="small" @click="router.push('/anomalies/shelving')">查看全部 →</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.shelving.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="箱号"      width="155" :formatter="(r: AnomalyPreviewItem) => r.cartonId ?? ''" />
        <el-table-column label="所属ASN"   width="155" :formatter="(r: AnomalyPreviewItem) => r.asnId ?? ''" />
        <el-table-column label="SKU总/未上架" width="115">
          <template #default="{ row }">
            {{ row.skuCount }} / <span style="color:#dc2626;font-weight:600">{{ row.unshelvedSkuCount }}</span>
          </template>
        </el-table-column>
        <el-table-column label="风险" width="90">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ riskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ row.timeValueLabel }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="165">
          <template #default="{ row }">{{ fmt(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 7天预测 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-title">📅 未来 7 天货量预测</span>
      </div>
      <el-table :data="data.forecast7Days" size="small" stripe>
        <el-table-column prop="date"        label="日期"      width="110" />
        <el-table-column prop="cartonCount" label="箱数"      width="80"  align="right" />
        <el-table-column label="重量(kg)"   width="100" align="right">
          <template #default="{ row }">{{ row.weight.toFixed(1) }}</template>
        </el-table-column>
        <el-table-column label="体积(m³)"   width="100" align="right">
          <template #default="{ row }">{{ row.volume.toFixed(1) }}</template>
        </el-table-column>
        <el-table-column label="高峰" width="65" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isPeakDay" type="danger" size="small">高峰</el-tag>
            <span v-else style="color:#94a3b8">—</span>
          </template>
        </el-table-column>
      </el-table>
    </div>

  </div>
</template>

<style scoped>
.page-loading { padding: 24px; }
.dashboard { display: flex; flex-direction: column; gap: 16px; }

.status-bar {
  display: flex; flex-wrap: wrap; gap: 24px;
  background: #fff; border-radius: 8px; padding: 14px 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.status-item { display: flex; align-items: center; gap: 8px; font-size: 13px; }
.status-label { color: #94a3b8; }
.status-time  { color: #94a3b8; font-size: 12px; }
.count-blue { font-size: 18px; color: #2563eb; }
.count-red  { font-size: 18px; color: #dc2626; }

.card-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr) 2fr;
  gap: 12px;
}
.risk-card {
  background: #fff; border-radius: 8px; padding: 16px;
  display: flex; align-items: center; gap: 14px;
  cursor: pointer; box-shadow: 0 1px 4px rgba(0,0,0,0.05);
  transition: box-shadow 0.2s;
}
.risk-card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
.risk-icon  { font-size: 28px; }
.risk-title { font-size: 13px; color: #64748b; }
.risk-total { font-size: 32px; font-weight: 700; color: #1e293b; line-height: 1.1; }
.risk-sub   { display: flex; flex-wrap: wrap; gap: 4px; margin-top: 4px; }

.tip-card {
  background: #fff; border-radius: 8px; padding: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.tip-header {
  display: flex; align-items: center; justify-content: space-between;
  font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 10px;
}
.tip-text { font-size: 13px; color: #4b5563; line-height: 1.6; margin: 0; }

.section-card {
  background: #fff; border-radius: 8px; padding: 16px 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.section-header {
  display: flex; align-items: center; gap: 10px; margin-bottom: 12px;
}
.section-title {
  font-size: 14px; font-weight: 600; color: #1e293b; flex-shrink: 0;
}
.section-summary {
  display: flex; align-items: center; font-size: 13px; color: #64748b; flex: 1;
}
</style>
