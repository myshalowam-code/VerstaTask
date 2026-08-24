import { Link } from 'react-router-dom'
import { InfiniteScrollTrigger } from '../components/InfiniteScrollTrigger'
import { OrderCard } from '../components/OrderCard'
import { useInfiniteOrders } from '../hooks/useInfiniteOrders'

export function OrdersPage() {
  const {
    orders,
    initialLoading,
    loadingMore,
    hasMore,
    error,
    loadMore,
  } = useInfiniteOrders()

  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">Ваши отправления</p>
          <h1>Заказы</h1>
        </div>
        <Link className="primary" to="/orders/new">+ Создать заказ</Link>
      </div>

      {initialLoading && <p className="state">Загружаем заказы…</p>}
      {!initialLoading && error && orders.length === 0 && (
        <div className="state">
          <p className="error">{error}</p>
          <button className="secondary" onClick={loadMore}>Повторить</button>
        </div>
      )}
      {!initialLoading && !error && orders.length === 0 && (
        <div className="empty card">
          <div className="package-icon">□</div>
          <h2>Пока нет заказов</h2>
          <p>Создайте первое отправление — это займет меньше минуты.</p>
          <Link className="primary" to="/orders/new">Создать заказ</Link>
        </div>
      )}
      {orders.length > 0 && (
        <>
          <div className="orders-grid">
            {orders.map(order => <OrderCard order={order} key={order.id} />)}
          </div>
          {error && (
            <div className="load-more-error">
              <span className="error">{error}</span>
              <button className="secondary" onClick={loadMore}>Повторить</button>
            </div>
          )}
          <InfiniteScrollTrigger
            hasMore={hasMore}
            loading={loadingMore}
            disabled={Boolean(error)}
            onLoadMore={loadMore}
          />
        </>
      )}
    </>
  )
}
