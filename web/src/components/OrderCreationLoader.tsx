type OrderCreationLoaderProps = {
  message: string
}

export function OrderCreationLoader({ message }: OrderCreationLoaderProps) {
  return (
    <div className="loader-overlay" role="status" aria-live="polite">
      <div className="loader-card card">
        <div className="spinner" />
        <h2>Создаём заказ</h2>
        <p>{message}</p>
      </div>
    </div>
  )
}
