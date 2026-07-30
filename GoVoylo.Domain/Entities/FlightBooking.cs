using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class FlightBooking
    {
        private string from;
        private string to;
        private DateTime journeyDate;
        private int numberOfPassengers;

        //private FlightBooking()
        //{
        //}
        public FlightBooking(
        string flightNumber,
        string passengerName,
        string from,
        string to,
        DateTime journeyDate,
        int numberOfPassengers)
        {
            FlightBookingId = Guid.NewGuid();

            FlightBookingReference = $"BK-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            FlightNumber = flightNumber;

            PassengerName = passengerName;

            From = from;

            To = to;

            JourneyDate = journeyDate;

            NumberOfPassengers = numberOfPassengers;

            FlightBookingStatus = "Confirmed";

            BookedAt = DateTime.UtcNow;
        }

        public Guid FlightBookingId { get; private set; }

        public string FlightBookingReference { get; private set; }

        public string FlightNumber { get; private set; }

        public string PassengerName { get; private set; }

        public string From { get; private set; }

        public string To { get; private set; }

        public DateTime JourneyDate { get; private set; }

        public int NumberOfPassengers { get; private set; }

        public string FlightBookingStatus { get; private set; }

        public DateTime BookedAt { get; private set; }
    }
}

