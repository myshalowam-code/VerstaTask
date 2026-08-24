import { Link } from 'react-router-dom'
import type { OrderListItem } from '../api'
import { formatDate } from '../utils/date'

type OrderCardProps = {
  order: OrderListItem
}

export function OrderCard({ order }: OrderCardProps) {
  return (
    <Link className="order-card card" to={`/orders/${order.id}`}>
      <div>
        <span className="number">{order.number}</span>
        <span className="status">Принят</span>
      </div>
      <div className="route">
        <strong>{order.senderCity}</strong>
        <span>→</span>
        <strong>{order.recipientCity}</strong>
      </div>
      <div className="meta">
        <span>{order.weightKg} кг</span>
        <span>Забор {formatDate(order.pickupDate)}</span>
      </div>
    </Link>
  )
}
