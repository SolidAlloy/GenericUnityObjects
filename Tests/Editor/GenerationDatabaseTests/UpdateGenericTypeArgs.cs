﻿namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class UpdateGenericTypeArgs : GenerationDatabaseTests
        {
            private static readonly string[] NewArgs = { "newArgs" };
            private static GenericTypeInfo _expectedGenericType;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedGenericType = new GenericTypeInfo(_genericType.TypeNameAndAssembly, _genericType.GUID, NewArgs);
            }

            [Test]
            public void Updates_generic_type_arguments_in_generic_types_list()
            {
                _database.UpdateGenericTypeArgsImpl(_genericType, NewArgs);

                Assert.IsTrue(_database.InstanceGenericTypes.Length == 1);
                Assert.Contains(_expectedGenericType, _database.InstanceGenericTypes);
            }

            [Test]
            public void Updates_generic_types_arguments_in_referenced_behaviours()
            {
                _database.UpdateGenericTypeArgsImpl(_genericType, NewArgs);

                var behaviours = _database.GetReferencedGenericTypesImpl(_firstArg);
                Assert.IsTrue(behaviours.Any(behaviour => behaviour.ArgNames.SequenceEqual(NewArgs)));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_generic_type()
            {
                _database.UpdateGenericTypeArgsImpl(_genericType, NewArgs);

                var concreteClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_argument_argNames()
            {
                _database.UpdateGenericTypeArgsImpl(_genericType, NewArgs);
                Assert.IsTrue(_genericType.ArgNames == NewArgs);
            }
        }
    }
}