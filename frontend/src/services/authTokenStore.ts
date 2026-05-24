const TOKEN_STORAGE_KEY = 'propeliq_access_token'

let accessToken: string | null = (() => {
  try {
    return sessionStorage.getItem(TOKEN_STORAGE_KEY)
  } catch {
    return null
  }
})()

export const setAuthToken = (token: string | null): void => {
  accessToken = token

  try {
    if (token) {
      sessionStorage.setItem(TOKEN_STORAGE_KEY, token)
    } else {
      sessionStorage.removeItem(TOKEN_STORAGE_KEY)
    }
  } catch {
    // sessionStorage unavailable
  }
}

export const authHeaders = (): Record<string, string> =>
  accessToken ? { Authorization: `Bearer ${accessToken}` } : {}
