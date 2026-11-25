using System.Globalization;

namespace proecti.Models;

public class Booking
{
    public string TicketCode { get; }
    public string CustomerName { get; }
    public Showtime Showtime { get; }
    public IReadOnlyList<Seat> Seats { get; }
    public decimal TotalPrice { get; }
    public DateTime CreatedAt { get; }

    public Booking(string ticketCode, string customerName, Showtime showtime, List<Seat> seats)
    {
        TicketCode = ticketCode;
        CustomerName = customerName;
        Showtime = showtime;
        Seats = seats;
        TotalPrice = seats.Sum(s => s.Price);
        CreatedAt = DateTime.Now;
    }

    public override string ToString()
    {
        string seatsStr = string.Join(", ", Seats.Select(s => $"{s.Code} ({s.Price.ToString("0.00", CultureInfo.InvariantCulture)}₾)"));
        return $"ბილეთი: {TicketCode} | მყიდველი: {CustomerName}\n" +
               $"ფილმი: {Showtime.Movie.Title} | სეანსი: {Showtime.StartTime:dd.MM HH:mm} | დარბაზი: {Showtime.Hall.Name}\n" +
               $"ადგილები: {seatsStr}\n" +
               $"ჯამი: {TotalPrice.ToString("0.00", CultureInfo.InvariantCulture)}₾";
    }
}