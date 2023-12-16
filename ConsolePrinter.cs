namespace BlufiPrinter;

internal static class ConsolePrinter
{
    public static void PrintInternal(params string[] args)
    {
        Console.WriteLine($"\u001b[36m{string.Join(' ', args)}\u001b[0m");
    }

    public static void Print(params string[] args)
    {
        Console.WriteLine($"\u001b[90m{DateTime.Now:[yyyy-MM-dd HH:mm:ss]}\u001b[0m{string.Join(' ', args)}");
    }
}
