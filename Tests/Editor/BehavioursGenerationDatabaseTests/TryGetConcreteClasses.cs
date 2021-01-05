namespace GenericUnityObjects.EditorTests
{
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class TryGetConcreteClasses : BehavioursGenerationDatabaseTests
        {
            private static bool CallMethodBool() => _database.TryGetConcreteClassesImpl(_behaviour, out _);

            private static ConcreteClass[] CallMethodArray()
            {
                _database.TryGetConcreteClassesImpl(_behaviour, out ConcreteClass[] concreteClasses);
                return concreteClasses;
            }

            [Test]
            public void When_behaviour_exists_returns_true()
            {
                AddEntries();

                bool success = CallMethodBool();

                Assert.IsTrue(success);
            }

            [Test]
            public void When_behaviour_does_not_exist_returns_false()
            {
                bool success = CallMethodBool();

                Assert.IsFalse(success);
            }

            [Test]
            public void When_behaviour_exists_returns_collection_with_items()
            {
                AddEntries();

                ConcreteClass[] concreteClasses = CallMethodArray();

                Assert.IsNotNull(concreteClasses);
                Assert.IsTrue(concreteClasses.Length == 1);
            }

            [Test]
            public void When_behaviour_does_not_exist_makes_collection_null()
            {
                ConcreteClass[] concreteClasses = CallMethodArray();

                Assert.IsNull(concreteClasses);
            }
        }
    }
}