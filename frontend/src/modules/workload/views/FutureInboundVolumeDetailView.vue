<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { workloadApi } from '@/services/api'
import type { FutureInboundVolumeData, FutureInboundVolumeItem, TransportMode, WorkloadBaseQuery } from '@/services/types'
import { downloadCsv } from '@/modules/workload/utils/exportCsv'
import { getDatePresetRange } from '@/modules/workload/utils/datePresets'
import { formatTransportMode } from '@/modules/workload/utils/formatters'

type FutureInboundQuery = WorkloadBaseQuery & { transportMode?: TransportMode }

const route = useRoute()
const loading = ref(false)
const rows = ref<FutureInboundVolumeItem[]>([])
const summary = ref<FutureInboundVolumeData['summary']>({
  totalCartons: 0,
  totalWeightKg: 0,
  totalVolumeM3: 0,
  seaContainerCount: 0,
  truckPalletCount: 0,
})

const _defaultRange = getDatePresetRange('last7days')
const query = reactive<FutureInboundQuery>({
  warehouseCode: String(route.query.warehouseCode ?? 'DE'),
  dateStart: String(route.query.dateStart || _defaultRange.dateStart),
  dateEnd: String(route.query.dateEnd || _defaultRange.dateEnd),
  transportMode: (route.query.transportMode as TransportMode | undefined) ?? undefined,
})

const dateRange = ref<[string, string]>([query.dateStart, query.dateEnd])

async function fetchData() {
  if (!dateRange.value?.[0] || !dateRange.value?.[1]) return
  query.dateStart = dateRange.value[0]
  query.dateEnd = dateRange.value[1]
  loading.value = true
  try {
    const res = await workloadApi.futureInboundVolume(query)
    const data = res.data.data
    rows.value = data?.items ?? []
    summary.value = data?.summary ?? summary.value
  } finally {
    loading.value = false
  }
}

function reset() {
  query.transportMode = undefined
  dateRange.value = [String(route.query.dateStart ?? ''), String(route.query.dateEnd ?? '')]
  fetchData()
}

function exportData() {
  downloadCsv({
    fileName: `future-inbound-volume-${query.warehouseCode}-${query.dateStart}-${query.dateEnd}.csv`,
    headers: ['到货日期', '货运方式', '总箱数', '总重量(kg)', '总体积(m3)', '总件数', '总SKU数', '卡派板数', '海运柜数'],
    rows: rows.value.map((item) => [
      item.arrivalDate,
      formatTransportMode(item.transportMode),
      item.totalCartons,
      item.totalWeightKg,
      item.totalVolumeM3,
      item.totalUnits,
      item.totalSkuCount,
      item.truckPalletCount ?? '-',
      item.seaContainerCount ?? '-',
    ]),
  })
}

onMounted(fetchData)
</script>

<template>
  <div class="workload-page" v-loading="loading">
    <h2>待到仓货量详情页</h2>
    <div class="filters">
      <el-select v-model="query.warehouseCode" style="width: 120px">
        <el-option label="德国仓" value="DE" />
        <el-option label="安大略仓" value="ON" />
      </el-select>
      <el-date-picker v-model="dateRange" type="daterange" value-format="YYYY-MM-DD" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" />
      <el-select v-model="query.transportMode" clearable style="width: 120px" placeholder="货运方式">
        <el-option label="海运" value="sea" />
        <el-option label="空运" value="air" />
        <el-option label="快递" value="express" />
        <el-option label="卡派" value="truck" />
      </el-select>
      <el-button type="primary" @click="fetchData">查询</el-button>
      <el-button @click="reset">重置</el-button>
      <el-button @click="exportData">导出</el-button>
    </div>
    <div class="stat-row">
      <div class="stat-box">
        <div class="stat-label">总箱数</div>
        <div class="stat-value">{{ summary.totalCartons }}</div>
      </div>
      <div class="stat-box">
        <div class="stat-label">总重量 (kg)</div>
        <div class="stat-value">{{ summary.totalWeightKg }}</div>
      </div>
      <div class="stat-box">
        <div class="stat-label">总体积 (m³)</div>
        <div class="stat-value">{{ summary.totalVolumeM3 }}</div>
      </div>
      <div class="stat-box">
        <div class="stat-label">海运柜数</div>
        <div class="stat-value">{{ summary.seaContainerCount }}</div>
      </div>
      <div class="stat-box">
        <div class="stat-label">卡派板数</div>
        <div class="stat-value">{{ summary.truckPalletCount }}</div>
      </div>
    </div>
    <el-table :data="rows" size="small" border>
      <el-table-column prop="arrivalDate" label="到货日期" width="120" />
      <el-table-column label="货运方式" width="100">
        <template #default="{ row }">{{ formatTransportMode(row.transportMode) }}</template>
      </el-table-column>
      <el-table-column prop="totalCartons" label="总箱数" width="90" />
      <el-table-column prop="totalWeightKg" label="总重量(kg)" width="110" />
      <el-table-column prop="totalVolumeM3" label="总体积(m3)" width="110" />
      <el-table-column prop="totalUnits" label="总件数" width="90" />
      <el-table-column prop="totalSkuCount" label="总SKU数" width="100" />
      <el-table-column label="卡派板数" width="100">
        <template #default="{ row }">{{ row.truckPalletCount ?? '-' }}</template>
      </el-table-column>
      <el-table-column label="海运柜数" width="100">
        <template #default="{ row }">{{ row.seaContainerCount ?? '-' }}</template>
      </el-table-column>
    </el-table>
  </div>
</template>

<style scoped>
.workload-page {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
}

.filters {
  display: flex;
  gap: 8px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.stat-row {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 16px;
}

.stat-box {
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #f8fafc;
  padding: 14px 16px;
}

.stat-label {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 8px;
}

.stat-value {
  font-size: 24px;
  font-weight: 800;
  color: #0f172a;
}
</style>
