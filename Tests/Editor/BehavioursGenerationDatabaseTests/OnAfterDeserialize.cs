namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using System.Reflection;
    using Editor.MonoBehaviour;
    using Editor.Util;
    using NUnit.Framework;
    using UnityEngine;

    internal partial class GenericBehavioursDatabaseTests
    {
        public class OnAfterDeserialize : GenericBehavioursDatabaseTests
        {
            private static FieldInfo _instanceField;

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                _instanceField = typeof(EditorOnlySingletonSO<BehavioursGenerationDatabase>)
                    .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

                Assert.IsNotNull(_instanceField);
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                var cleanInstance = ScriptableObject.CreateInstance<BehavioursGenerationDatabase>();
                _instanceField.SetValue(null, cleanInstance);
                BehavioursGenerationDatabase.Instance.Initialize();
                BehavioursGenerationDatabase.AddGenericBehaviour(_behaviour);
                BehavioursGenerationDatabase.Instance.InstanceAddConcreteClass(_behaviour, _firstSecondArgs, AssemblyGUID);
            }

            private static void ReserializeDatabase()
            {
                BehavioursGenerationDatabase.Instance.OnBeforeSerialize();
                BehavioursGenerationDatabase.Instance.OnAfterDeserialize();
            }

            [Test]
            public void Restores_arguments()
            {
                ReserializeDatabase();
                Assert.IsTrue(BehavioursGenerationDatabase.Arguments.SequenceEqual(_firstSecondArgs));
            }

            [Test]
            public void Restores_behaviours()
            {
                ReserializeDatabase();
                Assert.IsTrue(BehavioursGenerationDatabase.Behaviours.SequenceEqual(_expectedBehaviours));
            }

            [Test]
            public void Restores_referenced_behaviours()
            {
                ReserializeDatabase();
                bool behavioursSuccess = BehavioursGenerationDatabase.TryGetReferencedBehaviours(_firstArg, out GenericTypeInfo[] referencedBehaviours);
                Assert.IsTrue(behavioursSuccess);
                Assert.IsTrue(referencedBehaviours.SequenceEqual(_expectedBehaviours));
            }

            [Test]
            public void Restores_concrete_classes()
            {
                ReserializeDatabase();
                bool concreteClassesSuccess = BehavioursGenerationDatabase.TryGetConcreteClasses(_behaviour, out ConcreteClass[] concreteClasses);
                Assert.IsTrue(concreteClassesSuccess);
                Assert.IsTrue(concreteClasses.Length == 1);
                Assert.IsTrue(concreteClasses[0] == _expectedConcreteClass);
            }
        }
    }
}