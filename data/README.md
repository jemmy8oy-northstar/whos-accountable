# data/: the real cohort, as code

Every number the product publishes about a real company comes from a file in this folder.
Each record carries the source URL and the date it was retrieved. A change to any of them is a
reviewable diff (DESIGN.md, "Data-as-code").

`cohort-v1.json` is in the shape `DatasetLoader` reads. The synthetic fixture in
`backend/Balenthiran.WhosAccountable.Tests/Fixtures/` stays as the engine's behavioural pin.

**`violationsCoverage`** lists the sectors whose enforcement source this file holds. Only a company
in one of them scores a clean 100 on violations when it has no record. Today that's only
`WaterUtilities`, from the EDM returns. Without it, every power generator with emissions but no
enforcement record scored a clean 100, and so a composite of 100: the top of the do-gooder board.
Add a sector when its enforcement register (EA, OPRED, Ofwat) lands as a slice.

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

## v1, slice 3: oil and gas, UK ETS verified emissions (2025)

**Cohort.** The 16 UK ETS account holders with the largest 2025 verified emissions from oil and
gas installations: crude oil extraction (NACE 610), natural gas extraction (620) and refining
(1920). Offshore platforms are regulated by OPRED, refineries by the EA and NRW. Each holder is
identified by the Companies House number of the account holder named in the registry. Two have
been renamed since the 2025 returns, so the name shows the current one with the former in
brackets. Apache Beryl I is an overseas company (`FC…`). Prax Lindsey Oil Refinery is in
liquidation; the refinery closed in 2025, and its figure is what it emitted before that.

