namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class UpdateBehaviourGUID : GenericBehavioursDatabaseTests
        {
            private const string NewGUID = "newGUID";
            private static BehaviourInfo _expectedBehaviour;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedBehaviour = new BehaviourInfo(_behaviour.TypeFullName, NewGUID);
            }

            [Test]
            public void Returns_updated_behaviour()
            {
                var actualBehaviour = _database.InstanceUpdateBehaviourGUID(_behaviour, NewGUID);
                Assert.AreEqual(actualBehaviour, _expectedBehaviour);
            }

            [Test]
            public void Updates_behaviour_GUID_in_behaviours_list()
            {
                _database.InstanceUpdateBehaviourGUID(_behaviour, NewGUID);

                Assert.IsTrue(_database.InstanceBehaviours.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceBehaviours);
            }

            [Test]
            public void Updates_behaviour_GUID_in_referenced_behaviours()
            {
                _database.InstanceUpdateBehaviourGUID(_behaviour, NewGUID);

                bool success = _database.InstanceTryGetReferencedBehaviours(_firstArg, out BehaviourInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.GUID == NewGUID));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                _database.InstanceUpdateBehaviourGUID(_behaviour, NewGUID);

                bool success = _database.InstanceTryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Length != 0);
            }
        }
    }
}