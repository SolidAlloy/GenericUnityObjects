namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using UnityEngine;

    /// <summary>
    /// All the work is done in the parent class. This is implemented just to create a ScriptableObject asset.
    /// </summary>
    internal partial class BehavioursGenerationDatabase : GenerationDatabase<MonoBehaviour>
    {
        public static void UpdateComponentName(BehaviourInfo behaviourInfo, string newComponentName)
        {
            ((BehavioursGenerationDatabase)Instance).UpdateComponentNameImpl(behaviourInfo, newComponentName);
        }

        public void UpdateComponentNameImpl(BehaviourInfo behaviourInfo, string newComponentName)
        {
            TemporarilyRemovingGenericType(behaviourInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref behaviourInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateComponentName(newComponentName);
                });
            });
        }

        public static void UpdateOrder(BehaviourInfo behaviourInfo, int newOrder)
        {
            ((BehavioursGenerationDatabase)Instance).UpdateOrderImpl(behaviourInfo, newOrder);
        }

        public void UpdateOrderImpl(BehaviourInfo behaviourInfo, int newOrder)
        {
            TemporarilyRemovingGenericType(behaviourInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref behaviourInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateOrder(newOrder);
                });
            });
        }
    }
}