namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;
    using UnityEngine;

    internal partial class BehavioursGenerationDatabaseTests
    {
        private const string AssemblyGUID = "testAssemblyGUID";

        private static GenerationDatabase<BehavioursGenerationDatabase> _database;
        private static GenericTypeInfo _behaviour;
        private static ArgumentInfo _firstArg;
        private static ArgumentInfo _secondArg;
        private static ArgumentInfo[] _firstSecondArgs;
        private static GenericTypeInfo[] _expectedBehaviours;
        private static ConcreteClass _expectedConcreteClass;

        [SetUp]
        public virtual void BeforeEachTest()
        {
            _database = ScriptableObject.CreateInstance<BehavioursGenerationDatabase>();
            _database.Initialize();

            _behaviour = new GenericTypeInfo("genericBehaviourName", "genericBehaviourGUID", new[] { "genericArg" });
            _firstArg = new ArgumentInfo("firstArgumentName", "firstArgumentGUID");
            _secondArg = new ArgumentInfo("secondArgumentName", "secondArgumentGUID");
            _firstSecondArgs = new[] { _firstArg, _secondArg };
            _expectedBehaviours = new[] { _behaviour };
            _expectedConcreteClass = new ConcreteClass(_firstSecondArgs, AssemblyGUID);
        }

        private static void AddEntries()
        {
            _database.AddGenericBehaviourImpl(_behaviour, out List<ConcreteClass> _);
            _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
        }
    }
}