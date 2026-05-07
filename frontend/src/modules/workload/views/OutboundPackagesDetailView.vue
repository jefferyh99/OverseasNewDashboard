<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { workloadApi } from '@/services/api'
import type { OutboundPackageDetailItem, StatusSplit, WorkloadOutboundQuery } from '@/services/types'
import { downloadCsv } from '@/modules/workload/utils/exportCsv'

const route = useRoute()
const loading = ref(false)
const rows = ref<OutboundPackageDetailItem[]>([])
const summary = ref<StatusSplit>({ total: 0, processed: 0, pending: 0 })

const query = reactive<WorkloadOutboundQuery>({
  warehouseCode: String(route.query.warehouseCode ?? 'DE'),
  dateStart: String(route.query.dateStart ?? ''),
  dateEnd: String(route.query.dateEnd ?? ''),
  processingStatus: (route.query.processingStatus as WorkloadOutboundQuery['processingStatus']) ?? 'all',
  keyword: String(route.query.keyword ?? ''),
})

const dateRange = ref<[string, string]>([query.dateStart, query.dateEnd])

async function fetchData() {
  if (!dateRange.value?.[0] || !dateRange.value?.[1]) return
  query.dateStart = dateRange.value[0]
  query.dateEnd = dateRange.value[1]
  loading.value = true
  try {
    const res = await workloadApi.outboundPackages(query)
    const data = res.data.data
    rows.value = data?.items ?? []
    summary.value = data?.summary ?? { total: 0, processed: 0, pending: 0 }
  } finally {
    loading.value = false
  }
}

function reset() {
  query.processingStatus = 'all'
  query.keyword = ''
  dateRange.value = [String(route.query.dateStart ?? ''), String(route.query.dateEnd ?? '')]
  fetchData()
}

function exportData() {
  downloadCsv({
    fileName: `outbound-packages-${query.warehouseCode}-${query.dateStart}-${query.dateEnd}.csv`,
    headers: ['订单号', '客户', '物流商', '挂号', '产品服务', '发货规则', '下单时间', '应完成出库时间', '实际出库时间', '处理状态'],
    rows: rows.value.map((item) => [
      item.orderId,
      item.customer,
      item.logisticsProvider,
      item.trackingNo,
      item.productService,
      item.shippingRule,
      item.orderTime,
      item.deadlineAt,
      item.actualOutboundTime ?? '',
      item.processingStatus,
    ]),
  })
}

onMounted(fetchData)
</script>

<template>
  <div class="workload-page" v-loading="loading">
    <h2>出库包裹详情页</h2>
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
      <el-input v-model="query.keyword" placeholder="订单号" style="width: 180px" />
      <el-button type="primary" @click="fetchData">查询</el-button>
      <el-button @click="reset">重置</el-button>
      <el-button @click="exportData">导出</el-button>
    </div>
    <p class="summary">总量 {{ summary.total }} / 已处理 {{ summary.processed }} / 待处理 {{ summary.pending }}</p>
    <el-table :data="rows" size="small" border>
      <el-table-column prop="orderId" label="订单号" width="160" />
      <el-table-column prop="customer" label="客户" width="120" />
      <el-table-column prop="logisticsProvider" label="物流商" width="120" />
      <el-table-column prop="trackingNo" label="挂号" width="160" />
      <el-table-column prop="productService" label="产品服务" width="110" />
      <el-table-column prop="shippingRule" label="发货规则" width="110" />
      <el-table-column prop="orderTime" label="下单时间" width="170" />
      <el-table-column prop="deadlineAt" label="应完成出库时间" width="170" />
      <el-table-column prop="actualOutboundTime" label="实际出库时间" width="170" />
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

.summary {
  margin: 8px 0 12px;
  color: #64748b;
}
</style>
