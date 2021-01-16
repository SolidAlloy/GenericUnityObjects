namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class GetReferencedGenericTypes : GenerationDatabaseTests
        {
            private static GenericTypeInfo[] CallMethod() => _database.GetReferencedGenericTypesImpl(_firstArg);

            [Test]
            public void When_argument_does_not_exist_throws_KeyNotFound_exception()
            {
                Assert.Throws<KeyNotFoundException>(() => CallMethod());
            }

            [Test]
            public void When_argument_exists_returns_collection_with_items()
            {
                AddEntries();

                var genericTypes = CallMethod();

                Assert.IsNotNull(genericTypes);
                Assert.IsTrue(genericTypes.Length == 1);
            }
        }
    }
}