# data/: the real cohort, as code

Every number the product publishes about a real company comes from a file in this folder.
Each record carries the source URL and the date it was retrieved. A change to any of them is a
reviewable diff (DESIGN.md, "Data-as-code").

`cohort-v1.json` is in the shape `DatasetLoader` reads. The synthetic fixture in
`backend/Balenthiran.WhosAccountable.Tests/Fixtures/` stays as the engine's behavioural pin.

## v1, slice 1: English water and sewerage companies, storm-overflow spills (2025)

**Cohort.** The 10 water and sewerage companies that file an EDM annual return in England,
identified by the Companies House number of the licensed operating company, not its group.
Dŵr Cymru's figures cover only its overflows **in England** (122 of them). This is the first
slice of the ~150-company cohort (whos-accountable#13). Energy, Carbon Majors and the FTSE 100
come in later slices.

**Source.** Environment Agency, *Event Duration Monitoring – Storm Overflows – Annual Returns*,
2025 return. Open Government Licence.
<https://www.data.gov.uk/dataset/19f6064d-7356-466f-844e-d20ea10ae9fd/event-duration-monitoring-storm-overflows-annual-returns>

- File: `EDM_2025_Storm_Overflow_Annual_Return.zip`, revised in August 2026 after Anglian Water
  re-submitted. Retrieved 2026-10-30.
- Figures: *summary data* workbook, **Table 2: 2025 EDM Summary Statistics**. "Total number of
  monitored spill events" goes to `incidentCount`. "Total duration (hrs) of monitored spill
  events" goes to `dischargeHours`, rounded to 2 dp.

**Reconciled against the row-level data.** The per-company totals were recomputed from the
`All WaSC` sheet of the *all water and sewerage companies* workbook: 14,303 overflows in total,
summing "Counted spills using 12-24h count method" and "Total Duration". They match Table 2
exactly for all 10 companies, including the 292,119 spill events and 1,878,240.36 hours across
the industry. Two rows need handling to get there:

- One Dŵr Cymru row has a blank Unique ID: the storm water tank adjacent to Halls Factory,
  80 spills.
- One Wessex Water row (`WXW00498`, Hanging Langford) has its values shifted one column right
  from the treatment column onwards.

A future ingestion script (whos-accountable#7) must handle both.

**What these numbers are, and are not.** Storm overflows are *permitted* to discharge in
rainfall. An EDM spill is a recorded discharge, not by itself a breach or an enforcement
action. Each record's description says so. Fines and prosecutions (EA, Ofwat) are a separate
source and a later slice.

**One year only.** The engine sums every record it holds for a company, so adding 2024 next to
2025 would double-count. Year-on-year history waits until the engine scores per year.
