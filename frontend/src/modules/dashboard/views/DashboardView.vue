<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppIcon from '@/shared/components/AppIcon.vue'
import { formatRiskLabel, formatTimeValueLabel } from '@/modules/anomalies/utils/labels'
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
    <div class="toolbar">
      <div class="toolbar-copy">
        <div class="title-row">
          <span class="title-icon">
            <AppIcon name="alert-board" :size="20" />
          </span>
          <div>
            <h2>异常看板</h2>
            <p>优先关注超时和即将超时任务</p>
          </div>
        </div>
        <span class="alert-chip">重点关注</span>
      </div>
      <el-radio-group v-model="warehouseCode" @change="loadData">
        <el-radio-button label="DE">德国仓</el-radio-button>
        <el-radio-button label="ON">安大略仓</el-radio-button>
      </el-radio-group>
    </div>

    <div class="cards">
      <div class="card card-outbound" @click="goAnomalyPage('outbound')">
        <div class="card-title">
          <span class="card-icon">
            <AppIcon name="outbound-box" />
          </span>
          <p>箱子出库异常</p>
        </div>
        <strong>{{ data.todayOverview.outboundRiskCount }}</strong>
        <small>即将超时 {{ data.anomalyPreview.outbound.imminentCount }} / 已超时 {{ data.anomalyPreview.outbound.overdueCount }}</small>
      </div>
      <div class="card card-inbound" @click="goAnomalyPage('inbound')">
        <div class="card-title">
          <span class="card-icon">
            <AppIcon name="inbound-warehouse" />
          </span>
          <p>入库单箱子到仓不齐</p>
        </div>
        <strong>{{ data.todayOverview.inboundRiskCount }}</strong>
        <small>即将超时 {{ data.anomalyPreview.inbound.imminentCount }} / 已超时 {{ data.anomalyPreview.inbound.overdueCount }}</small>
      </div>
      <div class="card card-shelving" @click="goAnomalyPage('shelving')">
        <div class="card-title">
          <span class="card-icon">
            <AppIcon name="shelving-rack" />
          </span>
          <p>SKU上架异常</p>
        </div>
        <strong>{{ data.todayOverview.shelvingRiskCount }}</strong>
        <small>即将超时 {{ data.anomalyPreview.shelving.imminentCount }} / 已超时 {{ data.anomalyPreview.shelving.overdueCount }}</small>
      </div>
    </div>

    <section class="panel">
      <div class="panel-head">
        <div class="panel-title">
          <span class="panel-icon">
            <AppIcon name="outbound-box" />
          </span>
          <h3>箱子出库异常预览</h3>
        </div>
        <el-button link type="primary" @click="goAnomalyPage('outbound')">查看全部</el-button>
      </div>
      <el-table :data="(data.anomalyPreview.outbound.items as AnomalyPreviewItem[])" size="small" stripe>
        <el-table-column label="订单号" width="160" :formatter="(row: AnomalyPreviewItem) => row.orderId ?? ''" />
        <el-table-column label="客户/渠道" width="180" :formatter="(row: AnomalyPreviewItem) => row.customerOrChannel ?? ''" />
        <el-table-column label="风险状态" width="100">
          <template #default="{ row }">
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ formatRiskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效" width="150">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ formatTimeValueLabel(row.timeValueLabel) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </section>

    <section class="panel">
      <div class="panel-head">
        <div class="panel-title">
          <span class="panel-icon">
            <AppIcon name="inbound-warehouse" />
          </span>
          <h3>入库单箱子到仓不齐预览</h3>
        </div>
        <el-button link type="primary" @click="goAnomalyPage('inbound')">查看全部</el-button>
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
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ formatRiskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效" width="150">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ formatTimeValueLabel(row.timeValueLabel) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.deadlineAt) }}</template>
        </el-table-column>
      </el-table>
    </section>

    <section class="panel">
      <div class="panel-head">
        <div class="panel-title">
          <span class="panel-icon">
            <AppIcon name="shelving-rack" />
          </span>
          <h3>SKU上架异常预览</h3>
        </div>
        <el-button link type="primary" @click="goAnomalyPage('shelving')">查看全部</el-button>
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
            <el-tag :type="riskTagType(row.riskStatus)" size="small">{{ formatRiskLabel(row.riskStatus) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="时效" width="150">
          <template #default="{ row }">
            <el-tag :type="timeTagType(row.timeStatus)" size="small">{{ formatTimeValueLabel(row.timeValueLabel) }}</el-tag>
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
  background: linear-gradient(180deg, #fffaf4 0%, #ffffff 100%);
  border: 1px solid #f2d7b5;
  border-radius: 16px;
}

.dashboard {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
  padding: 18px 20px;
  border-radius: 16px;
  background:
    radial-gradient(circle at 0 0, rgba(255, 214, 153, 0.28), transparent 35%),
    linear-gradient(180deg, #fff7ee 0%, #fffdf9 100%);
  border: 1px solid #f2d7b5;
  box-shadow: 0 12px 26px rgba(188, 122, 48, 0.08);
}

.toolbar-copy {
  display: flex;
  align-items: center;
  gap: 14px;
}

.title-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.title-icon,
.panel-icon,
.card-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #c56d1f;
}

.title-icon {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: linear-gradient(145deg, #fff1dd 0%, #ffe4be 100%);
  box-shadow: inset 0 0 0 1px rgba(219, 135, 41, 0.18);
}

.toolbar h2 {
  color: #8a4513;
}

.toolbar p {
  color: #b07a4f;
  font-size: 12px;
  margin-top: 4px;
}

.alert-chip {
  padding: 6px 12px;
  border-radius: 999px;
  background: #ffe8c7;
  color: #b5671d;
  font-size: 12px;
  font-weight: 700;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.card {
  padding: 16px;
  border-radius: 16px;
  border: 1px solid #f1dcc4;
  background: linear-gradient(180deg, #fffaf3 0%, #ffffff 100%);
  cursor: pointer;
  transition: transform 0.16s ease, box-shadow 0.16s ease, border-color 0.16s ease;
}

.card:hover {
  transform: translateY(-1px);
  border-color: #e3be95;
  box-shadow: 0 14px 26px rgba(188, 122, 48, 0.1);
}

.card-outbound {
  background: linear-gradient(180deg, #fff3e8 0%, #ffffff 100%);
}

.card-inbound {
  background: linear-gradient(180deg, #fff7ec 0%, #ffffff 100%);
}

.card-shelving {
  background: linear-gradient(180deg, #fff4ea 0%, #ffffff 100%);
}

.card-title {
  display: flex;
  align-items: center;
  gap: 8px;
}

.card-icon {
  width: 30px;
  height: 30px;
  border-radius: 10px;
  background: #ffe9cf;
  flex-shrink: 0;
}

.card p {
  margin: 0;
  color: #9b6c44;
  font-size: 13px;
  font-weight: 600;
}

.card strong {
  display: block;
  margin-top: 10px;
  font-size: 32px;
  line-height: 1;
  color: #8a4513;
}

.card small {
  display: block;
  margin-top: 10px;
  color: #b07a4f;
}

.panel {
  padding: 18px;
  border-radius: 16px;
  background: #ffffff;
  border: 1px solid #f0dcc6;
  box-shadow: 0 10px 24px rgba(188, 122, 48, 0.06);
}

.panel-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.panel-icon {
  width: 30px;
  height: 30px;
  border-radius: 10px;
  background: #fff1de;
}

.panel h3 {
  color: #8a4513;
}

@media (max-width: 1200px) {
  .cards {
    grid-template-columns: 1fr;
  }
}
</style>
