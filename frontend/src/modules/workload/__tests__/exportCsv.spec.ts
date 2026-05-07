import { describe, expect, it } from 'vitest'
import { buildCsv } from '@/modules/workload/utils/exportCsv'

describe('buildCsv', () => {
  it('renders header and rows in csv order', () => {
    const csv = buildCsv(
      ['订单号', '客户'],
      [
        ['SO1001', '客户A'],
        ['SO1002', '客户B'],
      ],
    )

    expect(csv).toContain('"订单号","客户"')
    expect(csv).toContain('"SO1001","客户A"')
    expect(csv).toContain('"SO1002","客户B"')
  })
})
