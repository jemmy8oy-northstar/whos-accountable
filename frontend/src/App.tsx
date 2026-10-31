import { NavLink, Navigate, Route, Routes } from 'react-router-dom'
import LeaderboardPage from './LeaderboardPage'

export default function App() {
  return (
    <div className="app">
      <header>
        <h1>Who&apos;s Accountable</h1>
        <nav>
          <NavLink to="/" end>
            Worst offenders
          </NavLink>
          <NavLink to="/best">Top do-gooders</NavLink>
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<LeaderboardPage key="worst" direction="worst" />} />
          <Route path="/best" element={<LeaderboardPage key="best" direction="best" />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  )
}
