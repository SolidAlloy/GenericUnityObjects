namespace GenericUnityObjects.EditorTests
{
    using Editor.MonoBehaviour;
    using NUnit.Framework;
    using UnityEngine;
    using TypeInfo = Editor.MonoBehaviour.TypeInfo;

    internal partial class GenericBehavioursDatabaseTests
    {
        private const string AssemblyGUID = "testAssemblyGUID";

        private static GenericBehavioursDatabase _database;
        private static BehaviourInfo _behaviour;
        private static ArgumentInfo _firstArg;
        private static ArgumentInfo _secondArg;
        private static ArgumentInfo[] _firstSecondArgs;
        private static BehaviourInfo[] _expectedBehaviours;
        private static ConcreteClass _expectedConcreteClass;

        [SetUp]
        public virtual void BeforeEachTest()
        {
            _database = ScriptableObject.CreateInstance<GenericBehavioursDatabase>();
            _database.OnAfterDeserialize();

            _behaviour = new BehaviourInfo("genericBehaviourName", "genericBehaviourGUID");
            _firstArg = new ArgumentInfo("firstArgumentName", "firstArgumentGUID");
            _secondArg = new ArgumentInfo("secondArgumentName", "secondArgumentGUID");
            _firstSecondArgs = new[] { _firstArg, _secondArg };
            _expectedBehaviours = new[] { _behaviour };
            _expectedConcreteClass = new ConcreteClass(_firstSecondArgs, AssemblyGUID);
        }

        private static void AddEntries()
        {
            _database.InstanceAddGenericBehaviour(_behaviour);
            _database.InstanceAddConcreteClass(_behaviour, _firstSecondArgs, AssemblyGUID);
        }
    }
}