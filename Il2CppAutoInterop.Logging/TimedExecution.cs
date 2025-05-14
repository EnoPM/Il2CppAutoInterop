using System.Diagnostics;

namespace Il2CppAutoInterop.Logging;

public readonly struct TimedExecution : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly ConsoleColor? _color;

    public TimedExecution(string message, ConsoleColor? color = null)
    {
        _stopwatch = Stopwatch.StartNew();
        _color = color;
        Log(message);
    }

    private void Log(string message)
    {
        if (_color.HasValue)
        {
            Logger.Instance.Log(message, _color.Value);
        }
        else
        {
            Logger.Instance.Log(message);
        }
    }

    public void Dispose()
    {
        Log($"Done in {_stopwatch.Elapsed}");
        _stopwatch.Start();
    }
}
