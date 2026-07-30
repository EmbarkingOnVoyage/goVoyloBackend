using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using GoVoylo.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Persistence.Repositories
{
    public class BookFlightRepository : IBookFlightRepository
    {
        private readonly IBookFlightRepository _bookFlightRepository;
       
        private readonly ApplicationDbContext _context;

        public BookFlightRepository (ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FlightBooking?> GetByBookingReferenceAsync(
        string bookingReference,
        CancellationToken cancellationToken)
        {
            return await _context.FlightBookings
                .FirstOrDefaultAsync(
                    x => x.FlightBookingReference == bookingReference,
                    cancellationToken);
        }

        public async Task SaveAsync(FlightBooking booking, CancellationToken cancellationToken)
        {
            await _context.FlightBookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(
        FlightBooking booking,
        CancellationToken cancellationToken)
        {
            _context.FlightBookings.Update(booking);

            await _context.SaveChangesAsync(cancellationToken);
        }

     };
}
