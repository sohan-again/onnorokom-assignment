import type { ClassItem } from '../api'

type Props = {
  classes: ClassItem[]
  // Admin gets a read-only table, students get the Apply button.
  onApply?: (c: ClassItem) => void
  busyClassId?: string
}

export default function ClassTable({ classes, onApply, busyClassId }: Props) {
  return (
    <table className="table table-sm align-middle">
      <thead>
        <tr>
          <th>Class</th>
          <th>Total</th>
          <th>Held</th>
          <th>Confirmed</th>
          <th>Available</th>
          {onApply && <th></th>}
        </tr>
      </thead>
      <tbody>
        {classes.map((c) => (
          <tr key={c.id}>
            <td>{c.name}</td>
            <td>{c.totalSeats}</td>
            <td>{c.heldSeats}</td>
            <td>{c.confirmedSeats}</td>
            <td>{c.availableSeats}</td>
            {onApply && (
              <td className="text-end">
                <button
                  className="btn btn-sm btn-success"
                  disabled={c.availableSeats <= 0 || busyClassId === c.id}
                  onClick={() => onApply(c)}
                >
                  {busyClassId === c.id ? 'Applying...' : 'Apply'}
                </button>
              </td>
            )}
          </tr>
        ))}
        {classes.length === 0 && (
          <tr>
            <td colSpan={onApply ? 6 : 5} className="text-muted">
              No classes yet.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  )
}
