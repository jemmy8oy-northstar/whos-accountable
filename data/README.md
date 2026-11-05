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

## v1, slice 2: power generators, UK ETS verified emissions (2025)

**Cohort.** The 16 UK ETS account holders with the largest 2025 verified emissions from
installations classed as electricity generation (NACE 3511). Each is identified by the Companies
House number of the account holder named in the registry. Three are overseas companies
registered here for a UK establishment (`FC…`), one is an LLP (`OC…`) and two are Scottish
(`SC…`). Two have been renamed since the registry was written (TTEP, formerly EP).

**Source.** UK ETS Registry, *Compliance Report – Emissions and Surrenders*, published on the
registry's public reports page, section 5. Retrieved 2026-11-05.
<https://reports.view-emissions-trading-registry.service.gov.uk/ets-reports.html>

- File: `UK_ETS_Compliance_Report_Emissions_&_Surrenders_2026.xlsx`, sheet `Data`, one row per
  installation.
- Figure: "Recorded emissions 2025", summed over the account holder's installations, goes to
  `absoluteTco2e`. Blank cells are installations with no 2025 return, such as RWE's Seal Sands.

**Reconciled.** The whole file sums to 79,716,174 tCO2e for 2025 (248 cells `EXCLUDED`, 2
`N/A`). Holder accounts at NACE 35* installations hold 31,542,144 t across 154 holders, and these
16 hold 27,953,769 t of it (89%). Filter: `Account type` = `OPERATOR_HOLDING_ACCOUNT` and
`NACE` = 3511.

**Intensity is recorded as unknown, on purpose.** The emissions pillar scores tCO2e per £m of
revenue. Every company's latest turnover was read from its Companies House accounts. The result is
below, kept as evidence and not loaded into the engine, for two reasons:

1. **It saturates.** On the v1 scale of 200 t/£m, every one of the 16 scores between 0.00 and
   0.27 out of 100. That is the same problem SCORE-7 found for violations on the water data.
2. **It measures the contract, not the carbon.** A tolled plant (¹) is paid a fee by a trader
   who buys the gas and sells the power, so its revenue is a fraction of the energy it sells.
   Carrington at 33,292 t/£m and Uniper at 1,186 t/£m are both gas-fired. The 28× gap between
   them is a difference in revenue, not in efficiency.

Until the scale is set on real data, the pillar is unknown for these companies, and so is their
composite. DESIGN.md says missing data is never imputed to neutral.

| Company | CH number | Installations | 2025 tCO2e | Turnover (£m) | Accounts to | tCO2e/£m | v1 score at that intensity |
|---|---|--:|--:|--:|---|--:|--:|
| RWE Generation UK plc | 03892782 | 10 | 7,477,488 | 1,940 | 2025-12-31 | 3,854 | 0.00 |
| Uniper UK Limited | 02796628 | 7 | 3,999,118 | 3,372 | 2025-12-31 | 1,186 | 0.27 |
| Marchwood Power Limited ¹ | 04229146 | 1 | 1,769,145 | 63.1 | 2025-12-31 | 28,051 | 0.00 |
| West Burton B Limited ¹ | 13027787 | 1 | 1,665,374 | 325.7 | 2025-12-31 | 5,113 | 0.00 |
| VPI Immingham LLP | OC300980 | 1 | 1,580,880 | 961.8 | 2025-12-31 | 1,644 | 0.03 |
| Keadby Generation Limited | 02729513 | 1 | 1,549,883 | 542.8 | 2025-03-31 ² | 2,855 | 0.00 |
| Saltend Cogeneration Company Limited | 03274929 | 1 | 1,520,312 | 388.2 | 2025-12-31 | 3,916 | 0.00 |
| VPI Power Limited | SC189124 | 4 | 1,359,864 | 1,102.8 | 2025-12-31 | 1,233 | 0.21 |
| Carrington Power Limited ¹ | 04706728 | 1 | 1,148,464 | 34.5 | 2025-12-31 | 33,292 | 0.00 |
| TTEP SHB Ltd (formerly EP SHB Limited) | 02571241 | 1 | 1,135,353 | 360.9 | 2024-12-31 ² | 3,146 | 0.00 |
| Spalding Energy Company, Ltd. | FC019668 | 1 | 1,129,254 | 220.2 | 2025-12-31 | 5,129 | 0.00 |
| Seabank Power Limited ¹ | 02591188 | 1 | 969,285 | 250.8 | 2025-12-31 | 3,864 | 0.00 |
| Rocksavage Power Company, Ltd. | FC018868 | 1 | 716,350 | 165.2 | 2025-12-31 | 4,335 | 0.00 |
| TTEP Langage Power Ltd (formerly EP Langage Limited) | 03462783 | 1 | 700,264 | 213.4 | 2024-12-31 ² | 3,281 | 0.00 |
| SSE Thermal Generation (Scotland) Limited | SC664055 | 1 | 639,436 | 325.6 | 2025-03-31 ² | 1,964 | 0.01 |
| Coryton Energy Company, Ltd. | FC020597 | 1 | 593,299 | 145.4 | 2025-12-31 | 4,082 | 0.00 |

¹ Paid under a tolling agreement, according to its accounts. West Burton B's tolling began in
September 2025.
² The accounts don't cover calendar 2025. The year to March 2025 is SSE's year end, and FY2024
is the latest filed.

Turnover is from each company's latest full accounts on Companies House, filed as image-only
PDFs and read by OCR. Each figure was checked against its components or against gross profit.
The PDFs give statutory revenue, not "adjusted".

**What these numbers are, and are not.** Verified emissions are what an installation reported and
had verified under the scheme. They are not a breach, a fine or a finding. Biomass is zero-rated,
so Drax Power Station's 2025 figure is 62,949 t. That is why Drax is not in this slice, not
because its chimney emits little CO2. Biogenic emissions would need a separate source.
