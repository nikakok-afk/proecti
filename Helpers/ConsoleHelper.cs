using Spectre.Console;

namespace proecti.Helpers;

public static class ConsoleHelper
{
    public static int ReadInt(string prompt)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>($"[cyan]{Markup.Escape(prompt)}[/]")
                .ValidationErrorMessage("[red]გთხოვთ შეიყვანოთ ვალიდური რიცხვი.[/]")
        );
    }

    public static int ReadPositiveInt(string prompt, string errorMessage)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>($"[cyan]{Markup.Escape(prompt)}[/]")
                .ValidationErrorMessage("[red]გთხოვთ შეიყვანოთ ვალიდური რიცხვი.[/]")
                .Validate(val => val > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]{Markup.Escape(errorMessage)}[/]"))
        );
    }

    public static string ReadNonEmptyString(string prompt, string errorMessage)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[cyan]{Markup.Escape(prompt)}[/]")
                .Validate(input => !string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]{Markup.Escape(errorMessage)}[/]"))
        ).Trim();
    }

    public static string ReadCode(string prompt)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[yellow]{Markup.Escape(prompt)}[/]")
                .PromptStyle("red")
                .Secret('*') 
        ).Trim();
    }

    public static void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[bold green]✔ {Markup.Escape(message)}[/]");
    }

    public static void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[bold yellow]⚠ {Markup.Escape(message)}[/]");
    }

    public static void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[bold red]✖ {Markup.Escape(message)}[/]");
    }

    public static void WriteHeader(string title, string borderColor = "cyan")
    {
        var rule = new Rule($"[bold {borderColor}]{Markup.Escape(title)}[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.WriteLine();
        AnsiConsole.Write(rule);
    }
}