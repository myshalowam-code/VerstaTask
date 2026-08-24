import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type Order } from '../api'
import { formatDate } from '../utils/date'

export function OrderDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const [order, setOrder] = useState<Order | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return

    const controller = new AbortController()
    setOrder(null)
    setError('')
    api<Order>(`/api/orders/${id}`, { signal: controller.signal })
      .then(setOrder)
      .catch(exception => {
        if (exception instanceof DOMException && exception.name === 'AbortError') return
        setError(exception instanceof Error ? exception.message : 'Не удалось загрузить заказ')
      })

    return () => controller.abort()
  }, [id])

  if (!id) return <p className="error state">Некорректный идентификатор заказа</p>
  if (error) return <p className="error state">{error}</p>
  if (!order) return <p className="state">Загружаем заказ…</p>

  return (
    <div className="narrow">
      <Link to="/orders" className="back">← Все заказы</Link>
      <div className="page-title">
        <div>
          <p className="eyebrow">Заказ</p>
          <h1>{order.number}</h1>
        </div>
        <span className="status large">Принят</span>
      </div>
      <section className="card details">
        <div className="route-detail">
          <div>
            <small>Откуда</small>
            <h2>{order.senderCity}</h2>
            <p>{order.senderAddress}</p>
          </div>
          <div className="route-line">→</div>
          <div>
            <small>Куда</small>
            <h2>{order.recipientCity}</h2>
            <p>{order.recipientAddress}</p>
          </div>
        </div>
        <div className="facts">
          <div><small>Вес груза</small><strong>{order.weightKg} кг</strong></div>
          <div><small>Дата забора</small><strong>{formatDate(order.pickupDate)}</strong></div>
          <div><small>Создан</small><strong>{formatDate(order.createdAtUtc)}</strong></div>
        </div>
      </section>
    </div>
  )
}
