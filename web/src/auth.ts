const accessTokenKey = 'accessToken'

export const isAuthenticated = () => Boolean(localStorage.getItem(accessTokenKey))

export const getAccessToken = () => localStorage.getItem(accessTokenKey)

export const setAccessToken = (token: string) => localStorage.setItem(accessTokenKey, token)

export const clearAccessToken = () => localStorage.removeItem(accessTokenKey)
