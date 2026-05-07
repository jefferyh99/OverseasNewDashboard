import type { TransportMode } from '@/services/types'

export function formatTransportMode(mode: TransportMode) {
  const labels: Record<TransportMode, string> = {
    sea: '海运',
    air: '空运',
    express: '快递',
    truck: '卡派',
  }
  return labels[mode]
}

export function formatProcessingStatus(status: 'processed' | 'pending' | 'all') {
  if (status === 'processed') return '已处理'
  if (status === 'pending') return '待处理'
  return '全部'
}
