using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace TestPlugin;

[BepInPlugin("pm.eno.testplugin", "TestPlugin", "0.0.1")]
public class TestPlugin : BasePlugin
{
    public override void Load()
    {
        Register();
        var component = AddComponent<MyMonoBehaviour>();

        var casted = component.Cast<ISerializationCallbackReceiver>();
        System.Console.WriteLine($"Cast: {casted != null}");
    }

    private static void Register()
    {
        ClassInjector.RegisterTypeInIl2Cpp(typeof(MyAncestorMonoBehaviour), new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection(new Type[1] { typeof(ISerializationCallbackReceiver) })
        });
        ClassInjector.RegisterTypeInIl2Cpp<MyMonoBehaviour>(new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(ISerializationCallbackReceiver)])
        });
    }
}

public abstract class MyAncestorMonoBehaviour : MonoBehaviour
{
    protected MyAncestorMonoBehaviour(IntPtr ptr) : base(ptr) {}
}

public class MyMonoBehaviour(IntPtr ptr) : MyAncestorMonoBehaviour(ptr)
{
    public void OnBeforeSerialize()
    {
        System.Console.WriteLine("OnBeforeSerialize called");
    }

    public void OnAfterDeserialize()
    {
        System.Console.WriteLine("OnAfterDeserialize called");
    }
}