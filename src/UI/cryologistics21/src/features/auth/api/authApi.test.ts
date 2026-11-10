import { afterEach, expect, it, vi } from 'vitest'
import { login } from './authApi'
import { post, setAccessToken } from '../../../shared/api/httpClient'

vi.mock('../../../shared/api/httpClient', () => ({
  post: vi.fn(),
  setAccessToken: vi.fn(),
}))

afterEach(() => vi.clearAllMocks())

it('posts the credentials, stores the access token, and returns the response', async () => {
  const response = {
    accessToken: 'access-token',
    expiresIn: 300,
    refreshToken: 'refresh-token',
    refreshExpiresIn: 1800,
  }
  vi.mocked(post).mockResolvedValue(response)

  await expect(login('operator@cryologistics.com', 'Password123!')).resolves.toBe(response)

  expect(post).toHaveBeenCalledWith('/users/login', {
    email: 'operator@cryologistics.com',
    password: 'Password123!',
  })
  expect(setAccessToken).toHaveBeenCalledWith('access-token')
})