import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import { setAccessToken } from '../auth'

type LoginResponse = {
  accessToken: string
}

export function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('demo@versta.local')
  const [password, setPassword] = useState('Demo123!')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setLoading(true)

    try {
      const result = await api<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      })
      setAccessToken(result.accessToken)
      navigate('/orders')
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Ошибка входа')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-page">
      <section className="login-copy">
        <p className="eyebrow">Доставка без лишних шагов</p>
        <h1>От двери<br />до двери.</h1>
        <p>Оформляйте отправления и следите за заказами в одном спокойном интерфейсе.</p>
      </section>
      <form className="card login-card" onSubmit={submit}>
        <div>
          <p className="eyebrow">Личный кабинет</p>
          <h2>Добро пожаловать</h2>
        </div>
        <label>
          Email
          <input type="email" value={email} onChange={event => setEmail(event.target.value)} required />
        </label>
        <label>
          Пароль
          <input type="password" value={password} onChange={event => setPassword(event.target.value)} required />
        </label>
        {error && <p className="error">{error}</p>}
        <button className="primary" disabled={loading}>{loading ? 'Входим…' : 'Войти'}</button>
        <p className="hint">Демо-доступ уже заполнен</p>
      </form>
    </div>
  )
}
