export type Direction = 'worst' | 'best'

export interface PillarScore {
  pillar: string
  value: number | null
  weight: number
  contributed: boolean
  note: string
}

export interface LeaderboardEntry {
  rank: number
  companyId: string
  name: string
  sector: string
  composite: number | null
  coveredPillars: number
  totalPillars: number
  pillars: PillarScore[]
}

export async function fetchLeaderboard(
  direction: Direction,
  sector: string | null,
  signal?: AbortSignal,
): Promise<LeaderboardEntry[]> {
  const params = new URLSearchParams({ direction })
  if (sector) params.set('sector', sector)
  const res = await fetch(`/api/leaderboard?${params.toString()}`, { signal })
  if (!res.ok) throw new Error(`the leaderboard request failed (HTTP ${res.status})`)
  return (await res.json()) as LeaderboardEntry[]
}
