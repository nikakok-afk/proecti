using proecti.Helpers;
using proecti.Services;

namespace proecti.UI;

public class ConsoleMenu
{
    private readonly ShowtimeService _showtimeService;
    private readonly BookingService _bookingService;
    private readonly AdminConsole _adminConsole;

    public ConsoleMenu(ShowtimeService showtimeService, BookingService bookingService, AdminConsole adminConsole)
    {
        _showtimeService = showtimeService;
        _bookingService = bookingService;
        _adminConsole = adminConsole;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n=== კინოთეატრის სისტემა ===");
            Console.WriteLine("1. სეანსების ნახვა");
            Console.WriteLine("2. დარბაზის სქემის ნახვა");
            Console.WriteLine("3. ადგილების დაჯავშნა");
            Console.WriteLine("4. ჯავშნის გაუქმება");
            Console.WriteLine("5. ყველა აქტიური ბილეთი");
            Console.WriteLine("9. ადმინის კონსოლი (საჭიროებს კოდს)");
            Console.WriteLine("0. გამოსვლა");
            Console.Write("აირჩიეთ მოქმედება: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        ShowShowtimes();
                        break;
                    case "2":
                        ShowHallMap();
                        break;
                    case "3":
                        BookSeatsFlow();
                        break;
                    case "4":
                        CancelBookingFlow();
                        break;
                    case "5":
                        ShowAllBookings();
                        break;
                    case "9":
                        _adminConsole.EnterAdminMode();
                        break;
                    case "0":
                        Console.WriteLine("ნახვამდის!");
                        return;
                    default:
                        ConsoleHelper.WriteWarning("არასწორი არჩევანი, სცადეთ თავიდან.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"შეცდომა: {ex.Message}");
            }
        }
    }

    private void ShowShowtimes()
    {
        Console.WriteLine("\n--- მიმდინარე სეანსები ---");
        foreach (var s in _showtimeService.Showtimes)
        {
            Console.WriteLine(
                $"ID: {s.Id} | {s.Movie.Title} ({s.Movie.DurationMinutes} წთ, {s.Movie.AgeRating}+) | " +
                $"დრო: {s.StartTime:dd.MM HH:mm} | დარბაზი: {s.Hall.Name} | " +
                $"თავისუფალია: {s.Hall.FreeSeatsCount}/{s.Hall.TotalSeatsCount}"
            );
        }
    }

    private void ShowHallMap()
    {
        ShowShowtimes();
        int id = ConsoleHelper.ReadInt("შეიყვანეთ სეანსის ID: ");
        var showtime = _showtimeService.GetByIdOrThrow(id);
        showtime.Hall.PrintSeatMap();
    }

    private void BookSeatsFlow()
    {
        ShowShowtimes();
        int showtimeId = ConsoleHelper.ReadInt("შეიყვანეთ სეანსის ID: ");
        var showtime = _showtimeService.GetByIdOrThrow(showtimeId);
        showtime.Hall.PrintSeatMap();

        string customerName = ConsoleHelper.ReadNonEmptyString(
            "მომხმარებლის სახელი: ",
            "სახელი სავალდებულოა."
        );

        Console.Write("შეიყვანეთ ადგილები მძიმით გამოყოფილი (მაგ: A1,A2,B5): ");
        string rawSeats = Console.ReadLine() ?? "";
        var seatCodes = rawSeats
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToUpperInvariant())
            .Distinct()
            .ToList();

        if (seatCodes.Count == 0)
        {
            throw new ArgumentException("მინიმუმ ერთი ადგილი მაინც უნდა მიუთითოთ.");
        }

        var booking = _bookingService.BookSeats(showtimeId, customerName, seatCodes);
        ConsoleHelper.WriteSuccess("\nდაჯავშნა წარმატებით შესრულდა!");
        Console.WriteLine(booking);
    }

    private void CancelBookingFlow()
    {
        string code = ConsoleHelper.ReadNonEmptyString(
            "შეიყვანეთ ბილეთის კოდი (მაგ: TKT-1001): ",
            "ბილეთის კოდი სავალდებულოა."
        ).ToUpperInvariant();

        _bookingService.CancelBooking(code);
        ConsoleHelper.WriteWarning($"ჯავშანი {code} გაუქმდა და ადგილები გათავისუფლდა.");
    }

    private void ShowAllBookings()
    {
        Console.WriteLine("\n--- აქტიური ჯავშნები ---");
        if (_bookingService.Bookings.Count == 0)
        {
            Console.WriteLine("აქტიური ჯავშნები არ არის.");
            return;
        }

        foreach (var b in _bookingService.Bookings)
        {
            Console.WriteLine(b);
            Console.WriteLine("-----------------------------------------");
        }
    }
}