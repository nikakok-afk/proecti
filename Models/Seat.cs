using proecti.Exceptions;

namespace proecti.Models;

public class Seat
{
    public string Code { get; }
    public SeatType Type { get; }
    public decimal Price { get; }
    public bool IsBooked { get; private set; }

    public Seat(string code, SeatType type, decimal price)
    {
        Code = code;
        Type = type;
        Price = price;
    }

    public void Book()
    {
        if (IsBooked)
            throw new SeatAlreadyBookedException(Code);

        IsBooked = true;
    }

    public void Release() => IsBooked = false;
}