using proecti.Exceptions;
using proecti.Models;

namespace proecti.Services;

public class BookingService
{
    private readonly ShowtimeService _showtimeService;
    private readonly List<Booking> _bookings = new();
    private int _nextTicketNumber = 1001;

    public IReadOnlyList<Booking> Bookings => _bookings;

    public BookingService(ShowtimeService showtimeService)
    {
        _showtimeService = showtimeService;
    }

    public Booking BookSeats(int showtimeId, string customerName, List<string> seatCodes)
    {
        var showtime = _showtimeService.GetByIdOrThrow(showtimeId);
        var seatsToBook = seatCodes.Select(code => showtime.Hall.GetSeatOrThrow(code)).ToList();

        foreach (var seat in seatsToBook)
        {
            if (seat.IsBooked)
                throw new SeatAlreadyBookedException(seat.Code);
        }

        foreach (var seat in seatsToBook)
        {
            seat.Book();
        }

        var booking = new Booking($"TKT-{_nextTicketNumber++}", customerName, showtime, seatsToBook);
        _bookings.Add(booking);
        return booking;
    }

    public void CancelBooking(string ticketCode)
    {
        var booking = _bookings.FirstOrDefault(b => b.TicketCode.Equals(ticketCode, StringComparison.OrdinalIgnoreCase))
                      ?? throw new BookingNotFoundException(ticketCode);

        foreach (var seat in booking.Seats)
        {
            seat.Release();
        }

        _bookings.Remove(booking);
    }
}