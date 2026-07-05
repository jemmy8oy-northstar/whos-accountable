# WhosAccountable — Design

_Environmental accountability leaderboards: who's actually doing the most for
and against the environment. Idea: issue #1. Scope decisions from James
(2026-07-05, on #1): **UK-first**, **companies only in v1** (individuals
parked), **read-only v1** (no accounts), public-sentiment layer backlogged._

## The point

Individual behaviour change is dwarfed by corporate impact, yet the public
conversation mostly polices individuals. The app makes the asymmetry visible
and navigable: a **worst-offenders leaderboard** and a **top do-gooders
leaderboard**, every rank backed by evidence you can click through to.

Credibility is the entire product. One wrong or unsourced number kills it.
That drives every design decision below.

## v1 in one sentence

A read-only reference site ranking a fixed cohort of ~150 UK-linked companies
by a published, reproducible environmental score, where every input links to
a public source.

## Cohort (v1)

- **FTSE 100** (the companies people recognise), plus
- **UK water & energy utilities** (the sector with the most vivid, public,
  UK-specific accountability data — sewage discharges, EA fines), plus
- **UK-linked Carbon Majors** (Shell, BP, etc.).

Fixed list, versioned in-repo. Expansion (US → EU → global) is a backlog
issue, not a v1 concern.

## Data sources — published datasets only, no crowd input

Chosen for: public availability, UK coverage, structured format, and being
defensible ("their disclosed record, scored by a published formula" — not
"the internet voted them evil"). Defamation exposure is materially lower for
methodology-scored public data than for consensus rankings; this was the
deciding argument against starting with a voting/consensus algorithm (#1
discussion).

| Pillar | Source | What it gives | Access |
|---|---|---|---|
| Emissions | UK ETS registry; SECR disclosures in annual reports; CDP responses | Verified absolute emissions; intensity | ETS: open data. SECR: unstructured (defer full extraction). CDP: scores public per company, bulk data licensed — **license audit task** |
| Trajectory | SBTi target dashboard; company net-zero commitments | Has a real target? Validated? On track? | Public, scrapeable; ToS check needed |
| Violations | EA enforcement/prosecution records; Ofwat penalties; sewage Event Duration Monitoring data | Fines £, incident counts, discharge hours | Open government data (OGL) — the strongest, most UK-specific pillar |
| Obstruction | InfluenceMap climate lobbying grades | Lobbying for/against climate policy | Public grades; attribution/licensing check needed |

**Do-gooder side** is driven by *verified delivery*, not pledges: validated
science-based targets **plus** actual year-on-year reductions, renewables
build-out, absence of violations. Pledges alone score nothing — that's the
anti-greenwash stance, stated on the methodology page.

## Scoring

- Composite 0–100 with four **pillar sub-scores** (emissions, trajectory,
  violations, obstruction), weights published as constants in the repo.
- Every pillar score must be reproducible from the versioned input data by
  code in the repo — no analyst judgment calls hiding in numbers.
- Missing data is shown as *unknown*, never imputed to neutral — a company
  that discloses nothing should look like it discloses nothing (and gets a
  visible "opacity" penalty on the trajectory pillar).
- **Methodology page is a first-class feature**: formula, weights, source
  list, update cadence, correction policy, and a change-log.

## Data-as-code (the load-bearing bold assumption)

All input datasets live **versioned in the repo** (JSON, one file per source,
with retrieval date + source URL in the file). An offline ingestion step
(scripts, run by the bot) refreshes them via PR — so **every score change in
the product is a reviewable, attributable diff**, and the site itself stays a
dumb, cacheable read model. No live third-party calls at request time, no
ingestion infra in v1. If datasets outgrow the repo, that's a good problem —
revisit then (likely: object storage + manifest hashes in-repo).

Corrections policy: a company disputing a number can be pointed at the exact
source file + formula. Fixes are PRs with history. This is the credibility
story *and* the ops story in one mechanism.

## Architecture

web-template stack, same as language-vocab (.NET 10 API + React 19, helm,
oke-fleet deploy). App-specific:

- `Company` (id, name, sector, cohort tags, identifiers: LEI/Companies House no.)
- `SourceDataset` (source, version, retrievedAt, url) + per-source record types
- `PillarScore` / `CompositeScore` (companyId, pillar, value, datasetVersions[])
  — scores are **computed at ingestion time and stored**, not on request
- `ScoreEngine` service: pure function (datasets → scores). Dense unit tests;
  golden-file test pins the full leaderboard so any data/formula change shows
  up as an explicit test diff.
- Frontend: leaderboard (both directions, sector filter), company detail
  (pillar breakdown, evidence links, sparkline history), methodology page.

## Explicitly deferred (backlog issues to open)

1. Geographic expansion: UK+US → EU → global.
2. Individuals (billionaire footprints — private-jet studies, university
   datasets; needs its own defamation review before any build).
3. Public sentiment layer — clearly-labelled *opinion* signal, kept separate
   from the evidence-based score; brigading defences needed.
4. Automated ingestion on a schedule (v1 refreshes are bot-run PRs).
5. SECR full-text extraction from annual reports (LLM-assisted, verifiable).

## Delivery plan (when James green-lights the build)

- **PR 1**: scaffold from web-template + this doc + cohort list + source
  license audit results.
- **PR 2**: dataset schemas + ingestion scripts for the two open-data pillars
  (ETS emissions, EA/Ofwat violations incl. sewage EDM) + ScoreEngine with
  those pillars + golden tests.
- **PR 3**: trajectory + obstruction pillars (subject to license audit) +
  composite score.
- **PR 4**: frontend — leaderboards, company page, methodology page.
- Then: oke-fleet deploy PR.

**Before PR 2**: license/ToS audit of CDP, SBTi, InfluenceMap (the open-gov
sources are fine). If any are restricted, v1 ships on ETS + violations alone
— still a defensible, fully-open-data product.

## Open questions for James (non-blocking, answer on #1)

1. Name/branding: "WhosAccountable" as-is? Domain plans, or hang it off
   balenthiran.co.uk like the other apps?
2. Comfort check on the anti-greenwash stance ("pledges score nothing") —
   it's opinionated and will be visible.
3. v1 cohort of ~150 ok, or start even smaller (e.g. water companies only —
   maximum topicality, minimum surface)?
