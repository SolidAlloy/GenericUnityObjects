namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using System.Reflection;
    using Editor.GeneratedTypesDatabase;
    using Editor.Util;
    using NUnit.Framework;
    using UnityEngine;
    using Util;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class OnAfterDeserialize : BehavioursGenerationDatabaseTests
        {
            private static FieldInfo _instanceField;
            private static object _getterInstance;

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                FieldInfo getterField = typeof(EditorOnlySingletonSO<BehavioursGenerationDatabase>)
                    .GetField("Getter", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                Assert.IsNotNull(getterField);

                _getterInstance = getterField.GetValue(null);
                Assert.IsNotNull(_getterInstance);

                _instanceField = typeof(InstanceGetter<BehavioursGenerationDatabase>)
                    .GetField("_instance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                Assert.IsNotNull(_instanceField);
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                var cleanInstance = ScriptableObject.CreateInstance<BehavioursGenerationDatabase>();
                _instanceField.SetValue(_getterInstance, cleanInstance);
                BehavioursGenerationDatabase.Instance.Initialize();
                BehavioursGenerationDatabase.AddGenericType(_behaviour);
                BehavioursGenerationDatabase.Instance.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
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
                Assert.IsTrue(BehavioursGenerationDatabase.GenericUnityObjects.SequenceEqual(_expectedBehaviours));
            }

            [Test]
            public void Restores_referenced_behaviours()
            {
                ReserializeDatabase();
                bool behavioursSuccess = BehavioursGenerationDatabase.TryGetReferencedGenericTypes(_firstArg, out GenericTypeInfo[] referencedBehaviours);
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