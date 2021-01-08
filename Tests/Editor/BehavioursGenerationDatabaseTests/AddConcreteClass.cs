namespace GenericUnityObjects.EditorTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class AddConcreteClass : BehavioursGenerationDatabaseTests
        {
            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                _database.AddGenericTypeImpl(_behaviour, out List<ConcreteClass> _);
            }

            [Test]
            public void Adds_arguments_to_behaviours_dict()
            {
                _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);

                bool success = _database.TryGetConcreteClassesImpl(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Length == 1);
                Assert.IsTrue(concreteClasses[0] == _expectedConcreteClass);
            }

            [Test]
            public void Adds_arguments_to_arguments_dict()
            {
                _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);

                Assert.IsTrue(_database.InstanceArguments.SequenceEqual(_firstSecondArgs));
            }

            [Test]
            public void Adds_behaviour_to_each_argument_in_arguments_dict()
            {
                _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
                bool success = _database.TryGetReferencedGenericTypesImpl(_firstArg, out GenericTypeInfo[] firstBehaviours);
                Assert.IsTrue(success);
                Assert.IsTrue(firstBehaviours.SequenceEqual(_expectedBehaviours));
            }

            [Test]
            public void Adds_second_behaviour_to_each_argument_that_has_one()
            {
                var secondBehaviour = new GenericTypeInfo("secondBehaviourName", "secondBehaviourGUID", new[] { "secondArgs" });
                var expectedBehaviours = new[] { _behaviour, secondBehaviour };

                _database.AddGenericTypeImpl(secondBehaviour, out List<ConcreteClass> _);
                _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
                _database.AddConcreteClassImpl(secondBehaviour, _firstSecondArgs, AssemblyGUID);

                bool success = _database.TryGetReferencedGenericTypesImpl(_firstArg, out GenericTypeInfo[] actualBehaviours);
                Assert.IsTrue(success);
                Assert.IsTrue(actualBehaviours.SequenceEqual(expectedBehaviours));
            }

            [Test]
            public void When_behaviour_does_not_exist_throws_KeyNotFound_exception()
            {
                _database.RemoveGenericTypeImpl(_behaviour, _ => { });

                Assert.Throws<KeyNotFoundException>(() =>
                {
                    _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
                });
            }

            [Test]
            public void Throws_ArgumentException_if_concrete_class_exists()
            {
                _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);

                Assert.Throws<ArgumentException>(() =>
                {
                    _database.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
                });
            }
        }
    }
}