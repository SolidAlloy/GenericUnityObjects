namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class UpdateArgumentFullName : GenericBehavioursDatabaseTests
        {
            private const string NewName = "newName";
            private static ArgumentInfo _expectedArg;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedArg = new ArgumentInfo(NewName, _firstArg.GUID);
            }

            [Test]
            public void Returns_updated_argument()
            {
                var actualArgument = _database.InstanceUpdateArgumentFullName(_firstArg, NewName);
                Assert.AreEqual(actualArgument, _expectedArg);
            }

            [Test]
            public void Updates_argument_name_in_arguments_list()
            {
                _database.InstanceUpdateArgumentFullName(_firstArg, NewName);

                Assert.IsTrue(_database.InstanceArguments.Length == 2);
                Assert.Contains(_expectedArg, _database.InstanceArguments);
            }

            [Test]
            public void Updates_argument_name_in_concrete_classes()
            {
                _database.InstanceUpdateArgumentFullName(_firstArg, NewName);

                bool success = _database.InstanceTryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Any(concreteClass => concreteClass.Arguments.Contains(_expectedArg)));
            }

            [Test]
            public void Referenced_behaviours_can_be_found_by_new_argument()
            {
                _database.InstanceUpdateArgumentFullName(_firstArg, NewName);

                bool success = _database.InstanceTryGetReferencedBehaviours(_expectedArg, out BehaviourInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Length != 0);
            }
        }
    }
}