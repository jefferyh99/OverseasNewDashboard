<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { anomaliesApi } from '@/services/api'
import type { OutboundItem, OutboundQuery } from '@/services/types'

const loading = ref(false)
const total = ref(0)
const imminentCount = ref(0)
const overdueCount = ref(0)
const channels = ref<string[]>([])
const customers = ref<string[]>([])
const tableData = ref<OutboundItem[]>([])

const query = reactive<OutboundQuery>({
  pageNo: 1,
  pageSize: 20,
  riskStatus: '',
  channel: '',
  customer: '',
  sortBy: 'deadlineAt',
  sortDirection: 'asc',
})

async function fetchData() {
  loading.value = true
  try {
    const res = await anomaliesApi.outbound({ ...query, riskStatus: query.riskStatus || undefined, channel: query.channel || undefined, customer: query.customer || undefined })
    const d = res.data.data!
    total.value = d.summary.total
    imminentCount.value = d.summary.imminentCount
    overdueCount.value = d.summary.overdueCount
    channels.value = d.filterOptions.channels
    customers.value = d.filterOptions.customers
    tableData.value = d.list.items
  } finally {
    loading.value = false
  }
}

function reset() {
  query.riskStatus = ''
  query.channel = ''
  query.customer = ''
  query.orderTimeStart = undefined
  query.orderTimeEnd = undefined
  query.pageNo = 1
  fetchData()
}

onMounted(fetchData)

function riskTagType(s: string) { return s === 'overdue' ? 'danger' : 'warning' }
function riskLabel(s: string) { return s === 'overdue' ? '已超时' : '即将超时' }
function timeTagType(s: string) { return s === 'overdue' ? 'danger' : 'success' }
</script>

<template>
  <div class="anomaly-page">
    <!-- 汇总 -->
    <div class="summary-bar">
      <span>共 <strong>{{ total }}</strong> 条风险</span>
      <el-tag type="warning" size="small">即将超时 {{ imminentCount }}</el-tag>
      <el-tag type="danger"  size="small">已超时 {{ overdueCount }}</el-tag>
    </div>

    <!-- 筛选 -->
    <div class="filter-bar">
      <el-select v-model="query.riskStatus" placeholder="风险状态" clearable size="small" style="width:120px">
        <el-option label="即将超时" value="imminent" />
        <el-option label="已超时"   value="overdue" />
      </el-select>
      <el-select v-model="query.channel" placeholder="渠道" clearable size="small" style="width:120px">
        <el-option v-for="c in channels" :key="c" :label="c" :value="c" />
      </el-select>
      <el-select v-model="query.customer" placeholder="客户" clearable size="small" style="width:120px">
        <el-option v-for="c in customers" :key="c" :label="c" :value="c" />
      </el-select>
      <el-button size="small" type="primary" @click="fetchData">查询</el-button>
      <el-button size="small" @click="reset">重置</el-button>
    </div>

    <!-- 表格 -->
    <el-table v-loading="loading" :data="tableData" size="small" stripe border>
      <el-table-column prop="orderId"           label="订单号"       width="150" />
      <el-table-column prop="customerOrChannel" label="客户/渠道"    width="160" />
      <el-table-column prop="currentStatus"     label="当前状态"     width="100" />
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
      <el-table-column label="截止时间" width="170">
        <template #default="{ row }">{{ new Date(row.deadlineAt).toLocaleString('zh-CN') }}</template>
      </el-table-column>
      <el-table-column label="下单时间" width="170">
        <template #default="{ row }">{{ new Date(row.orderTime).toLocaleString('zh-CN') }}</template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
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
.anomaly-page { display: flex; flex-direction: column; gap: 12px; }
.summary-bar {
  display: flex; align-items: center; gap: 10px;
  background: #fff; border-radius: 8px; padding: 10px 16px;
  font-size: 13px; box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.filter-bar {
  display: flex; flex-wrap: wrap; gap: 8px; align-items: center;
  background: #fff; border-radius: 8px; padding: 10px 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.pagination {
  display: flex; justify-content: flex-end;
  background: #fff; border-radius: 8px; padding: 10px 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
</style>
