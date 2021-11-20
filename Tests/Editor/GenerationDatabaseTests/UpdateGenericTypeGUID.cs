namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class UpdateGenericTypeGUID : GenerationDatabaseTests
        {
            private const string NewGUID = "newGUID";
            private static GenericTypeInfo _expectedBehaviour;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedBehaviour = new GenericTypeInfo(_genericType.TypeNameAndAssembly, NewGUID, _genericType.ArgNames);
            }

            [Test]
            public void Updates_behaviour_GUID_in_behaviours_list()
            {
                _database.UpdateGenericTypeImpl(_genericType, genericTypeInfo => genericTypeInfo.UpdateGUID(NewGUID));

                Assert.IsTrue(_database.InstanceGenericTypeArguments.Keys.Count == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceGenericTypeArguments.Keys.ToArray());
            }

            [Test]
            public void Updates_behaviour_GUID_in_referenced_behaviours()
            {
                _database.UpdateGenericTypeImpl(_genericType, genericTypeInfo => genericTypeInfo.UpdateGUID(NewGUID));

                var behaviours = _database.GetReferencedGenericTypesImpl(_firstArg);

                Assert.IsTrue(behaviours.Any(behaviour => behaviour.GUID == NewGUID));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                _database.UpdateGenericTypeImpl(_genericType, genericTypeInfo => genericTypeInfo.UpdateGUID(NewGUID));

                var concreteClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_argument_GUID()
            {
                _database.UpdateGenericTypeImpl(_genericType, genericTypeInfo => genericTypeInfo.UpdateGUID(NewGUID));
                Assert.IsTrue(_genericType.GUID == NewGUID);
            }
        }
    }
}