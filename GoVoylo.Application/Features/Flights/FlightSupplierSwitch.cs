using Microsoft.Extensions.Configuration;

namespace GoVoylo.Application.Features.Flights
{
    // Turns a supplier off for new searches (FlightSuppliers:<code>:Enabled, an app
    // setting — e.g. FlightSuppliers__flyshop__Enabled = true to bring Flyshop back).
    // A supplier with no setting is on. Bookings already made with a supplier that is
    // off can still be viewed, cancelled and released: only new offers stop.
    public static class FlightSupplierSwitch
    {
        public static bool IsEnabled(IConfiguration configuration, string supplierCode) =>
            configuration.GetValue<bool?>($"FlightSuppliers:{supplierCode}:Enabled") ?? true;
    }
}
