const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')
let accessToken: string | null = null

export function setAccessToken(token: string | null) {
  accessToken = token
}

export async function post<TResponse, TBody>(path: string, body: TBody): Promise<TResponse> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { detail?: string; title?: string } | null
    throw new Error(problem?.detail || problem?.title || 'No se pudo completar la solicitud.')
  }

  return response.json() as Promise<TResponse>
}
