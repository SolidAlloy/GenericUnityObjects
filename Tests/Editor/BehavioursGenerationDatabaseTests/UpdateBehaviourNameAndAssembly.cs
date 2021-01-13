namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;
    using Util;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class UpdateBehaviourNameAndAssembly : BehavioursGenerationDatabaseTests
        {
            private const string NewName = "newName";
            private const string NewAssembly = "newAssembly";
            private static GenericTypeInfo _expectedBehaviour;
            private static TypeStub _typeStub;

            private static void CallUpdateArgumentNameAndAssembly() =>
                _database.UpdateGenericTypeImpl(_behaviour, _typeStub);

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                _typeStub = new TypeStub(NewName, NewAssembly);
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedBehaviour = new GenericTypeInfo(NewName, NewAssembly, _behaviour.GUID, _behaviour.ArgNames);
            }

            [Test]
            public void Updates_behaviour_name_in_behaviours_list()
            {
                CallUpdateArgumentNameAndAssembly();

                Assert.IsTrue(_database.InstanceGenericTypes.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceGenericTypes);
            }

            [Test]
            public void Updates_behaviour_name_in_referenced_behaviours()
            {
                CallUpdateArgumentNameAndAssembly();

                var behaviours = _database.GetReferencedGenericTypesImpl(_firstArg);

                Assert.IsTrue(behaviours.Any(behaviour => behaviour.TypeNameAndAssembly == TypeUtility.GetTypeNameAndAssembly(NewName, NewAssembly)));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                CallUpdateArgumentNameAndAssembly();

                var concreteClasses = _database.GetConcreteClassesImpl(_behaviour);

                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_behaviour_TypeNameAndAssembly()
            {
                CallUpdateArgumentNameAndAssembly();
                Assert.IsTrue(_behaviour.TypeNameAndAssembly == TypeUtility.GetTypeNameAndAssembly(NewName, NewAssembly));
            }
        }
    }
}