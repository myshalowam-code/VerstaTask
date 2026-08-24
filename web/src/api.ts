import { clearAccessToken, getAccessToken } from './auth'

export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080'

export type Order = {
  id: string
  number: string
  senderCity: string
  senderAddress: string
  recipientCity: string
  recipientAddress: string
  weightKg: number
  pickupDate: string
  createdAtUtc: string
}

export type NewOrder = Omit<Order, 'id' | 'number' | 'createdAtUtc'>

export type OrderListItem = Pick<
  Order,
  'id' | 'number' | 'senderCity' | 'recipientCity' | 'weightKg' | 'pickupDate' | 'createdAtUtc'
>

export type OrdersPageResponse = {
  items: OrderListItem[]
  nextCursor: string | null
  hasMore: boolean
}

export type CreateOrderAccepted = {
  orderId: string
}

export class ApiError extends Error {
  constructor(message: string, public readonly status: number) {
    super(message)
  }
}

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const token = getAccessToken()
  const response = await fetch(`${API_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  })

  if (response.status === 401) {
    clearAccessToken()
    window.location.href = '/login'
    throw new Error('Требуется авторизация')
  }
  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new ApiError(body.message ?? body.title ?? 'Не удалось выполнить запрос', response.status)
  }
  return response.json() as Promise<T>
}

export async function waitForOrder(id: string): Promise<Order> {
  for (let attempt = 0; attempt < 30; attempt += 1) {
    try {
      return await api<Order>(`/api/orders/${id}`)
    } catch (error) {
      if (!(error instanceof ApiError) || error.status !== 404) throw error
      await new Promise(resolve => window.setTimeout(resolve, 400))
    }
  }
  throw new Error('Заказ сохранён, но проекция MongoDB ещё не готова. Обновите список через несколько секунд.')
}
