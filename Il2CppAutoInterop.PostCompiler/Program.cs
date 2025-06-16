using CommandLine;
using Il2CppAutoInterop.BepInEx;

namespace Il2CppAutoInterop.PostCompiler;

internal static class Program
{
    private static void Main(string[] args)
    {
        Parser.Default
            .ParseArguments<PostCompilerOptions>(args)
            .WithParsed(BepInExIl2CppInterop.Run)
            .WithNotParsed(HandleArgumentErrors);
    }

    private static void HandleArgumentErrors(IEnumerable<Error> errors)
    {
        foreach (var error in errors)
        {
            Console.Error.WriteLine($"Error: {error.Tag.ToString()}");
        }
    }
}