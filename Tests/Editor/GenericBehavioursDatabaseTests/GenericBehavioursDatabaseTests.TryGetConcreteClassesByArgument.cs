namespace GenericUnityObjects.EditorTests
{
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class TryGetConcreteClassesByArgument : GenericBehavioursDatabaseTests
        {
            private static void AssertFalse()
            {
                bool success = _database.InstanceTryGetConcreteClassesByArgument(_behaviour, _firstArg, out ConcreteClass[] concreteClasses);

                Assert.IsFalse(success);
                Assert.IsNull(concreteClasses);
            }

            [Test]
            public void When_behaviour_and_argument_exist_returns_true_and_collection()
            {
                AddEntries();

                bool success = _database.InstanceTryGetConcreteClassesByArgument(_behaviour, _firstArg, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsNotNull(concreteClasses);
                Assert.IsTrue(concreteClasses.Length == 1);
            }

            public void When_behaviour_exists_but_argument_does_not_returns_false_and_null()
            {
                _database.InstanceRemoveArgument(_firstArg, (assemblyGUID) => { });
                AssertFalse();
            }

            public void When_behaviour_does_not_exist_but_argument_does_returns_false_and_null()
            {
                var secondBehaviour = new TypeInfo("secondName", "secondGUID");
                _database.InstanceAddGenericBehaviour(secondBehaviour);
                _database.InstanceAddConcreteClass(secondBehaviour, _firstSecondArgs, AssemblyGUID);
                _database.InstanceRemoveGenericBehaviour(_behaviour);

                AssertFalse();
            }

            public void When_behaviour_and_argument_do_not_exist_returns_false_and_null()
            {
                AssertFalse();
            }
        }
    }
}