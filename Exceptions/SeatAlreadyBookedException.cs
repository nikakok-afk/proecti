namespace proecti.Exceptions
{
    public class SeatAlreadyBookedException : InvalidOperationException
    {
        public string SeatCode { get; }

        public SeatAlreadyBookedException(string seatCode)
            : base($"ადგილი '{seatCode}' უკვე დაკავებულია.")
        {
            SeatCode = seatCode;
        }
    }
}