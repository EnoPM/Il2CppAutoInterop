using System.Diagnostics;

namespace Il2CppAutoInterop.Core.Utils;

public readonly struct TimedExecution : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly ConsoleColor _originalColor;
    private readonly ConsoleColor? _color;

    public TimedExecution(string message, ConsoleColor? color = null)
    {
        _originalColor = Console.ForegroundColor;
        _color = color;
        if (_color.HasValue)
        {
            Console.ForegroundColor = _color.Value;
        }
        
        Console.WriteLine($"{message}...");
        Console.ForegroundColor = _originalColor;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (_color.HasValue)
        {
            Console.ForegroundColor = _color.Value;
        }
        Console.WriteLine($"Done in {_stopwatch.Elapsed}");
        Console.ForegroundColor = _originalColor;
    }
}
