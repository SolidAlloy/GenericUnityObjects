namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class GetConcreteClassesByArgument : GenerationDatabaseTests
        {
            private const string SecondAssemblyGUID = "secondAssemblyGUID";

            private static ArgumentInfo _thirdArg;
            private static ArgumentInfo[] _secondThirdArgs;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                _database.AddGenericTypeImpl(_genericType);
                _database.AddConcreteClassImpl(_genericType, _firstSecondArgs, AssemblyGUID);
                _thirdArg = new ArgumentInfo("thirdArgumentName", "thirdArgumentGUID");
                _secondThirdArgs = new[] { _secondArg, _thirdArg };
                _database.AddConcreteClassImpl(_genericType, _secondThirdArgs, SecondAssemblyGUID);
            }

            private static ConcreteClass[] CallMethod() =>
                _database.GetConcreteClassesByArgumentImpl(_genericType, _firstArg);

            [Test]
            public void Returns_concrete_classes_where_argument_is_listed()
            {
                var actualClasses = CallMethod();

                Assert.IsTrue(actualClasses.Any(concreteClass => concreteClass.Arguments.SequenceEqual(_firstSecondArgs)));
            }

            [Test]
            public void Does_not_return_concrete_classes_where_argument_is_not_listed()
            {
                var actualClasses = CallMethod();

                Assert.IsFalse(actualClasses.Any(concreteClass => concreteClass.Arguments.SequenceEqual(_secondThirdArgs)));
            }

            [Test]
            public void Throws_KeyNotFoundException_when_generic_type_is_not_in_the_database()
            {
                _database.RemoveGenericTypeImpl(_genericType, null);

                Assert.Throws<KeyNotFoundException>(() => CallMethod());
            }

            [Test]
            public void Returns_empty_collection_when_argument_is_not_present()
            {
                _database.RemoveArgumentImpl(_firstArg, null);

                var actualClasses = CallMethod();
                
                Assert.IsEmpty(actualClasses);
            }
        }
    }
}