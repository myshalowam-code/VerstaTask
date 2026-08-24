type AddressFieldsProps = {
  step: number
  title: string
  city: string
  address: string
  cityPlaceholder: string
  addressPlaceholder: string
  onCityChange: (value: string) => void
  onAddressChange: (value: string) => void
}

export function AddressFields({
  step,
  title,
  city,
  address,
  cityPlaceholder,
  addressPlaceholder,
  onCityChange,
  onAddressChange,
}: AddressFieldsProps) {
  return (
    <fieldset>
      <legend><span>{step}</span> {title}</legend>
      <div className="two-cols">
        <label>
          Город
          <input
            value={city}
            onChange={event => onCityChange(event.target.value)}
            placeholder={cityPlaceholder}
            required
          />
        </label>
        <label>
          Адрес
          <input
            value={address}
            onChange={event => onAddressChange(event.target.value)}
            placeholder={addressPlaceholder}
            required
          />
        </label>
      </div>
    </fieldset>
  )
}
