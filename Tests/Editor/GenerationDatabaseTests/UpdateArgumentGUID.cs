namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class UpdateArgumentGUID : GenerationDatabaseTests
        {
            private const string NewGUID = "newGUID";
            private static ArgumentInfo _expectedArg;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedArg = new ArgumentInfo(_firstArg.TypeNameAndAssembly, NewGUID);
            }

            [Test]
            public void Updates_argument_GUID_in_arguments_list()
            {
                _database.UpdateArgumentGUIDImpl(_firstArg, NewGUID);

                Assert.IsTrue(_database.InstanceArgumentGenericTypes.Keys.Count == 2);
                Assert.Contains(_expectedArg, _database.InstanceArgumentGenericTypes.Keys.ToArray());
            }

            [Test]
            public void Updates_argument_GUID_in_concrete_classes()
            {
                _database.UpdateArgumentGUIDImpl(_firstArg, NewGUID);

                var concreteClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsTrue(concreteClasses.Any(concreteClass => concreteClass.Arguments.Contains(_expectedArg)));
            }

            [Test]
            public void Referenced_generic_types_can_be_found_by_new_argument()
            {
                _database.UpdateArgumentGUIDImpl(_firstArg, NewGUID);

                var genericTypes = _database.GetReferencedGenericTypesImpl(_expectedArg);
                Assert.IsTrue(genericTypes.Length != 0);
            }

            [Test]
            public void Updates_passed_argument_GUID()
            {
                _database.UpdateArgumentGUIDImpl(_firstArg, NewGUID);
                Assert.IsTrue(_firstArg.GUID == NewGUID);
            }
        }
    }
}