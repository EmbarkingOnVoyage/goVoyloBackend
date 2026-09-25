# Scope: Real Flyshop Ancillary Services (Baggage / Seat / Meal)

## Why

The mobile app's "Whats included" section (Traveler Details screen) currently shows
paid Baggage/Seat/Meal add-ons using fully static, hardcoded prices. Flyshop's API
does support real priced ancillaries — this document scopes the work to replace the
mock data with the real integration.

## Current state (updated)

**Done:**
- `WhatsIncludedSection.tsx` (frontend) fetches real priced Baggage/Meal options from
  `Air_GetSSR` and real seat pricing from `Air_GetSeatMap` (deduped into Standard/
  Preferred price tiers — not a visual seat-map picker) instead of static mock arrays.
  The "included" baggage cards (carry-on/checked-bag weight) read the real
  `FareOption.checkInBaggage`/`handBaggage` values.
- `FlyshopClient` implements `Air_Search`, `Air_Reprice`, `Air_GetSSR`, `Air_GetSeatMap`,
  `Air_TempBooking`, `Air_Ticketing` (both Block_Ticket and Book_Ticket — see below), and
  `AddPayment`.
- `POST /api/v1/flights/bookings` (authenticated) creates a Flyshop temp booking across
  every leg, with each leg's selected `SSR_Key`s attached via `BookingSSRDetails`, then
  immediately places a **Block_Ticket hold** (`Ticketing_Type: "0"`) — a reversible hold
  releasable via `Air_ReleasePNR` (`POST /api/v1/flights/bookings/release`, see below), not
  a final purchase. This step never moves money and is unchanged.
- **Now implemented**: `AddPayment` and `Ticketing_Type: "1"` (Book_Ticket), wired into
  `VerifyRazorpayPaymentCommandHandler`. Once the customer's Razorpay payment is verified,
  the handler looks up the `TripBooking` by the same reference (Flyshop's own
  `Booking_RefNo` — `TravelerDetailsScreen.handlePayNow` now passes that through to
  `CreateRazorpayOrder` instead of a client-generated id, so `BookingPayment` and
  `TripBooking` are always linked), and if it's still a Block_Ticket hold (`Status_Id 33`),
  calls `AddPaymentAsync` (debits GoVoylo's own Flyshop *agency wallet balance* — a real
  settlement charge, not a sandbox echo — via `RefNo`/`ProductId`/`TransactionType` against
  `tradehost/TradeAPIService.svc/JSONService/AddPayment`, a different Flyshop service host
  than every other call here) followed by `BookTicketAsync` (`Air_Ticketing` with
  `Ticketing_Type: "1"`), then updates the `TripBooking` with the real ticketed
  `Status_Id`/`Airline_PNR`/`Record_Locator`. Deliberately **not** wrapped in a
  swallow-and-continue try/catch like the best-effort email/persist paths elsewhere: the
  customer has already been charged via Razorpay by this point, so a Flyshop failure here
  surfaces as a real error rather than a false "booking confirmed" response — that failure
  mode (paid but unticketed) needs manual follow-up (refund or retry), which is not
  automated. `TradeBaseUrl` needs its own `FlyshopSettings__TradeBaseUrl` app setting
  wherever this runs (separate from the existing `FlyshopSettings__BaseUrl`) — not yet set
  on UAT as of this writing.
  Wire contract sourced directly from Flyshop's own "Client 2.6 Air" Postman collection
  (`documenter.getpostman.com/view/18594709/UVJfiFLA`, `AddPayment` section) via its
  underlying `documenter.gw.postman.com/api/collections/...` JSON — not guessed. Not yet
  verified end-to-end against a real ticketed booking (needs Flyshop wallet balance,
  confirmed present as of 2026-09-25, and one live Pay Now run to prove out).
- **Now wired up**: `TravelerDetailsScreen.handlePayNow` calls `POST /api/v1/flights/bookings`
  *before* the Razorpay create-order/checkout/verify flow — the flight is held with the
  supplier first, and the customer is only charged if that hold succeeds. On a hold
  failure, a clear error is shown and Razorpay never opens (verified live against a
  mock/expired offer). Selected baggage/seat/meal `SSR_Key`s are surfaced up from
  `WhatsIncludedSection` via a new `onSelectionsChange` callback and attached per
  traveller/leg. The trip's contact `PassengerMobile`/`PassengerEmail` come from the
  logged-in user's own profile (`useCustomerProfileMobile`), since saved travellers carry
  no contact info of their own — if the profile is missing either, Pay Now blocks with an
  error asking the user to complete their profile first, rather than sending an
  incomplete request. The success banner shows the booking reference and airline PNR
  when present. **Not yet verified**: an actual successful hold end-to-end, since that
  needs Flyshop reachable.
