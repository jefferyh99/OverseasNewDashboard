<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '@/services/api'
import type { DashboardData } from '@/services/types'

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

function riskTagType(level: string) {
  if (level === 'overdue') return 'danger'
  if (level === 'imminent') return 'warning'
  return 'info'
}

function syncTagType(status: string) {
  if (status === 'success') return 'success'
  if (status === 'delayed') return 'warning'
  return 'danger'
}

function channelTagType(status: string) {
  if (status === 'healthy') return 'success'
  if (status === 'degraded') return 'warning'
  return 'danger'
}

function pressureLabel(level: string) {
  return { low: '低', medium: '中', high: '高' }[level] ?? level
}

function pressureType(level: string) {
  return { low: 'success', medium: 'warning', high: 'danger' }[level] ?? 'info'
}
</script>

<template>
  <div v-if="loading" class="page-loading">
    <el-skeleton :rows="6" animated />
  </div>

  <div v-else-if="data" class="dashboard">

    <!-- 顶部状态栏 -->
    <div class="status-bar">
      <div class="status-item">
        <span class="status-label">仓库</span>
        <strong>{{ data.warehouse.warehouseName }}</strong>
      </div>
      <div class="status-item">
        <span class="status-label">同步状态</span>
        <el-tag :type="syncTagType(data.baseStatus.syncStatus)" size="small">
          {{ data.baseStatus.syncStatus === 'success' ? '正常' : data.baseStatus.syncStatus }}
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

    <!-- 风险概览卡片 -->
    <div class="card-row">
      <div class="risk-card" @click="router.push('/anomalies/outbound')">
        <div class="risk-icon outbound">📦</div>
        <div class="risk-info">
          <div class="risk-title">出库异常</div>
          <div class="risk-total">{{ data.todayOverview.outboundRiskCount }}</div>
          <div class="risk-sub">
            <el-tag type="warning" size="small">即将 {{ data.anomalyPreview.outbound.imminentCount }}</el-tag>
            <el-tag type="danger" size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.outbound.overdueCount }}</el-tag>
          </div>
        </div>
      </div>

      <div class="risk-card" @click="router.push('/anomalies/inbound')">
        <div class="risk-icon inbound">🚚</div>
        <div class="risk-info">
          <div class="risk-title">到仓不齐</div>
          <div class="risk-total">{{ data.todayOverview.inboundRiskCount }}</div>
          <div class="risk-sub">
            <el-tag type="warning" size="small">即将 {{ data.anomalyPreview.inbound.imminentCount }}</el-tag>
            <el-tag type="danger" size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.inbound.overdueCount }}</el-tag>
          </div>
        </div>
      </div>

      <div class="risk-card" @click="router.push('/anomalies/shelving')">
        <div class="risk-icon shelving">🗄️</div>
        <div class="risk-info">
          <div class="risk-title">上架异常</div>
          <div class="risk-total">{{ data.todayOverview.shelvingRiskCount }}</div>
          <div class="risk-sub">
            <el-tag type="warning" size="small">即将 {{ data.anomalyPreview.shelving.imminentCount }}</el-tag>
            <el-tag type="danger" size="small" style="margin-left:4px">超时 {{ data.anomalyPreview.shelving.overdueCount }}</el-tag>
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

    <!-- 7 天预测 -->
    <div class="section-card">
      <div class="section-title">未来 7 天货量预测</div>
      <el-table :data="data.forecast7Days" size="small" stripe>
        <el-table-column prop="date" label="日期" width="110" />
        <el-table-column prop="cartonCount" label="箱数" width="90" align="right" />
        <el-table-column prop="weight" label="重量(kg)" width="100" align="right">
          <template #default="{ row }">{{ row.weight.toFixed(1) }}</template>
        </el-table-column>
        <el-table-column prop="volume" label="体积(m³)" width="100" align="right">
          <template #default="{ row }">{{ row.volume.toFixed(1) }}</template>
        </el-table-column>
        <el-table-column label="峰值" width="70" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isPeakDay" type="danger" size="small">峰值</el-tag>
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

/* 顶部状态栏 */
.status-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  background: #fff;
  border-radius: 8px;
  padding: 14px 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}

.status-item { display: flex; align-items: center; gap: 8px; font-size: 13px; }
.status-label { color: #94a3b8; }
.status-time { color: #94a3b8; font-size: 12px; }
.count-blue { font-size: 18px; color: #2563eb; }
.count-red  { font-size: 18px; color: #dc2626; }

/* 风险卡片行 */
.card-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr) 2fr;
  gap: 12px;
}

.risk-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 14px;
  cursor: pointer;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
  transition: box-shadow 0.2s;
}
.risk-card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.1); }

.risk-icon { font-size: 28px; }
.risk-title { font-size: 13px; color: #64748b; }
.risk-total { font-size: 32px; font-weight: 700; color: #1e293b; line-height: 1.1; }
.risk-sub { display: flex; flex-wrap: wrap; gap: 4px; margin-top: 4px; }

.tip-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.tip-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 13px;
  font-weight: 600;
  color: #374151;
  margin-bottom: 10px;
}
.tip-text { font-size: 13px; color: #4b5563; line-height: 1.6; margin: 0; }

/* 7日预测 */
.section-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.section-title {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 12px;
}
</style>
