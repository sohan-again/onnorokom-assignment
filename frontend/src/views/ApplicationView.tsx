import { useEffect, useState } from 'react'
import { api, errorMessage, type HoldDetail } from '../api'
import StatusBadge from '../components/StatusBadge'

type Props = {
  holdId: string
  className: string
  studentId: string
  onDone: (message: string) => void
}

export default function ApplicationView({ holdId, className, studentId, onDone }: Props) {
  const [hold, setHold] = useState<HoldDetail | null>(null)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [now, setNow] = useState(Date.now())

  // Poll the application status so EXPIRED shows up without a reload.
  useEffect(() => {
    let alive = true

    async function load() {
      try {
        const h = await api.getHold(holdId)
        if (alive) setHold(h)
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
  }, [holdId])

  // Local ticking clock for the countdown.
  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 1000)
    return () => clearInterval(timer)
  }, [])

  async function pay() {
    setError('')
    setBusy(true)
    try {
      await api.confirmHold(holdId, studentId)
      onDone('Admission confirmed.')
    } catch (e) {
      setError(errorMessage(e))
    } finally {
      setBusy(false)
    }
  }

  async function cancel() {
    setError('')
    setBusy(true)
    try {
      await api.cancelHold(holdId, studentId)
      onDone('Application cancelled.')
    } catch (e) {
      setError(errorMessage(e))
    } finally {
      setBusy(false)
    }
  }

  const remaining = hold ? Math.max(0, new Date(hold.expiresAt).getTime() - now) : 0
  const minutes = String(Math.floor(remaining / 60000)).padStart(2, '0')
  const seconds = String(Math.floor((remaining % 60000) / 1000)).padStart(2, '0')
  const expired =
    hold?.status === 'EXPIRED' || (hold?.status === 'HELD' && remaining <= 0)
  const canAct = !busy && !!hold && hold.status === 'HELD' && !expired

  return (
    <div className="card">
      <div className="card-body">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="mb-0">Application</h5>
          {hold && <StatusBadge status={expired ? 'EXPIRED' : hold.status} />}
        </div>

        {error && <div className="alert alert-danger">{error}</div>}
        {!hold && !error && <p className="text-muted">Loading...</p>}

        {hold && (
          <>
            <p className="mb-1">
              <strong>Class:</strong> {className}
            </p>
            <p className="mb-3">
              <strong>Expires in:</strong>{' '}
              {expired ? (
                <span className="text-danger">time is up</span>
              ) : (
                <span className="font-monospace">
                  {minutes}:{seconds}
                </span>
              )}
            </p>

            <button className="btn btn-success me-2" disabled={!canAct} onClick={pay}>
              {busy ? 'Working...' : 'Pay admission fee'}
            </button>
            <button
              className="btn btn-outline-danger me-2"
              disabled={!canAct}
              onClick={cancel}
            >
              Cancel application
            </button>
            <button className="btn btn-link" onClick={() => onDone('')}>
              Back
            </button>
          </>
        )}
      </div>
    </div>
  )
}
