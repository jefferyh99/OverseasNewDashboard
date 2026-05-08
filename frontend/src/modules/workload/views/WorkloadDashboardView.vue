<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { workloadApi } from "@/services/api";
import type { WorkloadDashboardData } from "@/services/types";
import { getDatePresetRange } from "@/modules/workload/utils/datePresets";
import FutureInboundForecastChart from "@/modules/workload/components/FutureInboundForecastChart.vue";
import InboundMetricsMatrix from "@/modules/workload/components/InboundMetricsMatrix.vue";

const router = useRouter();
const loading = ref(false);
const warehouseCode = ref<"DE" | "ON">("DE");
const data = ref<WorkloadDashboardData | null>(null);

const tomorrowSummary = computed(() => data.value?.tomorrowInboundSummary);
const todaySummary = computed(() => data.value?.todayInboundSummary);

async function loadData() {
  loading.value = true;
  try {
    const res = await workloadApi.dashboard(warehouseCode.value);
    data.value = res.data.data ?? null;
  } finally {
    loading.value = false;
  }
}

function goOverdue(
  type: "anomaly-outbound" | "anomaly-inbound" | "anomaly-shelving",
) {
  router.push({
    name: type,
    query: {
      warehouseCode: warehouseCode.value,
      riskStatus: "overdue",
    },
  });
}

function openOutboundDetail() {
  router.push({
    name: "workload-outbound-detail",
    query: {
      warehouseCode: warehouseCode.value,
      processingStatus: "all",
      ...getDatePresetRange("today"),
    },
  });
}

function openSkuDetail() {
  router.push({
    name: "workload-sku-detail",
    query: {
      warehouseCode: warehouseCode.value,
      processingStatus: "all",
      ...getDatePresetRange("today"),
    },
  });
}

function openCartonDetail(context: "today" | "tomorrow") {
  router.push({
    name: "workload-carton-detail",
    query: {
      warehouseCode: warehouseCode.value,
      context,
      processingStatus: "all",
      ...getDatePresetRange(context),
    },
  });
}

function openFutureInboundDetail() {
  router.push({
    name: "workload-future-inbound-volume",
    query: {
      warehouseCode: warehouseCode.value,
      ...getDatePresetRange("future7days"),
    },
  });
}

onMounted(loadData);
</script>

