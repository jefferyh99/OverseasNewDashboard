<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { workloadApi } from '@/services/api'
import type { SkuDetailItem, StatusSplit, TransportMode, WorkloadSkuQuery } from '@/services/types'
import { downloadCsv } from '@/modules/workload/utils/exportCsv'
import { getDatePresetRange } from '@/modules/workload/utils/datePresets'
import { formatTransportMode } from '@/modules/workload/utils/formatters'

const route = useRoute()
const loading = ref(false)
const rows = ref<SkuDetailItem[]>([])
const summary = ref<StatusSplit>({ total: 0, processed: 0, pending: 0 })

const _defaultRange = getDatePresetRange('last7days')
const query = reactive<WorkloadSkuQuery>({
  warehouseCode: String(route.query.warehouseCode ?? 'DE'),
  dateStart: String(route.query.dateStart || _defaultRange.dateStart),
  dateEnd: String(route.query.dateEnd || _defaultRange.dateEnd),
  processingStatus: (route.query.processingStatus as WorkloadSkuQuery['processingStatus']) ?? 'all',
  transportMode: (route.query.transportMode as TransportMode | undefined) ?? undefined,
  keyword: String(route.query.keyword ?? ''),
})

const dateRange = ref<[string, string]>([query.dateStart, query.dateEnd])

async function fetchData() {
  if (!dateRange.value?.[0] || !dateRange.value?.[1]) return
  query.dateStart = dateRange.value[0]
  query.dateEnd = dateRange.value[1]
  loading.value = true
  try {
    const res = await workloadApi.skus(query)
    const data = res.data.data
    rows.value = data?.items ?? []
    summary.value = data?.summary ?? { total: 0, processed: 0, pending: 0 }
  } finally {
    loading.value = false
  }
}

function reset() {
  query.processingStatus = 'all'
  query.transportMode = undefined
  query.keyword = ''
  dateRange.value = [String(route.query.dateStart ?? ''), String(route.query.dateEnd ?? '')]
  fetchData()
}

function exportData() {
  downloadCsv({
    fileName: `sku-detail-${query.warehouseCode}-${query.dateStart}-${query.dateEnd}.csv`,
    headers: ['库存编码', 'SKU', '客户', '所属箱号', '所属入库单号', '货运方式', '总件数', '已上架数量', '待上架数量', '到仓时间', '应完成上架时间', '实际上架时间', '处理状态'],
    rows: rows.value.map((item) => [
      item.inventoryCode,
      item.sku,
      item.customer,
      item.cartonId,
      item.asnId,
      formatTransportMode(item.transportMode),
      item.totalUnits,
      item.processedUnits,
      item.pendingUnits,
      item.arrivalTime,
      item.deadlineAt,
      item.actualShelvedAt ?? '',
      item.processingStatus,
    ]),
  })
}

onMounted(fetchData)
</script>

<template>
  <div class="workload-page" v-loading="loading">
    <h2>SKU详情页</h2>
    <div class="filters">
      <el-select v-model="query.warehouseCode" style="width: 120px">
        <el-option label="德国仓" value="DE" />
        <el-option label="安大略仓" value="ON" />
      </el-select>
      <el-date-picker v-model="dateRange" type="daterange" value-format="YYYY-MM-DD" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" />
      <el-select v-model="query.processingStatus" style="width: 120px">
        <el-option label="全部" value="all" />
        <el-option label="已处理" value="processed" />
        <el-option label="待处理" value="pending" />
      </el-select>
      <el-select v-model="query.transportMode" clearable style="width: 120px" placeholder="货运方式">
        <el-option label="海运" value="sea" />
        <el-option label="空运" value="air" />
        <el-option label="快递" value="express" />
        <el-option label="卡派" value="truck" />
      </el-select>
      <el-input v-model="query.keyword" placeholder="库存编码/SKU/箱号/入库单号" style="width: 240px" />
      <el-button type="primary" @click="fetchData">查询</el-button>
      <el-button @click="reset">重置</el-button>
      <el-button @click="exportData">导出</el-button>
    </div>
    <div class="stat-row">
      <div class="stat-box">
        <div class="stat-label">总 SKU 数</div>
        <div class="stat-value">{{ summary.total }}</div>
      </div>
      <div class="stat-box">
        <div class="stat-label">已处理</div>
        <div class="stat-value">{{ summary.processed }}</div>
      </div>
      <div class="stat-box">
        <div class="stat-label">待处理</div>
        <div class="stat-value stat-value--pending">{{ summary.pending }}</div>
      </div>
    </div>
    <el-table :data="rows" size="small" border>
      <el-table-column prop="inventoryCode" label="库存编码" width="140" />
      <el-table-column prop="sku" label="SKU" width="120" />
      <el-table-column prop="customer" label="客户" width="120" />
      <el-table-column prop="cartonId" label="所属箱号" width="140" />
      <el-table-column prop="asnId" label="所属入库单号" width="160" />
      <el-table-column label="货运方式" width="100">
        <template #default="{ row }">{{ formatTransportMode(row.transportMode) }}</template>
      </el-table-column>
      <el-table-column prop="totalUnits" label="总件数" width="90" />
      <el-table-column prop="processedUnits" label="已上架数量" width="100" />
      <el-table-column prop="pendingUnits" label="待上架数量" width="100" />
      <el-table-column prop="arrivalTime" label="到仓时间" width="170" />
      <el-table-column prop="deadlineAt" label="应完成上架时间" width="170" />
      <el-table-column prop="actualShelvedAt" label="实际上架时间" width="170" />
      <el-table-column prop="processingStatus" label="处理状态" width="100" />
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
  grid-template-columns: repeat(3, minmax(0, 1fr));
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

.stat-value--pending {
  color: #d97706;
}
</style>
