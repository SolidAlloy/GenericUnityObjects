namespace GenericUnityObjects.EditorTests
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class AddGenericBehaviour : BehavioursGenerationDatabaseTests
        {
            [Test]
            public void Adds_behaviour_to_dict()
            {
                var expectedBehaviours = new[] { _behaviour };

                _database.InstanceAddGenericBehaviour(_behaviour);

                Assert.IsTrue(_database.InstanceBehaviours.SequenceEqual(expectedBehaviours));
                Assert.IsEmpty(_database.InstanceArguments);
            }

            [Test]
            public void Throws_ArgumentException_if_key_exists()
            {
                _database.InstanceAddGenericBehaviour(_behaviour);

                Assert.Throws<ArgumentException>(() =>
                {
                    _database.InstanceAddGenericBehaviour(_behaviour);
                });
            }
        }
    }
}