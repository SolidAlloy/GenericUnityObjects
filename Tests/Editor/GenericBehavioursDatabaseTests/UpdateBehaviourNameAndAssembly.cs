namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class UpdateBehaviourNameAndAssembly : GenericBehavioursDatabaseTests
        {
            private const string NewName = "newName";
            private const string NewAssembly = "newAssembly";
            private static BehaviourInfo _expectedBehaviour;

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
                _database.InstanceUpdateBehaviourNameAndAssembly(ref _behaviour, NewName, NewAssembly);

                Assert.IsTrue(_database.InstanceBehaviours.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceBehaviours);
            }

            [Test]
            public void Updates_behaviour_name_in_referenced_behaviours()
            {
                _database.InstanceUpdateBehaviourNameAndAssembly(ref _behaviour, NewName, NewAssembly);

                bool success = _database.InstanceTryGetReferencedBehaviours(_firstArg, out BehaviourInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.TypeNameAndAssembly == TypeInfo.GetTypeNameAndAssembly(NewName, NewAssembly)));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                _database.InstanceUpdateBehaviourNameAndAssembly(ref _behaviour, NewName, NewAssembly);

                bool success = _database.InstanceTryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_behaviour_TypeNameAndAssembly()
            {
                _database.InstanceUpdateBehaviourNameAndAssembly(ref _behaviour, NewName, NewAssembly);
                Assert.IsTrue(_behaviour.TypeNameAndAssembly == TypeInfo.GetTypeNameAndAssembly(NewName, NewAssembly));
            }
        }
    }
}