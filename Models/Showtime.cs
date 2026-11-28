namespace proecti.Models
{
    public class Showtime
    {
        public int Id { get; }
        public Movie Movie { get; }
        public Hall Hall { get; }
        public DateTime StartTime { get; }

        public Showtime(int id, Movie movie, Hall hall, DateTime startTime)
        {
            Id = id;
            Movie = movie;
            Hall = hall;    
            StartTime = startTime;
        }
    }
}
