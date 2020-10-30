# Generic ScriptableObjects
This package allows to create and use generic ScriptableObjects in Unity3D. Although generic serializable classes are now supported by Unity 2020, generic ScriptableObject and MonoBehaviour are not yet, and this plugin allows to overcome this limitation.

**What you can do with it:**

- Implement your own generic ScriptableObjects (like `Generic<T>`).
- Create them from a context menu and choose the generic argument type in the process.
- Create object fields for the generic ScriptableObjects and drag-and-drop the created assets to those fields.
- Instantiate generic ScriptableObjects from scripts (but see the limitations.)

**What you cannot do:**

- Instantiate a generic ScriptableObject from script if you haven't created a single asset of this ScriptableObject with such generic arguments (e.g. you are trying to instantiate `Generic<int>` but you haven't created a single `Generic<int>` asset yet.)

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

#### Context menu to create an asset

Now, to be able to create assets of this ScriptableObject from the context menu, you will need to implement a method with the [MenuItem](https://docs.unity3d.com/ScriptReference/MenuItem.html) attribute and use `GenericSOCreator.CreateAsset()` inside of it. The recommended way to do it is to create a separate class in the Editor folder:

```csharp
using GenericScriptableObjects.Editor;
using UnityEditor;

public static class WarriorStatsCreator
{
    [MenuItem(GenericSOCreator.AssetCreatePath + "Warrior Stats", priority = 0)]
    public static void CreateAsset() => GenericSOCreator.CreateAsset(typeof(WarriorStats<>));
}
```

But you can also do it in the same file:

```csharp
using GenericScriptableObjects;

#if UNITY_EDITOR
using GenericScriptableObjects.Editor;
using UnityEditor;
#endif

[System.Serializable]
public class WarriorStats<TClass> : GenericScriptableObject
    where TClass : Class
{
    public TClass Warrior;

    public int Health;
    public int Damage;
}

#if UNITY_EDITOR
public static class WarriorStatsCreator
{
    [MenuItem(GenericSOCreator.AssetCreatePath + "Warrior Stats", priority = 0)]
    public static void CreateAsset() => GenericSOCreator.CreateAsset(typeof(WarriorStats<>));
}
#endif
```

Why not a single attribute like `CreateAssetMenu`? Unfortunately, Unity doesn't allow to inherit from `CreateAssetMenu` or implement your own attribute to create assets, so this is the simplest way to be able to create custom assets. Please like [this post](https://forum.unity.com/threads/ability-to-create-custom-createassetmenu-derived-attributes.985262/) so that the Unity team notices this feature request sooner.

You need to provide the [MenuItem](https://docs.unity3d.com/ScriptReference/MenuItem.html) attribute with the whole context menu path, including "Assets/Create/...". You don't have to memorize it, just use `GenericSOCreator.AssetCreatePath` + the path you usually write in the CreateAssetMenu attribute.

Note that unlike CreateAssetMenu, MenuItem places the item to the bottom of the list by default, so if you want it on top, add `priority = 0`.

Now, you can create assets:

***place for gif***

When you create an asset with certain generic arguments for the first time, the assemblies will reload. This is an expected behavior because the plugin must create a non-generic class that inherits the generic class with the arguments you've chosen. Once the assemblies reload, the usual asset creation dialog will appear, where you will be prompted to enter the name of your new asset.

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

***image of the object field***

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

Warning! If you haven't created a single asset of type WarriorStats\<Knight>, the method will return null. You need to create an asset with a specific argument type at least once to be able to use CreateInstance with this argument type. This is needed to generate a concrete inheritor for this generic class. Without a concrete implementation, Unity won't allow to create a generic scriptable object.

