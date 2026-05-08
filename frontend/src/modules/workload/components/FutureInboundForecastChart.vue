<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import * as echarts from 'echarts'
import type { FutureInboundForecastBar } from '@/services/types'

const props = defineProps<{
  items: FutureInboundForecastBar[]
}>()

const chartRef = ref<HTMLDivElement | null>(null)
let chart: echarts.ECharts | null = null

const CN_WEEK = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']

// 将 "yyyy-MM-dd" 格式化为 "MM/dd 周X" 用于 X 轴显示
// 后端已保证返回恰好 7 个工作日（跨周时自动延至下周），前端无需再过滤
function formatLabel(dateStr: string) {
  const [y, m, d] = dateStr.split('-').map(Number)
  const date = new Date(y, m - 1, d) // 本地时间构造，避免 UTC 偏移导致星期错误
  return `${String(m).padStart(2, '0')}/${String(d).padStart(2, '0')} ${CN_WEEK[date.getDay()]}`
}

function renderChart() {
  if (!chartRef.value) return
  if (!chart) chart = echarts.init(chartRef.value)

  // 直接使用后端 7 条工作日数据，仅追加格式化标签
  const source = props.items.map((item) => ({ ...item, label: formatLabel(item.arrivalDate) }))

  chart.setOption({
    dataset: { source },
    grid: { left: 48, right: 16, top: 24, bottom: 36 },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const row = params?.[0]?.data as (FutureInboundForecastBar & { label: string }) | undefined
        if (!row) return ''
        return [
          row.label,
          `总箱数：${row.totalCartons}`,
          `总重量(kg)：${row.totalWeightKg}`,
          `总体积(m3)：${row.totalVolumeM3}`,
          `海运柜数：${row.seaContainerCount}`,
          `卡派板数：${row.truckPalletCount}`,
        ].join('<br/>')
      },
    },
    xAxis: {
      type: 'category',
      axisTick: { alignWithLabel: true },
    },
    yAxis: {
      type: 'value',
      name: '总箱数',
    },
    series: [
      {
        type: 'bar',
        barMaxWidth: 40,
        itemStyle: { color: '#1890ff' },
        encode: { x: 'label', y: 'totalCartons' },
      },
    ],
  })
}

watch(() => props.items, renderChart, { deep: true })

onMounted(renderChart)

onBeforeUnmount(() => {
  chart?.dispose()
  chart = null
})
</script>

<template>
  <div ref="chartRef" class="chart-wrap" />
</template>

<style scoped>
.chart-wrap {
  width: 100%;
  height: 260px;
}
</style>
