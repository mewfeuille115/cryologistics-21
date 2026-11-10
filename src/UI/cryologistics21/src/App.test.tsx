import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, it, vi } from 'vitest'
import App from './App'
import { login } from './features/auth/api/authApi'

vi.mock('./features/auth/api/authApi', () => ({ login: vi.fn() }))

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
  localStorage.clear()
})

it('renders the login and telemetry views and confirms authentication', async () => {
  const user = userEvent.setup()
  vi.mocked(login).mockResolvedValue({
    accessToken: 'access-token',
    expiresIn: 300,
    refreshToken: 'refresh-token',
    refreshExpiresIn: 1800,
  })

  render(<App />)

  expect(screen.getByRole('heading', { name: 'Cadena de frío bajo control, firmada y auditada' })).toBeTruthy()
  expect(screen.getByText('−71.2')).toBeTruthy()

  await user.type(screen.getByLabelText(/Correo corporativo/), 'operator@cryologistics.com')
  await user.type(screen.getByLabelText(/Contraseña/), 'Password123!')
  await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

  expect(await screen.findByText('Acceso validado. La autenticación respondió correctamente.')).toBeTruthy()
  expect(login).toHaveBeenCalledWith('operator@cryologistics.com', 'Password123!')
})