namespace proecti.Exceptions;

public class UnauthorizedAdminException : Exception
{
    public UnauthorizedAdminException()
        : base("არასწორი ადმინისტრატორის კოდი. წვდომა აკრძალულია.")
    {
    }

    public UnauthorizedAdminException(string message)
        : base(message)
    {
    }
}