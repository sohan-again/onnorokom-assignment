import LandingPage from './pages/LandingPage'
import AdminPage from './pages/AdminPage'
import StudentPage from './pages/StudentPage'

// Two portals, two URLs. No router needed: nginx (and the Vite dev server)
// serve index.html for any path, so we just read the path once.
export default function App() {
  const path = window.location.pathname

  if (path.startsWith('/admin')) return <AdminPage />
  if (path.startsWith('/student')) return <StudentPage />
  return <LandingPage />
}
