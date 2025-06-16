# Il2CppAutoInterop

Il2CppAutoInterop is a post-compilation tool designed to automate and simplify interoperability between managed .NET
assemblies and Unity's IL2CPP runtime, specifically for BepInEx IL2CPP plugins.

## ✨ What it does

### **- Automated IL2CPP interop for BepInEx plugins:**

Takes a compiled .NET assembly containing a BepInEx IL2CPP plugin and transforms it to ensure proper compatibility and
execution within an IL2CPP Unity game.

### **- Serialized field handling:**

Detects serialized fields in MonoBehaviour and adjusts their types to be Il2CppInterop-compatible. The tool rewrites all
field usages across the assembly accordingly.

- If `--experimental-serialization` is disabled, field conversion code is injected into the `Awake()` method.
- If enabled, the tool implements `ISerializationCallbackReceiver` and injects conversion logic into
  `OnAfterDeserialize()`.

### **- MonoBehaviour registration:**

Injects the necessary IL code to register custom Unity components with the IL2CPP runtime, enabling correct behavior
at runtime.

### **- C# source generation for Unity Editor project:**

Optionally generates stub C# MonoBehaviour code in a Unity project. This allows developers to work with and serialize
components directly in the Unity Editor, easing plugin development.

## Command

### 💻 Usage:

```shell
Il2CppAutoInterop.exe \
  --input "path/to/input/plugin.dll" \
  --output "path/to/output/directory" \
  --BepInEx "path/to/BepInEx/directory" \
  --unity-project "path/to/unity/project/directory" \
  --experimental-serialization \
  --randomize-version
```

### 🛠️ Options:

| Required | Option                         | Type              | Description                                                                                                   |
|----------|--------------------------------|-------------------|---------------------------------------------------------------------------------------------------------------|
| Yes      | `--input` `-i`                 | Dll assembly path | BepInEx Il2Cpp plugin dll assembly                                                                            |
| Yes      | `--output` `-o`                | Directory path    | Directory in which will be output post-processed assembly                                                     |
| Yes      | `--BepInEx` `-b`               | Directory path    | BepInEx directory with il2Cpp interop generated                                                               |
| No       | `--unity-project` `-u`         | Directory path    | Unity project directory path                                                                                  |
| No       | `--experimental-serialization` | boolean           | Use experimental `ISerializationCallbackReceiver.OnAfterDeserialize` method instead of `MonoBehaviour.Awake`. |
| No       | `--randomize-version`          | boolean           | Randomize post compiled assembly version (contribution usage)                                                 |

## Examples

### **Code written by the developer:**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;

namespace MyPluginProject;

public class MyMonoBehaviour : MonoBehaviour
{
     public Slider mySlider;
     public int myInt;
     public string myString;
     public TextMeshProUGUI myText;

     private void Start()
     {
        this.StartCoroutine(CoStart());
     }
     
     private IEnumerator CoStart()
     {
        myText.SetText(myString);
        yield break;
     }
     
     public void OnSliderValueChanged(int value)
     {
         
     }
}
```

### **Decompiled code of the component after Il2CppAutoInterop post-processing:**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Attributes;

namespace MyPluginProject;

public class MyMonoBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
     public Slider __Il2CppAutoInterop_UsableField_mySlider;
     public int __Il2CppAutoInterop_UsableField_myInt;
     public string __Il2CppAutoInterop_UsableField_myString;
     public TextMeshProUGUI __Il2CppAutoInterop_UsableField_myText;

     public Il2CppReferenceField<Slider> mySlider;
     public Il2CppValueField<int> myInt;
     public Il2CppStringField myString;
     public Il2CppReferenceField<TextMeshProUGUI> myText;

     private void Start()
     {
        this.StartCoroutine(CoStart());
     }
     
     [HideFromIl2Cpp]
     private IEnumerator CoStart()
     {
        __Il2CppAutoInterop_UsableField_myText.SetText(__Il2CppAutoInterop_UsableField_myString);
        yield break;
     }
     
     public void OnSliderValueChanged(int value)
     {
         
     }

     private void __Il2CppAutoInterop_MyMonoBehaviour_AfterDeserializeMethod()
     {
        this.__Il2CppAutoInterop_UsableField_myButton = myButton.Get();
        this.__Il2CppAutoInterop_UsableField_myInt = myInt.Get();
        this.__Il2CppAutoInterop_UsableField_myString = myString.Get();
        this.__Il2CppAutoInterop_UsableField_myText = myText.Get();
     }
     
     private void Awake() => this.__Il2CppAutoInterop_MyMonoBehaviour_AfterDeserializeMethod();
}
```

### **Code generated in Unity Editor project:**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyPluginProject;

public class MyMonoBehaviour : MonoBehaviour
{
     public Slider mySlider;
     public int myInt;
     public string myString;
     public TextMeshProUGUI myText;
     
     public void OnSliderValueChanged(int value)
     {
         
     }
}
```