**Why not the Carbon Majors dataset.** DATA-4 first named "UK-linked Carbon Majors" as this slice.
That dataset is InfluenceMap's, offered for non-commercial use only, and the licence audit
(DESIGN.md, issue #9) already rules out bulk use of InfluenceMap data in the composite. The UK ETS
registry is open data and covers the UK operations of the same producers: BP, Shell, Esso
(ExxonMobil), TotalEnergies' former UK arm, CNOOC and others. It measures what they emit in the UK,
not the emissions of the fuel they sell. That is narrower than Carbon Majors' scope 3 figure, and
it is the figure this site can publish.

**Source.** The same UK ETS compliance report as slice 2, retrieved 2026-11-05.

- Filter: `Account type` = `OPERATOR_HOLDING_ACCOUNT` and `NACE` in 610, 620, 1920.
- Figure: "Recorded emissions 2025", summed over the holder's installations in that filter, goes
  to `absoluteTco2e`. Ten installations have a blank 2025 cell (no return, mostly decommissioned
  platforms such as Brent Charlie, Thistle A and Leman 49/27A) and count as zero.

**Reconciled.** 113 installations across 48 holders emitted 20,548,140 tCO2e in 2025 under the
filter, and these 16 hold 16,741,576 t of it (81%). None of the 16 holds an installation outside
the filter, so each figure is the holder's whole 2025 UK ETS total. No holder is in slice 2.

**Intensity is unknown, as in slice 2.** No turnover was read for this slice. Revenue for an
operating subsidiary of a group is a transfer-pricing figure as much as a sales one, and the v1
scale saturates on real emitters anyway (BEH-SCORE-8). These companies are listed without an
emissions score or a composite until the scale is set on real data.

**No violations yet.** OPRED and EA enforcement records for these sites are a later source.
`OilAndGas` is not in `violationsCoverage`, so the violations pillar is unknown, not a clean 100.

| Company | CH number | Installations | 2025 tCO2e |
|---|---|--:|--:|
| Esso Petroleum Company, Limited | 00026538 | 2 | 3,084,685 |
| Valero Energy Ltd | 08566216 | 1 | 2,598,886 |
| Phillips 66 Limited | 00529086 | 1 | 2,114,915 |
| Essar Oil (UK) Limited | 07071400 | 1 | 1,751,168 |
| BP Exploration Operating Company Limited | 00305943 | 6 | 1,001,082 |
| Adura Operations Limited | 16172712 | 7 | 920,690 |
| NEO NEXT+ Energy Resources UK Limited (formerly NEO Energy Resources UK Limited) | 00825828 | 7 | 675,306 |
| NEO NEXT+ Energy E&P UK Limited (formerly TotalEnergies E&P UK Limited) | 00811900 | 3 | 661,110 |
| Prax Lindsey Oil Refinery Limited | 00564599 | 1 | 661,031 |
| Shell U.K. Limited | 00140141 | 7 | 626,627 |
| EnQuest Heather Limited | 02748866 | 7 | 599,021 |
| CNOOC Petroleum Europe Limited | 01051137 | 3 | 548,922 |
| Apache Beryl I Limited | FC005975 | 2 | 424,867 |
| CNR International (U.K.) Limited | 00813187 | 3 | 387,635 |
| Perenco UK Limited | 04653066 | 9 | 347,747 |
| Spirit Energy Production UK Limited | 03115179 | 2 | 337,884 |

Installations counts include those with a blank 2025 cell.

## v1, slice 4: heavy industry, UK ETS verified emissions (2025)

**Cohort.** The 16 UK ETS account holders with the largest 2025 verified emissions from heavy
industry: iron and steel (NACE 2410), cement (2351), lime (2352), basic chemicals (2011 to 2017),
glass (2311 to 2319), pulp and paper (1711, 1712), bricks (2332), plaster (2362) and other mineral
products (2399). They take the `Industrials` sector and the `heavy-industry` tag. As in the earlier
slices, each is identified by the Companies House number of the account holder named in the
registry.

**British Steel.** The registry names "British Steel Limited". In 2025 that was company 12303256.
On 16 July 2026 it was renamed "2026 Transferor Company Limited", and a new holding company
(17312541, incorporated 1 July 2026) took the British Steel name. The 2025 emissions belong to the
company that ran Scunthorpe in 2025, so this slice uses 12303256 and shows the current name with
the former one in brackets, as for TTEP. Its id is `british-steel`.

**Why not the FTSE 100 yet.** DATA-4 names the FTSE 100 as the slice after oil and gas. Measured
against the same registry, only 14 of the 100 constituents (as listed after the October 2026
changes) hold any UK ETS emissions. The largest are BP, Shell and SSE, whose operating
subsidiaries are already in slices 2 and 3. Banks, insurers and retailers hold none, because the
ETS covers installations, not offices or shops. A FTSE 100 slice needs a scope 1 and 2 source
(annual-report SECR disclosures), which is unstructured and not yet read. Heavy industry is the
largest open-data block of emissions not yet held.

**Source.** The same UK ETS compliance report as slices 2 and 3, retrieved 2026-11-05.

- Filter: `Account type` = `OPERATOR_HOLDING_ACCOUNT` and `NACE` in the codes above.
- Figure: "Recorded emissions 2025", summed over the holder's installations in that filter, goes
  to `absoluteTco2e`. One installation has a blank 2025 cell (CEMEX's South Ferriby works) and
  counts as zero.

**Reconciled.** 157 installations across 98 holders emitted 14,580,928 tCO2e in 2025 under the
filter, and these 16 hold 11,273,477 t of it (77%). One holder has an installation outside the
filter: Tata Steel's Corby tubeworks (NACE 2420, 15,779 t), which is not counted. No holder is in
slice 2 or 3.

**Intensity is unknown, and there are no violations yet, as in slice 3.** `Industrials` is not
in `violationsCoverage`, so these companies are listed without a composite.

| Company | CH number | Installations | 2025 tCO2e |
|---|---|--:|--:|
| 2026 Transferor Company Limited (formerly British Steel Limited) | 12303256 | 3 | 3,214,101 |
| Castle Cement Limited | 02182762 | 3 | 1,268,533 |
| Tarmac Cement Limited | 00066558 | 3 | 1,266,478 |
| Breedon Cement Limited | 08284549 | 1 | 892,364 |
| CEMEX UK Cement Limited | 00475212 | 2 | 863,814 |
| ExxonMobil Chemical Limited | 00867162 | 2 | 663,024 |
| Holcim UK Cement Limited | 09326237 | 1 | 598,499 |
| Buxton Lime Limited | 14245883 | 2 | 483,542 |
| INEOS Chemicals Grangemouth Limited | 06981897 | 1 | 375,054 |
| Ardagh Glass Limited | 00567801 | 4 | 341,183 |
| Tata Steel UK Limited | 02280000 | 5 | 288,882 |
| Cemcor Ltd | 09559778 | 1 | 270,575 |
| Singleton Birch Limited | 00009433 | 1 | 220,156 |
| Saint-Gobain Construction Products UK Limited | 00734396 | 5 | 186,351 |
| Steetley Dolomite Limited | 04071554 | 1 | 173,366 |
| Forterra Building Products Limited | 08960430 | 5 | 167,555 |

Installations counts include those with a blank 2025 cell.

## v1, slice 5: aircraft operators, UK ETS verified emissions (2025)

**Cohort.** The 16 UK ETS aircraft operators incorporated in the UK (England and Wales, Scotland
or Northern Ireland) with the largest 2025 verified emissions. They take the `Industrials` sector
and the `aviation` tag, so they are told apart from slice 4 by tag. Each is identified by the
Companies House number of the operator named in the registry.

**Why incorporated in the UK, not just registered here.** Slice 2 counts overseas companies with a
UK establishment (`FC…`), because their power stations are in the UK. Most large foreign airlines
are registered here too (Lufthansa, Air France, TAP, Swiss, Wizz Air Hungary), often for little
more than a sales office, and their UK ETS figure covers only their flights to and from the UK.
Counting them would make "UK-linked" mean "sells tickets here". So Ryanair DAC (1.97 Mt, the
largest operator), easyJet Europe and Wizz Air's Hungarian and Maltese companies are left out,
while their UK subsidiaries are in.

**What these numbers are, and are not.** The UK ETS covers flights within the UK and from the UK
to the EEA, Gibraltar and Switzerland. Long-haul flights are not in it. British Airways' figure is
therefore a fraction of its emissions, and Virgin Atlantic records 0 t and is not in the slice.
Each figure is what the scheme covers, and it is not comparable with the airline's total footprint.
A short-haul carrier will rank above a long-haul one that emits far more.

**Source.** The same UK ETS compliance report as slices 2 to 4, retrieved 2026-11-05.

- Filter: `Account type` = `AIRCRAFT_OPERATOR_HOLDING_ACCOUNT`. Each operator here holds one
  account.
- Figure: "Recorded emissions 2025" goes to `absoluteTco2e`. Cells reading `EXCLUDED` (an
  operator under the de minimis threshold that year) count as zero.

**Reconciled.** 636 aircraft operator accounts, 365 of them with emissions, recorded 8,591,412
tCO2e in 2025. These 16 hold 4,644,616 t of it (54%). No operator is in an earlier slice.

**DHL Air.** The registry names "DHL Air Ltd". Company 01671114 was renamed "DHL Air (UK) Limited"
on 30 May 2024. It is shown under the current name, with the former one in brackets.

**Intensity is unknown, and there are no violations yet, as in slices 3 and 4.** These companies
are listed without a composite.

| Company | CH number | 2025 tCO2e |
|---|---|--:|
| easyJet UK Limited | 10886978 | 1,785,360 |
| Jet2.com Limited | 02739537 | 827,054 |
| British Airways plc | 01777777 | 687,409 |
| TUI Airways Limited | 00444359 | 400,312 |
| BA Euroflyer Limited | 13734241 | 196,915 |
| BA CityFlyer Limited | 02571224 | 169,680 |
| Ryanair UK Limited | 01917579 | 150,293 |
| Wizz Air UK Limited | 10982241 | 129,944 |
| Loganair Limited | SC170072 | 104,142 |
| DHL Air (UK) Limited (formerly DHL Air Limited) | 01671114 | 65,081 |
| Emerald Airlines UK Limited | NI683576 | 31,190 |
| Offshore Helicopter Services UK Limited | 04278474 | 24,681 |
| West Atlantic UK Limited | 05059096 | 24,255 |
| Bristow Helicopters Limited | 00551102 | 23,728 |
| CHC Scotia Limited | 00936569 | 14,047 |
| NHV Helicopters Ltd | 09900216 | 10,525 |
