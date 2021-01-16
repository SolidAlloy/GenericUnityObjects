namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class GetConcreteClasses : GenerationDatabaseTests
        {
            private static ConcreteClass[] CallMethod()
            {
                return _database.GetConcreteClassesImpl(_genericType);
            }

            [Test]
            public void When_generic_type_does_not_exist_throws_KeyNotFound_exception()
            {
                Assert.Throws<KeyNotFoundException>(() => CallMethod());
            }

            [Test]
            public void When_generic_type_exists_returns_collection_with_items()
            {
                AddEntries();

                ConcreteClass[] concreteClasses = CallMethod();

                Assert.IsNotNull(concreteClasses);
                Assert.IsTrue(concreteClasses.Length == 1);
            }
        }
    }
}