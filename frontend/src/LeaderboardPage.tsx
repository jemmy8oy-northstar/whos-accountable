import { useEffect, useState } from 'react'
import { fetchLeaderboard, type Direction, type LeaderboardEntry } from './api'

const TITLES: Record<Direction, string> = {
  worst: 'Worst offenders',
  best: 'Top do-gooders',
}

type State =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; entries: LeaderboardEntry[] }

function formatComposite(composite: number | null): string {
  // BEH-SCORE-4: a missing score is Unknown, never 0 or 50.
  return composite === null ? 'Unknown' : composite.toFixed(1)
}

export default function LeaderboardPage({ direction }: { direction: Direction }) {
  const [sector, setSector] = useState('')
  // Sectors seen so far: a filtered response only holds one sector, so the options accumulate.
  const [sectors, setSectors] = useState<string[]>([])
  // BEH-BACKLOG-1: countries accumulate the same way, and the filter and column only appear once
  // there is more than one to choose between.
  const [country, setCountry] = useState('')
  const [countries, setCountries] = useState<string[]>([])
  const [state, setState] = useState<State>({ status: 'loading' })
  const manyCountries = countries.length > 1

  useEffect(() => {
    const controller = new AbortController()
    fetchLeaderboard(direction, sector || null, country || null, controller.signal)
      .then((entries) => {
        setSectors((prev) => [...new Set([...prev, ...entries.map((e) => e.sector)])].sort())
        setCountries((prev) => [...new Set([...prev, ...entries.map((e) => e.country)])].sort())
        setState({ status: 'ready', entries })
      })
      .catch((err: unknown) => {
        if (controller.signal.aborted) return
        const reason = err instanceof Error ? err.message : 'unknown error'
        setState({ status: 'error', message: `Could not load the ${TITLES[direction]} leaderboard: ${reason}.` })
      })
    return () => controller.abort()
  }, [direction, sector, country])

  function onSectorChange(next: string) {
    setState({ status: 'loading' })
    setSector(next)
  }

  function onCountryChange(next: string) {
    setState({ status: 'loading' })
    setCountry(next)
  }

  return (
    <section>
      <h2>{TITLES[direction]}</h2>
      <label className="filter">
        Sector{' '}
        <select value={sector} onChange={(e) => onSectorChange(e.target.value)}>
          <option value="">All</option>
          {sectors.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </label>
      {manyCountries && (
        <label className="filter">
          Country{' '}
          <select value={country} onChange={(e) => onCountryChange(e.target.value)}>
            <option value="">All</option>
            {countries.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
        </label>
      )}

      {state.status === 'loading' && <p role="status">Loading…</p>}
      {state.status === 'error' && <p role="alert">{state.message}</p>}
      {state.status === 'ready' && state.entries.length === 0 && <p>No companies to show.</p>}
      {state.status === 'ready' && state.entries.length > 0 && (
        <table>
          <thead>
            <tr>
              <th scope="col">Rank</th>
              <th scope="col">Company</th>
              <th scope="col">Sector</th>
              {manyCountries && <th scope="col">Country</th>}
              <th scope="col">Score</th>
              <th scope="col">Coverage</th>
            </tr>
          </thead>
          <tbody>
            {state.entries.map((e) => (
              <tr key={e.companyId}>
                <td data-label="Rank">{e.rank}</td>
                <td data-label="Company">{e.name}</td>
                <td data-label="Sector">{e.sector}</td>
                {manyCountries && <td data-label="Country">{e.country}</td>}
                <td data-label="Score" className={e.composite === null ? 'unknown' : undefined}>
                  {formatComposite(e.composite)}
                </td>
                <td data-label="Coverage">
                  {e.coveredPillars} of {e.totalPillars} pillars
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
