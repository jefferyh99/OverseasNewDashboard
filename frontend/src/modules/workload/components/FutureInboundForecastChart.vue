<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import * as echarts from 'echarts'
import type { FutureInboundForecastBar } from '@/services/types'

const props = defineProps<{
  items: FutureInboundForecastBar[]
}>()

const chartRef = ref<HTMLDivElement | null>(null)
let chart: echarts.ECharts | null = null

function renderChart() {
  if (!chartRef.value) return
  if (!chart) chart = echarts.init(chartRef.value)

  chart.setOption({
    dataset: { source: props.items },
    grid: { left: 48, right: 16, top: 24, bottom: 36 },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const row = params?.[0]?.data as FutureInboundForecastBar | undefined
        if (!row) return ''
        return [
          row.arrivalDate,
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
        encode: { x: 'arrivalDate', y: 'totalCartons' },
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
