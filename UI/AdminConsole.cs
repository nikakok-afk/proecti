using proecti.Exceptions;
using proecti.Helpers;
using proecti.Services;
using Spectre.Console;

namespace proecti.UI;

public class AdminConsole
{
    private const string AdminPinCode = "12345678";

    private readonly ShowtimeService _showtimeService;
    private readonly BookingService _bookingService;

    public AdminConsole(ShowtimeService showtimeService, BookingService bookingService)
    {
        _showtimeService = showtimeService;
        _bookingService = bookingService;
    }

    public void EnterAdminMode()
    {
        string enteredCode = ConsoleHelper.ReadCode("შეიყვანეთ ადმინის კოდი: ");
        if (enteredCode != AdminPinCode)
        {
            throw new UnauthorizedAdminException();
        }

        ConsoleHelper.WriteSuccess("წარმატებით შეხვედით ადმინის კონსოლში!");
        RunAdminMenu();
    }

    private void RunAdminMenu()
    {
        while (true)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new Panel($"[bold yellow]ადმინისტრატორის კონსოლი[/] [grey](დღე: {_showtimeService.CurrentDay:dd.MM.yyyy})[/]")
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Yellow)
                    .Padding(1, 0)
            );

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]აირჩიეთ ადმინის მოქმედება:[/]")
                    .HighlightStyle(new Style(Color.Black, Color.Yellow, Decoration.Bold))
                    .AddChoices(
                        "1. ფილმების სიის ნახვა",
                        "2. ფილმის დამატება (და სეანსის ჩასმა დღევანდელ გრაფიკში)",
                        "3. ფილმის წაშლა",
                        "0. მთავარ მენიუში დაბრუნება"
                    )
            );

            try
            {
                if (choice.StartsWith("1.")) ShowMovies();
                else if (choice.StartsWith("2.")) AddMovieFlow();
                else if (choice.StartsWith("3.")) RemoveMovieFlow();
                else if (choice.StartsWith("0."))
                {
                    ConsoleHelper.WriteWarning("გახვედით ადმინისტრატორის რეჟიმიდან.");
                    return;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"შეცდომა: {ex.Message}");
            }
        }
    }

    private void ShowMovies()
    {
        ConsoleHelper.WriteHeader("ფილმების სია", "yellow");
        if (_showtimeService.Movies.Count == 0)
        {
            ConsoleHelper.WriteWarning("ფილმები არ არის დამატებული.");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow);

        table.AddColumn("[bold yellow]ID[/]");
        table.AddColumn("[bold white]სახელწოდება[/]");
        table.AddColumn("[bold cyan]ხანგრძლივობა[/]");
        table.AddColumn("[bold magenta]ასაკობრივი შეზღუდვა[/]");

        foreach (var movie in _showtimeService.Movies)
        {
            table.AddRow(
                $"[yellow]{movie.Id}[/]",
                $"[bold]{Markup.Escape(movie.Title)}[/]",
                $"{movie.DurationMinutes} წთ",
                $"{movie.AgeRating}+"
            );
        }

        AnsiConsole.Write(table);
    }

    private void AddMovieFlow()
    {
        string title = ConsoleHelper.ReadNonEmptyString("ფილმის სახელწოდება: ", "ფილმის სახელწოდება სავალდებულოა.");
        int duration = ConsoleHelper.ReadPositiveInt("ხანგრძლივობა (წუთებში): ", "ხანგრძლივობა უნდა იყოს 0-ზე მეტი.");
        int ageRating = ConsoleHelper.ReadInt("ასაკობრივი შეზღუდვა (მაგ: 0, 13, 16, 18): ");

        if (ageRating < 0)
        {
            throw new ArgumentException("ასაკობრივი შეზღუდვა უარყოფითი ვერ იქნება.");
        }

        var addedMovie = _showtimeService.AddMovieAndScheduleToday(title, duration, ageRating);
        ConsoleHelper.WriteSuccess($"ფილმი '{addedMovie.Title}' (ID: {addedMovie.Id}) დაემატა და ჩაისვა დღევანდელ სეანსებში!");
    }

    private void RemoveMovieFlow()
    {
        ShowMovies();
        int movieId = ConsoleHelper.ReadInt("შეიყვანეთ წასაშლელი ფილმის ID: ");
        _showtimeService.RemoveMovie(movieId, _bookingService.Bookings);
        ConsoleHelper.WriteWarning($"ფილმი (ID: {movieId}) წაიშალა.");
    }
}