# Scope: Real Flyshop Ancillary Services (Baggage / Seat / Meal)

## Why

The mobile app's "Whats included" section (Traveler Details screen) currently shows
paid Baggage/Seat/Meal add-ons using fully static, hardcoded prices. Flyshop's API
does support real priced ancillaries — this document scopes the work to replace the
mock data with the real integration.

## Current state

- `WhatsIncludedSection.tsx` (frontend) uses static mock arrays for paid Baggage/Seat/Meal
  options. The "included" baggage cards (carry-on/checked-bag weight) were fixed to read
  the real `FareOption.checkInBaggage`/`handBaggage` values already returned by search —
  that part is done.
- **No real airline booking exists yet.** `Air_TempBooking` and `Air_Ticketing` are not
  implemented anywhere in `GoVoylo.Infrastructure`. "Pay Now" only captures a Razorpay
  payment; it never creates a PNR with Flyshop. The `FlightBooking` domain entity exists
  but is an unused stub (no PNR/ticketing fields, never constructed in the payment path).
- `FlyshopClient` currently implements `Air_Search` and `Air_Reprice` only.
- `FlightOfferSession` (created at search time, see `SearchFlightsQueryHandler`) already
  stores the real Flyshop `FlightKey` + `FareId` server-side, keyed by `offerId` — so the
  backend can resolve an offer back to Flyshop identifiers without new session plumbing.

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

Both endpoints return an `SSR_Key` per option, which is presumably what attaches an
ancillary selection to a booking — **not yet confirmed** (see open questions).

## Open questions (need answers before/during implementation)

1. Does `Air_TempBooking` accept SSR selections directly in its request body, or is
   there a separate call to attach SSRs to a temp booking afterward? The `06 -
   Air_TempBooking` section of the Postman docs has not been fully reviewed yet.
2. Is the `Flight_Key` from `Air_Search` (already stored in `FlightOfferSession`) valid
   as-is for `Air_GetSSR`/`Air_GetSeatMap`, or does it genuinely require a fresh
   `Air_Reprice` call first, as the docs' wording for `Air_GetSSR` suggests? If reprice
   is mandatory, every visit to Traveler Details needs a reprice round-trip before
   ancillaries can be fetched.
3. `Air_GetSeatMap` requires `PAXDetails` — does it return a usable seat map with
   placeholder/generic PAX data before travelers are finalized, or does it only work
   once real traveler details are known? This affects UI ordering: the seat map may need
   to load only *after* "Add travellers," not eagerly alongside Baggage/Meal.
4. Whether SSR prices are per-traveler-per-leg or journey-wide when `SegmentWise` is
   false, and how currency/rounding should be handled.

## Prerequisite (blocking)

Real `Air_TempBooking` (and ideally `Air_Ticketing`) needs to exist before ancillary
selections mean anything — otherwise the app would be collecting SSR choices with
nowhere to send them. This should be scoped and built either as a prerequisite story or
as Phase 0 of this work.

## Proposed phasing

**Phase 0 — Booking foundation (prerequisite)**
- Implement `Air_TempBooking` in `FlyshopClient`, wire it into the payment flow so
  "Pay Now" actually creates a real PNR, and give `FlightBooking` real fields (PNR,
  ticketing status, etc.) instead of leaving it an unused stub.

**Phase 1 — Backend ancillary APIs**
- `FlyshopClient`: add `GetSsrAsync(flightKey)` and
  `GetSeatMapAsync(flightKeys, paxDetails)`.
- New DTOs (`AncillaryOptionDto`, `SeatMapDto`) and Application-layer queries
  (`GetFlightAncillaries`, `GetSeatMap`) that resolve `offerId` -> stored
  `FlightKey`/`FareId` -> reprice (if required by open question #2) -> SSR/seat-map call.
- New controller endpoints, e.g. `GET /api/v1/flights/{offerId}/ancillaries`,
  `POST /api/v1/flights/{offerId}/seatmap` (POST, since it needs traveler PAX data in
  the request body).

**Phase 2 — Frontend integration**
- Replace `BAGGAGE_PAID_OPTIONS` / `SEAT_PAID_OPTIONS` / `MEAL_PAID_OPTIONS` in
  `WhatsIncludedSection.tsx` (frontend repo) with real API-fetched, per-leg,
  per-traveler options.
- Track selected `SSR_Key`(s) per traveler/leg instead of a local mock option id.
- Update the running-total calculation to sum real `TotalAmount` values from selected
  SSRs (the existing total/Save logic stays structurally the same — only the option
  data source changes).

**Phase 3 — Attach to booking**
- Pass selected `SSR_Key`(s) into the `Air_TempBooking` request (pending answer to open
  question #1) so they are actually booked, not just priced and discarded.
