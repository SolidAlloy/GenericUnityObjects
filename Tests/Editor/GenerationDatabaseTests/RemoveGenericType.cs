namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class RemoveGenericType : GenerationDatabaseTests
        {
            private static GenericTypeInfo _secondGenericType;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                _secondGenericType = new GenericTypeInfo("secondGenericTypeName", "secondGenericTypeGUID", new[] { "secondArgs" });

                _database.AddGenericTypeImpl(_genericType);
                _database.AddGenericTypeImpl(_secondGenericType);

                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
                _database.AddConcreteClassImpl(_secondGenericType, _firstSecondArgs, AssemblyGUID);
            }

            private void CallRemoveGenericType(GenericTypeInfo behaviour) => _database.RemoveGenericTypeImpl(behaviour, null);

            [Test]
            public void Removes_generic_type_from_generic_types_list()
            {
                CallRemoveGenericType(_genericType);

                Assert.IsFalse(_database.InstanceGenericTypeArguments.Keys.Contains(_genericType));
            }

            [Test]
            public void Removes_generic_type_from_referenced_generic_types_of_each_argument()
            {
                CallRemoveGenericType(_genericType);

                var firstGenericTypes = _database.GetReferencedGenericTypesImpl(_firstArg);
                Assert.IsFalse(firstGenericTypes.Contains(_genericType));

                var secondGenericTypes = _database.GetReferencedGenericTypesImpl(_secondArg);
                Assert.IsFalse(secondGenericTypes.Contains(_genericType));
            }

            [Test]
            public void When_argument_has_only_this_generic_type_referenced_removes_argument_from_arguments_list()
            {
                CallRemoveGenericType(_genericType);
                CallRemoveGenericType(_secondGenericType);

                Assert.IsEmpty(_database.InstanceArgumentGenericTypes.Keys);
            }

            [Test]
            public void When_generic_type_is_not_found_throws_KeyNotFound_exception()
            {
                CallRemoveGenericType(_genericType);
                CallRemoveGenericType(_secondGenericType);

                Assert.Throws<KeyNotFoundException>(() =>
                {
                    _database.RemoveGenericTypeImpl(_genericType, null);
                });
            }
        }
    }
}