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
const selectedWarehouseId = ref<'DE' | 'ON'>('DE')
const newHolidayDate = ref('')

const warehouseOptions = [
  { label: '德国仓（DE）', value: 'DE' },
  { label: '安大略仓（ON）', value: 'ON' },
] as const

const weekendOptions = [
  { label: 'Monday', value: 'Monday' },
  { label: 'Tuesday', value: 'Tuesday' },
  { label: 'Wednesday', value: 'Wednesday' },
  { label: 'Thursday', value: 'Thursday' },
  { label: 'Friday', value: 'Friday' },
  { label: 'Saturday', value: 'Saturday' },
  { label: 'Sunday', value: 'Sunday' },
]

const timezoneIds = (() => {
  const intlWithSupportedValues = Intl as typeof Intl & {
    supportedValuesOf?: (key: 'timeZone') => string[]
  }
  const all = intlWithSupportedValues.supportedValuesOf?.('timeZone') ?? []

  const ids = all
    .filter(timeZoneId => timeZoneId.startsWith('America/') || timeZoneId.startsWith('Europe/'))
    .sort((a, b) => a.localeCompare(b))

  // Fallback for very old runtimes that do not expose Intl.supportedValuesOf
  if (ids.length === 0) {
    return ['America/Toronto', 'Europe/Berlin']
  }

  return ids
})()

function getOffsetLabel(timeZoneId: string) {
  try {
    const parts = new Intl.DateTimeFormat('en-US', {
      timeZone: timeZoneId,
      timeZoneName: 'shortOffset',
    }).formatToParts(new Date())
    const raw = parts.find(part => part.type === 'timeZoneName')?.value ?? 'GMT+00:00'
    return raw.replace('GMT', 'UTC')
  } catch {
    return 'UTC+00:00'
  }
}

const timezoneOptions = timezoneIds.map(timeZoneId => ({
  value: timeZoneId,
  label: `(${getOffsetLabel(timeZoneId)}) ${timeZoneId}`,
}))

const outboundLeadTime = computed(
  () => config.value?.leadTimes.find(item => item.monitorType === 'outbound') ?? null,
)

const otherLeadTimes = computed(
  () => config.value?.leadTimes.filter(item => item.monitorType !== 'outbound') ?? [],
)

onMounted(async () => {
  await loadConfig(selectedWarehouseId.value)
})

async function loadConfig(warehouseId: string) {
  loading.value = true
  try {
    const res = await settingsApi.getAlerts(warehouseId)
    config.value = res.data.data ?? null
    if (config.value?.warehouseId) {
      selectedWarehouseId.value = (config.value.warehouseId as 'DE' | 'ON')
    }
  } finally {
    loading.value = false
  }
}

async function handleWarehouseChange(value: 'DE' | 'ON') {
  await loadConfig(value)
}

function addHoliday() {
  if (!config.value || !newHolidayDate.value) return
  if (!config.value.holidayDates.includes(newHolidayDate.value)) {
    config.value.holidayDates.push(newHolidayDate.value)
    config.value.holidayDates.sort()
  }
  newHolidayDate.value = ''
}

async function save() {
  if (!config.value) return
  saving.value = true
  try {
    const { updatedAt, updatedBy, ...payload } = config.value
    payload.warehouseId = selectedWarehouseId.value
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
    <div class="section-card">
      <div class="section-title">仓库基础信息</div>
      <div class="field-grid">
        <div class="field-row">
          <label>仓库</label>
          <el-select v-model="selectedWarehouseId" style="width: 260px" @change="handleWarehouseChange">
            <el-option v-for="item in warehouseOptions" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
        </div>
        <div class="field-row">
          <label>仓库时区</label>
          <el-select v-model="config.timeZoneId" filterable style="width: 320px">
            <el-option
              v-for="item in timezoneOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>
      </div>
    </div>
    <div class="section-card">
      <div class="section-title">出库时效规则配置（仓库当地时间）</div>
      <div class="field-grid">
        
        <div class="field-row">
          <label>冬令时截单时间</label>
          <el-time-picker
            v-model="config.outboundRule.cutoffTimeStandard"
            value-format="HH:mm"
            format="HH:mm"
            style="width: 160px"
          />
        </div>
        <div class="field-row">
          <label>夏令时截单时间</label>
          <el-time-picker
            v-model="config.outboundRule.cutoffTimeDaylight"
            value-format="HH:mm"
            format="HH:mm"
            style="width: 160px"
          />
        </div>
        <div class="field-row">
          <label>出库超时时间</label>
          <el-time-picker
            v-model="config.outboundRule.overdueTime"
            value-format="HH:mm"
            format="HH:mm"
            style="width: 160px"
          />
        </div>
        <div v-if="outboundLeadTime" class="field-row">
          <label>出库预警提前量（小时）</label>
          <el-input-number v-model="outboundLeadTime.leadTimeHours" :min="1" :max="72" size="small" />
        </div>
      </div>
    </div>

    <div class="section-card">
      <div class="section-title">工作日历配置</div>
      <div class="field-grid">
        <div class="field-row">
          <label>周末定义</label>
          <el-select v-model="config.weekendDays" multiple style="width: 320px">
            <el-option v-for="item in weekendOptions" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
        </div>
        <div class="field-row holiday-row">
          <label>法定节假日</label>
          <div class="holiday-editor">
            <div class="holiday-tools">
              <el-date-picker
                v-model="newHolidayDate"
                type="date"
                value-format="YYYY-MM-DD"
                format="YYYY-MM-DD"
                placeholder="选择日期"
                style="width: 180px"
              />
              <el-button type="primary" plain size="small" @click="addHoliday">添加</el-button>
            </div>
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
    </div>

    <div class="section-card">
      <div class="section-title">提前预警时长（小时）</div>
      <div class="field-grid">
        <div v-for="lt in otherLeadTimes" :key="lt.monitorType" class="field-row">
          <label>{{ monitorTypeLabel(lt.monitorType) }}</label>
          <el-input-number v-model="lt.leadTimeHours" :min="1" :max="72" size="small" />
        </div>
      </div>
    </div>

    <div class="section-card">
      <div class="section-title">严重告警阈值</div>
      <div class="field-grid">
        <div v-for="st in config.severityThresholds" :key="st.monitorType + st.metricCode" class="field-row">
          <label>{{ monitorTypeLabel(st.monitorType) }} — {{ st.metricCode }}</label>
          <el-input-number v-model="st.thresholdValue" :min="1" size="small" />
        </div>
      </div>
    </div>

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

    <div class="meta-row">
      最后保存：{{ new Date(config.updatedAt).toLocaleString('zh-CN') }}，操作人：{{ config.updatedBy }}
    </div>

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
.settings-page { display: flex; flex-direction: column; gap: 16px; max-width: 760px; }

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

.holiday-row { align-items: flex-start; }
.holiday-editor { display: flex; flex-direction: column; gap: 8px; }
.holiday-tools { display: flex; align-items: center; gap: 8px; }

.channel-list { display: flex; flex-direction: column; gap: 10px; }
.channel-row { display: flex; align-items: center; gap: 10px; }
.channel-name { font-size: 13px; color: #374151; width: 80px; }

.tag-edit { display: flex; flex-wrap: wrap; }

.meta-row { font-size: 12px; color: #94a3b8; }
</style>
