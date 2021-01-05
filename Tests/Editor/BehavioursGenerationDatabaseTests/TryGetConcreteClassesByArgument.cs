namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class TryGetConcreteClassesByArgument : BehavioursGenerationDatabaseTests
        {
            private static void AssertFalse()
            {
                bool success = _database.TryGetConcreteClassesByArgumentImpl(_behaviour, _firstArg, out ConcreteClass[] concreteClasses);

                Assert.IsFalse(success);
                Assert.IsNull(concreteClasses);
            }

            [Test]
            public void When_behaviour_and_argument_exist_returns_true_and_collection()
            {
                AddEntries();

                bool success = _database.TryGetConcreteClassesByArgumentImpl(_behaviour, _firstArg, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsNotNull(concreteClasses);
                Assert.IsTrue(concreteClasses.Length == 1);
            }

            [Test]
            public void When_behaviour_exists_but_argument_does_not_returns_false_and_null()
            {
                AddEntries();

                _database.RemoveArgumentImpl(_firstArg, (assemblyGUID) => { });

                AssertFalse();
            }

            [Test]
            public void When_behaviour_does_not_exist_but_argument_does_returns_false_and_null()
            {
                // Set up
                AddEntries();

                var secondBehaviour = new GenericTypeInfo("secondName", "secondGUID", new[] { "secondArgs" });
                _database.AddGenericBehaviourImpl(secondBehaviour, out List<ConcreteClass> _);
                _database.AddConcreteClassImpl(secondBehaviour, _firstSecondArgs, AssemblyGUID);

                // Action
                _database.RemoveGenericBehaviourImpl(_behaviour, _ => { });

                // Check
                AssertFalse();
            }

            [Test]
            public void When_behaviour_and_argument_do_not_exist_returns_false_and_null()
            {
                AssertFalse();
            }
        }
    }
}