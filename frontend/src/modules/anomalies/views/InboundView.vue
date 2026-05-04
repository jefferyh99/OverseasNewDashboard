<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { anomaliesApi } from '@/services/api'
import type { InboundItem, InboundQuery } from '@/services/types'

const loading = ref(false)
const total = ref(0)
const imminentCount = ref(0)
const overdueCount = ref(0)
const tableData = ref<InboundItem[]>([])

const query = reactive<InboundQuery>({ pageNo: 1, pageSize: 20 })

async function fetchData() {
  loading.value = true
  try {
    const res = await anomaliesApi.inbound({ ...query, riskStatus: query.riskStatus || undefined })
    const d = res.data.data!
    total.value = d.summary.total
    imminentCount.value = d.summary.imminentCount
    overdueCount.value = d.summary.overdueCount
    tableData.value = d.list.items
  } finally {
    loading.value = false
  }
}

function reset() {
  query.riskStatus = undefined
  query.etaDateStart = undefined
  query.etaDateEnd = undefined
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
    <div class="summary-bar">
      <span>共 <strong>{{ total }}</strong> 条风险</span>
      <el-tag type="warning" size="small">即将超时 {{ imminentCount }}</el-tag>
      <el-tag type="danger"  size="small">已超时 {{ overdueCount }}</el-tag>
    </div>

    <div class="filter-bar">
      <el-select v-model="query.riskStatus" placeholder="风险状态" clearable size="small" style="width:120px">
        <el-option label="即将超时" value="imminent" />
        <el-option label="已超时"   value="overdue" />
      </el-select>
      <el-button size="small" type="primary" @click="fetchData">查询</el-button>
      <el-button size="small" @click="reset">重置</el-button>
    </div>

    <el-table v-loading="loading" :data="tableData" size="small" stripe border>
      <el-table-column prop="asnId"   label="ASN 号"     width="160" />
      <el-table-column prop="etaDate" label="ETA 日期"   width="110" />
      <el-table-column prop="plannedCartonCount"  label="计划箱数" width="90" align="right" />
      <el-table-column prop="arrivedCartonCount"  label="已到箱数" width="90" align="right" />
      <el-table-column prop="missingCartonCount"  label="缺少箱数" width="90" align="right">
        <template #default="{ row }">
          <span style="color:#dc2626;font-weight:600">{{ row.missingCartonCount }}</span>
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
      <el-table-column label="截止时间" width="170">
        <template #default="{ row }">{{ new Date(row.deadlineAt).toLocaleString('zh-CN') }}</template>
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
