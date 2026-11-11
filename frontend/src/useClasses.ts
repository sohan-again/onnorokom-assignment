import { useEffect, useState } from 'react'
import { api, errorMessage, type ClassItem } from './api'

// Both portals show the same list and need it to move on its own,
// so the polling lives in one place.
export function useClasses() {
  const [classes, setClasses] = useState<ClassItem[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    let alive = true

    async function load() {
      try {
        const list = await api.getClasses()
        if (alive) setClasses(list)
      } catch (e) {
        if (alive) setError(errorMessage(e))
      }
    }

    load()
    const timer = setInterval(load, 2000)
    return () => {
      alive = false
      clearInterval(timer)
    }
  }, [])

  return { classes, error }
}
