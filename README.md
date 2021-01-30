# Generic UnityEngine.Objects
[![openupm](https://img.shields.io/npm/v/com.solidalloy.generic-scriptable-objects?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.solidalloy.generic-scriptable-objects/) [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/SolidAlloy/GenericScriptableObjects/blob/main/LICENSE) ![Unity: 2020.2](https://img.shields.io/badge/unity-2020.2-yellow) ![.NET 4.x](https://img.shields.io/badge/.NET-4.x-9cf)

This package allows to create and use generic ScriptableObjects and MonoBehaviours in Unity3D. Although generic serializable classes are now supported by Unity 2020, generic ScriptableObject and MonoBehaviour are not yet, and this plugin allows to overcome this limitation.

**What you can do with it:**

- Implement generic classes that inherit from ScriptableObject or MonoBehaviour (e.g. `class GenericBehaviour<T> : MonoBehaviour { }`).
- Create assets/components from a context menu and choose the generic argument type in the process.
- Create object fields for generic UnityEngine.Objects and assigned created assets/components to those fields.
- Instantiate generic UnityEngine.Objects from scripts (but see the limitations.)

## How To Install

#### OpenUPM

Once you have the OpenUPM cli, run the following command:

```
openupm install com.solidalloy.generic-unity-objects
```

Or if you don't have it, add the scoped registry to manifest.json with the desired dependency semantic version:

```json
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.solidalloy.util",
        "com.solidalloy.type.references",
        "com.solidalloy.generic-unity-objects",
        "com.openupm"
      ]
    }
  ],
  "dependencies": {
    "com.solidalloy.generic-unity-objects": "1.1.5"
  },
```



#### Git URL

Project supports Unity Package Manager. To install it as a Git package do the following:

1. In Unity, open **Window** -> **Package Manager**.
2. Press the **+** button, choose "**Add package from git URL...**"
3. Enter "https://github.com/SolidAlloy/SolidUtilities.git" and press **Add**.
4. Do the same with two more packages:
   - https://github.com/SolidAlloy/ClassTypeReference-for-Unity.git
   - https://github.com/SolidAlloy/GenericUnityObjects.git



## Generic ScriptableObject Usage

#### Implementing a generic ScriptableObject

To create a generic ScriptableObject, you need to derive the class from `GenericScriptableObject`:

```csharp
using System;
using GenericScriptableObjects;

[Serializable]
public class WarriorStats<TClass> : GenericScriptableObject
    where TClass : Class
{
    public int Health;
    public int Damage;
        
	public TClass[] FindAllWarriorsWithTheseStats()
    {
        return FindObjectsOfType<TClass>();
    }
}
```

If you use Unity 2020, you need to specify the `Serializable` attribute explicitly. Otherwise, fields of type `WarriorStats<TClass>` will not be serialized. If you use Unity 2021, this bug is fixed and Unity automatically marks generic UnityEngine.Objects as serializable.

In this example, there is only one generic argument, but you can use as many as you want.

#### CreateGenericAssetMenu attribute

Now, to be able to create assets from the context menu, you can add the `[CreateGenericAssetMenu]` attribute to the class. It has all the same optional properties as the [[CreateAssetMenu]](https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html) attribute: FileName, MenuName, Order.

```csharp
using System;
using GenericScriptableObjects;

[Serializable]
[CreateGenericAssetMenu] // You can find "WarriorStats<TClass>" at the top of Create menu.
public class WarriorStats<TClass> : GenericScriptableObject
    where TClass : Class
{
    // ...
}
```

Now you can create assets:

![Asset Creation GIF](https://media.githubusercontent.com/media/SolidAlloy/GenericUnityObjects/main/.asset-creation.gif)

When you create an asset with certain generic arguments for the first time, a short compile dialog will show up. This is an expected behavior because the plugin needs to generate a non-generic class that derives from the generic class with the arguments you've chosen. Once the class is generated, a usual asset creation dialog will appear, where you will be prompted to enter the name of your new asset.

The type selection pop-up is powered by [ClassTypeReference-for-Unity](https://github.com/SolidAlloy/ClassTypeReference-for-Unity).

#### Creating an instance at runtime

A generic ScriptableObject instance can be created at runtime. The CreateInstance method looks very similar, you just need pass a generic type instead:

```csharp
var knightStats = GenericScriptableObject.CreateInstance<WarriorStats<Knight>>();

var knightStats2 = GenericScriptableObject.CreateInstance(typeof(WarriorStats<Knight>));
```

You can also use these methods inside the generic ScriptableObject without specifying the GenericScriptableObject class:

```csharp
public class WarriorStats<TClass> : GenericScriptableObject
    where TClass : Class
{
    public int Health;
    public int Damage;
        
    public WarriorStats<TClass> Create()
    {
        // Use without "GenericScriptableObject."
        var stats = CreateInstance<WarriorStats<TClass>>();
        stats.Health = 100;
        return stats;
    }
}
```

***Warning!*** [See the limitations](#Limitations) of creating instances at runtime.

## Generic MonoBehaviour Usage

#### Implementing a generic MonoBehaviour

Unlike generic ScriptableObjects, Generic MonoBehaviour can inherit directly from MonoBehaviour:

```csharp
using System;
using UnityEngine;

[Serializable]
public class Unit<TWarrior> : MonoBehaviour
    where TWarrior : Warrior
{
    public TWarrior[] Warriors;
}
```

Note that the `Serializable` attribute is needed for a generic class to be serialized by Unity 2020. It is fixed in Unity 2021, and you don't need to put `[Serializable]` above generic UnityEngine.Objects.

Once the script is saved, you will be able to add a generic component through the **Add Component** button:

![Add Component GIF](https://media.githubusercontent.com/media/SolidAlloy/GenericUnityObjects/main/.add-component.gif)

#### Creating an instance at runtime

A generic MonoBehaviour component can be manipulated at runtime. The method names are the same, only "Component" part is replaced with "GenericComponent":

```csharp
var archersSquad = gameObject.AddGenericComponent<Unit<Archer>>();
var knightsGroupComponent = gameObject.GetGenericComponent(typeof(Unit<Knight>));
```

***Warning!*** [See the limitations](#Limitations) of creating instances at runtime.



## Common

#### Referencing a generic UnityEngine.Object

You can create a serialized field for a generic ScriptableObject or MonoBehaviour just like for the usual one:

```csharp
public class Knight : Class
{
    [SerializeField] private WarriorStats<Knight> _stats;
}
```

In Unity 2020, remember to add the Serializable attribute to your class to be able to reference it in other classes.

You will get an object field in the inspector:

![Object Field](https://raw.githubusercontent.com/SolidAlloy/GenericScriptableObjects/main/.object-field.png)

#### File Naming

The file name of a generic UnityEngine.Object must contain the name of the type (e.g. "WarriorStats" in `WarriorStats<TClass>`). Suffixes are up to you:

- WarriorStats`1.cs :heavy_check_mark:
- WarriorStatsOfTClass.cs :heavy_check_mark:
- WarriorStats.cs :heavy_check_mark:
- Stats.cs :x:

This way the plugin will be able to detect a class name change.

## Limitations

There are a few limitations that cannot be overcome, unfortunately.

##### If a GenericScriptableObject with specific generic arguments has not been created through the Assets/Create menu yet, you can instantiate it at runtime, but cannot create an asset out of it.

Let's say you have a GenericScriptableObject class called `GenericSO<T>`, and you created a `GenericSO<int>` asset through the **Assets/Create** menu. Then, in a script, you call `GenericScriptableObject.CreateInstance<GenericSO<int>>()` and `GenericScriptableObject.CreateInstance<GenericSO<bool>>()`. Both instances will be created just fine. However, `AssetDatabase.CreateAsset<GenericSO<int>>()` will be successful, but an asset created with `AssetDatabase.CreateAsset<GenericSO<bool>>>()` will start showing "Missing Mono Script" after assemblies recompilation.

When you create a GenericScriptableObject asset with new sequence of generic arguments, a concrete class is generated to support this specific sequence. But if you create an instance with new sequence of generic arguments at runtime, the new concrete class cannot be generated without recompilation of the assembly, so it is emitted and will be destroyed after the program quits.

##### If a generic MonoBehaviour with specific generic arguments has not been added as component through the Add Component button yet, you can add it in play mode, but not in edit mode.

The principle is the same as with GenericScriptableObjects. Let's say you have a MonoBehaviour class called `GenericBehaviour<T>`, and have already added a `GenericBehaviour<int>` component through the **Add Component** button. In **Play Mode**, both calls will be fine: `gameObject.AddGenericComponent<GenericBehaviour<int>>()` and `gameObject.AddGenericComponent<GenericBehaviour<bool>>()`. But in Edit Mode, `gameObject.AddGenericComponent<GenericBehaviour<bool>>()` will produce a component that will start showing "Missing Mono Script" after recompilation.

##### If a generic UnityEngine.Object with specific generic arguments has not been created/added through Editor UI, it cannot be instantiated in IL2CPP builds.

If you've created an asset of `GenericSO<int>` through the **Assets/Create** menu once, a supporting concrete class has already been generated, so you can instantiate `GenericSO<int>` in IL2CPP build. But `GenericScriptableObject.CreateInstance<GenericSO<bool>>()` will throw `NotSupportedException` because an underlying concrete class was not generated in Editor, and IL2CPP doesn't support [Reflection.Emit](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit) to generate classes dynamically.

##### Using a generic class as a generic argument is prohibited (e.g. `GenericBehaviour<AnotherClass<int>>`).

In theory, it can be implemented, but it will add more complexity to the system and is used so rarely that I decided not to add such a feature.

##### Generic UnityEngine.Object cannot be internal

Otherwise, when a concrete class is generated, it cannot access the constructor of the internal generic class. [IgnoreAccessCheckTo](https://www.strathweb.com/2018/10/no-internalvisibleto-no-problem-bypassing-c-visibility-rules-with-roslyn/) should work but in Unity it doesn't for some reason. You will be able to create assets and add components of internal generic types, and see their fields in the inspector just fine, but every time you instantiate a generic UnityEngine.Object, an error will show up in the Console.

## Custom Editors

#### MonoScript field

By default, inspector shows incorrect script in the ***Script*** field of generic objects. To draw the MonoScript field correctly, the plugin uses a custom editor for MonoBehaviour and GenericScriptableObject types. If you need to implement your own custom editor but still want to see the correct ***Script*** field, use the GenericUnityObjectHelper class. Instantiate a helper in `OnEnable()`, then draw the ***Script*** field inside `OnInspectorGUI()` with the `DrawMonoScript(property)` method.

You can also disable custom editors completely by defining the `DISABLE_GENERIC_OBJECT_EDITOR` directive.

#### Object field

Unity can't handle object fields for generic objects properly. For example, it will show ``GenericBehaviour`1`` instead of `GenericBehaviour<int>`, and will not list assets when you want to choose a generic ScriptableObject.

The plugin uses custom ObjectField() methods for fields of generic objects. You can also use it in your custom editor. **GenericObjectDrawer** class overloads of EditorGUI.ObjectField and EditorGUILayout.ObjectField that support generic objects.

## Contributing

First-of-all, thank you for considering contributing to the project!

It may be useful to enable additional debug logs during development. They will let you know if the plugin processes generic types as expected, how much time it takes, etc. You can do it by defining `GENERIC_UNITY_OBJECTS_DEBUG` in the project.

There is a number of unit tests, they run pretty fast, but they only cover the `GenerationDatabase` class for now. Integration tests, on the other hand, cover most of the code but they take some time to run, so it's recommended that you run only the ones that affect a part of the code you changed. It's also better to run them in an empty project where only the GenericUnityObjects plugin is installed.