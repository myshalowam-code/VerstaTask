import { Link, Outlet, useNavigate } from 'react-router-dom'
import { clearAccessToken } from '../auth'

export function AppLayout() {
  const navigate = useNavigate()

  function logout() {
    clearAccessToken()
    navigate('/login')
  }

  return (
    <div className="shell">
      <header>
        <Link to="/orders" className="brand">
          <span>V</span> Versta Delivery
        </Link>
        <nav>
          <Link to="/orders">Заказы</Link>
          <Link to="/orders/new" className="primary small">Новый заказ</Link>
          <button className="link-button" onClick={logout}>Выйти</button>
        </nav>
      </header>
      <main><Outlet /></main>
    </div>
  )
}
