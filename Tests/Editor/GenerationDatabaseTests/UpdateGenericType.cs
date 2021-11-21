namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;
    using Util;

    internal partial class GenerationDatabaseTests
    {
        public class UpdateGenericType : GenerationDatabaseTests
        {
            private const string NewName = "newName";
            private const string NewAssembly = "newAssembly";
            private const string ArgName = "newArg";
            private static readonly string[] _newArgs = { ArgName };

            private static GenericTypeInfo _expectedGenericType;
            private static TypeStub _typeStub;

            private static void CallUpdateGenericType() =>
                _database.UpdateGenericTypeImpl(_genericType, info =>
                {
                    info.UpdateNameAndAssembly(_typeStub);
                    info.UpdateArgNames(_typeStub.GetGenericArguments());
                });

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                var argTypeStub = new TypeStub(ArgName, "argAssembly");
                _typeStub = new TypeStub(NewName, NewAssembly, new[] { argTypeStub });
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedGenericType = new GenericTypeInfo(NewName, NewAssembly, _genericType.GUID, _newArgs);
            }

            [Test]
            public void Updates_generic_type_name_in_behaviours_list()
            {
                CallUpdateGenericType();

                Assert.IsTrue(_database.InstanceGenericTypeArguments.Keys.Count == 1);
                Assert.Contains(_expectedGenericType, _database.InstanceGenericTypeArguments.Keys.ToArray());
            }

            [Test]
            public void Updates_generic_type_name_in_referenced_behaviours()
            {
                CallUpdateGenericType();

                var genericTypes = _database.GetReferencedGenericTypesImpl(_firstArg);

                Assert.IsTrue(genericTypes.Any(genericType =>
                    genericType.TypeNameAndAssembly == TypeUtility.GetTypeNameAndAssembly(NewName, NewAssembly)));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_generic_types()
            {
                CallUpdateGenericType();

                var concreteClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_generic_type_TypeNameAndAssembly()
            {
                CallUpdateGenericType();
                Assert.IsTrue(_genericType.TypeNameAndAssembly == TypeUtility.GetTypeNameAndAssembly(NewName, NewAssembly));
            }

            [Test]
            public void Updates_generic_type_arguments_in_generic_types_list()
            {
                CallUpdateGenericType();

                Assert.IsTrue(_database.InstanceGenericTypeArguments.Keys.Count == 1);
                Assert.Contains(_expectedGenericType, _database.InstanceGenericTypeArguments.Keys.ToArray());
            }

            [Test]
            public void Updates_generic_types_arguments_in_referenced_behaviours()
            {
                CallUpdateGenericType();

                var behaviours = _database.GetReferencedGenericTypesImpl(_firstArg);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.ArgNames.SequenceEqual(_newArgs)));
            }

            [Test]
            public void Updates_passed_argument_argNames()
            {
                CallUpdateGenericType();
                Assert.IsTrue(_genericType.ArgNames.SequenceEqual(_newArgs));
            }
        }
    }
}