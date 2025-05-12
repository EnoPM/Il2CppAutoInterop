using Il2CppAutoInterop.Core.Contexts;
using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.BepInEx.Processors;

public class SerializedFieldProcessor : IFieldProcessor
{
    public readonly AutoInteropField Field;
    
    public SerializedFieldProcessor(AutoInteropField field)
    {
        Field = field;
    }
    
    public async Task<bool> ProcessAsync()
    {
        Console.WriteLine($"Processing field: {Field.Field.FullName}");
        return true;
    }
}