// All backend calls live here, so the base URL and the X-Student-Id header
// are set in exactly one place.

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'

export type ClassItem = {
  id: string
  name: string
  totalSeats: number
  heldSeats: number
  confirmedSeats: number
  availableSeats: number
}

export type HoldCreated = {
  holdId: string
  classId: string
  studentId: string
  status: string
  expiresAt: string
}

export type HoldDetail = {
  holdId: string
  classId: string
  status: string
  expiresAt: string
}

export type MyHold = {
  holdId: string
  classId: string
  className: string
  status: string
  expiresAt: string
}

export class ApiError extends Error {}

async function request<T>(
  path: string,
  options: RequestInit = {},
  studentId?: string,
): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> | undefined),
  }
  if (studentId) headers['X-Student-Id'] = studentId

  const res = await fetch(BASE_URL + path, { ...options, headers })

  if (!res.ok) {
    // The API returns { "message": "..." } for every error.
    let message = `Request failed (${res.status})`
    try {
      const body = await res.json()
      if (body?.message) message = body.message
    } catch {
      // response had no JSON body, keep the default message
    }
    throw new ApiError(message)
  }

  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

export const api = {
  getClasses: () => request<ClassItem[]>('/classes'),

  createClass: (name: string, totalSeats: number) =>
    request<ClassItem>('/classes', {
      method: 'POST',
      body: JSON.stringify({ name, totalSeats }),
    }),

  createHold: (classId: string, studentId: string) =>
    request<HoldCreated>(`/classes/${classId}/holds`, { method: 'POST' }, studentId),

  getHold: (holdId: string) => request<HoldDetail>(`/holds/${holdId}`),

  confirmHold: (holdId: string, studentId: string) =>
    request<{ holdId: string; status: string }>(
      `/holds/${holdId}/confirm`,
      { method: 'POST' },
      studentId,
    ),

  cancelHold: (holdId: string, studentId: string) =>
    request<void>(`/holds/${holdId}`, { method: 'DELETE' }, studentId),

  getMyHolds: (studentId: string) => request<MyHold[]>('/me/holds', {}, studentId),
}

export function errorMessage(e: unknown): string {
  if (e instanceof Error) return e.message
  return 'Something went wrong. Please try again.'
}
