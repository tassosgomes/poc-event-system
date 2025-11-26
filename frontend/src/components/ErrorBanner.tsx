interface Props {
  message: string
  actionLabel?: string
  onAction?: () => void
}

export function ErrorBanner({ message, actionLabel = 'Tentar novamente', onAction }: Props) {
  return (
    <div className="error-banner">
      <span>{message}</span>
      {onAction && (
        <button type="button" className="button secondary" onClick={onAction}>
          {actionLabel}
        </button>
      )}
    </div>
  )
}
