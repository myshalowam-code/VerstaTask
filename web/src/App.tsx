import { Navigate, Route, Routes } from 'react-router-dom'
import { isAuthenticated } from './auth'
import { AppLayout } from './components/AppLayout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { CreateOrderPage } from './pages/CreateOrderPage'
import { LoginPage } from './pages/LoginPage'
import { OrderDetailsPage } from './pages/OrderDetailsPage'
import { OrdersPage } from './pages/OrdersPage'

export default function App() {
  return (
    <Routes>
      <Route
        path="/login"
        element={isAuthenticated() ? <Navigate to="/orders" replace /> : <LoginPage />}
      />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/orders" element={<OrdersPage />} />
          <Route path="/orders/new" element={<CreateOrderPage />} />
          <Route path="/orders/:id" element={<OrderDetailsPage />} />
        </Route>
      </Route>
      <Route
        path="*"
        element={<Navigate to={isAuthenticated() ? '/orders' : '/login'} replace />}
      />
    </Routes>
  )
}
