using System.Globalization;
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
    // (one-way, round trip and multi-city share this layout: one flight card per trip).
    // QuestPDF runs under its Community licence.
    public class QuestPdfETicketGenerator : IETicketPdfGenerator
    {
        private const string Purple = "#7C1AEE";
        private const string Ink = "#182339";
        private const string Muted = "#697691";
        private const string Border = "#D9E1EC";
        private const string Green = "#15803D";
        private const string Blue = "#2563EB";

        private static readonly CultureInfo India = CultureInfo.GetCultureInfo("en-IN");
        private static readonly Lazy<string?> LogoSvg = new(LoadLogo);

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
                page.Margin(28);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(9).FontColor(Ink));

                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Element(c => Header(c, ticket));
                    column.Item().Element(DangerousGoodsNotice);
                    foreach (var trip in ticket.Trips)
                    {
                        column.Item().Element(c => TripCard(c, trip));
                    }
                    column.Item().Element(c => Travellers(c, ticket));
                    if (ticket.Baggage.Count > 0)
                    {
                        column.Item().Element(c => BaggagePolicy(c, ticket));
                    }
                    column.Item().Element(c => PaymentSummary(c, ticket.Payment));
                    column.Item().Element(c => SharedOn(c, ticket));
                    column.Item().Element(ImportantInformation);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(7).FontColor(Muted));
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            })).GeneratePdf();

        private void Header(IContainer container, ETicketDocumentDto ticket) =>
            container.Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (LogoSvg.Value is { } svg)
                    {
                        left.Item().Width(110).Svg(svg);
                    }
                    else
                    {
                        left.Item().Text("goVoylo").FontSize(18).Bold().FontColor(Purple);
                    }
                    left.Item().PaddingTop(6).Text("For assistance, contact us:").FontSize(8).FontColor(Muted);
                    left.Item().Text(_settings.SupportPhone).FontSize(9).SemiBold();
                });

                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text(ticket.TripTitle).FontSize(12).Bold();
                    right.Item().AlignRight().Text(t =>
                    {
                        t.Span("Booking ID: ").FontColor(Muted);
                        t.Span(ticket.BookingRefNo).SemiBold();
                    });
                    right.Item().AlignRight().Text($"Booked on: {BookedOn(ticket.BookedOnUtc)}").FontColor(Muted);
                    right.Item().PaddingTop(4).AlignRight().Element(c => Chip(c, "CONFIRMED", Green, "#E7F8EE"));
                });
            });

        private static void DangerousGoodsNotice(IContainer container) =>
            container.Background("#EEF3FF").Border(1).BorderColor("#B9CCF5").CornerRadius(6).Padding(8)
                .Text("Any explosive, infectious, flammable, toxic, corrosive or radioactive substances are Dangerous " +
                      "Goods and are prohibited in baggage. Kindly check airline guidelines.")
                .FontSize(8);

        private static void TripCard(IContainer container, ETicketTripDto trip)
        {
            var first = trip.Segments[0];
            var last = trip.Segments[^1];
            Card(container, card =>
            {
                card.Item().Row(row =>
                {
                    row.AutoItem().Element(c => Chip(c, trip.Label, Ink, Colors.White, outlined: true));
                    row.RelativeItem().AlignRight().Text(first.Departure.ToString("ddd, d MMM yyyy", India)).FontColor(Muted);
                });
                card.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text($"{CityOrCode(first.From)} – {CityOrCode(last.To)}").FontSize(14).Bold();
                    if (!string.IsNullOrWhiteSpace(trip.Pnr))
                    {
                        row.AutoItem().AlignRight().Element(c => Chip(c, $"PNR  {trip.Pnr}", Blue, "#EEF3FF"));
                    }
                });

                foreach (var segment in trip.Segments)
                {
                    card.Item().PaddingTop(8).Element(c => Segment(c, segment));
                }

                card.Item().PaddingTop(8).LineHorizontal(1).LineColor(Border);
                card.Item().PaddingTop(6).Row(row =>
                {
                    Field(row.RelativeItem(), "CLASS", trip.CabinClass ?? "–");
                    Field(row.RelativeItem(2), "BAGGAGE", trip.Baggage ?? "–");
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("STATUS").FontSize(7).FontColor(Muted).SemiBold();
                        c.Item().Text("Confirmed").FontColor(Green).SemiBold();
                    });
                });
            });
        }

        private static void Segment(IContainer container, ETicketSegmentDto segment) =>
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span(segment.AirlineName).SemiBold();
                        t.Span($"   {segment.AirlineCode}-{segment.FlightNumber}").FontColor(Muted);
                    });
                    row.AutoItem().Element(c => Chip(c, "TICKETED", Blue, "#EEF3FF", outlined: true));
                });
                column.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem(3).Element(c => Airport(c, segment.From, segment.Departure, alignRight: false));
                    row.RelativeItem(2).AlignMiddle().Column(mid =>
                    {
                        mid.Item().AlignCenter().Text(Duration(segment.DurationMinutes)).FontColor(Muted);
                        mid.Item().PaddingVertical(2).LineHorizontal(1).LineColor(Purple);
                        mid.Item().AlignCenter()
                            .Text(segment.Stops == 0 ? "Non-stop" : $"{segment.Stops} stop{(segment.Stops > 1 ? "s" : "")}")
                            .FontColor(Muted);
                    });
                    row.RelativeItem(3).Element(c => Airport(c, segment.To, segment.Arrival, alignRight: true));
                });
            });

        private static void Airport(IContainer container, ETicketAirportDto airport, DateTime? time, bool alignRight) =>
            container.Column(column =>
            {
                IContainer Align(IContainer c) => alignRight ? c.AlignRight() : c.AlignLeft();
                Align(column.Item()).Text(airport.Code).FontSize(16).Bold();
                Align(column.Item()).Text(time?.ToString("HH:mm", India) ?? "--:--").FontSize(11).Bold();
                if (!string.IsNullOrWhiteSpace(airport.City))
                {
                    Align(column.Item()).Text(airport.City!).FontColor(Muted);
                }
                if (!string.IsNullOrWhiteSpace(airport.Name))
                {
                    Align(column.Item()).Text(airport.Name!).FontSize(7.5f).FontColor(Muted);
                }
                if (!string.IsNullOrWhiteSpace(airport.Terminal))
                {
                    Align(column.Item()).Text(airport.Terminal!).FontSize(7.5f).FontColor(Muted);
                }
            });

        private static void Travellers(IContainer container, ETicketDocumentDto ticket) =>
            Card(container, card =>
            {
                SectionTitle(card, "TRAVELLER DETAILS");
                foreach (var passenger in ticket.Passengers)
                {
                    // A passenger's block moves to the next page whole rather than splitting.
                    card.Item().PaddingTop(6).ShowEntire().Border(1).BorderColor(Border).CornerRadius(6).Column(box =>
                    {
                        box.Item().Background("#F1F3F7").Padding(8).Row(row =>
                        {
                            row.RelativeItem().Text($"{passenger.Name} ({passenger.PaxType})").SemiBold();
                            row.AutoItem().Element(c => Chip(c, "Confirmed", Green, "#E7F8EE"));
                        });
                        foreach (var route in passenger.Routes)
                        {
                            box.Item().Padding(8).Column(r =>
                            {
                                r.Item().Text(route.Route.Replace("-", "–")).FontColor(Blue).SemiBold();
                                r.Item().PaddingTop(4).Row(row =>
                                {
                                    Field(row.RelativeItem(), "CLASS", route.CabinClass ?? "–");
                                    Field(row.RelativeItem(), "SEAT", route.Seat ?? "–");
                                    Field(row.RelativeItem(), "MEAL", route.Meal ?? "–");
                                    Field(row.RelativeItem(), "EXTRA BAGGAGE", route.ExtraBaggage ?? "–");
                                });
                                r.Item().PaddingTop(4).LineHorizontal(1).LineColor(Border);
                                r.Item().PaddingTop(4).Row(ticketRow =>
                                {
                                    ticketRow.RelativeItem().Element(c => Field(c, "E-TICKET NO.", route.TicketNumber ?? "–"));
                                    // The e-ticket number, or the route's PNR when the supplier gave none.
                                    if (Barcode(route.TicketNumber ?? route.Pnr) is { } barcode)
                                    {
                                        ticketRow.ConstantItem(150).AlignRight().AlignMiddle().Height(26).Svg(barcode);
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
                card.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(2.4f);
                        c.RelativeColumn(1.4f);
                        c.RelativeColumn(1.4f);
                    });
                    foreach (var heading in new[] { "Person", "Sector / Flight", "Check-in Baggage", "Cabin Baggage" })
                    {
                        table.Cell().BorderBottom(1).BorderColor(Border).PaddingVertical(4)
                            .Text(heading).FontColor(Muted).SemiBold();
                    }
                    foreach (var row in ticket.Baggage)
                    {
                        foreach (var value in new[] { row.PaxType, row.Sector, row.CheckIn, row.Cabin })
                        {
                            table.Cell().PaddingVertical(4).Text(value);
                        }
                    }
                });
            });

        private static void PaymentSummary(IContainer container, ETicketPaymentDto payment) =>
            Card(container, card =>
            {
                SectionTitle(card, "PAYMENT SUMMARY");
                if (payment.BaseFare is { } baseFare)
                {
                    AmountRow(card, "Base Fare", Money(baseFare, payment.CurrencyCode));
                }
                if (payment.TaxesAndFees is { } taxes)
                {
                    AmountRow(card, "Taxes & Fees", Money(taxes, payment.CurrencyCode));
                }
                if (payment.AddOns is { } addOns)
                {
                    AmountRow(card, "Add-Ons", Money(addOns, payment.CurrencyCode));
                }
                card.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("Total").FontSize(11).Bold();
                    row.AutoItem().Text(Money(payment.Total, payment.CurrencyCode)).FontSize(11).Bold();
                });
                card.Item().Text("Paid online").FontSize(8).FontColor(Muted);
            });

        private static void SharedOn(IContainer container, ETicketDocumentDto ticket) =>
            Card(container, card =>
            {
                SectionTitle(card, "BOOKING CONFIRMATION SHARED ON");
                card.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("Email ID").FontColor(Muted);
                    row.AutoItem().Text(ticket.ContactEmail ?? "–").FontColor(Blue);
                });
                card.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("Contact Number").FontColor(Muted);
                    row.AutoItem().Text(ticket.ContactPhone ?? "–").SemiBold();
                });
            });

        private static void ImportantInformation(IContainer container) =>
            container.ShowEntire().Background("#FFF9E6").Border(1).BorderColor("#F5E3A3").CornerRadius(8).Padding(10).Column(column =>
            {
                column.Item().Text("IMPORTANT INFORMATION").FontSize(8).SemiBold().FontColor(Muted);
                foreach (var line in new[]
                         {
                             "Carry a valid photo ID that matches the ticket name.",
                             "Check the airline baggage allowance and dimensions before travel.",
                             "Airport and airline operational timings may change.",
                             "Please quote your booking reference for all future communications.",
                             "Your travel is subject to airline terms and conditions.",
                         })
                {
                    column.Item().PaddingTop(3).Text($"•  {line}").FontSize(8);
                }
            });

        // --- Building blocks

        private static void Card(IContainer container, Action<ColumnDescriptor> content) =>
            container.Border(1).BorderColor(Border).CornerRadius(8).Padding(12).Column(content);

        private static void SectionTitle(ColumnDescriptor column, string title) =>
            column.Item().Text(title).FontSize(8).SemiBold().FontColor(Muted).LetterSpacing(0.05f);

        private static void Field(IContainer container, string label, string value) =>
            container.Column(c =>
            {
                c.Item().Text(label).FontSize(7).FontColor(Muted).SemiBold();
                c.Item().Text(value);
            });

        private static void AmountRow(ColumnDescriptor column, string label, string value) =>
            column.Item().BorderBottom(1).BorderColor(Border).PaddingVertical(5).Row(row =>
            {
                row.RelativeItem().Text(label);
                row.AutoItem().Text(value);
            });

        private static void Chip(IContainer container, string text, string color, string background, bool outlined = false)
        {
            var chip = container.Background(background).CornerRadius(4);
            if (outlined)
            {
                chip = chip.Border(1).BorderColor(color);
            }
            chip.PaddingVertical(2).PaddingHorizontal(6).Text(text).FontSize(7.5f).SemiBold().FontColor(color);
        }

        private static string CityOrCode(ETicketAirportDto airport) =>
            string.IsNullOrWhiteSpace(airport.City) ? airport.Code : airport.City!;

        private static string Duration(int minutes) => minutes <= 0 ? "" : $"{minutes / 60}h {minutes % 60}m";

        private static string Money(decimal amount, string currencyCode) =>
            (currencyCode == "INR" ? "₹" : currencyCode + " ") +
            Math.Round(amount, MidpointRounding.AwayFromZero).ToString("N0", India);

        // Stored UTC → India time, as the booking was made from India.
        private static string BookedOn(DateTime utc)
        {
            var ist = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utc, DateTimeKind.Utc),
                TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata"));
            return ist.ToString("ddd, d MMM yyyy, HH:mm", India);
        }

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
                    Options = new EncodingOptions { Width = 300, Height = 52, Margin = 0, PureBarcode = true }
                };
                return writer.Write(value.Trim()).Content;
            }
            catch (Exception)
            {
                // A value Code 128 can't encode just goes without a barcode.
                return null;
            }
        }

        private static string? LoadLogo()
        {
            using var stream = typeof(QuestPdfETicketGenerator).Assembly
                .GetManifestResourceStream("GoVoylo.Infrastructure.Documents.govoylo-logo.svg");
            if (stream == null)
            {
                return null;
            }
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
