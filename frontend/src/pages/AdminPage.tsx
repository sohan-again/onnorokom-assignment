import { FormEvent, useState } from 'react'
import { api, errorMessage } from '../api'
import ClassTable from '../components/ClassTable'
import { useClasses } from '../useClasses'

export default function AdminPage() {
  const { classes, error } = useClasses()
  const [name, setName] = useState('')
  const [seats, setSeats] = useState('120')
  const [actionError, setActionError] = useState('')
  const [busy, setBusy] = useState(false)

  async function createClass(e: FormEvent) {
    e.preventDefault()
    setActionError('')
    setBusy(true)
    try {
      await api.createClass(name, Number(seats))
      setName('')
      setSeats('120')
    } catch (e) {
      setActionError(errorMessage(e))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="container py-4" style={{ maxWidth: 900 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h3 className="mb-0">School admin</h3>
        <a className="btn btn-sm btn-outline-secondary" href="/">
          Home
        </a>
      </div>

      {(actionError || error) && (
        <div className="alert alert-danger">{actionError || error}</div>
      )}

      <div className="card card-body mb-4">
        <h5 className="card-title">New class</h5>
        <form className="row g-2 align-items-end" onSubmit={createClass}>
          <div className="col-md-7">
            <label className="form-label">Class name</label>
            <input
              className="form-control"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Class 6 - Morning Shift"
              required
            />
          </div>
          <div className="col-md-3">
            <label className="form-label">Total seats</label>
            <input
              type="number"
              min={1}
              className="form-control"
              value={seats}
              onChange={(e) => setSeats(e.target.value)}
              required
            />
          </div>
          <div className="col-md-2">
            <button className="btn btn-primary w-100" type="submit" disabled={busy}>
              {busy ? 'Creating...' : 'Create'}
            </button>
          </div>
        </form>
      </div>

      <h5>Classes</h5>
      <ClassTable classes={classes} />
    </div>
  )
}
