﻿namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;
    using Util;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class UpdateArgumentNameAndAssembly : BehavioursGenerationDatabaseTests
        {
            private const string NewName = "newName";
            private const string NewAssembly = "newAssembly";
            private static ArgumentInfo _expectedArg;
            private static TypeStub _typeStub;

            private static void CallUpdateArgumentNameAndAssembly() =>
                _database.UpdateArgumentNameAndAssemblyImpl(ref _firstArg, _typeStub);

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                _typeStub = new TypeStub(NewName, NewAssembly);
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();

                _expectedArg = new ArgumentInfo(NewName, NewAssembly, _firstArg.GUID);
            }

            [Test]
            public void Updates_argument_name_in_arguments_list()
            {
                CallUpdateArgumentNameAndAssembly();

                Assert.IsTrue(_database.InstanceArguments.Length == 2);
                Assert.Contains(_expectedArg, _database.InstanceArguments);
            }

            [Test]
            public void Updates_argument_name_in_concrete_classes()
            {
                CallUpdateArgumentNameAndAssembly();

                bool success = _database.TryGetConcreteClassesImpl(_behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);
                Assert.IsTrue(concreteClasses.Any(concreteClass => concreteClass.Arguments.Contains(_expectedArg)));
            }

            [Test]
            public void Referenced_behaviours_can_be_found_by_new_argument()
            {
                CallUpdateArgumentNameAndAssembly();

                bool success = _database.TryGetReferencedGenericTypesImpl(_expectedArg, out GenericTypeInfo[] behaviours);

                Assert.IsTrue(success);
                Assert.IsTrue(behaviours.Length != 0);
            }

            [Test]
            public void Updates_passed_argument_TypeNameAndAssembly()
            {
                CallUpdateArgumentNameAndAssembly();
                Assert.IsTrue(_firstArg.TypeNameAndAssembly == TypeUtility.GetTypeNameAndAssembly(NewName, NewAssembly));
            }
        }
    }
}