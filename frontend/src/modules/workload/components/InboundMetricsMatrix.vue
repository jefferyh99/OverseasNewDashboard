<script setup lang="ts">
import { formatTransportMode } from '@/modules/workload/utils/formatters'
import type { InboundTransportRow, TomorrowInboundTransportRow } from '@/services/types'

const props = defineProps<{
  showStatusSplit: boolean
  rows: InboundTransportRow[] | TomorrowInboundTransportRow[]
}>()

function formatTotal(value: number) {
  return Number.isInteger(value) ? `${value}` : value.toFixed(2)
}
</script>

<template>
  <el-table :data="props.rows" size="small" border>
    <el-table-column label="货运方式" width="110">
      <template #default="{ row }">{{ formatTransportMode(row.transportMode) }}</template>
    </el-table-column>
    <el-table-column label="总箱数 / 已处理 / 待处理">
      <template #default="{ row }">
        <template v-if="props.showStatusSplit">
          {{ formatTotal(row.cartons.total) }} / {{ formatTotal(row.cartons.processed) }} / {{ formatTotal(row.cartons.pending) }}
        </template>
        <template v-else>{{ formatTotal(row.totalCartons) }}</template>
      </template>
    </el-table-column>
    <el-table-column label="总重量(kg)">
      <template #default="{ row }">
        <template v-if="props.showStatusSplit">{{ formatTotal(row.weightKg.total) }}</template>
        <template v-else>{{ formatTotal(row.totalWeightKg) }}</template>
      </template>
    </el-table-column>
    <el-table-column label="总体积(m3)">
      <template #default="{ row }">
        <template v-if="props.showStatusSplit">{{ formatTotal(row.volumeM3.total) }}</template>
        <template v-else>{{ formatTotal(row.totalVolumeM3) }}</template>
      </template>
    </el-table-column>
    <el-table-column label="总件数" width="100">
      <template #default="{ row }">
        <template v-if="props.showStatusSplit">{{ formatTotal(row.units.total) }}</template>
        <template v-else>{{ formatTotal(row.totalUnits) }}</template>
      </template>
    </el-table-column>
    <el-table-column label="总SKU数" width="100">
      <template #default="{ row }">
        <template v-if="props.showStatusSplit">{{ formatTotal(row.skuCount.total) }}</template>
        <template v-else>{{ formatTotal(row.totalSkuCount) }}</template>
      </template>
    </el-table-column>
    <el-table-column label="卡派板数" width="100">
      <template #default="{ row }">
        {{ row.transportMode === 'truck' ? (row.truckPalletCount ?? '-') : '-' }}
      </template>
    </el-table-column>
    <el-table-column label="海运柜数" width="100">
      <template #default="{ row }">
        {{ row.transportMode === 'sea' ? (row.seaContainerCount ?? '-') : '-' }}
      </template>
    </el-table-column>
  </el-table>
</template>
