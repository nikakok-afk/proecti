using System.Text;
using proecti.Services;
using proecti.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var showtimeService = new ShowtimeService();
var bookingService = new BookingService(showtimeService);
var adminConsole = new AdminConsole(showtimeService, bookingService);
var aiAssistant = new AiBookingAssistant(showtimeService, bookingService);

var menu = new ConsoleMenu(showtimeService, bookingService, adminConsole, aiAssistant);
menu.Run();