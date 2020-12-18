namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class UpdateBehaviourFullName : GenericBehavioursDatabaseTests
        {
            private const string NewName = "newName";
            private static BehaviourInfo _expectedBehaviour;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedBehaviour = new BehaviourInfo(NewName, _behaviour.GUID);
            }

            [Test]
            public void Returns_updated_behaviour()
            {
                var actualBehaviour = _database.InstanceUpdateBehaviourFullName(_behaviour, NewName);
                Assert.AreEqual(actualBehaviour, _expectedBehaviour);
            }

            [Test]
            public void Updates_behaviour_name_in_behaviours_list()
            {
                _database.InstanceUpdateBehaviourFullName(_behaviour, NewName);

                Assert.IsTrue(_database.InstanceBehaviours.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceBehaviours);
            }

            [Test]
            public void Updates_behaviour_name_in_referenced_behaviours()
            {
                _database.InstanceUpdateBehaviourFullName(_behaviour, NewName);

                bool success = _database.InstanceTryGetReferencedBehaviours(_firstArg, out BehaviourInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.TypeFullName == NewName));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                _database.InstanceUpdateBehaviourFullName(_behaviour, NewName);

                bool success = _database.InstanceTryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Length != 0);
            }
        }
    }
}