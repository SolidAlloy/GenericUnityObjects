namespace GenericUnityObjects.EditorTests
{
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;
    using UnityEngine;

    internal partial class GenerationDatabaseTests
    {
        private const string AssemblyGUID = "testAssemblyGUID";

        private static GenerationDatabase<MonoBehaviour> _database;
        private static GenericTypeInfo _genericType;
        private static ArgumentInfo _firstArg;
        private static ArgumentInfo _secondArg;
        private static ArgumentInfo[] _firstSecondArgs;
        private static GenericTypeInfo[] _expectedGenericTypes;
        private static ConcreteClass _expectedConcreteClass;

        [SetUp]
        public virtual void BeforeEachTest()
        {
            _database = ScriptableObject.CreateInstance<BehavioursGenerationDatabase>();
            _database.Initialize();

            _genericType = new BehaviourInfo("genericTypeName", "genericTypeGUID", new[] { "genericArg" });
            _firstArg = new ArgumentInfo("firstArgumentName", "firstArgumentGUID");
            _secondArg = new ArgumentInfo("secondArgumentName", "secondArgumentGUID");
            _firstSecondArgs = new[] { _firstArg, _secondArg };
            _expectedGenericTypes = new[] { _genericType };
            _expectedConcreteClass = new ConcreteClass(_firstSecondArgs, AssemblyGUID);
        }

        private static void AddEntries()
        {
            _database.AddGenericTypeImpl(_genericType);
            _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
        }
    }
}