import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createAppRouter } from '@/router'

describe('workload routes', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('registers anomaly dashboard route', () => {
    const router = createAppRouter()
    const match = router.resolve('/anomaly-dashboard')
    expect(match.name).toBe('anomaly-dashboard')
  })
})
