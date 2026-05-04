import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createAppRouter } from '@/router'

describe('route guard', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('redirects unauthenticated users to login', async () => {
    const router = createAppRouter()

    await router.push('/dashboard')
    await router.isReady()

    expect(router.currentRoute.value.fullPath).toBe('/login')
  })
})
