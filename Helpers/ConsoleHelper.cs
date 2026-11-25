namespace proecti.Helpers;

public static class ConsoleHelper
{
    public static int ReadInt(string prompt)
    {
        Console.Write(prompt);
        if (!int.TryParse(Console.ReadLine(), out int value))
        {
            throw new FormatException("გთხოვთ შეიყვანოთ ვალიდური რიცხვი.");
        }
        return value;
    }

    public static int ReadPositiveInt(string prompt, string errorMessage)
    {
        int value = ReadInt(prompt);
        if (value <= 0)
        {
            throw new ArgumentException(errorMessage);
        }
        return value;
    }

    public static string ReadNonEmptyString(string prompt, string errorMessage)
    {
        Console.Write(prompt);
        string input = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException(errorMessage);
        }
        return input;
    }

    public static string ReadCode(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? "";
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}