
using proecti.UI;
using proecti.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var showtimeService = new ShowtimeService();
showtimeService.SeedRandomShowtimesForDay(DateTime.Today, pickedMoviesCount: 5);

var bookingService = new BookingService(showtimeService);
var adminConsole = new AdminConsole(showtimeService, bookingService);

var menu = new ConsoleMenu(showtimeService, bookingService, adminConsole);
menu.Run();