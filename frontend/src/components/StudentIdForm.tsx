import { FormEvent, useState } from 'react'

type Props = {
  onSaved: (studentId: string) => void
}

export const STUDENT_ID_KEY = 'studentId'

// The gate: a student cannot see anything until this is filled in.
export default function StudentIdForm({ onSaved }: Props) {
  const [input, setInput] = useState('')
  const [error, setError] = useState('')

  function submit(e: FormEvent) {
    e.preventDefault()
    const id = input.trim()

    if (!/^\d{17}$/.test(id)) {
      setError('Birth registration number must be exactly 17 digits.')
      return
    }

    localStorage.setItem(STUDENT_ID_KEY, id)
    onSaved(id)
  }

  return (
    <div className="card" style={{ maxWidth: 480 }}>
      <div className="card-body">
        <h5 className="card-title mb-3">Student portal</h5>
        <p className="text-muted">Enter your birth registration number to continue.</p>

        <form onSubmit={submit}>
          <label className="form-label">Birth Registration Number</label>
          <input
            className="form-control mb-2"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="17 digits"
            autoFocus
          />
          {error && <div className="alert alert-danger py-2">{error}</div>}
          <button className="btn btn-primary w-100" type="submit">
            Continue
          </button>
        </form>

        <a className="btn btn-link px-0 mt-2" href="/">
          Back
        </a>
      </div>
    </div>
  )
}
