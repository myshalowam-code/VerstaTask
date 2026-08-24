export function dateInputValue(date: Date) {
  const timezoneOffset = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - timezoneOffset).toISOString().slice(0, 10)
}

export function formatDate(value: string) {
  const date = value.includes('T') ? new Date(value) : new Date(`${value}T00:00:00`)
  return date.toLocaleDateString('ru-RU')
}
