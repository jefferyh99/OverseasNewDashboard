<script setup lang="ts">
import { nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import * as echarts from 'echarts'
import type { FutureInboundForecastBar } from '@/services/types'

const props = defineProps<{
  items: FutureInboundForecastBar[]
}>()

const chartRef = ref<HTMLDivElement | null>(null)
let chart: echarts.ECharts | null = null
let resizeObserver: ResizeObserver | null = null

function getTooltipHtml(index: number) {
  const row = props.items[index]
  if (!row) return ''
  return [
    row.arrivalDate,
    `总箱数: ${row.totalCartons}`,
    `总重量(kg): ${row.totalWeightKg}`,
    `总体积(m3): ${row.totalVolumeM3}`,
    `海运柜数: ${row.seaContainerCount}`,
    `卡派板数: ${row.truckPalletCount}`,
  ].join('<br/>')
}

async function renderChart() {
  if (!chartRef.value) return
  await nextTick()
  if (!chart) chart = echarts.init(chartRef.value)

  const xData = props.items.map((item) => item.arrivalDate)
  const yData = props.items.map((item) => item.totalCartons)

  chart.setOption({
    animationDuration: 320,
    grid: { left: 48, right: 18, top: 26, bottom: 38 },
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(9, 24, 40, 0.95)',
      borderColor: '#2f5f8f',
      textStyle: { color: '#d9ebff', fontSize: 12 },
      formatter: (params: any) => getTooltipHtml(params?.[0]?.dataIndex ?? -1),
    },
    xAxis: {
      type: 'category',
      data: xData,
      axisTick: { alignWithLabel: true },
      axisLine: { lineStyle: { color: '#33577d' } },
      axisLabel: { color: '#87a9c9', fontSize: 11 },
    },
    yAxis: {
      type: 'value',
      name: '总箱数',
      nameTextStyle: { color: '#87a9c9', padding: [0, 0, 0, 6] },
      splitLine: { lineStyle: { color: 'rgba(53, 89, 122, 0.55)', type: 'dashed' } },
      axisLine: { lineStyle: { color: '#33577d' } },
      axisLabel: { color: '#87a9c9' },
    },
    series: [
      {
        type: 'bar',
        data: yData,
        barWidth: 20,
        itemStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: '#67C2FF' },
            { offset: 1, color: '#2F93E6' },
          ]),
          borderRadius: [5, 5, 0, 0],
          shadowBlur: 10,
          shadowColor: 'rgba(52, 149, 229, 0.28)',
        },
      },
    ],
  })

  chart.resize()
}

function handleResize() {
  chart?.resize()
}

watch(() => props.items, () => {
  renderChart()
}, { deep: true })

onMounted(() => {
  renderChart()
  window.addEventListener('resize', handleResize)
  if (chartRef.value && typeof ResizeObserver !== 'undefined') {
    resizeObserver = new ResizeObserver(() => {
      chart?.resize()
    })
    resizeObserver.observe(chartRef.value)
  }
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  resizeObserver?.disconnect()
  resizeObserver = null
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
  height: 286px;
}
</style>
