export default function LandingPage() {
  return (
    <div className="container py-5" style={{ maxWidth: 700 }}>
      <h3 className="mb-1">School Admission System</h3>
      <p className="text-muted mb-4">Choose where you want to go.</p>

      <div className="row g-3">
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-body">
              <h5 className="card-title">School admin</h5>
              <p className="card-text text-muted">
                Create classes and watch the seat counts.
              </p>
              <a className="btn btn-primary" href="/admin">
                Open admin
              </a>
            </div>
          </div>
        </div>

        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-body">
              <h5 className="card-title">Student</h5>
              <p className="card-text text-muted">
                Hold a seat and pay the admission fee.
              </p>
              <a className="btn btn-success" href="/student">
                Open student portal
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
