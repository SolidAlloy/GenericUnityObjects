namespace GenericUnityObjects.EditorTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class AddGenericType : GenerationDatabaseTests
        {
            [Test]
            public void Adds_generic_type_to_dict()
            {
                var expectedGenericTypes = new[] { _genericType };

                _database.AddGenericTypeImpl(_genericType);

                Assert.IsTrue(_database.InstanceGenericTypes.SequenceEqual(expectedGenericTypes));
                Assert.IsEmpty(_database.InstanceArguments);
            }

            [Test]
            public void Throws_ArgumentException_if_key_exists()
            {
                _database.AddGenericTypeImpl(_genericType);

                Assert.Throws<ArgumentException>(() =>
                {
                    _database.AddGenericTypeImpl(_genericType);
                });
            }
        }
    }
}