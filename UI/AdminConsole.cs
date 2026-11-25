using proecti.Exceptions;
using proecti.Helpers;
using proecti.Services;

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
            Console.WriteLine($"\n--- ადმინისტრატორის კონსოლი (დღე: {_showtimeService.CurrentDay:dd.MM.yyyy}) ---");
            Console.WriteLine("1. ფილმების სიის ნახვა");
            Console.WriteLine("2. ფილმის დამატება (და სეანსის ჩასმა დღევანდელ გრაფიკში)");
            Console.WriteLine("3. ფილმის წაშლა");
            Console.WriteLine("0. მთავარ მენიუში დაბრუნება");
            Console.Write("აირჩიეთ მოქმედება: ");

            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        ShowMovies();
                        break;
                    case "2":
                        AddMovieFlow();
                        break;
                    case "3":
                        RemoveMovieFlow();
                        break;
                    case "0":
                        ConsoleHelper.WriteWarning("გახვედით ადმინისტრატორის რეჟიმიდან.");
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

    private void ShowMovies()
    {
        Console.WriteLine("\n--- ფილმების სია ---");
        if (_showtimeService.Movies.Count == 0)
        {
            Console.WriteLine("ფილმები არ არის დამატებული.");
            return;
        }

        foreach (var movie in _showtimeService.Movies)
        {
            Console.WriteLine($"ID: {movie.Id} | {movie.Title} | ხანგრძლივობა: {movie.DurationMinutes} წთ | ასაკი: {movie.AgeRating}+");
        }
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