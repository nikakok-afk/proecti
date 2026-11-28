using proecti.Helpers;
using proecti.Services;
using Spectre.Console;

namespace proecti.UI;

public class ConsoleMenu
{
    private readonly ShowtimeService _showtimeService;
    private readonly BookingService _bookingService;
    private readonly AdminConsole _adminConsole;
    private readonly AiBookingAssistant _aiAssistant;

    public ConsoleMenu(
        ShowtimeService showtimeService,
        BookingService bookingService,
        AdminConsole adminConsole,
        AiBookingAssistant aiAssistant)
    {
        _showtimeService = showtimeService;
        _bookingService = bookingService;
        _adminConsole = adminConsole;
        _aiAssistant = aiAssistant;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new Panel("[bold white]კინოთეატრის ბილეთების სისტემა[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Cyan1)
                    .Padding(1, 0)
            );

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]აირჩიეთ მოქმედება (გამოიყენეთ ზემოთ/ქვემოთ ისრები და Enter):[/]")
                    .HighlightStyle(new Style(Color.Black, Color.Cyan1, Decoration.Bold))
                    .AddChoices(
                        "1. სეანსების ნახვა",
                        "2. დარბაზის სქემის ნახვა",
                        "3. ადგილების დაჯავშნა (თავად)",
                        "4. AI ასისტენტით დაჯავშნა (ტექსტით + პოპკორნი/კოლა)",
                        "5. ჯავშნის გაუქმება",
                        "6. ყველა აქტიური ბილეთი",
                        "9. ადმინის კონსოლი (საჭიროებს კოდს)",
                        "0. გამოსვლა"
                    )
            );

            try
            {
                if (choice.StartsWith("1.")) ShowShowtimes();
                else if (choice.StartsWith("2.")) ShowHallMap();
                else if (choice.StartsWith("3.")) BookSeatsFlow();
                else if (choice.StartsWith("4.")) _aiAssistant.StartChat();
                else if (choice.StartsWith("5.")) CancelBookingFlow();
                else if (choice.StartsWith("6.")) ShowAllBookings();
                else if (choice.StartsWith("9.")) _adminConsole.EnterAdminMode();
                else if (choice.StartsWith("0."))
                {
                    AnsiConsole.MarkupLine("[bold cyan]ნახვამდის![/]");
                    return;
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
        ConsoleHelper.WriteHeader("მიმდინარე სეანსები", "cyan");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn("[bold yellow]ID[/]");
        table.AddColumn("[bold cyan]ფილმი[/]");
        table.AddColumn("[bold white]ხანგრძლივობა[/]");
        table.AddColumn("[bold magenta]ასაკი[/]");
        table.AddColumn("[bold green]დრო[/]");
        table.AddColumn("[bold blue]დარბაზი[/]");
        table.AddColumn("[bold white]თავისუფალი ადგილები[/]");

        foreach (var s in _showtimeService.Showtimes)
        {
            string seatsColor = s.Hall.FreeSeatsCount > 0 ? "green" : "red";
            table.AddRow(
                $"[yellow]{s.Id}[/]",
                $"[bold]{Markup.Escape(s.Movie.Title)}[/]",
                $"{s.Movie.DurationMinutes} წთ",
                $"{s.Movie.AgeRating}+",
                $"[green]{s.StartTime:dd.MM HH:mm}[/]",
                Markup.Escape(s.Hall.Name),
                $"[{seatsColor}]{s.Hall.FreeSeatsCount}/{s.Hall.TotalSeatsCount}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    private void ShowHallMap()
    {
        ConsoleHelper.WriteHeader("Showtime Selection", "yellow");
        ShowShowtimes();
        int id = ConsoleHelper.ReadInt("შეიყვანეთ სეანსის ID: ");
        var showtime = _showtimeService.GetByIdOrThrow(id);
        ConsoleHelper.WriteHeader($"დარბაზი: {showtime.Hall.Name} — {showtime.Movie.Title}", "blue");
        showtime.Hall.PrintSeatMap();
    }

    private void BookSeatsFlow()
    {
        ConsoleHelper.WriteHeader("Showtime Selection", "yellow");
        ShowShowtimes();
        int showtimeId = ConsoleHelper.ReadInt("შეიყვანეთ სეანსის ID: ");
        var showtime = _showtimeService.GetByIdOrThrow(showtimeId);
        showtime.Hall.PrintSeatMap();

        string customerName = ConsoleHelper.ReadNonEmptyString(
            "მომხმარებლის სახელი: ",
            "სახელი სავალდებულოა."
        );

        string rawSeats = ConsoleHelper.ReadNonEmptyString(
            "შეიყვანეთ ადგილები მძიმით გამოყოფილი (მაგ: A1,A2,B5): ",
            "მინიმუმ ერთი ადგილი მაინც უნდა მიუთითოთ."
        );

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
        ConsoleHelper.WriteSuccess("დაჯავშნა წარმატებით შესრულდა!");

        var ticketPanel = new Panel(Markup.Escape(booking.ToString() ?? ""))
            .Header("[bold green]ბილეთის დეტალები[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Green);

        AnsiConsole.Write(ticketPanel);
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
        ConsoleHelper.WriteHeader("აქტიური ჯავშნები", "green");
        if (_bookingService.Bookings.Count == 0)
        {
            ConsoleHelper.WriteWarning("აქტიური ჯავშნები არ არის.");
            return;
        }

        foreach (var b in _bookingService.Bookings)
        {
            var panel = new Panel(Markup.Escape(b.ToString() ?? ""))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey);
            AnsiConsole.Write(panel);
        }
    }
}