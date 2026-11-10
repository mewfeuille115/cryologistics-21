import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, it, vi } from 'vitest'
import { LoginForm } from './LoginForm'
import { login } from '../api/authApi'

vi.mock('../api/authApi', () => ({ login: vi.fn() }))

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

it('requires credentials before submitting', async () => {
  const user = userEvent.setup()
  render(<LoginForm onAuthenticated={vi.fn()} />)

  await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

  expect(screen.getByRole('alert').textContent).toBe('Completa tu correo y contraseña para continuar.')
  expect(login).not.toHaveBeenCalled()
})

it('rejects an invalid email address', async () => {
  const user = userEvent.setup()
  render(<LoginForm onAuthenticated={vi.fn()} />)

  await user.type(screen.getByLabelText(/Correo corporativo/), 'not-an-email')
  await user.type(screen.getByLabelText(/Contraseña/), 'Password123!')
  await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

  expect(screen.getByRole('alert').textContent).toBe('Ingresa un correo electrónico válido.')
  expect(login).not.toHaveBeenCalled()
})

it('toggles password visibility', async () => {
  const user = userEvent.setup()
  render(<LoginForm onAuthenticated={vi.fn()} />)
  const password = screen.getByLabelText(/Contraseña/)

  expect(password.getAttribute('type')).toBe('password')
  await user.click(screen.getByRole('button', { name: 'Mostrar contraseña' }))
  expect(password.getAttribute('type')).toBe('text')
  await user.click(screen.getByRole('button', { name: 'Ocultar contraseña' }))
  expect(password.getAttribute('type')).toBe('password')
})

it('submits trimmed credentials and calls the authenticated callback', async () => {
  const user = userEvent.setup()
  const onAuthenticated = vi.fn()
  vi.mocked(login).mockResolvedValue({
    accessToken: 'access-token',
    expiresIn: 300,
    refreshToken: 'refresh-token',
    refreshExpiresIn: 1800,
  })
  render(<LoginForm onAuthenticated={onAuthenticated} />)

  await user.type(screen.getByLabelText(/Correo corporativo/), '  operator@cryologistics.com  ')
  await user.type(screen.getByLabelText(/Contraseña/), 'Password123!')
  await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

  expect(login).toHaveBeenCalledWith('operator@cryologistics.com', 'Password123!')
  expect(onAuthenticated).toHaveBeenCalledOnce()
})

it('displays the service error when login fails', async () => {
  const user = userEvent.setup()
  vi.mocked(login).mockRejectedValue(new Error('Credenciales no válidas.'))
  render(<LoginForm onAuthenticated={vi.fn()} />)

  await user.type(screen.getByLabelText(/Correo corporativo/), 'operator@cryologistics.com')
  await user.type(screen.getByLabelText(/Contraseña/), 'wrong')
  await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

  expect(await screen.findByRole('alert')).toHaveProperty('textContent', 'Credenciales no válidas.')
})

it('displays a fallback message for unexpected login failures', async () => {
  const user = userEvent.setup()
  vi.mocked(login).mockRejectedValue('unexpected failure')
  render(<LoginForm onAuthenticated={vi.fn()} />)

  await user.type(screen.getByLabelText(/Correo corporativo/), 'operator@cryologistics.com')
  await user.type(screen.getByLabelText(/Contraseña/), 'wrong')
  fireEvent.submit(screen.getByRole('button', { name: 'Iniciar sesión' }).closest('form')!)

  expect(await screen.findByRole('alert')).toHaveProperty('textContent', 'No se pudo iniciar sesión.')
})