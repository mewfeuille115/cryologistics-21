import { act, cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, it, vi } from 'vitest'
import { AppHeader } from './AppHeader'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
  vi.useRealTimers()
  localStorage.clear()
  delete document.documentElement.dataset.theme
})

it('loads the saved light theme and persists theme changes', async () => {
  const user = userEvent.setup()
  localStorage.setItem('cryo-theme', 'light')

  render(<AppHeader />)

  expect(document.documentElement.dataset.theme).toBe('light')
  await user.click(screen.getByRole('button', { name: 'Cambiar a tema oscuro' }))
  expect(document.documentElement.dataset.theme).toBe('dark')
  expect(localStorage.getItem('cryo-theme')).toBe('dark')
})

it('falls back to dark when local storage cannot be read or written', async () => {
  const user = userEvent.setup()
  vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
    throw new Error('Storage unavailable')
  })
  vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
    throw new Error('Storage unavailable')
  })

  render(<AppHeader />)

  expect(document.documentElement.dataset.theme).toBe('dark')
  await user.click(screen.getByRole('button', { name: 'Cambiar a tema claro' }))
  expect(document.documentElement.dataset.theme).toBe('light')
})

it('refreshes the displayed UTC clock every second', () => {
  vi.useFakeTimers()
  vi.setSystemTime(new Date('2026-11-09T12:00:00.000Z'))
  render(<AppHeader />)

  expect(screen.getByLabelText('Hora UTC').textContent).toBe('12:00:00 UTC')
  act(() => vi.advanceTimersByTime(1000))
  expect(screen.getByLabelText('Hora UTC').textContent).toBe('12:00:01 UTC')
})