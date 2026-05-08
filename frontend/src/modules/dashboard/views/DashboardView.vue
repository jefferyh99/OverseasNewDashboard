<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '@/services/api'
import type { AnomalyPreviewItem, DashboardData } from '@/services/types'

const router = useRouter()
const loading = ref(true)
const warehouseCode = ref<'DE' | 'ON'>('DE')
const data = ref<DashboardData | null>(null)

async function loadData() {
  loading.value = true
  try {
    const res = await dashboardApi.get(warehouseCode.value)
    data.value = res.data.data ?? null
  } finally {
    loading.value = false
  }
}

function goAnomalyPage(type: 'outbound' | 'inbound' | 'shelving') {
  router.push({
    path: `/anomalies/${type}`,
    query: { warehouseCode: warehouseCode.value },
  })
}

function riskLabel(status: string) {
  return status === 'overdue' ? '已超时' : '即将超时'
}

function riskTagType(status: string) {
  return status === 'overdue' ? 'danger' : 'warning'
}

function timeTagType(status: string) {
  return status === 'overdue' ? 'danger' : 'success'
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString('zh-CN')
}

onMounted(loadData)
</script>

<template>
  <div v-if="loading" class="page-loading">
    <el-skeleton :rows="8" animated />
  </div>

  <div v-else-if="data" class="dashboard">
    <div class="topbar">
      <h1 class="page-title">异常看板</h1>
      <el-radio-group v-model="warehouseCode" @change="loadData" size="default">
        <el-radio-button value="DE">德国仓</el-radio-button>
        <el-radio-button value="ON">安大略仓</el-radio-button>
      </el-radio-group>
    </div>

    <section class="panel">
      <div class="section-header">
        <h2 class="section-title">异常汇总</h2>
      </div>
      <div class="anomaly-grid">
        <div class="anomaly-card" @click="goAnomalyPage('outbound')">
          <div class="anomaly-card-title">箱子出库异常</div>
          <div class="anomaly-card-total">{{ data.todayOverview.outboundRiskCount }}</div>
          <div class="anomaly-card-sub">
            <span class="sub-imminent">即将 {{ data.anomalyPreview.outbound.imminentCount }}</span>
            <span class="sub-sep"> / </span>
            <span class="sub-overdue">超时 {{ data.anomalyPreview.outbound.overdueCount }}</span>
          </div>
        </div>
        <div class="anomaly-card" @click="goAnomalyPage('inbound')">
          <div class="anomaly-card-title">入库单箱子到仓不齐</div>
          <div class="anomaly-card-total">{{ data.todayOverview.inboundRiskCount }}</div>
          <div class="anomaly-card-sub">
            <span class="sub-imminent">即将 {{ data.anomalyPreview.inbound.imminentCount }}</span>
            <span class="sub-sep"> / </span>
            <span class="sub-overdue">超时 {{ data.anomalyPreview.inbound.overdueCount }}</span>
          </div>
        </div>
        <div class="anomaly-card" @click="goAnomalyPage('shelving')">
          <div class="anomaly-card-title">SKU 上架异常</div>
          <div class="anomaly-card-total">{{ data.todayOverview.shelvingRiskCount }}</div>
          <div class="anomaly-card-sub">
            <span class="sub-imminent">即将 {{ data.anomalyPreview.shelving.imminentCount }}</span>
            <span class="sub-sep"> / </span>
            <span class="sub-overdue">超时 {{ data.anomalyPreview.shelving.overdueCount }}</span>
          </div>
        </div>
      </div>
    </section>

    <section class="panel">
      <div class="section-header">
        <h2 class="section-title">箱子出库异常预览</h2>
        <el-button link type="primary" @click="goAnomalyPage('outbound')">查看全部 →</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.outbound.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="订单号" width="160" :formatter="(row: AnomalyPreviewItem) => row.orderId ?? ''" />
        <el-table-column label="客户/渠道" width="180" :formatter="(row: AnomalyPreviewItem) => row.customerOrChannel ?? ''" />
        <el-table-column label="风险状态" width="100">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ riskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效" width="130">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ row.timeValueLabel }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </section>

    <section class="panel">
      <div class="section-header">
        <h2 class="section-title">入库单箱子到仓不齐预览</h2>
        <el-button link type="primary" @click="goAnomalyPage('inbound')">查看全部 →</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.inbound.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="ASN 号" width="160" :formatter="(row: AnomalyPreviewItem) => row.asnId ?? ''" />
        <el-table-column label="计划/已到/缺少" width="160">
          <template #default="{ row }">
            {{ row.plannedCartonCount }} / {{ row.arrivedCartonCount }} / {{ row.missingCartonCount }}
          </template>
        </el-table-column>
        <el-table-column label="风险状态" width="100">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ riskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效" width="130">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ row.timeValueLabel }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </section>

    <section class="panel">
      <div class="section-header">
        <h2 class="section-title">SKU 上架异常预览</h2>
        <el-button link type="primary" @click="goAnomalyPage('shelving')">查看全部 →</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.shelving.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="箱号" width="160" :formatter="(row: AnomalyPreviewItem) => row.cartonId ?? ''" />
        <el-table-column label="ASN 号" width="160" :formatter="(row: AnomalyPreviewItem) => row.asnId ?? ''" />
        <el-table-column label="SKU总数/未上架" width="150">
          <template #default="{ row }">
            {{ row.skuCount }} / {{ row.unshelvedSkuCount }}
          </template>
        </el-table-column>
        <el-table-column label="风险状态" width="100">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ riskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效" width="130">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ row.timeValueLabel }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </section>
  </div>
</template>

<style scoped>
.page-loading {
  padding: 24px;
}

.dashboard {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* ── 顶部栏 ── */
.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #fff;
  border-radius: 12px;
  padding: 14px 20px;
  box-shadow: 0 1px 4px rgba(15, 23, 42, 0.06);
}

.page-title {
  font-size: 20px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

/* ── 通用 panel ── */
.panel {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(15, 23, 42, 0.06);
}

/* ── 模块标题区域 ── */
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}

.section-title {
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  line-height: 1.4;
}

/* ── 异常汇总卡 ── */
.anomaly-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 14px;
}

.anomaly-card {
  border: 1px solid #fed7aa;
  border-radius: 14px;
  padding: 18px;
  background: #fff7ed;
  cursor: pointer;
  transition: box-shadow 0.18s ease, transform 0.18s ease;
}

.anomaly-card:hover {
  box-shadow: 0 6px 18px rgba(194, 65, 12, 0.14);
  transform: translateY(-2px);
}

.anomaly-card-title {
  font-size: 14px;
  font-weight: 600;
  color: #9a3412;
  margin-bottom: 10px;
}

.anomaly-card-total {
  font-size: 36px;
  font-weight: 800;
  color: #c2410c;
  line-height: 1.1;
  margin-bottom: 10px;
}

.anomaly-card-sub {
  font-size: 13px;
  font-weight: 600;
}

.sub-imminent {
  color: #d97706;
}

.sub-sep {
  color: #9a3412;
}

.sub-overdue {
  color: #dc2626;
}

@media (max-width: 700px) {
  .anomaly-grid {
    grid-template-columns: 1fr;
  }
}
</style>
