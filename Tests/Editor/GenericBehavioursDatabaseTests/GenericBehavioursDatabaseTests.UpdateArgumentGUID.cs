namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class UpdateArgumentGUID : GenericBehavioursDatabaseTests
        {
            private const string NewGUID = "newGUID";
            private static TypeInfo _expectedArg;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedArg = new TypeInfo(_firstArg.TypeFullName, NewGUID);
            }

            [Test]
            public void Returns_updated_argument()
            {
                var actualArgument = _database.InstanceUpdateArgumentGUID(_firstArg, NewGUID);
                Assert.AreEqual(actualArgument, _expectedArg);
            }

            [Test]
            public void Updates_argument_GUID_in_arguments_list()
            {
                _database.InstanceUpdateArgumentGUID(_firstArg, NewGUID);

                Assert.IsTrue(_database.InstanceArguments.Length == 2);
                Assert.Contains(_expectedArg, _database.InstanceArguments);
            }

            [Test]
            public void Updates_argument_GUID_in_concrete_classes()
            {
                _database.InstanceUpdateArgumentGUID(_firstArg, NewGUID);

                bool success = _database.InstanceTryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Any(concreteClass => concreteClass.Arguments.Contains(_expectedArg)));
            }

            [Test]
            public void Referenced_behaviours_can_be_found_by_new_argument()
            {
                _database.InstanceUpdateArgumentGUID(_firstArg, NewGUID);

                bool success = _database.InstanceTryGetReferencedBehaviours(_expectedArg, out TypeInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Length != 0);
            }
        }
    }
}