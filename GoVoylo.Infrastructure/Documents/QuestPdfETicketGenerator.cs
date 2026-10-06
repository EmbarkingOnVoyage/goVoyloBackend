using System.Globalization;
using System.Text.Json;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;

namespace GoVoylo.Infrastructure.Documents
{
    // The e-ticket / invoice PDF, laid out after the Figma "Ticket Design" frames
    // (Domestic/International × One Way / Round Trip / Multi-City). The frames are
    // 760px wide; sizes here are those scaled to the A4 content width (~0.71).
    // QuestPDF runs under its Community licence.
    public class QuestPdfETicketGenerator : IETicketPdfGenerator
    {
        private const string Ink = "#182339";
        private const string Body = "#3E4B64";
        private const string Muted = "#6B7891";
        private const string Border = "#D9E1EC";
        private const string CardBorder = "#C9D3E1";
        private const string Blue = "#1D5BD8";
        private const string BlueBorder = "#8FA9F2";
        private const string BlueSoft = "#EEF2FE";
        private const string Green = "#15803D";
        private const string GreenSoft = "#E7F8EE";
        private const string Orange = "#C2580C";
        private const string OrangeBorder = "#F1A766";
        private const string OrangeSoft = "#FFF4EA";
        private const string HeaderGrey = "#EEF1F5";
        private const string Tagline = "#F5A100";

        private static readonly CultureInfo India = CultureInfo.GetCultureInfo("en-IN");
        private static readonly TimeSpan IndiaOffset = TimeSpan.FromHours(5.5);
        private static readonly Lazy<string?> LogoSvg = new(() => LoadResource("govoylo-logo.svg"));
        private static readonly Lazy<Dictionary<string, string>> AirlineLogos = new(LoadAirlineLogos);

        // Lucide icons (ISC licence) — the icon set the app itself uses.
        private const string PhoneIcon =
            "<path d=\"M13.832 16.568a1 1 0 0 0 1.213-.303l.355-.465A2 2 0 0 1 17 15h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2A18 18 0 0 1 2 4a2 2 0 0 1 2-2h3a2 2 0 0 1 2 2v3a2 2 0 0 1-.8 1.6l-.468.351a1 1 0 0 0-.292 1.233 14 14 0 0 0 6.392 6.384\"/>";
        private const string ShieldAlertIcon =
            "<path d=\"M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z\"/><path d=\"M12 8v4\"/><path d=\"M12 16h.01\"/>";
        private const string PlaneIcon =
            "<path d=\"M17.8 19.2 16 11l3.5-3.5C21 6 21.5 4 21 3c-1-.5-3 0-4.5 1.5L13 8 4.8 6.2c-.5-.1-.9.1-1.1.5l-.3.5c-.2.5-.1 1 .3 1.3L9 12l-2 3H4l-1 1 3 2 2 3 1-1v-3l3-2 3.5 5.3c.3.4.8.5 1.3.3l.5-.2c.4-.3.6-.7.5-1.2z\"/>";

        private readonly ETicketSettings _settings;

        static QuestPdfETicketGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public QuestPdfETicketGenerator(IOptions<ETicketSettings> settings)
        {
            _settings = settings.Value;
        }

