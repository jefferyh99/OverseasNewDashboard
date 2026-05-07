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
    <div class="toolbar">
      <h2>异常看板</h2>
      <el-radio-group v-model="warehouseCode" @change="loadData">
        <el-radio-button label="DE">德国仓</el-radio-button>
        <el-radio-button label="ON">安大略仓</el-radio-button>
      </el-radio-group>
    </div>

    <div class="cards">
      <div class="card" @click="goAnomalyPage('outbound')">
        <p>出库异常</p>
        <strong>{{ data.todayOverview.outboundRiskCount }}</strong>
        <small>即将 {{ data.anomalyPreview.outbound.imminentCount }} / 超时 {{ data.anomalyPreview.outbound.overdueCount }}</small>
      </div>
      <div class="card" @click="goAnomalyPage('inbound')">
        <p>到仓不齐</p>
        <strong>{{ data.todayOverview.inboundRiskCount }}</strong>
        <small>即将 {{ data.anomalyPreview.inbound.imminentCount }} / 超时 {{ data.anomalyPreview.inbound.overdueCount }}</small>
      </div>
      <div class="card" @click="goAnomalyPage('shelving')">
        <p>上架异常</p>
        <strong>{{ data.todayOverview.shelvingRiskCount }}</strong>
        <small>即将 {{ data.anomalyPreview.shelving.imminentCount }} / 超时 {{ data.anomalyPreview.shelving.overdueCount }}</small>
      </div>
    </div>

    <section class="panel">
      <div class="panel-head">
        <h3>出库异常预览</h3>
        <el-button link type="primary" @click="goAnomalyPage('outbound')">查看全部</el-button>
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
      <div class="panel-head">
        <h3>到仓不齐预览</h3>
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
      <div class="panel-head">
        <h3>上架异常预览</h3>
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
  gap: 12px;
}

.toolbar,
.panel {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
}

.toolbar,
.panel-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}

.card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 14px;
  cursor: pointer;
}

.card p {
  margin: 0;
  color: #64748b;
}

.card strong {
  display: block;
  margin-top: 4px;
  font-size: 28px;
  color: #1e293b;
}

.card small {
  color: #64748b;
}
</style>
