import { useEffect, useState } from 'react'
import { api, errorMessage, type MyHold } from '../api'
import StatusBadge from '../components/StatusBadge'

type Props = {
  studentId: string
  onOpen: (hold: { holdId: string; className: string }) => void
}

export default function MyHoldsView({ studentId, onOpen }: Props) {
  const [holds, setHolds] = useState<MyHold[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    if (!studentId) {
      setHolds([])
      return
    }

    let alive = true

    async function load() {
      try {
        const list = await api.getMyHolds(studentId)
        if (alive) setHolds(list)
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
  }, [studentId])

  if (!studentId) {
    return <p className="text-muted">Enter your birth registration number above.</p>
  }

  return (
    <div>
      {error && <div className="alert alert-danger">{error}</div>}

      <table className="table table-sm align-middle">
        <thead>
          <tr>
            <th>Class</th>
            <th>Status</th>
            <th>Expires</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {holds.map((h) => (
            <tr key={h.holdId}>
              <td>{h.className}</td>
              <td>
                <StatusBadge status={h.status} />
              </td>
              <td>{new Date(h.expiresAt).toLocaleString()}</td>
              <td className="text-end">
                <button
                  className="btn btn-sm btn-outline-primary"
                  onClick={() => onOpen({ holdId: h.holdId, className: h.className })}
                >
                  Open
                </button>
              </td>
            </tr>
          ))}
          {holds.length === 0 && (
            <tr>
              <td colSpan={4} className="text-muted">
                No applications yet.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
