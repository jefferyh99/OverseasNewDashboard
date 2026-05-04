import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>('')
  const displayName = ref<string>('')
  const menuPermissionCodes = ref<string[]>([])
  const buttonPermissionCodes = ref<string[]>([])

  const isAuthenticated = computed(() => token.value.length > 0)

  function signIn() {
    token.value = 'mock-jwt-token'
    displayName.value = '系统管理员'
    menuPermissionCodes.value = [
      'dashboard',
      'anomaly-outbound',
      'anomaly-inbound',
      'anomaly-shelving',
      'settings-alerts',
    ]
    buttonPermissionCodes.value = ['settings-alerts-save']
  }

  function signOut() {
    token.value = ''
    displayName.value = ''
    menuPermissionCodes.value = []
    buttonPermissionCodes.value = []
  }

  return {
    token,
    displayName,
    menuPermissionCodes,
    buttonPermissionCodes,
    isAuthenticated,
    signIn,
    signOut,
  }
})
