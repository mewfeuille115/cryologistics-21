import { post, setAccessToken } from '../../../shared/api/httpClient'

export interface LoginResponse {
  accessToken: string
  expiresIn: number
  refreshToken: string
  refreshExpiresIn: number
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const result = await post<LoginResponse, { email: string; password: string }>(
    '/users/login',
    { email, password },
  )

  setAccessToken(result.accessToken)
  return result
}
