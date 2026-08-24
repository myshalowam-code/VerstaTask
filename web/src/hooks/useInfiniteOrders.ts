import { useCallback, useEffect, useRef, useState } from 'react'
import { api, type OrderListItem, type OrdersPageResponse } from '../api'

export function useInfiniteOrders(pageSize = 30) {
  const [orders, setOrders] = useState<OrderListItem[]>([])
  const [initialLoading, setInitialLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [hasMore, setHasMore] = useState(true)
  const [error, setError] = useState('')
  const cursorRef = useRef<string | null>(null)
  const hasMoreRef = useRef(true)
  const loadingRef = useRef(false)
  const orderCountRef = useRef(0)
  const controllerRef = useRef<AbortController | null>(null)

  const loadMore = useCallback(async () => {
    if (loadingRef.current || !hasMoreRef.current) return

    const isInitialPage = orderCountRef.current === 0 && cursorRef.current === null
    const controller = new AbortController()
    controllerRef.current = controller
    loadingRef.current = true
    setError('')
    if (isInitialPage) setInitialLoading(true)
    else setLoadingMore(true)

    const search = new URLSearchParams({ limit: String(pageSize) })
    if (cursorRef.current) search.set('cursor', cursorRef.current)

    try {
      const page = await api<OrdersPageResponse>(`/api/orders?${search}`, {
        signal: controller.signal,
      })
      if (controllerRef.current !== controller) return

      setOrders(current => {
        const knownIds = new Set(current.map(order => order.id))
        const uniqueItems = page.items.filter(order => !knownIds.has(order.id))
        const next = [...current, ...uniqueItems]
        orderCountRef.current = next.length
        return next
      })
      cursorRef.current = page.nextCursor
      hasMoreRef.current = page.hasMore
      setHasMore(page.hasMore)
    } catch (exception) {
      if (exception instanceof DOMException && exception.name === 'AbortError') return
      setError(exception instanceof Error ? exception.message : 'Не удалось загрузить заказы')
    } finally {
      if (controllerRef.current === controller) {
        controllerRef.current = null
        loadingRef.current = false
        setInitialLoading(false)
        setLoadingMore(false)
      }
    }
  }, [pageSize])

  useEffect(() => {
    void loadMore()

    return () => {
      controllerRef.current?.abort()
      controllerRef.current = null
      loadingRef.current = false
    }
  }, [loadMore])

  return {
    orders,
    initialLoading,
    loadingMore,
    hasMore,
    error,
    loadMore,
  }
}