<template>
  <div class="workload-page" v-loading="loading">
    <!-- 顶部栏：标题 + 仓库切换 -->
    <div class="topbar">
      <div class="topbar-left">
        <h1 class="page-title">工作看板</h1>
      </div>
      <div class="topbar-right">
        <el-radio-group
          v-model="warehouseCode"
          @change="loadData"
          size="default"
        >
          <el-radio-button value="DE">德国仓</el-radio-button>
          <el-radio-button value="ON">安大略仓</el-radio-button>
        </el-radio-group>
      </div>
    </div>

    <!-- 已超时待处理任务 -->
    <section class="panel">
      <div class="section-header">
        <div>
          <h2 class="section-title">已超时待处理任务</h2>
        </div>
      </div>
      <div class="overdue-grid">
        <div class="overdue-card" @click="goOverdue('anomaly-outbound')">
          <div class="overdue-title">箱子出库异常</div>
          <div class="overdue-count">
            {{ data?.overdueTasks?.[0]?.total ?? 0 }}
          </div>
        </div>
        <div class="overdue-card" @click="goOverdue('anomaly-inbound')">
          <div class="overdue-title">入库单箱子到仓不齐</div>
          <div class="overdue-count">
            {{ data?.overdueTasks?.[1]?.total ?? 0 }}
          </div>
        </div>
        <div class="overdue-card" @click="goOverdue('anomaly-shelving')">
          <div class="overdue-title">SKU 上架异常</div>
          <div class="overdue-count">
            {{ data?.overdueTasks?.[2]?.total ?? 0 }}
          </div>
        </div>
      </div>
    </section>

    <!-- 今天待处理任务 -->
    <section class="panel">
      <div class="section-header">
        <div>
          <h2 class="section-title">今天待处理任务</h2>
        </div>
      </div>

      <div class="today-top-grid">
        <!-- 今天待出库包裹 -->
        <div class="mini-card">
          <div class="mini-card-header">
            <span class="mini-card-title">今天待出库包裹</span>
            <el-button
              link
              type="primary"
              class="link-btn"
              @click="openOutboundDetail"
              >查看全部 →</el-button
            >
          </div>

          <div class="stat-row stat-row-3">
            <div class="stat-box">
              <div class="stat-label">总包裹量</div>
              <div class="stat-value">
                {{ data?.todayOutboundPackages.total ?? 0 }}
              </div>
            </div>
            <div class="stat-box">
              <div class="stat-label">已处理</div>
              <div class="stat-value">
                {{ data?.todayOutboundPackages.processed ?? 0 }}
              </div>
            </div>
            <div class="stat-box">
              <div class="stat-label">待处理</div>
              <div class="stat-value stat-value--pending">
                {{ data?.todayOutboundPackages.pending ?? 0 }}
              </div>
            </div>
          </div>
        </div>

        <!-- 今天待上架 SKU -->
        <div class="mini-card">
          <div class="mini-card-header">
            <span class="mini-card-title">今天待上架 SKU</span>
            <el-button
              link
              type="primary"
              class="link-btn"
              @click="openSkuDetail"
              >查看全部 →</el-button
            >
          </div>

          <div class="stat-row stat-row-3">
            <div class="stat-box">
              <div class="stat-label">总 SKU 数</div>
              <div class="stat-value">
                {{ data?.todayShelvingSkus.total ?? 0 }}
              </div>
            </div>
            <div class="stat-box">
              <div class="stat-label">已处理</div>
              <div class="stat-value">
                {{ data?.todayShelvingSkus.processed ?? 0 }}
              </div>
            </div>
            <div class="stat-box">
              <div class="stat-label">待处理</div>
              <div class="stat-value stat-value--pending">
                {{ data?.todayShelvingSkus.pending ?? 0 }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 今天待到仓箱子 -->
      <div class="matrix-block">
        <div class="mini-card-header">
          <span class="mini-card-title">今天待到仓箱子</span>
          <el-button
            link
            type="primary"
            class="link-btn"
            @click="openCartonDetail('today')"
            >查看全部 →</el-button
          >
        </div>

        <div class="stat-row stat-row-5">
          <div class="stat-box">
            <div class="stat-label">总箱数</div>
            <div class="stat-value">
              {{ todaySummary?.totalCartons.total ?? 0 }}
            </div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总重量 (kg)</div>
            <div class="stat-value">
              {{ todaySummary?.totalWeightKg.total ?? 0 }}
            </div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总体积 (m³)</div>
            <div class="stat-value">
              {{ todaySummary?.totalVolumeM3.total ?? 0 }}
            </div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总件数</div>
            <div class="stat-value">{{ todaySummary?.totalUnits.total ?? 0 }}</div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总 SKU 数</div>
            <div class="stat-value">{{ todaySummary?.totalSkuCount.total ?? 0 }}</div>
          </div>
          <div class="stat-box">
            <div class="stat-label">海运柜数</div>
            <div class="stat-value">{{ todaySummary?.seaContainerCount ?? 0 }}</div>
          </div>
          <div class="stat-box">
            <div class="stat-label">卡派板数</div>
            <div class="stat-value">{{ todaySummary?.truckPalletCount ?? 0 }}</div>
          </div>
        </div>
        <InboundMetricsMatrix
          :show-status-split="true"
          :rows="data?.todayInbound ?? []"
        />
      </div>
    </section>

    <!-- 明天待处理任务 -->
    <section class="panel">
      <div class="section-header">
        <div>
          <h2 class="section-title">明天待处理任务</h2>
        </div>
      </div>

      <div class="matrix-block" style="margin-top: 0">
        <div class="mini-card-header">
          <span class="mini-card-title">明天待到仓箱子</span>
          <el-button
            link
            type="primary"
            class="link-btn"
            @click="openCartonDetail('tomorrow')"
            >查看全部 →</el-button
          >
        </div>

        <div class="stat-row stat-row-5">
          <div class="stat-box">
            <div class="stat-label">总箱数</div>
            <div class="stat-value">
              {{ tomorrowSummary?.totalCartons ?? 0 }}
            </div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总重量 (kg)</div>
            <div class="stat-value">
              {{ tomorrowSummary?.totalWeightKg ?? 0 }}
            </div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总体积 (m³)</div>
            <div class="stat-value">
              {{ tomorrowSummary?.totalVolumeM3 ?? 0 }}
            </div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总件数</div>
            <div class="stat-value">{{ tomorrowSummary?.totalUnits ?? 0 }}</div>
          </div>
          <div class="stat-box">
            <div class="stat-label">总 SKU 数</div>
            <div class="stat-value">{{ tomorrowSummary?.totalSkuCount ?? 0 }}</div>
          </div>
          <div class="stat-box">
            <div class="stat-label">海运柜数</div>
            <div class="stat-value">{{ tomorrowSummary?.seaContainerCount ?? 0 }}</div>
          </div>
          <div class="stat-box">
            <div class="stat-label">卡派板数</div>
            <div class="stat-value">{{ tomorrowSummary?.truckPalletCount ?? 0 }}</div>
          </div>
        </div>
        <InboundMetricsMatrix
          :show-status-split="false"
          :rows="data?.tomorrowInbound ?? []"
        />
      </div>
    </section>

    <!-- 未来 7 天货量预估 -->
    <section class="panel">
      <div class="section-header">
        <div>
          <h2 class="section-title">未来待处理任务</h2>
        </div>
        <el-button
          link
          type="primary"
          class="link-btn"
          @click="openFutureInboundDetail"
          >查看全部 →</el-button
        >
      </div>
      <div class="matrix-block" style="margin-top: 0">
        <div class="mini-card-header">
          <span class="mini-card-title">未来 7 天待到仓货量预估</span>
        </div>
        <FutureInboundForecastChart
          :items="data?.futureInboundForecast ?? []"
        />
      </div>
    </section>
  </div>
</template>

<style scoped>
.workload-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* ── 顶部栏 ── */
.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #fff;
  border-radius: 12px;
  padding: 14px 20px;
  box-shadow: 0 1px 4px rgba(15, 23, 42, 0.06);
}

