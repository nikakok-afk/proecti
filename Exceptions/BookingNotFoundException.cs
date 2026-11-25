namespace proecti.Exceptions;

public class BookingNotFoundException : KeyNotFoundException
{
    public string TicketCode { get; }

    public BookingNotFoundException(string ticketCode)
        : base($"ბილეთი '{ticketCode}' ვერ მოიძებნა.")
    {
        TicketCode = ticketCode;
    }
}