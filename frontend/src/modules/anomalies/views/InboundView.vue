<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppIcon from '@/shared/components/AppIcon.vue'
import { formatRiskLabel, formatTimeValueLabel } from '@/modules/anomalies/utils/labels'
import { anomaliesApi } from '@/services/api'
import type { InboundItem, InboundQuery } from '@/services/types'

const route = useRoute()
const loading = ref(false)
const total = ref(0)
const imminentCount = ref(0)
const overdueCount = ref(0)
const tableData = ref<InboundItem[]>([])

const query = reactive<InboundQuery>({
  warehouseCode: 'DE',
  pageNo: 1,
  pageSize: 20,
  riskStatus: undefined,
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
    const res = await anomaliesApi.inbound({
      ...query,
      warehouseCode: query.warehouseCode || undefined,
      riskStatus: query.riskStatus || undefined
    })
    const d = res.data.data
    total.value = d?.summary.total ?? 0
    imminentCount.value = d?.summary.imminentCount ?? 0
    overdueCount.value = d?.summary.overdueCount ?? 0
    tableData.value = d?.list.items ?? []
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
          <AppIcon name="inbound-warehouse" />
        </span>
        <span>入库单箱子到仓不齐</span>
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
      <el-button size="small" type="primary" @click="fetchData">查询</el-button>
      <el-button size="small" @click="reset">重置</el-button>
    </div>

    <el-table v-loading="loading" :data="tableData" size="small" stripe border>
      <el-table-column prop="asnId" label="ASN 号" width="160" />
      <el-table-column prop="etaDate" label="ETA 日期" width="110" />
      <el-table-column prop="plannedCartonCount" label="计划箱数" width="90" align="right" />
      <el-table-column prop="arrivedCartonCount" label="已到箱数" width="90" align="right" />
      <el-table-column prop="missingCartonCount" label="缺少箱数" width="90" align="right">
        <template #default="{ row }">
          <span class="emphasis">{{ row.missingCartonCount }}</span>
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

.emphasis {
  color: #dc2626;
  font-weight: 600;
}
</style>
