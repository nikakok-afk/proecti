namespace proecti.Models;

public class Hall
{
    public string Name { get; }
    private readonly List<Seat> _seats = new();
    private readonly int _rows;
    private readonly int _cols;

    public IReadOnlyList<Seat> Seats => _seats;
    public int TotalSeatsCount => _seats.Count;
    public int FreeSeatsCount => _seats.Count(s => !s.IsBooked);

    public Hall(string name, int rows, int cols, decimal standardPrice, decimal vipPrice)
    {
        Name = name;
        _rows = rows;
        _cols = cols;

        for (int r = 0; r < rows; r++)
        {
            char rowLetter = (char)('A' + r);
            bool isVipRow = r == rows - 1;
            var seatType = isVipRow ? SeatType.Vip : SeatType.Standard;
            decimal price = isVipRow ? vipPrice : standardPrice;

            for (int c = 1; c <= cols; c++)
            {
                _seats.Add(new Seat($"{rowLetter}{c}", seatType, price));
            }
        }
    }

    public Seat GetSeatOrThrow(string code)
    {
        return _seats.FirstOrDefault(s => s.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException($"ადგილი '{code}' დარბაზში '{Name}' არ არსებობს.");
    }

    public void PrintSeatMap()
    {
        Console.WriteLine($"\n--- დარბაზი: {Name} ( [A1]=თავისუფალი, [X1]=დაკავებული, * = VIP ) ---");
        for (int r = 0; r < _rows; r++)
        {
            char rowLetter = (char)('A' + r);
            for (int c = 1; c <= _cols; c++)
            {
                string code = $"{rowLetter}{c}";
                var seat = _seats.First(s => s.Code == code);
                string prefix = seat.IsBooked ? "X" : seat.Code[..1];
                string suffix = seat.Type == SeatType.Vip ? "*" : " ";
                Console.Write($"[{prefix}{c}{suffix}] ");
            }
            Console.WriteLine();
        }
    }
}