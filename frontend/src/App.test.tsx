import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import App from './App'
import type { LeaderboardEntry } from './api'

function entry(rank: number, name: string, sector: string, composite: number | null, covered = 3): LeaderboardEntry {
  return {
    rank,
    companyId: name.toLowerCase().replace(/\s+/g, '-'),
    name,
    sector,
    composite,
    coveredPillars: covered,
    totalPillars: 4,
    pillars: [],
  }
}

const ROWS = [
  entry(1, 'Alpha Water', 'Water', 91.25),
  entry(2, 'Beta Energy', 'Energy', 80),
  entry(3, 'Gamma Gas', 'Energy', null, 0),
]

function mockFetch(handler: (url: string) => Response | Promise<Response>) {
  const fn = vi.fn((input: RequestInfo | URL) => Promise.resolve(handler(String(input))))
  vi.stubGlobal('fetch', fn)
  return fn
}

const json = (body: unknown) => new Response(JSON.stringify(body), { status: 200 })

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <App />
    </MemoryRouter>,
  )
}

describe('leaderboard', () => {
  it('worst page renders rows in API order (BEH-LEADER-1)', async () => {
    const fetchFn = mockFetch(() => json(ROWS))
    renderAt('/')
    await screen.findByText('Alpha Water')
    const rows = screen.getAllByRole('row').slice(1)
    expect(rows.map((r) => within(r).getAllByRole('cell')[1].textContent)).toEqual([
      'Alpha Water',
      'Beta Energy',
      'Gamma Gas',
    ])
    expect(fetchFn.mock.calls[0][0]).toBe('/api/leaderboard?direction=worst')
    expect(screen.getByRole('heading', { name: 'Worst offenders' })).toBeInTheDocument()
    expect(screen.getAllByText('3 of 4 pillars')).toHaveLength(2)
  })

  it('best page requests direction=best (BEH-LEADER-2)', async () => {
    const fetchFn = mockFetch(() => json(ROWS))
    renderAt('/best')
    await screen.findByText('Alpha Water')
    expect(fetchFn.mock.calls[0][0]).toBe('/api/leaderboard?direction=best')
    expect(screen.getByRole('heading', { name: 'Top do-gooders' })).toBeInTheDocument()
  })

  it('null composite renders Unknown, never 0 or 50 (BEH-SCORE-4)', async () => {
    mockFetch(() => json([entry(1, 'Gamma Gas', 'Energy', null, 0)]))
    renderAt('/')
    await screen.findByText('Gamma Gas')
    expect(screen.getByText('Unknown')).toBeInTheDocument()
    expect(screen.getByText('0 of 4 pillars')).toBeInTheDocument()
    expect(screen.queryByText('0.0')).not.toBeInTheDocument()
    expect(screen.queryByText('50.0')).not.toBeInTheDocument()
  })

  it('sector filter re-requests with sector=... (BEH-LEADER-1)', async () => {
    const fetchFn = mockFetch((url) =>
      json(url.includes('sector=Energy') ? ROWS.filter((r) => r.sector === 'Energy') : ROWS),
    )
    renderAt('/')
    await screen.findByText('Alpha Water')
    const select = screen.getByLabelText('Sector')
    expect(within(select).getAllByRole('option').map((o) => o.textContent)).toEqual(['All', 'Energy', 'Water'])
    await userEvent.selectOptions(select, 'Energy')
    await vi.waitFor(() => expect(screen.queryByText('Alpha Water')).not.toBeInTheDocument())
    expect(fetchFn.mock.calls.at(-1)?.[0]).toBe('/api/leaderboard?direction=worst&sector=Energy')
    expect(screen.getByText('Beta Energy')).toBeInTheDocument()
    // Options persist after filtering so the user can switch back.
    expect(within(screen.getByLabelText('Sector')).getAllByRole('option')).toHaveLength(3)
  })

  it('error state names what failed (BEH-LEADER-1)', async () => {
    mockFetch(() => new Response('boom', { status: 500 }))
    renderAt('/best')
    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('Could not load the Top do-gooders leaderboard')
    expect(alert).toHaveTextContent('HTTP 500')
  })

  it('shows a loading state before data arrives (BEH-LEADER-1)', () => {
    mockFetch(() => new Promise<Response>(() => {}) as unknown as Response)
    renderAt('/')
    expect(screen.getByRole('status')).toHaveTextContent('Loading')
  })
})
