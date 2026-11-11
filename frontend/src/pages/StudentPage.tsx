import { useState } from 'react'
import { api, errorMessage, type ClassItem } from '../api'
import ClassTable from '../components/ClassTable'
import StudentIdForm, { STUDENT_ID_KEY } from '../components/StudentIdForm'
import ApplicationView from '../views/ApplicationView'
import MyHoldsView from '../views/MyHoldsView'
import { useClasses } from '../useClasses'

type ActiveHold = { holdId: string; className: string }

export default function StudentPage() {
  const [studentId, setStudentId] = useState(
    () => localStorage.getItem(STUDENT_ID_KEY) ?? '',
  )

  function changeId() {
    localStorage.removeItem(STUDENT_ID_KEY)
    setStudentId('')
  }

  // No ID yet: show the gate and nothing else.
  if (!studentId) {
    return (
      <div className="container py-5">
        <StudentIdForm onSaved={setStudentId} />
      </div>
    )
  }

  return <StudentPortal studentId={studentId} onChangeId={changeId} />
}

function StudentPortal({
  studentId,
  onChangeId,
}: {
  studentId: string
  onChangeId: () => void
}) {
  const { classes, error } = useClasses()
  const [tab, setTab] = useState<'classes' | 'mine'>('classes')
  const [activeHold, setActiveHold] = useState<ActiveHold | null>(null)
  const [notice, setNotice] = useState('')
  const [actionError, setActionError] = useState('')
  const [busyClassId, setBusyClassId] = useState('')

  async function apply(c: ClassItem) {
    setActionError('')
    setBusyClassId(c.id)
    try {
      const hold = await api.createHold(c.id, studentId)
      setActiveHold({ holdId: hold.holdId, className: c.name })
    } catch (e) {
      setActionError(errorMessage(e))
    } finally {
      setBusyClassId('')
    }
  }

  function finishAction(message: string) {
    setActiveHold(null)
    setNotice(message)
    setTab('mine')
  }

  return (
    <div className="container py-4" style={{ maxWidth: 900 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h3 className="mb-0">Student portal</h3>
        <div className="d-flex align-items-center gap-2">
          <span className="text-muted small">ID: {studentId}</span>
          <button className="btn btn-sm btn-outline-secondary" onClick={onChangeId}>
            Change
          </button>
          <a className="btn btn-sm btn-outline-secondary" href="/">
            Home
          </a>
        </div>
      </div>

      {notice && <div className="alert alert-success">{notice}</div>}
      {(actionError || error) && (
        <div className="alert alert-danger">{actionError || error}</div>
      )}

      {activeHold ? (
        <ApplicationView
          holdId={activeHold.holdId}
          className={activeHold.className}
          studentId={studentId}
          onDone={finishAction}
        />
      ) : (
        <>
          <ul className="nav nav-tabs mb-3">
            <li className="nav-item">
              <button
                className={'nav-link ' + (tab === 'classes' ? 'active' : '')}
                onClick={() => setTab('classes')}
              >
                Classes
              </button>
            </li>
            <li className="nav-item">
              <button
                className={'nav-link ' + (tab === 'mine' ? 'active' : '')}
                onClick={() => setTab('mine')}
              >
                My Applications
              </button>
            </li>
          </ul>

          {tab === 'classes' ? (
            <ClassTable classes={classes} onApply={apply} busyClassId={busyClassId} />
          ) : (
            <MyHoldsView studentId={studentId} onOpen={setActiveHold} />
          )}
        </>
      )}
    </div>
  )
}
