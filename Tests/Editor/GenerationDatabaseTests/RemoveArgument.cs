namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class RemoveArgument : GenerationDatabaseTests
        {
            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
            }

            private static void CallRemoveArgument()
            {
                _database.RemoveArgumentImpl(_firstArg, null);
            }

            [Test]
            public void Removes_argument_from_arguments_list()
            {
                CallRemoveArgument();
                Assert.IsFalse(_database.InstanceArgumentGenericTypes.Keys.Contains(_firstArg));
            }

            [Test]
            public void Removes_concrete_classes_that_used_the_argument()
            {
                CallRemoveArgument();

                var actualClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsFalse(actualClasses.Contains(_expectedConcreteClass));
            }

            [Test]
            public void Executes_action_for_assembly_name_of_each_removed_class()
            {
                bool acceptedAssemblyName = false;

                _database.RemoveArgumentImpl(_firstArg, assemblyName =>
                {
                    if (assemblyName == AssemblyGUID)
                        acceptedAssemblyName = true;
                });

                Assert.IsTrue(acceptedAssemblyName);
            }

            [Test]
            public void When_argument_is_not_found_throws_KeyNotFound_exception()
            {
                CallRemoveArgument();

                Assert.Throws<KeyNotFoundException>(() =>
                {
                    _database.RemoveArgumentImpl(_firstArg, assemblyName => { });
                });
            }
        }
    }
}