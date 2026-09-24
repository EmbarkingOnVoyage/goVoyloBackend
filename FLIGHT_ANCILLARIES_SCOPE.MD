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
  `Air_TempBooking`, and `Air_Ticketing` (Block_Ticket only — see below).
- `POST /api/v1/flights/bookings` (authenticated) creates a Flyshop temp booking across
  every leg, with each leg's selected `SSR_Key`s attached via `BookingSSRDetails`, then
  immediately places a **Block_Ticket hold** (`Ticketing_Type: "0"`) — a reversible hold
  releasable via `Air_ReleasePNR` (`POST /api/v1/flights/bookings/release`, see below), not
  a final purchase.
- **Not implemented, deliberately**: `Add_Payment` and `Ticketing_Type: "1"` (Book_Ticket).
  `Air_Ticketing`'s docs reveal the real flow is TempBooking → `Add_Payment` (which debits
  GoVoylo's own Flyshop *agency wallet balance* — a real settlement charge, not a sandbox
  echo) → Book_Ticket (an essentially final, non-trivially-reversible airline PNR). That's
  a materially bigger, real-money decision that was explicitly deferred pending separate
  authorization. Right now `POST /bookings` produces a hold only; nothing pays Flyshop and
  no final ticket is issued.
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

## Flyshop certification test matrix — coverage gaps

Flyshop's own test sheet (`01_Test Cases_For_APITesting.xls`) lists 5 booking scenarios
and 4 cancellation scenarios that need to pass for certification. Cancellation is now
implemented (above). Status of the booking-side scenarios:
- **GST details** (test case 5, "2ADT/1CHD/1INF With GST Details") — DONE. `CreateBooking`
  request/command now accept optional `GstNumber`/`GstHolderName`/`GstAddress`, threaded
  through to `Air_TempBooking`'s `GST`/`GST_Number`/`GST_HolderName`/`GST_Address` fields.
  Validator requires holder name + address once a GST number is supplied. Verified locally
  (clean build, validation and pipeline-passthrough checked via direct curl); not yet run
  against a real successful Flyshop response.
- **Special Fare** (test case 3, "Roundtrip Special Fare 1ADT/1CHD/1INF, connecting
  flight") — still open, deliberately out of scope for now. The mobile app's Special Fare
  UI is already known to be disconnected from the search request (see the separate Special
  Fare investigation earlier this engagement); the booking flow itself has no
  special-fare-specific handling either. Not started pending a separate scoping decision.
- Test cases 1, 2, 4 (plain Oneway/Roundtrip, SEAT & SSR, connecting flights) should
  already be coverable by what's built (`CreateBooking` + ancillary/seat selection), but
  haven't been run against real Flyshop data.

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

**Real purchase (separate, larger, explicitly not started)**
- `Add_Payment` (debits the real Flyshop agency wallet) and `Ticketing_Type: "1"`
  (Book_Ticket, an essentially final PNR) — needs its own explicit go-ahead given the
  real-money/hard-to-reverse nature, per the discussion that led to Block_Ticket-only.
- `Air_ReleasePNR` to actually cancel a Block_Ticket hold (currently holds are created
  but nothing in the app can release one yet).

**Everything above is unverified against a live Flyshop response** — build/auth/
validation/session-lookup were tested directly against the local backend, but
`uat.flyshop.in` has been unreachable this entire time.
