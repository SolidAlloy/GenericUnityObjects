namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;
    using Util;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class UpdateBehaviourNameAndAssembly : GenericBehavioursDatabaseTests
        {
            private const string NewName = "newName";
            private const string NewAssembly = "newAssembly";
            private static BehaviourInfo _expectedBehaviour;
            private static TypeStub _typeStub;

            private static void CallUpdateArgumentNameAndAssembly() =>
                _database.InstanceUpdateBehaviourNameAndAssembly(ref _behaviour, _typeStub);

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
                _expectedBehaviour = new BehaviourInfo(NewName, NewAssembly, _behaviour.GUID);
            }

            [Test]
            public void Updates_behaviour_name_in_behaviours_list()
            {
                CallUpdateArgumentNameAndAssembly();

                Assert.IsTrue(_database.InstanceBehaviours.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceBehaviours);
            }

            [Test]
            public void Updates_behaviour_name_in_referenced_behaviours()
            {
                CallUpdateArgumentNameAndAssembly();

                bool success = _database.InstanceTryGetReferencedBehaviours(_firstArg, out BehaviourInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.TypeNameAndAssembly == TypeHelper.GetTypeNameAndAssembly(NewName, NewAssembly)));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                CallUpdateArgumentNameAndAssembly();

                bool success = _database.InstanceTryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_behaviour_TypeNameAndAssembly()
            {
                CallUpdateArgumentNameAndAssembly();
                Assert.IsTrue(_behaviour.TypeNameAndAssembly == TypeHelper.GetTypeNameAndAssembly(NewName, NewAssembly));
            }
        }
    }
}