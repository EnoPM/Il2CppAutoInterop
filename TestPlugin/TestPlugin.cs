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
        ClassInjector.RegisterTypeInIl2Cpp<MyMonoBehaviour>(new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(ISerializationCallbackReceiver)])
        });

        var component = AddComponent<MyMonoBehaviour>();

        var casted = component.Cast<ISerializationCallbackReceiver>();
        System.Console.WriteLine($"Cast: {casted != null}");
    }
}

public class MyMonoBehaviour : MonoBehaviour
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