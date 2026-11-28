using proecti.Helpers;
using proecti.Models;
using proecti.Services;
using Spectre.Console;

namespace proecti.UI;

public class AiBookingAssistant
{
    private readonly ShowtimeService _showtimeService;
    private readonly BookingService _bookingService;

    public AiBookingAssistant(ShowtimeService showtimeService, BookingService bookingService)
    {
        _showtimeService = showtimeService;
        _bookingService = bookingService;
    }

    public void StartChat()
    {
        ConsoleHelper.WriteHeader("AI კინო-ასისტენტი (ტექსტური დაჯავშნა)", "magenta");
        AnsiConsole.MarkupLine("[grey]მომწერეთ რას ეძებთ (მაგ: ფილმის სახელი, \"საღამოს\", \"დილით\", ან \"ყველა\"). ჩასაწერად გამოიყენეთ ტექსტი, არა ID.[/]");

        string userQuery = ConsoleHelper.ReadNonEmptyString(
            "თქვენ: ",
            "გთხოვთ ჩაწეროთ ტექსტი."
        );

        var matchingShowtimes = FindMatchingShowtimes(userQuery);

        if (matchingShowtimes.Count == 0)
        {
            ConsoleHelper.WriteWarning("ამ მოთხოვნით სეანსი ვერ ვიპოვე. გაჩვენებთ დღევანდელ ყველა სეანსს:");
            matchingShowtimes = _showtimeService.Showtimes.ToList();
        }

        if (matchingShowtimes.Count == 0)
        {
            ConsoleHelper.WriteError("სამწუხაროდ, ამჟამად სეანსები არ არის.");
            return;
        }

        var choices = matchingShowtimes
            .Select(s => $"{s.Movie.Title} | {s.StartTime:HH:mm} | {s.Hall.Name} (თავისუფალია: {s.Hall.FreeSeatsCount})")
            .ToList();

        string selectedText = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[magenta]AI ასისტენტი: აი რა სეანსები შევარჩიე. რომელს ისურვებთ?[/]")
                .HighlightStyle(new Style(Color.Black, Color.Magenta1, Decoration.Bold))
                .AddChoices(choices)
        );

        int selectedIndex = choices.IndexOf(selectedText);
        var chosenShowtime = matchingShowtimes[selectedIndex];

        AnsiConsole.MarkupLine($"[magenta]AI ასისტენტი:[/] მშვენიერი არჩევანია — [bold]{Markup.Escape(chosenShowtime.Movie.Title)}[/] ({chosenShowtime.StartTime:HH:mm}, {Markup.Escape(chosenShowtime.Hall.Name)}).");
        chosenShowtime.Hall.PrintSeatMap();

        string customerName = ConsoleHelper.ReadNonEmptyString(
            "AI ასისტენტი: რა გქვიათ? (სახელი): ",
            "სახელი სავალდებულოა."
        );

        string rawSeats = ConsoleHelper.ReadNonEmptyString(
            "AI ასისტენტი: რომელი ადგილები გნებავთ? ჩაწერეთ ტექსტით (მაგ: A1, A2): ",
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

        string popcornChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]AI ასისტენტი: პოპკორნი ხომ არ გნებავთ?[/]")
                .HighlightStyle(new Style(Color.Black, Color.Yellow, Decoration.Bold))
                .AddChoices("არ მინდა", "Small", "Normal", "Big")
        );

        string colaChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]AI ასისტენტი: კოლა ხომ არ გნებავთ?[/]")
                .HighlightStyle(new Style(Color.Black, Color.Cyan1, Decoration.Bold))
                .AddChoices("არ მინდა", "Small", "Normal", "Big")
        );

        var booking = _bookingService.BookSeats(chosenShowtime.Id, customerName, seatCodes);

        ConsoleHelper.WriteSuccess("AI ასისტენტმა ჯავშანი წარმატებით გააფორმა!");

        string snacksSummary = $"პოპკორნი: {popcornChoice} | კოლა: {colaChoice}";
        var summaryPanel = new Panel(
            $"{Markup.Escape(booking.ToString() ?? "")}\n[bold yellow]{Markup.Escape(snacksSummary)}[/]"
        )
            .Header("[bold green]თქვენი AI ჯავშანი[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Green);

        AnsiConsole.Write(summaryPanel);
    }

    private List<Showtime> FindMatchingShowtimes(string query)
    {
        string q = query.Trim().ToLowerInvariant();

        var all = _showtimeService.Showtimes.ToList();
        if (q.Contains("ყველა") || q.Contains("all"))
        {
            return all;
        }

        if (q.Contains("საღამო") || q.Contains("evening") || q.Contains("ღამე"))
        {
            var evening = all.Where(s => s.StartTime.Hour >= 17).ToList();
            if (evening.Count > 0) return evening;
        }

        if (q.Contains("დილა") || q.Contains("morning"))
        {
            var morning = all.Where(s => s.StartTime.Hour < 13).ToList();
            if (morning.Count > 0) return morning;
        }

        return all.Where(s =>
            s.Movie.Title.ToLowerInvariant().Contains(q) ||
            s.Hall.Name.ToLowerInvariant().Contains(q)
        ).ToList();
    }
}