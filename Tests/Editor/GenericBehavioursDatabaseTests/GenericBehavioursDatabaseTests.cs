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
        private static TypeInfo _behaviour;
        private static TypeInfo _firstArg;
        private static TypeInfo _secondArg;
        private static TypeInfo[] _firstSecondArgs;
        private static TypeInfo[] _expectedBehaviours;
        private static ConcreteClass _expectedConcreteClass;

        [SetUp]
        public virtual void BeforeEachTest()
        {
            _database = ScriptableObject.CreateInstance<GenericBehavioursDatabase>();
            _database.OnAfterDeserialize();

            _behaviour = new TypeInfo("genericBehaviourName", "genericBehaviourGUID");
            _firstArg = new TypeInfo("firstArgumentName", "firstArgumentGUID");
            _secondArg = new TypeInfo("secondArgumentName", "secondArgumentGUID");
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