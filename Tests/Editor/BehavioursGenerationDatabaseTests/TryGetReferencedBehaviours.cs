namespace GenericUnityObjects.EditorTests
{
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class TryGetReferencedBehaviours : GenericBehavioursDatabaseTests
        {
            private static bool CallMethodBool() =>
                _database.InstanceTryGetReferencedBehaviours(_firstArg, out _);

            private static BehaviourInfo[] CallMethodArray()
            {
                _database.InstanceTryGetReferencedBehaviours(_firstArg, out BehaviourInfo[] behaviours);
                return behaviours;
            }

            [Test]
            public void When_argument_exists_returns_true()
            {
                AddEntries();

                bool success = CallMethodBool();
                Assert.IsTrue(success);
            }

            [Test]
            public void When_argument_does_not_exist_returns_false()
            {
                bool success = CallMethodBool();
                Assert.IsFalse(success);
            }

            [Test]
            public void When_argument_exists_fills_collection_with_items()
            {
                AddEntries();

                var behaviours = CallMethodArray();

                Assert.IsNotNull(behaviours);
                Assert.IsTrue(behaviours.Length == 1);
            }

            [Test]
            public void When_argument_does_not_exist_makes_collection_null()
            {
                var behaviours = CallMethodArray();
                Assert.IsNull(behaviours);
            }
        }
    }
}