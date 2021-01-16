namespace GenericUnityObjects.EditorTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class AddGenericBehaviour : BehavioursGenerationDatabaseTests
        {
            [Test]
            public void Adds_behaviour_to_dict()
            {
                var expectedBehaviours = new[] { _behaviour };

                _database.AddGenericTypeImpl(_behaviour);

                Assert.IsTrue(_database.InstanceGenericTypes.SequenceEqual(expectedBehaviours));
                Assert.IsEmpty(_database.InstanceArguments);
            }

            [Test]
            public void Throws_ArgumentException_if_key_exists()
            {
                _database.AddGenericTypeImpl(_behaviour);

                Assert.Throws<ArgumentException>(() =>
                {
                    _database.AddGenericTypeImpl(_behaviour);
                });
            }
        }
    }
}