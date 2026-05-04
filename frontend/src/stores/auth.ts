import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { authApi } from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>('')
  const displayName = ref<string>('')
  const userId = ref<string>('')
  const menuPermissionCodes = ref<string[]>([])
  const buttonPermissionCodes = ref<string[]>([])

  const isAuthenticated = computed(() => token.value.length > 0)

  function hasMenu(code: string) {
    return menuPermissionCodes.value.includes(code)
  }

  function hasButton(code: string) {
    return buttonPermissionCodes.value.includes(code)
  }

  async function signIn(username: string, password: string) {
    const res = await authApi.login({ username, password })
    const data = res.data.data!
    token.value = data.token
    displayName.value = data.displayName
    userId.value = data.userId

    // 登录后拉取权限
    const permRes = await authApi.permissions()
    const perm = permRes.data.data!
    menuPermissionCodes.value = perm.menuPermissions
    buttonPermissionCodes.value = perm.buttonPermissions
  }

  function signOut() {
    token.value = ''
    displayName.value = ''
    userId.value = ''
    menuPermissionCodes.value = []
    buttonPermissionCodes.value = []
  }

  return {
    token,
    displayName,
    userId,
    menuPermissionCodes,
    buttonPermissionCodes,
    isAuthenticated,
    hasMenu,
    hasButton,
    signIn,
    signOut,
  }
})
