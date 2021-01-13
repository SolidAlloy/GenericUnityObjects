namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class GetConcreteClasses : BehavioursGenerationDatabaseTests
        {
            private static ConcreteClass[] CallMethod()
            {
                return _database.GetConcreteClassesImpl(_behaviour);
            }

            [Test]
            public void When_behaviour_does_not_exist_throws_KeyNotFound_exception()
            {
                Assert.Throws<KeyNotFoundException>(() => CallMethod());
            }

            [Test]
            public void When_behaviour_exists_returns_collection_with_items()
            {
                AddEntries();

                ConcreteClass[] concreteClasses = CallMethod();

                Assert.IsNotNull(concreteClasses);
                Assert.IsTrue(concreteClasses.Length == 1);
            }
        }
    }
}