- When a Block_Ticket hold succeeds, `CreateBookingCommandHandler` emails the caller's
  *registered account email* (looked up via `ICurrentUserService`/`IUserRepository`, not
  `PassengerEmail`) the booking reference, airline PNR, and record locator, via a new
  `IEmailService.SendBookingConfirmationAsync`. Best-effort — a failed send never turns a
  successful hold into an error response. Not yet verified against a real send (needs a
  real Flyshop hold to trigger it, which needs Flyshop reachable).
- The `FlightBooking` domain entity is still an unused stub (no PNR/ticketing/SSR
  fields) — booking results from `POST /bookings` are returned to the caller but not
  yet persisted anywhere.
- `FlightOfferSession` (created at search time, see `SearchFlightsQueryHandler`) stores
  the real Flyshop `FlightKey` + `FareId` server-side, keyed by `offerId`.

- `Air_TicketCancellation` (the collection's sidebar shorthand calls it "Air_Cancellation")
  is implemented, exposed as authenticated `POST /api/v1/flights/bookings/cancel`.
  Supports `CancellationType` 0-Normal Cancel/1-Full Refund/2-No Show with per-segment
  `FlightId`/`PassengerId`/`SegmentId`, so a single leg of a multi-leg booking can be
  cancelled independently (e.g. Flyshop's own certification scenario "cancel return trip
  only"). Needed for Flyshop's API certification test matrix — see
  `01_Test Cases_For_APITesting.xls` provided by Flyshop.

**Verified live against `uat.flyshop.in` on 2026-09-24** (it came back up after being
unreachable for the entire body of work above). Real results, not mocked:
- `Air_Search` → `Air_Reprice` → `Air_GetSSR` → `Air_GetSeatMap` → `Air_FareRule`: all
  returned real IndiGo data for a DEL→BOM one-way search.
- `POST /api/v1/flights/bookings` (no GST): real Block_Ticket hold created —
  `BookingRefNo FLYB6ZOWY`, `Airline_PNR RP5WNY`, `Status_Id 33` (Blocked).
- `POST /api/v1/flights/bookings` (with GST): same, with `GstNumber`/`GstHolderName`/
  `GstAddress` populated — `BookingRefNo FLYB6ZOWZ`, `Airline_PNR BMQD9L`, `Status_Id 33`.
  Confirms the GST fix (test case 5, below) round-trips correctly against real Flyshop,
  not just local validation.
- Booking-confirmation email: the code path ran without throwing (account email looked
  up, `SendBookingConfirmationAsync` called), but the test account's email is on a
  `.test` domain (non-routable), so actual delivery could not be confirmed. Needs a real
  mailbox to verify end-to-end.
- **`Air_TicketCancellation` does not work on an un-ticketed Block_Ticket hold** — Flyshop
  returns `Error_Code 0016 / Err007: "Please check your RefNo Status"` even with the
  correct `FlightId`/`PassengerId`/`SegmentId` (looked up via `Air_History`, since our own
  responses don't currently surface `Flight_Id`). This looks like a genuine business rule
  (cancellation only applies to already-ticketed bookings), not a bug in our request
  shape — the request was accepted and processed, just rejected on business logic. This is
  worth flagging for the cancellation certification test cases, which may need an
  actually-ticketed booking to exercise `Air_TicketCancellation` meaningfully rather than
  a hold.
- **`Air_ReleasePNR` is now implemented** for releasing a Block_Ticket hold specifically —
  authenticated `POST /api/v1/flights/bookings/release` (`BookingRefNo` + `Airline_PNR`).
  Verified live: created a fresh hold, released it via this endpoint, confirmed success.
  The two test holds from the paragraph above (`FLYB6ZOWY`/`RP5WNY` and
  `FLYB6ZOWZ`/`BMQD9L`) were also released this way rather than left to self-expire.
- `Booking_Type: 2` (SPECIALROUNDTRIP), tested for a 2-leg round trip: confirmed it
  returns a genuinely different response shape than `Booking_Type: 1` — one combined
  itinerary per result (a single `Flight` object whose `Segments` bundle both outbound
  and return together) instead of two separate `TripDetails` per leg. Confirms this is
  real, separately-scoped work, not a simple flag flip — see the Special Fare gap below.

## Flyshop certification test matrix — results (run live on 2026-09-24/25)

Flyshop's own test sheet (`01_Test Cases_For_APITesting.xls`) lists 5 booking scenarios
and 4 cancellation scenarios. All bookings below were created as **Block_Ticket holds**
(`Ticketing_Type: 0`) against live `uat.flyshop.in`, then released via `Air_ReleasePNR`
once confirmed — no real purchase (`Book_Ticket`/`Add_Payment`) was made for any of them.

### Booking scenarios

| # | Scenario | Result | Evidence |
|---|---|---|---|
| 1 | Oneway, 1 ADT, direct flight | **PASS** | `FLYB6ZOXC` / `IZ8YSD` |
| 2 | Roundtrip, 2 ADT/2 CHD, with SEAT & SSR | **PARTIAL** — see below | `FLYB6ZOXS` / `CP226F` (SSR only) |
| 3 | Roundtrip Special Fare, 1 ADT/1 CHD/1 INF, connecting flight | **NOT STARTED** — separate scope, see below | — |
| 4 | Oneway, 2 ADT/1 CHD, SSR for one adult + the child | **PASS** | `FLYB6ZOXQ` / `RSSQ8Z` |
| 5 | Oneway, 2 ADT/1 CHD/1 INF, with GST details | **PASS** | `FLYB6ZOXR` / `AT5S2G` |

**Test case 2 detail — a real bug found and fixed, plus one still-open Flyshop-side issue:**
- The underlying roundtrip + 4-passenger (2 adults, 2 children) booking mechanics work
  correctly on their own (`FLYB6ZOXF` / `Q33IPK`, no SSR).
- Adding a **meal** SSR to that same booking also works correctly (`FLYB6ZOXS` /
  `CP226F` — the PASS result recorded above).
- Adding a **seat** SSR consistently fails — `Air_TempBooking` succeeds (accepts the
  seat `SSR_Key` fine), but `Air_Ticketing` then fails at the airline host itself:
  `"0009: Your ticket request failed with the airline... (Err002: This transaction is
  already Rejected/Deleted, Please check in transaction track)"`. Reproduced 3 times,
  including with a single adult and no other complexity (simplest possible case), so
  this isn't about passenger count or roundtrip specifically — it's specific to seat
  assignment.
  - Along the way, found and fixed a real bug: `CreateBookingCommandHandler` was
    re-repricing a leg immediately before `Air_TempBooking` even when
    `Air_GetSeatMap`/`Air_GetSSR` had already repriced it to obtain the SSR key,
    handing `Air_TempBooking` a different `Flight_Key` than the one the key was
    issued against. Fixed: a leg with SSR selections now reuses the session's
    current `Flight_Key`/`Fare_Id` instead of repricing again.
  - That fix did **not** resolve the seat-ticketing failure — it's reproducible
    identically before and after. This looks like a Flyshop UAT-side limitation
    specific to seat assignment during Block_Ticket, not a defect in our request
    shape (structurally accepted at every step up to the airline itself). **Needs
    Flyshop's input** — worth sending them the reproduction above (or a fresh one)
    directly, since it's not something fixable from our side without knowing what
    their sandbox expects differently for seats specifically.

**Test case 3 (Special Fare)** — deliberately not attempted. Confirmed earlier this
engagement that `Booking_Type: 2` (SPECIALROUNDTRIP) returns a genuinely different
response shape (one combined itinerary bundling both directions into a single `Flight`
object) than the per-leg model the rest of this integration is built around — real,
separately-scoped work, not a simple flag flip. Not started pending a scoping decision.

**GST + infant DOB fix**: test case 5 initially had no way to supply a traveler's date
of birth at all. `Air_Reprice`'s own `Required_PAX_Details` marks `DOB` mandatory for
infants — added `DateOfBirth` end-to-end (`BookingTravelerRequestDto` →
`CreateBookingCommand` → `Air_TempBooking`'s `PAX_Details.DOB`) and confirmed the fix
live with a real infant traveler.

### Cancellation scenarios — blocked, need a real ticketed booking

| # | Scenario | Status |
|---|---|---|
| 1 | Oneway, 1 ADT, CancellationType 0 (Normal Cancel) | **BLOCKED** |
| 2 | Oneway, 1 ADT, CancellationType 1 (Full Refund) | **BLOCKED** |
| 3 | Oneway, 1 ADT, CancellationType 2 (No Show) | **BLOCKED** |
| 4 | Roundtrip, 1 ADT, cancel return leg only, CancellationType 0 | **BLOCKED** |

All four need `Air_TicketCancellation` to run against a genuinely **ticketed** booking
(`Status_Id 11`) — confirmed live earlier that it rejects an un-ticketed Block_Ticket
hold outright (`Err007: "Please check your RefNo Status"`). A ticketed booking only
exists after `Air_Ticketing` with `Ticketing_Type: 1` (Book_Ticket), which requires
`Add_Payment` first — a real debit against GoVoylo's Flyshop wallet balance and an
essentially final airline PNR. That authorization hasn't been given, so these four
remain unexecuted. The code path itself is implemented and ready
(`POST /api/v1/flights/bookings/cancel` already supports arbitrary `CancellationType`
and per-segment targeting, which is exactly what test case 4 needs for "return leg
only") — it just has nothing valid to run against yet.

Two ways to unblock, either is fine:
1. Ask Flyshop whether they can supply pre-ticketed sandbox PNRs for certification,
   so cancellation can be exercised without a real purchase.
2. Explicitly authorize one real `Book_Ticket` + `Add_Payment` purely to produce a
   ticketed booking to cancel.

## Flyshop APIs to integrate

Source: Flyshop "Client 2.6 Air" Postman collection.

### `Air_GetSSR`
- Requires a `Flight_Key` — per the docs, specifically "the Flight_Key from the
  Air_Reprice API method response."
- Returns priced SSRs (Special Service Requests) for the flight. Each SSR detail has:
  `SSR_Type` (0-BAGGAGE / 1-MEALS / 2-COMPLIMENTARY_MEALS / 3-SEAT / 4-SPORTS /
  5-BAGOUTFIRST / 6-LOUNGE / 7-CELEBRATION / 8-CARRYMORE / 9-FASTFORWARD /
  10-WHEELCHAIR / 11-FREQUENTFLYER / 15-OTHERS), `SSR_TypeName`, `SSR_TypeDesc`,
  `SegmentWise`, `LegIndex`, `Segment_Id`, `FlightID`, `CurrencyCode`, `TotalAmount`,
  `ApplicablePaxTypes` (ADT/CHD/INF), `SSR_Key`, `SSR_Status`.

### `Air_GetSeatMap`
- Requires `Flight_Key`(s) plus `PAXDetails` (Pax_Id, Pax_type, Title, FirstName,
  LastName, Gender, Age, DOB, passport fields, frequent-flyer details).
- Returns a seat map: `SeatSegments` (by `LegIndex`) -> `SeatRow` -> `SeatDetails`, each
  an SSR detail (`SSR_Type` = 3-SEAT) with real `CurrencyCode`/`TotalAmount` and
  `SSR_Status` (0-ISLE / 1-AVAILABLE / 2-BLOCKED / 3-BOOKED). `0-ISLE` means the cell is
  an aisle gap, not a purchasable seat.

Both endpoints return an `SSR_Key` per option; `Air_TempBooking` does accept these
directly (see resolved question #1 below).

## Open questions

1. ~~Does `Air_TempBooking` accept SSR selections directly?~~ **Resolved**: yes, via
   `BookingFlightDetails[].BookingSSRDetails[]` (`Pax_Id` + `SSR_Key` pairs), one list per
   leg. Already wired into `POST /api/v1/flights/bookings`.
2. ~~Does `Air_GetSSR`/`Air_GetSeatMap` need a fresh `Air_Reprice` first?~~ **Resolved**:
   yes — implemented that way (every ancillary/seatmap/booking call reprices first to get
   a fresh `Flight_Key`, matching the docs' explicit wording).
3. ~~Does `Air_GetSeatMap` need full PAX details or work with minimal data?~~
   **Resolved**: the collection's own sample sends only Title/First_Name/Last_Name/Gender
   with everything else (Age/DOB/passport) null — minimal traveler data is sufficient.
4. Whether SSR prices are per-traveler-per-leg or journey-wide when `SegmentWise` is
   false, and how currency/rounding should be handled — **still open**, not hit in
   practice since Flyshop has been unreachable for live testing.
5. **New**: does `Air_TempBooking`/`Air_Ticketing` succeeding against UAT have any real
   financial or booking-system consequence (real wallet debit, a booking that appears on
   Flyshop's own books, needing manual cleanup)? Not confirmed by Flyshop in writing —
   this is exactly why Ticketing_Type is hardcoded to Block_Ticket (reversible hold) and
   `Add_Payment`/Book_Ticket were left out.

## Remaining work

**Wire the booking hold into the payment flow**
- Decide *when* `POST /api/v1/flights/bookings` gets called relative to the Razorpay
  charge in `TravelerDetailsScreen.handlePayNow` — before payment (hold the seat first,
  then charge) or after (charge first, then hold)? What happens on partial failure
  (payment succeeds but the Block_Ticket hold fails, or vice versa)? This is a product
  decision, not yet made.
- Give `FlightBooking` real fields (PNR, `BookingRefNo`, hold status, selected SSRs) and
  actually persist `POST /bookings`' result instead of just returning it to the caller.
- Surface the booking reference / hold status to the user on the success screen.

**Real purchase — now implemented, see "Now implemented" above.** `Add_Payment` +
`Ticketing_Type: "1"` (Book_Ticket) run automatically after a verified Razorpay payment.
`Air_ReleasePNR` (releasing a Block_Ticket hold) was already implemented earlier and is
exposed via `POST /api/v1/flights/bookings/release` / My Trips' "Release hold" action.

**Everything above is unverified against a live Flyshop response** — build/auth/
validation/session-lookup were tested directly against the local backend, but
`uat.flyshop.in` has been unreachable this entire time.
