# Generic ScriptableObjects
[![openupm](https://img.shields.io/npm/v/com.solidalloy.generic-scriptable-objects?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.solidalloy.generic-scriptable-objects/) [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/SolidAlloy/GenericScriptableObjects/blob/main/LICENSE)

This package allows to create and use generic ScriptableObjects in Unity3D. Although generic serializable classes are now supported by Unity 2020, generic ScriptableObject and MonoBehaviour are not yet, and this plugin allows to overcome this limitation.

**What you can do with it:**

- Implement your own generic ScriptableObjects (e.g. `Generic<T>`).
- Create them from a context menu (similarly to the `[CreateAssetMenu]` attribute) and choose the generic argument type in the process.
- Create object fields for the generic ScriptableObjects and drag-and-drop the created assets to those fields.
- Instantiate generic ScriptableObjects from scripts (but see the limitations.)

**What you cannot do:**

- Instantiate a generic ScriptableObject from script if you haven't created a single asset of this scriptable object with such generic arguments (e.g. you are trying to instantiate `Generic<int>` but you haven't created a single `Generic<int>` asset yet.)

## How To Install

#### OpenUPM

Once you have the OpenUPM cli, run the following command:

```
openupm install com.solidalloy.generic-scriptable-objects
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
        "com.solidalloy.generic-scriptable-objects",
        "com.openupm"
      ]
    }
  ],
  "dependencies": {
    "com.solidalloy.generic-scriptable-objects": "1.0.0"
  },
```



#### Git URL

Project supports Unity Package Manager. To install it as a Git package do the following:

1. In Unity, open **Window** -> **Package Manager**.
2. Press the **+** button, choose "**Add package from git URL...**"
3. Enter "https://github.com/SolidAlloy/SolidUtilities.git" and press **Add**.
4. Do the same with two more packages:
   - https://github.com/SolidAlloy/ClassTypeReference-for-Unity.git
   - https://github.com/SolidAlloy/GenericScriptableObjects.git



## Usage

#### Implementing a generic ScriptableObject

To create a generic ScriptableObject, you need to derive the class from `GenericScriptableObject`:

```csharp
using GenericScriptableObjects;

[System.Serializable]
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

Note that you can skip the `Serializable` attribute. You will be able to create assets without it, but if you want to reference an asset in other classes' serialized fields, you need to include the attribute. Unity implicitly marks inheritors of `ScriptableObject` as serializable, but not if they are generic.

In this example, there is only one generic argument, but you can use as many as you want.

#### CreateGenericAssetMenu attribute

Now, to be able to create assets from the context menu, you can add the `[CreateGenericAssetMenu]` attribute to the class. It has all the same optional properties as the [[CreateAssetMenu]](https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html) attribute: FileName, MenuName, Order. But there are also two additional ones: NamespaceName and ScriptsPath.

- NamespaceName - Custom namespace name to set for auto-generated classes derived from this class. Default is "GenericScriptableObjectTypes".
- ScriptsPath - Custom path to a folder where auto-generated classes must be kept. Default is "Scripts/GenericSOTypes".

```csharp
using GenericScriptableObjects;

[System.Serializable]
[CreateGenericAssetMenu] // You can find "WarriorStats<TClass>" at the top of Create menu.
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

Now you can create assets:

![](https://raw.githubusercontent.com/SolidAlloy/GenericScriptableObjects/master/.asset-creation.gif)

When you create an asset with certain generic arguments for the first time, the assemblies will reload. This is an expected behavior because the plugin must create a non-generic class that is derived from the generic class with the arguments you've chosen. Once the assemblies reload, the usual asset creation dialog will appear, where you will be prompted to enter the name of your new asset.

The type selection pop-up is powered by [ClassTypeReference-for-Unity](https://github.com/SolidAlloy/ClassTypeReference-for-Unity).

#### Referencing a generic ScriptableObject

You can create a serialized field for a generic ScriptableObject just like for the usual one:

```csharp
public class Knight : Class
{
    [SerializeField] private WarriorStats<Knight> _stats;
}
```

Remember to add the Serializable attribute to your class to be able to reference it in other classes.

You will get the object field in the inspector:

![](https://raw.githubusercontent.com/SolidAlloy/GenericScriptableObjects/master/.object-field.gif)

Unfortunately, Unity shows *WarriorStats`1* there instead of *WarriorStats\<Knight>*. They are aware of [this problem](https://forum.unity.com/threads/generic-scriptable-object-fields.790763/), and hopefully it will be fixed soon.

#### Creating an instance at runtime

A generic ScriptableObject instance can be created at runtime. The CreateInstance method looks very similar, you just need pass a generic type instead:

```csharp
var knightStats = GenericScriptableObject.CreateInstance<WarriorStats<Knight>>();

var knightStats2 = GenericScriptableObject.CreateInstance(typeof(WarriorStats<Knight>));

var knightStats3 = GenericScriptableObject.CreateInstance(typeof(WarriorStats<>), typeof(Knight));
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

***Warning!*** If you haven't created a single asset of type WarriorStats\<Knight>, the method will return null. You need to create an asset with a specific argument type at least once to be able to use CreateInstance with this argument type. This is needed to generate a concrete inheritor for this generic class. Without a concrete implementation, Unity won't allow to create a generic scriptable object.

#### File Naming

When you create a GenericScriptableObject class, please name the file specifying the number of generic arguments at the end:

- WarriorStats`1.cs :heavy_check_mark:
- WarriorStats.cs :x:

This way you ensure that the plugin will not lose reference to the class and there will be no issues with instantiating your class if you decide to rename it later.

Also, when renaming a class, it's recommended to rename the file first, and then the class itself.