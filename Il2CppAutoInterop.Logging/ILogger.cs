namespace Il2CppAutoInterop.Logging;

public interface ILogger
{
    void Log(string message, ConsoleColor color);
    
    void Log(string message);
    
    void Warning(string message);
    
    void Error(string message);
    
    void Info(string message);
}