<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppIcon from '@/shared/components/AppIcon.vue'
import { formatRiskLabel, formatTimeValueLabel } from '@/modules/anomalies/utils/labels'
import { anomaliesApi } from '@/services/api'
import type { OutboundItem, OutboundQuery } from '@/services/types'

const route = useRoute()
const loading = ref(false)
const total = ref(0)
const imminentCount = ref(0)
const overdueCount = ref(0)
const channels = ref<string[]>([])
const customers = ref<string[]>([])
const tableData = ref<OutboundItem[]>([])

const query = reactive<OutboundQuery>({
  warehouseCode: 'DE',
  pageNo: 1,
  pageSize: 20,
  riskStatus: undefined,
  channel: undefined,
  customer: undefined,
  sortBy: 'deadlineAt',
  sortDirection: 'asc'
})

function applyRouteFilters() {
  query.warehouseCode = String(route.query.warehouseCode ?? 'DE')
  query.riskStatus = route.query.riskStatus ? String(route.query.riskStatus) : undefined
}

async function fetchData() {
  loading.value = true
  try {
    const res = await anomaliesApi.outbound({
      ...query,
      riskStatus: query.riskStatus || undefined,
      channel: query.channel || undefined,
      customer: query.customer || undefined,
      warehouseCode: query.warehouseCode || undefined
    })
    const d = res.data.data
    total.value = d?.summary.total ?? 0
    imminentCount.value = d?.summary.imminentCount ?? 0
    overdueCount.value = d?.summary.overdueCount ?? 0
    channels.value = d?.filterOptions.channels ?? []
    customers.value = d?.filterOptions.customers ?? []
    tableData.value = d?.list.items ?? []
  } finally {
    loading.value = false
  }
}

function reset() {
  query.riskStatus = undefined
  query.channel = undefined
  query.customer = undefined
  query.orderTimeStart = undefined
  query.orderTimeEnd = undefined
  query.pageNo = 1
  fetchData()
}

onMounted(() => {
  applyRouteFilters()
  fetchData()
})

function riskTagType(status: string) {
  return status === 'overdue' ? 'danger' : 'warning'
}

function timeTagType(status: string) {
  return status === 'overdue' ? 'danger' : 'success'
}
</script>

<template>
  <div class="anomaly-page">
    <div class="summary-bar">
      <div class="summary-title">
        <span class="summary-icon">
          <AppIcon name="outbound-box" />
        </span>
        <span>箱子出库异常</span>
      </div>
      <span>共 <strong>{{ total }}</strong> 条风险</span>
      <el-tag type="warning" size="small">即将超时 {{ imminentCount }}</el-tag>
      <el-tag type="danger" size="small">已超时 {{ overdueCount }}</el-tag>
    </div>

    <div class="filter-bar">
      <el-select v-model="query.warehouseCode" size="small" style="width: 120px">
        <el-option label="德国仓" value="DE" />
        <el-option label="安大略仓" value="ON" />
      </el-select>
      <el-select v-model="query.riskStatus" placeholder="风险状态" clearable size="small" style="width: 120px">
        <el-option label="即将超时" value="imminent" />
        <el-option label="已超时" value="overdue" />
      </el-select>
      <el-select v-model="query.channel" placeholder="渠道" clearable size="small" style="width: 120px">
        <el-option v-for="channel in channels" :key="channel" :label="channel" :value="channel" />
      </el-select>
      <el-select v-model="query.customer" placeholder="客户" clearable size="small" style="width: 120px">
        <el-option v-for="customer in customers" :key="customer" :label="customer" :value="customer" />
      </el-select>
      <el-button size="small" type="primary" @click="fetchData">查询</el-button>
      <el-button size="small" @click="reset">重置</el-button>
    </div>

    <el-table v-loading="loading" :data="tableData" size="small" stripe border>
      <el-table-column prop="orderId" label="订单号" width="160" />
      <el-table-column prop="customerOrChannel" label="客户/渠道" width="180" />
      <el-table-column prop="currentStatus" label="当前状态" width="120" />
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
        <template #default="{ row }">{{ new Date(row.deadlineAt).toLocaleString('zh-CN') }}</template>
      </el-table-column>
      <el-table-column label="下单时间" width="180">
        <template #default="{ row }">{{ new Date(row.orderTime).toLocaleString('zh-CN') }}</template>
      </el-table-column>
    </el-table>

    <div class="pagination">
      <el-pagination
        v-model:current-page="query.pageNo"
        v-model:page-size="query.pageSize"
        :total="total"
        :page-sizes="[20, 50, 100]"
        layout="total, sizes, prev, pager, next"
        @change="fetchData"
      />
    </div>
  </div>
</template>

<style scoped>
.anomaly-page {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.summary-bar,
.filter-bar,
.pagination {
  background: #fff;
  border-radius: 12px;
  border: 1px solid #f0dcc6;
  box-shadow: 0 10px 24px rgba(188, 122, 48, 0.06);
}

.summary-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  padding: 12px 16px;
  background: linear-gradient(180deg, #fff7ee 0%, #ffffff 100%);
}

.summary-title {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  margin-right: 8px;
  color: #8a4513;
  font-weight: 700;
}

.summary-icon {
  width: 28px;
  height: 28px;
  border-radius: 10px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #ffe8cb;
  color: #c56d1f;
}

.filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
  padding: 12px 16px;
}

.pagination {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
}
</style>
