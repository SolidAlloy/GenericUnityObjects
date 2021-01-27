# Generic Unity.Objects
[![openupm](https://img.shields.io/npm/v/com.solidalloy.generic-scriptable-objects?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.solidalloy.generic-scriptable-objects/) [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/SolidAlloy/GenericScriptableObjects/blob/main/LICENSE) ![Unity: 2020.2](https://img.shields.io/badge/unity-2020.2-yellow)

This package allows to create and use generic ScriptableObjects and MonoBehaviours in Unity3D. Although generic serializable classes are now supported by Unity 2020, generic ScriptableObject and MonoBehaviour are not yet, and this plugin allows to overcome this limitation.

**What you can do with it:**

- Implement generic classes that inherit from ScriptableObject or MonoBehaviour (e.g. `class GenericBehaviour<T> : MonoBehaviour { }`).
- Create assets/components from a context menu and choose the generic argument type in the process.
- Create object fields for generic Unity.Objects and assigned created assets/components to those fields.
- Instantiate generic Unity.Objects from scripts (but see the limitations.)

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
    "com.solidalloy.generic-unity-objects": "1.15.0"
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

If you use Unity 2020, you need to specify the `Serializable` attribute explicitly. Otherwise, fields of type `WarriorStats<TClass>` will not be serialized. If you use Unity 2021, this bug is fixed and Unity automatically marks generic Unity.Objects as serializable.

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

![](https://media.githubusercontent.com/media/SolidAlloy/GenericScriptableObjects/main/.asset-creation.gif?token=AFOZEDUUFHKZ3NXOYTFZFTS7ULXZE)

When you create an asset with certain generic arguments for the first time, a short compile dialog will show up. This is an expected behavior because the plugin needs to generate a non-generic class that derives from the generic class with the arguments you've chosen. Once the class is generated, a usual asset creation dialog will appear, where you will be prompted to enter the name of your new asset.

The type selection pop-up is powered by [ClassTypeReference-for-Unity](https://github.com/SolidAlloy/ClassTypeReference-for-Unity).

#### Referencing a generic ScriptableObject

You can create a serialized field for a generic ScriptableObject just like for the usual one:

```csharp
public class Knight : Class
{
    [SerializeField] private WarriorStats<Knight> _stats;
}
```

In Unity 2020, remember to add the Serializable attribute to your class to be able to reference it in other classes.

You will get the object field in the inspector:

![](https://raw.githubusercontent.com/SolidAlloy/GenericScriptableObjects/main/.object-field.png?token=AFOZEDWJGJ7L4OHVQ537XRS7ULQ4E)

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

***Warning!*** See the limitations of creating instances at runtime. **(TODO: Add link)**

#### File Naming

The file name of a generic Unity.Object must contain the name of the type ("WarriorStats" in `WarriorStats<TClass>`). Suffixes are up to you:

- WarriorStats`1.cs :heavy_check_mark:
- WarriorStatsOfTClass.cs :heavy_check_mark:
- WarriorStats.cs :heavy_check_mark:
- Stats.cs :x:

This way the plugin will be able to detect class name change.