.page-title {
  font-size: 20px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

/* ── 通用 panel ── */
.panel {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(15, 23, 42, 0.06);
}

/* ── 模块标题区域 ── */
.section-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}

.section-title {
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px;
  line-height: 1.4;
}

/* ── 超时任务卡 ── */
.overdue-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 14px;
}

.overdue-card {
  border: 1px solid #fed7aa;
  border-radius: 14px;
  padding: 18px;
  background: #fff7ed;
  cursor: pointer;
  transition:
    box-shadow 0.18s ease,
    transform 0.18s ease;
}

.overdue-card:hover {
  box-shadow: 0 6px 18px rgba(194, 65, 12, 0.14);
  transform: translateY(-2px);
}

.overdue-title {
  font-size: 14px;
  font-weight: 600;
  color: #9a3412;
  margin-bottom: 10px;
}

.overdue-count {
  font-size: 36px;
  font-weight: 800;
  color: #c2410c;
  line-height: 1.1;
  margin-bottom: 10px;
}

.overdue-hint {
  font-size: 12px;
  color: #92400e;
  line-height: 1.6;
}

/* ── 今天两列卡 ── */
.today-top-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
  margin-bottom: 14px;
}

/* ── 子块（带背景的虚线卡） ── */
.mini-card {
  border: 1px dashed #94a3b8;
  border-radius: 14px;
  padding: 16px;
  background: #f8fafc;
}

.matrix-block {
  border: 1px dashed #94a3b8;
  border-radius: 14px;
  padding: 16px;
  background: #f8fafc;
  margin-top: 14px;
}

.mini-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 6px;
}

.mini-card-title {
  font-size: 14px;
  font-weight: 700;
  color: #1e293b;
}

.link-btn {
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}

/* ── 统计数字行 ── */
.stat-row {
  display: grid;
  gap: 10px;
  margin-bottom: 14px;
}

.stat-row-3 {
  grid-template-columns: repeat(3, 1fr);
}

.stat-row-5 {
  grid-template-columns: repeat(7, 1fr);
}

.stat-box {
  border-radius: 10px;
  background: #fff;
  border: 1px solid #e2e8f0;
  padding: 10px 12px;
}

.stat-label {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 6px;
  line-height: 1.4;
}

.stat-value {
  font-size: 22px;
  font-weight: 700;
  color: #0f172a;
  line-height: 1.2;
}

.stat-value--pending {
  color: #d97706;
}

/* ── 响应式 ── */
@media (max-width: 900px) {
  .overdue-grid,
  .today-top-grid {
    grid-template-columns: 1fr;
  }

  .stat-row-5 {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 560px) {
  .stat-row-3,
  .stat-row-5 {
    grid-template-columns: 1fr 1fr;
  }
}
</style>
