import { useState, type SubmitEvent } from 'react'
import { login } from '../api/authApi'

interface LoginFormProps {
  readonly onAuthenticated: () => void
}

export function LoginForm({ onAuthenticated }: LoginFormProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')

    if (!email.trim() || !password) {
      setError('Completa tu correo y contraseña para continuar.')
      return
    }

    const emailField = event.currentTarget.elements.namedItem('email') as HTMLInputElement
    if (!emailField.validity.valid) {
      setError('Ingresa un correo electrónico válido.')
      return
    }

    setIsSubmitting(true)
    try {
      await login(email.trim(), password)
      onAuthenticated()
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : 'No se pudo iniciar sesión.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="login-form" onSubmit={handleSubmit} noValidate>
      <label className="form-field" htmlFor="email">
        <span className="field-label">Correo corporativo <span aria-hidden="true">*</span></span>
        <input
          id="email"
          name="email"
          type="email"
          autoComplete="username"
          placeholder="nombre@empresa.com"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          disabled={isSubmitting}
          required
        />
      </label>

      <label className="form-field" htmlFor="password">
        <span className="field-label">Contraseña <span aria-hidden="true">*</span></span>
        <span className="password-control">
          <input
            id="password"
            name="password"
            type={showPassword ? 'text' : 'password'}
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            disabled={isSubmitting}
            required
          />
          <button
            className="password-toggle"
            type="button"
            aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
            aria-pressed={showPassword}
            onClick={() => setShowPassword((visible) => !visible)}
          >
            <span className="material-symbols" aria-hidden="true">
              {showPassword ? 'visibility_off' : 'visibility'}
            </span>
          </button>
        </span>
      </label>

      {error && <p className="form-message form-message-error" role="alert">{error}</p>}

      <button className="submit-button" type="submit" disabled={isSubmitting}>
        <span className="material-symbols" aria-hidden="true">
          {isSubmitting ? 'progress_activity' : 'login'}
        </span>
        <span>{isSubmitting ? 'Verificando…' : 'Iniciar sesión'}</span>
      </button>
    </form>
  )
}
