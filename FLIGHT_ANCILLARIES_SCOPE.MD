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
  cancellable via `Air_ReleasePNR` (not yet implemented), not a final purchase.
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

**None of this has been verified against a live Flyshop response** — `uat.flyshop.in`
has been unreachable (connection timeout) for this entire body of work. Everything above
is verified structurally (builds clean, auth/validation/session-lookup tested against the
local backend directly) but not against a real successful Flyshop response.

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