        public byte[] Generate(ETicketDocumentDto ticket) =>
            Document.Create(document => document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(24);
                page.MarginVertical(22);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(9).FontColor(Ink));

                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Element(c => Header(c, ticket));
                    column.Item().Element(c => DangerousGoodsNotice(c, ticket.IsMultiCity));
                    if (ticket.IsMultiCity)
                    {
                        column.Item().Element(c => RouteStrip(c, ticket));
                    }
                    for (var i = 0; i < ticket.Trips.Count; i++)
                    {
                        var trip = ticket.Trips[i];
                        var index = i;
                        column.Item().ShowEntire().Column(block =>
                        {
                            if (ticket.IsMultiCity)
                            {
                                block.Item().PaddingBottom(8).Element(c => FlightOfLabel(c, trip, index, ticket.Trips.Count));
                            }
                            block.Item().Element(c => TripCard(c, trip, ticket.IsMultiCity));
                        });
                    }
                    // Starts on a new page rather than leave its title stranded at the bottom.
                    column.Item().EnsureSpace(230).Element(c => Travellers(c, ticket));
                    if (ticket.Baggage.Count > 0)
                    {
                        column.Item().ShowEntire().Element(c => BaggagePolicy(c, ticket));
                    }
                    column.Item().ShowEntire().Element(c => PaymentSummary(c, ticket));
                    column.Item().ShowEntire().Element(c => SharedOn(c, ticket));
                    column.Item().ShowEntire().Element(ImportantInformation);
                });

                page.Footer().PaddingTop(8).Column(footer =>
                {
                    footer.Item().LineHorizontal(0.75f).LineColor(Border);
                    footer.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("This is an electronically generated e-ticket and does not require a signature.")
                            .FontSize(7).FontColor(Muted);
                        row.AutoItem().Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontSize(7).FontColor(Muted));
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });
                });
            })).GeneratePdf();

        // --- Header

        private void Header(IContainer container, ETicketDocumentDto ticket) =>
            container.Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Width(94).Column(logo =>
                    {
                        if (LogoSvg.Value is { } svg)
                        {
                            logo.Item().Svg(svg);
                        }
                        else
                        {
                            logo.Item().Text("goVoylo").FontSize(16).Bold().FontColor("#7C1AEE");
                        }
                        logo.Item().AlignRight().PaddingRight(6).Text("Anywhere Anyday").FontSize(4.5f).SemiBold().FontColor(Tagline);
                    });
                    left.Item().PaddingTop(8).Text("For assistance, contact us:").FontSize(7.5f).FontColor(Muted);
                    left.Item().PaddingTop(2).Row(phone =>
                    {
                        phone.AutoItem().AlignMiddle().Width(8).Height(8).Svg(Icon(PhoneIcon, Body));
                        phone.AutoItem().PaddingLeft(4).Text(_settings.SupportPhone).FontSize(8.5f).SemiBold();
                    });
                });

                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text(ticket.TripTitle).FontSize(13).Bold();
                    right.Item().PaddingTop(2).AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(s => s.FontSize(8.5f).FontColor(Muted));
                        t.Span("Booking ID: ");
                        t.Span(ticket.BookingRefNo).Bold().FontColor(Body);
                    });
                    right.Item().AlignRight().Text($"Ref ID: {ticket.RefId}").FontSize(8.5f).FontColor(Muted);
                    right.Item().AlignRight().Text($"Booked on: {BookedOn(ticket.BookedOnUtc)}").FontSize(8.5f).FontColor(Muted);
                    right.Item().PaddingTop(4).AlignRight().Element(c => Chip(c, "CONFIRMED", Green, Colors.White, Green));
                });
            });

        private static void DangerousGoodsNotice(IContainer container, bool multiCity) =>
            container.Background(BlueSoft).Border(0.75f).BorderColor(BlueBorder).CornerRadius(6)
                .PaddingVertical(7).PaddingHorizontal(10).Row(row =>
                {
                    row.AutoItem().PaddingTop(1).Width(9).Height(9).Svg(Icon(ShieldAlertIcon, Blue));
                    row.RelativeItem().PaddingLeft(7).Text(
                            "Any explosive, infectious, flammable, toxic, corrosive or radioactive substances are Dangerous " +
                            "Goods and are prohibited in baggage. " +
                            (multiCity ? "Kindly check guidelines for all sectors." : "Kindly check airline guidelines."))
                        .FontSize(7.5f).FontColor(Body).LineHeight(1.35f);
                });

        // Multi-city: "BOM › DXB › LHR › JFK" with "3 Flights · 3 Travellers".
        private static void RouteStrip(IContainer container, ETicketDocumentDto ticket)
        {
            var codes = ticket.Trips.Select(t => t.Segments[0].From.Code)
                .Append(ticket.Trips[^1].Segments[^1].To.Code);
            container.Background(BlueSoft).Border(0.75f).BorderColor(BlueBorder).CornerRadius(6)
                .PaddingVertical(8).PaddingHorizontal(12).Row(row =>
                {
                    row.RelativeItem().AlignMiddle().Text(string.Join("  ›  ", codes)).FontSize(9).Bold().FontColor(Blue);
                    row.AutoItem().AlignMiddle().Background(Colors.White).CornerRadius(8).PaddingVertical(2).PaddingHorizontal(7)
                        .Text($"{ticket.Trips.Count} Flights · {ticket.Passengers.Count} Traveller{(ticket.Passengers.Count == 1 ? "" : "s")}")
                        .FontSize(7).FontColor(Body);
                });
        }

        private static void FlightOfLabel(IContainer container, ETicketTripDto trip, int index, int count) =>
            container.Row(row =>
            {
                row.AutoItem().Background(Ink).CornerRadius(3).PaddingVertical(2).PaddingHorizontal(6)
                    .Text($"FLIGHT {index + 1} OF {count}").FontSize(6.5f).Bold().FontColor(Colors.White);
                row.AutoItem().AlignMiddle().PaddingLeft(6)
                    .Text($"{CityOrCode(trip.Segments[0].From)} → {CityOrCode(trip.Segments[^1].To)}").FontSize(7.5f).FontColor(Muted);
            });

        // --- Flight card

        private static void TripCard(IContainer container, ETicketTripDto trip, bool multiCity)
        {
            var first = trip.Segments[0];
            var last = trip.Segments[^1];
            Card(container, card =>
            {
                card.Item().Row(row =>
                {
                    var pill = multiCity ? $"{CityOrCode(first.From)} → {CityOrCode(last.To)}" : trip.Label;
                    row.AutoItem().Border(0.75f).BorderColor("#B9C3D3").CornerRadius(9).PaddingVertical(2).PaddingHorizontal(8)
                        .Text(pill).FontSize(8).FontColor(Body);
                    row.RelativeItem();
                    if (!string.IsNullOrWhiteSpace(trip.FareType))
                    {
                        row.AutoItem().AlignMiddle().PaddingRight(6)
                            .Element(c => Chip(c, trip.FareType!.ToUpperInvariant(), Orange, OrangeSoft, OrangeBorder));
                    }
                    row.AutoItem().AlignMiddle().Text(first.Departure.ToString("ddd, d MMM yyyy", India)).FontSize(9).FontColor(Body);
                });

                card.Item().PaddingTop(7).Row(row =>
                {
                    row.RelativeItem().AlignMiddle()
                        .Text($"{CityOrCode(first.From)} – {CityOrCode(last.To)}").FontSize(16).Bold();
                    if (!string.IsNullOrWhiteSpace(trip.Pnr))
                    {
                        row.AutoItem().AlignMiddle().Background(BlueSoft).Border(0.75f).BorderColor(BlueBorder).CornerRadius(4)
                            .PaddingVertical(3).PaddingHorizontal(7).Text(t =>
                            {
                                t.Span("PNR  ").FontSize(7).FontColor(Muted);
                                t.Span(trip.Pnr!).FontSize(9.5f).Bold().FontColor(Blue).LetterSpacing(0.04f);
                            });
                    }
                });

                foreach (var segment in trip.Segments)
                {
                    card.Item().PaddingTop(8).Element(c => Segment(c, segment, trip.CabinClass, showDates: multiCity));
                }

                card.Item().PaddingTop(10).LineHorizontal(0.75f).LineColor(Border);
                card.Item().PaddingTop(7).Row(row =>
                {
                    row.ConstantItem(80).Element(c => Field(c, "FARE", trip.FareType ?? "–"));
                    row.ConstantItem(150).Element(c => Field(c, "BAGGAGE", trip.Baggage ?? "–"));
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("STATUS").FontSize(6.5f).Bold().FontColor(Muted).LetterSpacing(0.06f);
                        c.Item().PaddingTop(1).Text("Confirmed").FontSize(8.5f).Bold().FontColor(Green);
                    });
                });
            });
        }

        private static void Segment(IContainer container, ETicketSegmentDto segment, string? cabinClass, bool showDates) =>
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.AutoItem().AlignMiddle().Element(c => AirlineMark(c, segment.AirlineCode));
                    row.RelativeItem().PaddingLeft(8).AlignMiddle().Column(name =>
                    {
                        name.Item().Text(segment.AirlineName).FontSize(10).Bold();
                        name.Item().Text($"{segment.AirlineCode}-{segment.FlightNumber}").FontSize(8.5f).FontColor(Muted);
                    });
                    row.AutoItem().AlignMiddle().Element(c => Chip(c, "TICKETED", Blue, BlueSoft, Blue));
                });

                var nextDay = segment.Arrival is { } arrival && arrival.Date != segment.Departure.Date;
                var stops = segment.Stops == 0 ? "Non-stop" : $"{segment.Stops} Stop{(segment.Stops > 1 ? "s" : "")}";
                column.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem(3).Element(c => Airport(c, segment.From, segment.Departure, showDates || nextDay, alignRight: false));
                    row.RelativeItem(4).PaddingTop(2).Column(mid =>
                    {
                        mid.Item().AlignCenter().Text(Duration(segment.DurationMinutes)).FontSize(8).FontColor(Muted);
                        mid.Item().PaddingVertical(2).Height(6).Layers(layers =>
                        {
                            layers.PrimaryLayer().AlignMiddle().LineHorizontal(1).LineColor("#9DB4E8");
                            layers.Layer().AlignCenter().AlignMiddle().Width(5).Height(5).CornerRadius(2.5f).Background(Blue);
                        });
                        mid.Item().AlignCenter()
                            .Text(string.IsNullOrWhiteSpace(cabinClass) ? stops : $"{cabinClass} · {stops}")
                            .FontSize(8).FontColor(Muted);
                    });
                    row.RelativeItem(3).Element(c => Airport(c, segment.To, segment.Arrival, showDates || nextDay, alignRight: true));
                });
            });

        private static void Airport(IContainer container, ETicketAirportDto airport, DateTime? time, bool showDate, bool alignRight) =>
            container.Column(column =>
            {
                IContainer Align(IContainer c) => alignRight ? c.AlignRight() : c.AlignLeft();
                Align(column.Item()).Text(airport.Code).FontSize(20).Bold();
                Align(column.Item()).Text(time?.ToString("HH:mm", India) ?? "--:--").FontSize(11).Bold();
                if (showDate && time is { } t)
                {
                    Align(column.Item()).Text(t.ToString("ddd, dd MMM", India)).FontSize(7.5f).FontColor(Muted);
                }
                Align(column.Item()).PaddingTop(2).Text(CityOrCode(airport)).FontSize(8.5f).FontColor(Muted);
                if (!string.IsNullOrWhiteSpace(airport.Name))
                {
                    Align(column.Item()).Text(airport.Name!).FontSize(7.5f).FontColor(Muted);
                }
                Align(column.Item()).PaddingTop(1).Text($"Terminal: {TerminalText(airport.Terminal)}").FontSize(7.5f).FontColor(Muted);
            });

        // --- Travellers

        private static void Travellers(IContainer container, ETicketDocumentDto ticket) =>
            Card(container, card =>
            {
                SectionTitle(card, "TRAVELLER DETAILS");
                foreach (var passenger in ticket.Passengers)
                {
                    // A passenger's block moves to the next page whole rather than splitting.
                    card.Item().PaddingTop(8).ShowEntire().Border(0.75f).BorderColor(CardBorder).CornerRadius(6).Column(box =>
                    {
                        box.Item().Background(HeaderGrey).PaddingVertical(7).PaddingHorizontal(10).Row(row =>
                        {
                            row.RelativeItem().AlignMiddle().Text($"{passenger.Name} ({passenger.PaxType})").FontSize(9.5f).Bold();
                            row.AutoItem().AlignMiddle().Background(GreenSoft).CornerRadius(3).PaddingVertical(2).PaddingHorizontal(6)
                                .Text("Confirmed").FontSize(7.5f).Bold().FontColor(Green);
                        });
                        for (var i = 0; i < passenger.Routes.Count; i++)
                        {
                            var route = passenger.Routes[i];
                            var section = box.Item();
                            if (i > 0)
                            {
                                section = section.BorderTop(0.75f).BorderColor(Border);
                            }
                            section.PaddingHorizontal(10).PaddingVertical(8).Column(r =>
                            {
                                r.Item().Row(head =>
                                {
                                    head.AutoItem().AlignMiddle().Width(8).Height(8).Svg(Icon(PlaneIcon, Blue));
                                    head.AutoItem().PaddingLeft(3).Text(route.Route).FontSize(8).Bold().FontColor(Blue);
                                });
                                r.Item().PaddingTop(6).Row(row =>
                                {
                                    Field(row.RelativeItem(), "CLASS", route.CabinClass ?? "–");
                                    Field(row.RelativeItem(), "SEAT", route.Seat ?? "–");
                                    Field(row.RelativeItem(), "MEAL", route.Meal ?? "–");
                                    Field(row.RelativeItem(), "EXTRA BAGGAGE", route.ExtraBaggage ?? "–");
                                    Field(row.RelativeItem(), "INSURANCE", "–");
                                });
                                r.Item().PaddingTop(7).LineHorizontal(0.75f).LineColor(Border);
                                r.Item().PaddingTop(6).Row(ticketRow =>
                                {
                                    ticketRow.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("E-TICKET NO.").FontSize(6.5f).Bold().FontColor(Muted).LetterSpacing(0.06f);
                                        c.Item().PaddingTop(1).Text(route.TicketNumber ?? "–").FontSize(9).Bold();
                                    });
                                    // The e-ticket number, or the route's PNR when the supplier gave none.
                                    if (Barcode(route.TicketNumber ?? route.Pnr) is { } barcode)
                                    {
                                        // FitArea: a fixed box the barcode scales into. Without it, a box
                                        // narrower than the barcode's aspect ratio needs makes QuestPDF loop.
                                        ticketRow.ConstantItem(120).AlignMiddle().Height(26).Svg(barcode).FitArea();
                                    }
                                });
                            });
                        }
                    });
                }
            });

        private static void BaggagePolicy(IContainer container, ETicketDocumentDto ticket) =>
            Card(container, card =>
            {
                SectionTitle(card, "BAGGAGE POLICY");
                card.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1.3f);
                        c.RelativeColumn(2.1f);
                        c.RelativeColumn(1.6f);
                        c.RelativeColumn(1.3f);
                    });
                    foreach (var heading in new[] { "Person", "Sector / Flight", "Check-in Baggage", "Cabin Baggage" })
                    {
                        table.Cell().BorderBottom(0.75f).BorderColor("#B9C3D3").PaddingBottom(5)
                            .Text(heading).FontSize(7.5f).Bold().FontColor(Muted);
                    }
                    for (var i = 0; i < ticket.Baggage.Count; i++)
                    {
                        var row = ticket.Baggage[i];
                        var last = i == ticket.Baggage.Count - 1;
                        foreach (var value in new[] { row.PaxType, row.Sector, row.CheckIn, row.Cabin })
                        {
                            table.Cell().BorderBottom(last ? 0 : 0.5f).BorderColor(Border)
                                .PaddingVertical(5).Text(value).FontSize(8).FontColor(Body);
                        }
                    }
                });
            });

        // --- Payment, contact, information

        private static void PaymentSummary(IContainer container, ETicketDocumentDto ticket)
        {
            var payment = ticket.Payment;
            Card(container, card =>
            {
                SectionTitle(card, "PAYMENT SUMMARY");
                card.Item().PaddingTop(4);
                if (payment.BaseFare is { } baseFare)
                {
                    AmountRow(card, "Base Fare", ticket.PassengerSummary == null ? null : $"({ticket.PassengerSummary})",
                        Money(baseFare, payment.CurrencyCode));
                }
                if (payment.TaxesAndFees is { } taxes)
                {
                    AmountRow(card, "Total Tax", null, Money(taxes, payment.CurrencyCode));
                }
                if (payment.AddOns is { } addOns)
                {
                    AmountRow(card, "Add-Ons", null, Money(addOns, payment.CurrencyCode));
                }
                card.Item().PaddingTop(1).LineHorizontal(0.75f).LineColor(Border);
                card.Item().PaddingTop(1.5f).LineHorizontal(0.75f).LineColor(Border);
                card.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Total").FontSize(11).Bold();
                    row.AutoItem().Text(Money(payment.Total, payment.CurrencyCode)).FontSize(11).Bold();
                });
                card.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Paid online").FontSize(7.5f).FontColor(Muted);
                    row.AutoItem().Text(Money(payment.Total, payment.CurrencyCode)).FontSize(7.5f).FontColor(Muted);
                });
            });
        }

        private static void SharedOn(IContainer container, ETicketDocumentDto ticket) =>
            Card(container, card =>
            {
                SectionTitle(card, "BOOKING CONFIRMATION SHARED ON");
                card.Item().PaddingTop(6).BorderBottom(0.75f).BorderColor(Border).PaddingBottom(6).Row(row =>
                {
                    row.RelativeItem().Text("Email ID").FontSize(8.5f).FontColor(Muted);
                    row.AutoItem().Text(ticket.ContactEmail ?? "–").FontSize(8.5f).FontColor(Blue);
                });
                card.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text("Contact Number").FontSize(8.5f).FontColor(Muted);
                    row.AutoItem().Text(PhoneText(ticket.ContactPhone)).FontSize(8.5f).SemiBold();
                });
            });

        private static void ImportantInformation(IContainer container) =>
            container.Background("#FFFBEA").Border(0.75f).BorderColor("#F6E3A1").CornerRadius(8)
                .PaddingVertical(10).PaddingHorizontal(14).Column(column =>
                {
                    column.Item().Text("IMPORTANT INFORMATION").FontSize(7).Bold().FontColor(Muted).LetterSpacing(0.06f);
                    foreach (var line in new[]
                             {
                                 "Carry a valid photo ID that matches the ticket name.",
                                 "Check the airline baggage allowance and dimensions before travel.",
                                 "Airport and airline operational timings may change.",
                                 "Please quote your booking reference for all future communications.",
                                 "Your travel is subject to airline terms and conditions.",
                             })
                    {
                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.AutoItem().PaddingTop(2.5f).Width(3.5f).Height(3.5f).CornerRadius(1.75f).Background(Blue);
                            row.RelativeItem().PaddingLeft(6).Text(line).FontSize(8).FontColor(Body);
                        });
                    }
                });

        // --- Building blocks

        private static void Card(IContainer container, Action<ColumnDescriptor> content) =>
            container.Border(0.75f).BorderColor(CardBorder).CornerRadius(8).PaddingVertical(12).PaddingHorizontal(13).Column(content);

        private static void SectionTitle(ColumnDescriptor column, string title) =>
            column.Item().Text(title).FontSize(7).Bold().FontColor(Muted).LetterSpacing(0.06f);

        private static void Field(IContainer container, string label, string value) =>
            container.Column(c =>
            {
                c.Item().Text(label).FontSize(6.5f).Bold().FontColor(Muted).LetterSpacing(0.06f);
                c.Item().PaddingTop(1).Text(value).FontSize(8.5f).FontColor(Body);
            });

        private static void AmountRow(ColumnDescriptor column, string label, string? note, string value) =>
            column.Item().BorderBottom(0.75f).BorderColor(Border).PaddingVertical(6).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span(label).FontSize(9.5f).FontColor(Body);
                    if (note != null)
                    {
                        t.Span($"  {note}").FontSize(7).FontColor(Muted);
                    }
                });
                row.AutoItem().Text(value).FontSize(9.5f).FontColor(Body);
            });

        private static void Chip(IContainer container, string text, string color, string background, string border) =>
            container.Background(background).Border(0.75f).BorderColor(border).CornerRadius(3)
                .PaddingVertical(2).PaddingHorizontal(6).Text(text).FontSize(6.5f).Bold().FontColor(color).LetterSpacing(0.04f);

        // The airline's own mark when bundled (same set as the app), else a plane badge.
        private static void AirlineMark(IContainer container, string airlineCode)
        {
            if (AirlineLogos.Value.TryGetValue(airlineCode.ToUpperInvariant(), out var svg))
            {
                container.Width(46).Height(18).AlignMiddle().Svg(svg).FitArea();
            }
            else
            {
                container.Width(22).Height(22).Background("#F3EAFE").CornerRadius(5).AlignCenter().AlignMiddle()
                    .Width(12).Height(12).Svg(Icon(PlaneIcon, "#7C1AEE"));
            }
        }

        private static string Icon(string paths, string color) =>
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"{color}\" " +
            $"stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{paths}</svg>";

        // Code 128 as vector SVG (no native dependency).
        private static string? Barcode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            try
            {
                var writer = new BarcodeWriterSvg
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions { Width = 240, Height = 56, Margin = 0, PureBarcode = true }
                };
                return writer.Write(value.Trim()).Content;
            }
            catch (Exception)
            {
                // A value Code 128 can't encode just goes without a barcode.
                return null;
            }
        }

        private static string CityOrCode(ETicketAirportDto airport) =>
            string.IsNullOrWhiteSpace(airport.City) ? airport.Code : airport.City!;

        // "Terminal 3" → "T3"; nothing → "NA", as on the design.
        private static string TerminalText(string? terminal)
        {
            if (string.IsNullOrWhiteSpace(terminal))
            {
                return "NA";
            }
            var text = terminal.Trim();
            if (text.StartsWith("Terminal", StringComparison.OrdinalIgnoreCase))
            {
                text = text["Terminal".Length..].Trim();
            }
            return text.Length > 0 && char.IsDigit(text[0]) ? $"T{text}" : text;
        }

        // "+919960519916" → "+91 9960519916".
        private static string PhoneText(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return "–";
            }
            var p = phone.Trim();
            return p.StartsWith("+91", StringComparison.Ordinal) && p.Length == 13 ? $"+91 {p[3..]}" : p;
        }

        private static string Duration(int minutes) => minutes <= 0 ? "" : $"{minutes / 60}h {minutes % 60}m";

        private static string Money(decimal amount, string currencyCode) =>
            (currencyCode == "INR" ? "₹" : currencyCode + " ") +
            Math.Round(amount, MidpointRounding.AwayFromZero).ToString("N0", India);

        // Stored UTC → India time (IST has no daylight saving, so a fixed offset).
        private static string BookedOn(DateTime utc) =>
            DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(IndiaOffset).ToString("ddd, d MMM yyyy, HH:mm", India);

        private static string? LoadResource(string name)
        {
            using var stream = typeof(QuestPdfETicketGenerator).Assembly
                .GetManifestResourceStream($"GoVoylo.Infrastructure.Documents.{name}");
            if (stream == null)
            {
                return null;
            }
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static Dictionary<string, string> LoadAirlineLogos() =>
            LoadResource("airline-logos.json") is { } json
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new()
                : new();
    }
}
