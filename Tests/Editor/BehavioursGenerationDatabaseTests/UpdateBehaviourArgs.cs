namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class UpdateBehaviourArgs : BehavioursGenerationDatabaseTests
        {
            private static readonly string[] NewArgs = { "newArgs" };
            private static GenericTypeInfo _expectedBehaviour;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedBehaviour = new GenericTypeInfo(_behaviour.TypeNameAndAssembly, _behaviour.GUID, NewArgs);
            }

            [Test]
            public void Updates_behaviour_arguments_in_behaviours_list()
            {
                _database.UpdateGenericTypeArgsImpl(_behaviour, NewArgs);

                Assert.IsTrue(_database.InstanceGenericTypes.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceGenericTypes);
            }

            [Test]
            public void Updates_behaviour_arguments_in_referenced_behaviours()
            {
                _database.UpdateGenericTypeArgsImpl(_behaviour, NewArgs);

                var behaviours = _database.GetReferencedGenericTypesImpl(_firstArg);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.ArgNames.SequenceEqual(NewArgs)));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                _database.UpdateGenericTypeArgsImpl(_behaviour, NewArgs);

                var concreteClasses = _database.GetConcreteClassesImpl(_behaviour);

                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_argument_argNames()
            {
                _database.UpdateGenericTypeArgsImpl(_behaviour, NewArgs);
                Assert.IsTrue(_behaviour.ArgNames == NewArgs);
            }
        }
    }
}