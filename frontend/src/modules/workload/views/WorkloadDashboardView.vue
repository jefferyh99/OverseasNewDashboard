<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { workloadApi } from '@/services/api'
import type { WorkloadDashboardData } from '@/services/types'
import { getDatePresetRange } from '@/modules/workload/utils/datePresets'
import FutureInboundForecastChart from '@/modules/workload/components/FutureInboundForecastChart.vue'
import InboundMetricsMatrix from '@/modules/workload/components/InboundMetricsMatrix.vue'

const router = useRouter()
const loading = ref(false)
const warehouseCode = ref<'DE' | 'ON'>('DE')
const data = ref<WorkloadDashboardData | null>(null)

const tomorrowSummary = computed(() => data.value?.tomorrowInboundSummary)
const todaySummary = computed(() => data.value?.todayInboundSummary)

async function loadData() {
  loading.value = true
  try {
    const res = await workloadApi.dashboard(warehouseCode.value)
    data.value = res.data.data ?? null
  } finally {
    loading.value = false
  }
}

function goOverdue(type: 'anomaly-outbound' | 'anomaly-inbound' | 'anomaly-shelving') {
  router.push({
    name: type,
    query: {
      warehouseCode: warehouseCode.value,
      riskStatus: 'overdue',
    },
  })
}

function openOutboundDetail() {
  router.push({
    name: 'workload-outbound-detail',
    query: {
      warehouseCode: warehouseCode.value,
      processingStatus: 'all',
      ...getDatePresetRange('today'),
    },
  })
}

function openSkuDetail() {
  router.push({
    name: 'workload-sku-detail',
    query: {
      warehouseCode: warehouseCode.value,
      processingStatus: 'all',
      ...getDatePresetRange('today'),
    },
  })
}

function openCartonDetail(context: 'today' | 'tomorrow') {
  router.push({
    name: 'workload-carton-detail',
    query: {
      warehouseCode: warehouseCode.value,
      context,
      processingStatus: 'all',
      ...getDatePresetRange(context),
    },
  })
}

function openFutureInboundDetail() {
  router.push({
    name: 'workload-future-inbound-volume',
    query: {
      warehouseCode: warehouseCode.value,
      ...getDatePresetRange('future7days'),
    },
  })
}

onMounted(loadData)
</script>

<template>
  <div class="workload-page" v-loading="loading">
    <div class="toolbar">
      <h2>工作量看板</h2>
      <el-radio-group v-model="warehouseCode" @change="loadData">
        <el-radio-button label="DE">德国仓</el-radio-button>
        <el-radio-button label="ON">安大略仓</el-radio-button>
      </el-radio-group>
    </div>

    <section class="panel">
      <div class="panel-head">
        <h3>已超时待处理任务</h3>
      </div>
      <div class="cards cards-three">
        <div class="card" @click="goOverdue('anomaly-outbound')">
          <span class="label">箱子出库异常</span>
          <strong>{{ data?.overdueTasks?.[0]?.total ?? 0 }}</strong>
        </div>
        <div class="card" @click="goOverdue('anomaly-inbound')">
          <span class="label">入库单箱子到仓不齐</span>
          <strong>{{ data?.overdueTasks?.[1]?.total ?? 0 }}</strong>
        </div>
        <div class="card" @click="goOverdue('anomaly-shelving')">
          <span class="label">SKU上架异常</span>
          <strong>{{ data?.overdueTasks?.[2]?.total ?? 0 }}</strong>
        </div>
      </div>
    </section>

    <section class="panel">
      <div class="panel-head">
        <h3>今天待处理任务</h3>
      </div>
      <div class="cards">
        <div class="card">
          <div class="card-head">
            <span class="label">今天待出库包裹</span>
            <el-button link type="primary" @click="openOutboundDetail">查看全部</el-button>
          </div>
          <p>总量 {{ data?.todayOutboundPackages.total ?? 0 }}</p>
          <p>已处理 {{ data?.todayOutboundPackages.processed ?? 0 }} / 待处理 {{ data?.todayOutboundPackages.pending ?? 0 }}</p>
        </div>
        <div class="card">
          <div class="card-head">
            <span class="label">今天待上架SKU</span>
            <el-button link type="primary" @click="openSkuDetail">查看全部</el-button>
          </div>
          <p>总量 {{ data?.todayShelvingSkus.total ?? 0 }}</p>
          <p>已处理 {{ data?.todayShelvingSkus.processed ?? 0 }} / 待处理 {{ data?.todayShelvingSkus.pending ?? 0 }}</p>
        </div>
      </div>
      <div class="sub-panel">
        <div class="sub-head">
          <span>今天待到仓箱子</span>
          <el-button link type="primary" @click="openCartonDetail('today')">查看全部</el-button>
        </div>
        <p class="summary-line">
          总箱数 {{ todaySummary?.totalCartons.total ?? 0 }}，
          总重量 {{ todaySummary?.totalWeightKg.total ?? 0 }}，
          总体积 {{ todaySummary?.totalVolumeM3.total ?? 0 }}，
          总件数 {{ todaySummary?.totalUnits.total ?? 0 }}，
          总SKU数 {{ todaySummary?.totalSkuCount.total ?? 0 }}
        </p>
        <InboundMetricsMatrix :show-status-split="true" :rows="data?.todayInbound ?? []" />
      </div>
    </section>

    <section class="panel">
      <div class="panel-head">
        <h3>明天待处理任务</h3>
      </div>
      <div class="sub-panel">
        <div class="sub-head">
          <span>明天待到仓箱子</span>
          <el-button link type="primary" @click="openCartonDetail('tomorrow')">查看全部</el-button>
        </div>
        <p class="summary-line">
          总箱数 {{ tomorrowSummary?.totalCartons ?? 0 }}，
          总重量 {{ tomorrowSummary?.totalWeightKg ?? 0 }}，
          总体积 {{ tomorrowSummary?.totalVolumeM3 ?? 0 }}，
          总件数 {{ tomorrowSummary?.totalUnits ?? 0 }}，
          总SKU数 {{ tomorrowSummary?.totalSkuCount ?? 0 }}
        </p>
        <InboundMetricsMatrix :show-status-split="false" :rows="data?.tomorrowInbound ?? []" />
      </div>
    </section>

    <section class="panel">
      <div class="panel-head">
        <h3>未来7天待到仓货量预估</h3>
        <el-button link type="primary" @click="openFutureInboundDetail">查看全部</el-button>
      </div>
      <FutureInboundForecastChart :items="data?.futureInboundForecast ?? []" />
    </section>
  </div>
</template>

<style scoped>
.workload-page {
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

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.panel-head,
.sub-head,
.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}

.cards-three {
  grid-template-columns: repeat(3, 1fr);
}

.card {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 12px;
  cursor: pointer;
}

.card .label {
  color: #64748b;
}

.sub-panel {
  margin-top: 12px;
}

.summary-line {
  color: #64748b;
}
</style>
