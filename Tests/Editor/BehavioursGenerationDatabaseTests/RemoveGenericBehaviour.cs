﻿namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class RemoveGenericBehaviour : GenericBehavioursDatabaseTests
        {
            private static GenericTypeInfo _secondBehaviour;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                _secondBehaviour = new GenericTypeInfo("secondBehaviourName", "secondBehaviourGUID", new[] { "secondArgs" });

                _database.InstanceAddGenericBehaviour(_behaviour);
                _database.InstanceAddGenericBehaviour(_secondBehaviour);

                _database.InstanceAddConcreteClass(_behaviour, _firstSecondArgs, AssemblyGUID);
                _database.InstanceAddConcreteClass(_secondBehaviour, _firstSecondArgs, AssemblyGUID);
            }

            private void CallRemoveGenericBehaviour(GenericTypeInfo behaviour) => _database.InstanceRemoveGenericBehaviour(behaviour, _ => { });

            [Test]
            public void Removes_behaviour_from_behaviours_list()
            {
                CallRemoveGenericBehaviour(_behaviour);

                Assert.IsFalse(_database.InstanceBehaviours.Contains(_behaviour));
            }

            [Test]
            public void Removes_behaviour_from_referenced_behaviours_of_each_argument()
            {
                CallRemoveGenericBehaviour(_behaviour);

                bool firstSuccess = _database.InstanceTryGetReferencedBehaviours(_firstArg, out GenericTypeInfo[] firstBehaviours);
                Assert.IsTrue(firstSuccess);
                Assert.IsFalse(firstBehaviours.Contains(_behaviour));

                bool secondSuccess = _database.InstanceTryGetReferencedBehaviours(_secondArg, out GenericTypeInfo[] secondBehaviours);
                Assert.IsTrue(secondSuccess);
                Assert.IsFalse(secondBehaviours.Contains(_behaviour));
            }

            [Test]
            public void When_argument_has_only_this_behaviour_referenced_removes_argument_from_arguments_list()
            {
                CallRemoveGenericBehaviour(_behaviour);
                CallRemoveGenericBehaviour(_secondBehaviour);

                Assert.IsEmpty(_database.InstanceArguments);
            }

            [Test]
            public void When_behaviour_is_not_found_throws_KeyNotFound_exception()
            {
                CallRemoveGenericBehaviour(_behaviour);
                CallRemoveGenericBehaviour(_secondBehaviour);

                Assert.Throws<KeyNotFoundException>(() =>
                {
                    _database.InstanceRemoveGenericBehaviour(_behaviour, _ => { });
                });
            }
        }
    }
}