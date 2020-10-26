# Generic ScriptableObjects
This package allows to create and use generic ScriptableObjects in Unity3D. Although generic serializable classes are now supported by Unity 2020, generic ScriptableObject and MonoBehaviour are not yet, and this plugin allows to overcome this limitation.

**What you can do with it:**

- Implement your own generic ScriptableObjects (like Generic\<T>)
- Create them from a context menu and choose the generic argument type in the process.
- Create object fields for the generic ScriptableObjects and drag-and-drop the created assets to those fields.
- Instantiate generic ScriptableObjects from scripts (but see the limitations.)

**What you cannot do:**

- Instantiate a generic ScriptableObject from script if you haven't created a single asset of this ScriptableObject with such generic arguments (e.g. you are trying to instantiate Generic\<int> but you haven't created a single Generic\<int> asset yet.)

## Usage

To create a generic ScriptableObject, you need to derive the class from GenericScriptableObject:

```csharp
using GenericScriptableObjects;

[System.Serializable]
public class WarriorStats<TClass> : GenericScriptableObject
    where TClass : Class
{
    public TClass Warrior;

    public int Health;
    public int Damage;
}
```

Note that you can skip the Serializable attribute. You will be able to create assets without it, but if you want to reference an asset in object fields, you need to include it. Unity implicitly marks inheritors of ScriptableObject as serializable, but not if they are generic.

In this example, there is only one generic argument, but you can use as many as you want.

Now, to be able to create assets of this ScriptableObject from context menu, you will need to implement a method with the MenuItem attribute and use GenericSOCreator.CreateAsset inside of it. The recommended way to do it is to create a separate class in the Editor folder:

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
        
#if UNITY_EDITOR
    [MenuItem(GenericSOCreator.AssetCreatePath + "Warrior Stats", priority = 0)]
    public static void CreateAsset() => GenericSOCreator.CreateAsset(typeof(WarriorStats<>));
#endif
}
```

Why not a single attribute like CreateAssetMenu? Unfortunately, Unity doesn't allow to inherit from CreateAssetMenu or implement your own attribute to create assets, so this is the simplest way to be able to create custom assets. Please like [this post](https://forum.unity.com/threads/ability-to-create-custom-createassetmenu-derived-attributes.985262/) so that Unity team notices this feature request and adds it sooner.

