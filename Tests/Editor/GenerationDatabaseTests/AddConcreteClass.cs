namespace GenericUnityObjects.EditorTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class AddConcreteClass : GenerationDatabaseTests
        {
            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                _database.AddGenericTypeImpl(_genericType);
            }

            [Test]
            public void Adds_arguments_to_generic_types_dict()
            {
                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);

                var concreteClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsTrue(concreteClasses.Length == 1);
                Assert.IsTrue(concreteClasses[0] == _expectedConcreteClass);
            }

            [Test]
            public void Adds_arguments_to_arguments_dict()
            {
                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);

                Assert.IsTrue(_database.InstanceArgumentGenericTypes.Keys.SequenceEqual(_firstSecondArgs));
            }

            [Test]
            public void Adds_behaviour_to_each_argument_in_arguments_dict()
            {
                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
                GenericTypeInfo[] firstBehaviours = _database.GetReferencedGenericTypesImpl(_firstArg);
                Assert.IsTrue(firstBehaviours.SequenceEqual(_expectedGenericTypes));
            }

            [Test]
            public void Adds_second_generic_type_to_each_argument_that_has_one()
            {
                var secondGenericType = new GenericTypeInfo("secondGenericTypeName", "secondGenericTypeGUID", new[] { "secondArgs" });
                var expectedGenericTypes = new[] { _genericType, secondGenericType };

                _database.AddGenericTypeImpl(secondGenericType);
                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
                _database.AddConcreteClassImpl(secondGenericType, _firstSecondArgs, AssemblyGUID);

                GenericTypeInfo[] actualGenericTypes = _database.GetReferencedGenericTypesImpl(_firstArg);
                Assert.IsTrue(actualGenericTypes.SequenceEqual(expectedGenericTypes));
            }

            [Test]
            public void Throws_KeyNotFound_exception_when_generic_type_does_not_exist()
            {
                _database.RemoveGenericTypeImpl(_genericType, _ => { });

                Assert.Throws<KeyNotFoundException>(() =>
                {
                    _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
                });
            }

            [Test]
            public void Throws_ArgumentException_if_concrete_class_exists()
            {
                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);

                Assert.Throws<ArgumentException>(() =>
                {
                    _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
                });
            }
        }
    }
}