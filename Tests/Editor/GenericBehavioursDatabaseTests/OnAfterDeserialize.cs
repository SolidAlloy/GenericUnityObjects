namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using System.Reflection;
    using Editor.MonoBehaviour;
    using NUnit.Framework;
    using UnityEngine;
    using Util;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class OnAfterDeserialize : GenericBehavioursDatabaseTests
        {
            private static FieldInfo _instanceField;

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                _instanceField = typeof(SingletonScriptableObject<GenericBehavioursDatabase>)
                    .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

                Assert.IsNotNull(_instanceField);
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                var cleanInstance = ScriptableObject.CreateInstance<GenericBehavioursDatabase>();
                _instanceField.SetValue(null, cleanInstance);
                GenericBehavioursDatabase.Instance.OnAfterDeserialize();
                GenericBehavioursDatabase.AddGenericBehaviour(_behaviour);
                GenericBehavioursDatabase.AddConcreteClass(_behaviour, _firstSecondArgs, AssemblyGUID);
            }

            private static void ReserializeDatabase()
            {
                GenericBehavioursDatabase.Instance.OnBeforeSerialize();
                GenericBehavioursDatabase.Instance.OnAfterDeserialize();
            }

            public void Restores_arguments()
            {
                ReserializeDatabase();
                Assert.IsTrue(GenericBehavioursDatabase.Arguments.SequenceEqual(_firstSecondArgs));
            }

            public void Restores_behaviours()
            {
                ReserializeDatabase();
                Assert.IsTrue(GenericBehavioursDatabase.Behaviours.SequenceEqual(_expectedBehaviours));
            }

            public void Restores_referenced_behaviours()
            {
                ReserializeDatabase();
                bool behavioursSuccess = GenericBehavioursDatabase.TryGetReferencedBehaviours(_firstArg, out BehaviourInfo[] referencedBehaviours);
                Assert.IsTrue(behavioursSuccess);
                Assert.IsTrue(referencedBehaviours.SequenceEqual(_expectedBehaviours));
            }

            public void Restores_concrete_classes()
            {
                ReserializeDatabase();
                bool concreteClassesSuccess = GenericBehavioursDatabase.TryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);
                Assert.IsTrue(concreteClassesSuccess);
                Assert.IsTrue(concreteClasses.Length == 1);
                Assert.IsTrue(concreteClasses[0] == _expectedConcreteClass);
            }
        }
    }
}