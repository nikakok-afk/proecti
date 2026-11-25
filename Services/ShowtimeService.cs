
using proecti.Models;

namespace proecti.Services;

public class ShowtimeService
{
    private readonly List<Movie> _movies = new();
    private readonly List<Hall> _halls = new();
    private readonly List<Showtime> _showtimes = new();

    private DateTime _currentDay = DateTime.Today;
    private int _nextMovieId = 1;
    private int _nextShowtimeId = 101;

    public IReadOnlyList<Movie> Movies => _movies;
    public IReadOnlyList<Showtime> Showtimes => _showtimes;
    public DateTime CurrentDay => _currentDay;

    public void SeedRandomShowtimesForDay(DateTime? targetDate = null, int pickedMoviesCount = 5)
    {
        _movies.Clear();
        _halls.Clear();
        _showtimes.Clear();
        _nextMovieId = 1;
        _nextShowtimeId = 101;

        var allMovies = new List<Movie>
        {
            new(_nextMovieId++, "Dune: Part Two", 166, 13),
            new(_nextMovieId++, "Oppenheimer", 180, 16),
            new(_nextMovieId++, "Inside Out 2", 96, 0),
            new(_nextMovieId++, "Interstellar", 169, 13),
            new(_nextMovieId++, "The Dark Knight", 152, 16),
            new(_nextMovieId++, "Inception", 148, 13),
            new(_nextMovieId++, "Spider-Man: Across the Spider-Verse", 140, 6),
            new(_nextMovieId++, "John Wick: Chapter 4", 169, 18),
            new(_nextMovieId++, "Avatar: The Way of Water", 192, 13),
            new(_nextMovieId++, "Parasite", 132, 16)
        };

        _halls.AddRange(new[]
        {
            new Hall("დარბაზი 1 (IMAX)", rows: 4, cols: 6, standardPrice: 15m, vipPrice: 25m),
            new Hall("დარბაზი 2", rows: 3, cols: 5, standardPrice: 12m, vipPrice: 20m),
            new Hall("დარბაზი 3 (VIP)", rows: 3, cols: 4, standardPrice: 18m, vipPrice: 30m)
        });

        _currentDay = (targetDate ?? DateTime.Today.AddDays(Random.Shared.Next(0, 4))).Date;
        var hallNextAvailableTime = _halls.ToDictionary(h => h, _ => _currentDay.AddHours(11));

        int countToPick = Math.Clamp(pickedMoviesCount, 1, allMovies.Count);
        var randomMovies = allMovies
            .OrderBy(_ => Random.Shared.Next())
            .Take(countToPick)
            .ToList();

        foreach (var movie in randomMovies)
        {
            _movies.Add(movie);

            var hall = _halls[Random.Shared.Next(_halls.Count)];
            DateTime startTime = hallNextAvailableTime[hall];

            if (startTime.AddMinutes(movie.DurationMinutes) > _currentDay.AddHours(23).AddMinutes(30))
            {
                hall = _halls.FirstOrDefault(h =>
                    hallNextAvailableTime[h].AddMinutes(movie.DurationMinutes) <= _currentDay.AddHours(23).AddMinutes(30)) ?? hall;
                startTime = hallNextAvailableTime[hall];
            }

            _showtimes.Add(new Showtime(_nextShowtimeId++, movie, hall, startTime));
            hallNextAvailableTime[hall] = startTime.AddMinutes(movie.DurationMinutes + 20);
        }
    }

    public Movie AddMovieAndScheduleToday(string title, int durationMinutes, int ageRating)
    {
        if (_halls.Count == 0)
            throw new InvalidOperationException("დარბაზები არ არის ინიციალიზებული.");

        var movie = new Movie(_nextMovieId++, title, durationMinutes, ageRating);
        _movies.Add(movie);

        // ვპოულობთ ყველაზე ადრე თავისუფალ დროს იმავე დღეს (_currentDay)
        var bestHall = _halls
            .OrderBy(h => GetNextFreeTimeForHall(h))
            .First();

        DateTime startTime = GetNextFreeTimeForHall(bestHall);
        _showtimes.Add(new Showtime(_nextShowtimeId++, movie, bestHall, startTime));

        return movie;
    }

    public void RemoveMovie(int movieId, IReadOnlyCollection<Booking> activeBookings)
    {
        var movie = _movies.FirstOrDefault(m => m.Id == movieId)
                    ?? throw new KeyNotFoundException($"ფილმი ID={movieId} ვერ მოიძებნა.");

        bool hasActiveBookings = activeBookings.Any(b => b.Showtime.Movie.Id == movieId);
        if (hasActiveBookings)
            throw new InvalidOperationException($"ფილმის '{movie.Title}' წაშლა შეუძლებელია, რადგან მასზე არსებობს აქტიური ჯავშნები.");

        _showtimes.RemoveAll(s => s.Movie.Id == movieId);
        _movies.Remove(movie);
    }

    public Showtime GetByIdOrThrow(int showtimeId)
    {
        return _showtimes.FirstOrDefault(s => s.Id == showtimeId)
               ?? throw new KeyNotFoundException($"სეანსი ID={showtimeId} ვერ მოიძებნა.");
    }

    private DateTime GetNextFreeTimeForHall(Hall hall)
    {
        var lastShowtime = _showtimes
            .Where(s => s.Hall == hall && s.StartTime.Date == _currentDay.Date)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefault();

        if (lastShowtime is null)
            return _currentDay.AddHours(11);

        return lastShowtime.StartTime.AddMinutes(lastShowtime.Movie.DurationMinutes + 20);
    }
}