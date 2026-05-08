<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { settingsApi } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import type { AlertsConfigData } from '@/services/types'

const authStore = useAuthStore()
const loading = ref(false)
const saving = ref(false)
const config = ref<AlertsConfigData | null>(null)
const outboundLeadTime = computed(
  () => config.value?.leadTimes.find(item => item.monitorType === 'outbound') ?? null,
)
const otherLeadTimes = computed(
  () => config.value?.leadTimes.filter(item => item.monitorType !== 'outbound') ?? [],
)

onMounted(async () => {
  loading.value = true
  try {
    const res = await settingsApi.getAlerts()
    config.value = res.data.data ?? null
  } finally {
    loading.value = false
  }
})

async function save() {
  if (!config.value) return
  saving.value = true
  try {
    const { updatedAt, updatedBy, ...payload } = config.value
    const res = await settingsApi.saveAlerts(payload)
    config.value = res.data.data ?? config.value
    ElMessage.success('保存成功')
  } catch {
    ElMessage.error('保存失败，请重试')
  } finally {
    saving.value = false
  }
}

function monitorTypeLabel(t: string) {
  return { outbound: '出库', inbound: '到仓不齐', shelving: '上架' }[t] ?? t
}
</script>

<template>
  <div v-if="loading" class="page-loading">
    <el-skeleton :rows="8" animated />
  </div>

  <div v-else-if="config" class="settings-page">

    <!-- 出库规则配置 -->
    <div class="section-card">
      <div class="section-title">出库规则配置</div>
      <div class="field-grid">
        <div class="field-row">
          <label>仓库时区</label>
          <el-input v-model="config.timeZoneId" />
        </div>
        <div class="field-row">
          <label>冬令时截单时间</label>
          <el-input v-model="config.outboundRule.cutoffTimeStandard" />
        </div>
        <div class="field-row">
          <label>夏令时截单时间</label>
          <el-input v-model="config.outboundRule.cutoffTimeDaylight" />
        </div>
        <div class="field-row">
          <label>超时时点</label>
          <el-input v-model="config.outboundRule.overdueTime" />
        </div>
        <div v-if="outboundLeadTime" class="field-row">
          <label>出库预警提前量（小时）</label>
          <el-input-number v-model="outboundLeadTime.leadTimeHours" :min="1" :max="72" size="small" />
        </div>
      </div>
    </div>

    <!-- 工作日历配置 -->
    <div class="section-card">
      <div class="section-title">工作日历配置</div>
      <div class="field-grid">
        <div class="field-row">
          <label>周末定义</label>
          <el-select v-model="config.weekendDays" multiple>
            <el-option label="Saturday" value="Saturday" />
            <el-option label="Sunday" value="Sunday" />
          </el-select>
        </div>
        <div class="field-row">
          <label>法定节假日</label>
          <div class="tag-edit">
            <el-tag
              v-for="(holiday, i) in config.holidayDates"
              :key="holiday"
              closable
              @close="config!.holidayDates.splice(i, 1)"
              style="margin-right:6px;margin-bottom:6px"
            >{{ holiday }}</el-tag>
          </div>
        </div>
      </div>
    </div>

    <!-- 提前预警时长 -->
    <div class="section-card">
      <div class="section-title">提前预警时长（小时）</div>
      <div class="field-grid">
        <div v-for="lt in otherLeadTimes" :key="lt.monitorType" class="field-row">
          <label>{{ monitorTypeLabel(lt.monitorType) }}</label>
          <el-input-number v-model="lt.leadTimeHours" :min="1" :max="72" size="small" />
        </div>
      </div>
    </div>

    <!-- 严重阈值 -->
    <div class="section-card">
      <div class="section-title">严重告警阈值</div>
      <div class="field-grid">
        <div v-for="st in config.severityThresholds" :key="st.monitorType + st.metricCode" class="field-row">
          <label>{{ monitorTypeLabel(st.monitorType) }} — {{ st.metricCode }}</label>
          <el-input-number v-model="st.thresholdValue" :min="1" size="small" />
        </div>
      </div>
    </div>

    <!-- 推送渠道 -->
    <div class="section-card">
      <div class="section-title">推送渠道</div>
      <div class="channel-list">
        <div v-for="ch in config.channels" :key="ch.channelCode" class="channel-row">
          <el-switch v-model="ch.enabled" />
          <span class="channel-name">{{ ch.channelCode === 'wechat' ? '企业微信' : ch.channelCode === 'email' ? '邮件' : ch.channelCode }}</span>
          <el-tag :type="ch.enabled ? 'success' : 'info'" size="small">
            {{ ch.enabled ? '已启用' : '已禁用' }}
          </el-tag>
        </div>
      </div>
    </div>

    <!-- 接收人邮箱 -->
    <div class="section-card">
      <div class="section-title">接收邮箱</div>
      <div class="tag-edit">
        <el-tag
          v-for="(email, i) in config.receivers.emails"
          :key="email"
          closable
          @close="config!.receivers.emails.splice(i, 1)"
          style="margin-right:6px;margin-bottom:6px"
        >{{ email }}</el-tag>
      </div>
    </div>

    <!-- 更新信息 -->
    <div class="meta-row">
      最后保存：{{ new Date(config.updatedAt).toLocaleString('zh-CN') }}，操作人：{{ config.updatedBy }}
    </div>

    <!-- 保存按钮 -->
    <div>
      <el-button
        v-if="authStore.hasButton('settings-alerts-save')"
        type="primary"
        :loading="saving"
        @click="save"
      >保存配置</el-button>
    </div>

  </div>
</template>

<style scoped>
.page-loading { padding: 24px; }
.settings-page { display: flex; flex-direction: column; gap: 16px; max-width: 680px; }

.section-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.05);
}
.section-title {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 12px;
}

.field-grid { display: flex; flex-direction: column; gap: 10px; }
.field-row {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 13px;
  color: #374151;
}
.field-row label { width: 180px; flex-shrink: 0; }

.channel-list { display: flex; flex-direction: column; gap: 10px; }
.channel-row { display: flex; align-items: center; gap: 10px; }
.channel-name { font-size: 13px; color: #374151; width: 80px; }

.tag-edit { display: flex; flex-wrap: wrap; }

.meta-row { font-size: 12px; color: #94a3b8; }
</style>
