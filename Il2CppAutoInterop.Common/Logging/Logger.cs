namespace Il2CppAutoInterop.Common.Logging;

public static class Logger
{
    public static ILogger Instance { get; set; }

    static Logger()
    {
        Instance = new ConsoleLogger();
    }
    
    private class ConsoleLogger : ILogger
    {
        public  void Log(string message, ConsoleColor color)
        {
            var cache = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = cache;
        }
        
        public void Log(string message) => Log(message, Console.ForegroundColor);
        
        public void Warning(string message) => Log(message, ConsoleColor.Yellow);
        
        public void Info(string message) => Log(message, ConsoleColor.Cyan);
        
        public void Error(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}