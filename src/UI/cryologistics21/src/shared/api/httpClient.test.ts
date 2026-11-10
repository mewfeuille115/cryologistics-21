import { afterEach, expect, it, vi } from 'vitest'
import { post, setAccessToken } from './httpClient'

afterEach(() => {
  vi.unstubAllGlobals()
  setAccessToken(null)
})

it('posts JSON and returns the successful response body', async () => {
  const fetchMock = vi.fn().mockResolvedValue(new Response('{"id":"user-1"}', { status: 200 }))
  vi.stubGlobal('fetch', fetchMock)

  const response = await post<{ id: string }, { name: string }>('/users', { name: 'Operator' })

  expect(response).toEqual({ id: 'user-1' })
  expect(fetchMock).toHaveBeenCalledWith('/api/users', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: '{"name":"Operator"}',
  })
})

it('adds the bearer token when one is set', async () => {
  const fetchMock = vi.fn().mockResolvedValue(new Response('{}', { status: 200 }))
  vi.stubGlobal('fetch', fetchMock)
  setAccessToken('access-token')

  await post('/users', {})

  expect(fetchMock.mock.calls[0][1]).toMatchObject({
    headers: {
      'Content-Type': 'application/json',
      Authorization: 'Bearer access-token',
    },
  })
})

it('uses the problem detail or title for failed responses', async () => {
  const fetchMock = vi.fn()
    .mockResolvedValueOnce({ ok: false, json: () => Promise.resolve({ detail: 'Not allowed' }) })
    .mockResolvedValueOnce({ ok: false, json: () => Promise.resolve({ title: 'Unavailable' }) })
  vi.stubGlobal('fetch', fetchMock)

  await expect(post('/users', {})).rejects.toThrow('Not allowed')
  await expect(post('/users', {})).rejects.toThrow('Unavailable')
})

it('uses a fallback when a failed response has no readable problem body', async () => {
  const fetchMock = vi.fn().mockResolvedValue({
    ok: false,
    json: () => Promise.reject(new Error('Invalid JSON')),
  })
  vi.stubGlobal('fetch', fetchMock)

  await expect(post('/users', {})).rejects.toThrow('No se pudo completar la solicitud.')
})