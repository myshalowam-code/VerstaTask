import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api, waitForOrder, type CreateOrderAccepted, type NewOrder } from '../api'
import { AddressFields } from '../components/AddressFields'
import { OrderCreationLoader } from '../components/OrderCreationLoader'
import { dateInputValue } from '../utils/date'

function createInitialOrder(): NewOrder {
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)

  return {
    senderCity: '',
    senderAddress: '',
    recipientCity: '',
    recipientAddress: '',
    weightKg: 1,
    pickupDate: dateInputValue(tomorrow),
  }
}

export function CreateOrderPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState<NewOrder>(createInitialOrder)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [loadingText, setLoadingText] = useState('Заказ сохраняется, подождите пожалуйста')

  function update(field: keyof NewOrder, value: string | number) {
    setForm(current => ({ ...current, [field]: value }))
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setLoading(true)
    setError('')
    setLoadingText('Заказ сохраняется, подождите пожалуйста')

    try {
      const accepted = await api<CreateOrderAccepted>('/api/orders', {
        method: 'POST',
        body: JSON.stringify(form),
      })
      await waitForOrder(accepted.orderId)
      navigate(`/orders/${accepted.orderId}`)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось создать заказ')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="narrow">
      {loading && <OrderCreationLoader message={loadingText} />}
      <Link to="/orders" className="back">← Все заказы</Link>
      <div className="page-title">
        <div>
          <p className="eyebrow">Новое отправление</p>
          <h1>Оформить доставку</h1>
          <p>Заполните маршрут и параметры груза.</p>
        </div>
      </div>
      <form className="card order-form" onSubmit={submit}>
        <AddressFields
          step={1}
          title="Отправитель"
          city={form.senderCity}
          address={form.senderAddress}
          cityPlaceholder="Санкт-Петербург"
          addressPlaceholder="Невский пр., 28"
          onCityChange={value => update('senderCity', value)}
          onAddressChange={value => update('senderAddress', value)}
        />
        <AddressFields
          step={2}
          title="Получатель"
          city={form.recipientCity}
          address={form.recipientAddress}
          cityPlaceholder="Москва"
          addressPlaceholder="ул. Тверская, 12"
          onCityChange={value => update('recipientCity', value)}
          onAddressChange={value => update('recipientAddress', value)}
        />
        <fieldset>
          <legend><span>3</span> Груз</legend>
          <div className="two-cols">
            <label>
              Вес, кг
              <input
                type="number"
                min="0.001"
                max="100000"
                step="0.001"
                value={form.weightKg}
                onChange={event => update('weightKg', Number(event.target.value))}
                required
              />
            </label>
            <label>
              Дата забора
              <input
                type="date"
                min={dateInputValue(new Date())}
                value={form.pickupDate}
                onChange={event => update('pickupDate', event.target.value)}
                required
              />
            </label>
          </div>
        </fieldset>
        {error && <p className="error">{error}</p>}
        <div className="actions">
          <Link to="/orders" className="secondary">Отмена</Link>
          <button className="primary" disabled={loading}>
            {loading ? 'Создаём…' : 'Создать заказ'}
          </button>
        </div>
      </form>
    </div>
  )
}
