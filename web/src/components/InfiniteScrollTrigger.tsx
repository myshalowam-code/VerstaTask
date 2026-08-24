import { useEffect, useRef } from 'react'

type InfiniteScrollTriggerProps = {
  hasMore: boolean
  loading: boolean
  disabled?: boolean
  onLoadMore: () => void
}

export function InfiniteScrollTrigger({
  hasMore,
  loading,
  disabled = false,
  onLoadMore,
}: InfiniteScrollTriggerProps) {
  const triggerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const trigger = triggerRef.current
    if (!trigger || !hasMore || loading || disabled) return

    const observer = new IntersectionObserver(
      entries => {
        if (entries[0]?.isIntersecting) onLoadMore()
      },
      { rootMargin: '400px 0px' },
    )
    observer.observe(trigger)

    return () => observer.disconnect()
  }, [disabled, hasMore, loading, onLoadMore])

  return (
    <div ref={triggerRef} className="infinite-scroll-trigger" aria-live="polite">
      {loading && <><span className="inline-spinner" /> Загружаем ещё…</>}
      {!hasMore && <span>Все заказы загружены</span>}
    </div>
  )
}
