namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Editor.MonoBehaviour;
    using NUnit.Framework;

    internal class AssembliesCheckerTests
    {
        public class GetTypesForAssemblyChange
        {
            private GenericTypeInfo _firstInfo;
            private GenericTypeInfo _secondInfo;
            private GenericTypeInfo _thirdInfo;
            private GenericTypeInfo _fourthInfo;

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                _firstInfo = new GenericTypeInfo("firstFullName", "firstGUID");
                _secondInfo = new GenericTypeInfo("secondFullName", "secondGUID");
                _thirdInfo = new GenericTypeInfo("thirdFullName", "thirdGUID");
                _fourthInfo = new GenericTypeInfo("fourthFullName", "fourthGUID");
            }

            [Test]
            public void When_identical_collections_are_passed_returns_empty_collections()
            {
                var oldTypes = new[] { _firstInfo, _secondInfo };
                var newTypes = new[] { _firstInfo, _secondInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                Assert.IsEmpty(typesToRemove);
                Assert.IsEmpty(typesToAdd);
                Assert.IsEmpty(typesToUpdate);
            }

            [Test]
            public void When_newTypes_is_bigger_returns_needAssemblyAdd()
            {
                var oldTypes = new[] { _firstInfo, _secondInfo };
                var newTypes = new[] { _firstInfo, _secondInfo, _thirdInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                var expectedTypesToAdd = new List<GenericTypeInfo> { _thirdInfo };

                Assert.IsEmpty(typesToRemove);
                Assert.IsEmpty(typesToUpdate);
                Assert.IsTrue(typesToAdd.SequenceEqual(expectedTypesToAdd));
            }

            [Test]
            public void When_oldTypes_is_bigger_returns_needAssemblyRemove()
            {
                var oldTypes = new[] { _firstInfo, _secondInfo, _thirdInfo };
                var newTypes = new[] { _firstInfo, _secondInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                var expectedTypesToRemove = new List<GenericTypeInfo> { _thirdInfo };

                Assert.IsEmpty(typesToAdd);
                Assert.IsEmpty(typesToUpdate);
                Assert.IsTrue(typesToRemove.SequenceEqual(expectedTypesToRemove));
            }

            [Test]
            public void When_GUID_differs_returns_empty_collections()
            {
                var firstTestInfo = new GenericTypeInfo("testFullName", "testFirstGuid");
                var secondTestInfo = new GenericTypeInfo("testFullName", "testSecondGuid");

                var oldTypes = new[] { _firstInfo, _secondInfo, firstTestInfo };
                var newTypes = new[] { _firstInfo, _secondInfo, secondTestInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                Assert.IsEmpty(typesToRemove);
                Assert.IsEmpty(typesToAdd);
                Assert.IsEmpty(typesToUpdate);
            }

            [Test]
            public void When_TypeFullName_differs_but_GUID_is_same_adds_types_to_needAssemblyUpdate()
            {
                var firstTestInfo = new GenericTypeInfo("firstTestType", "testGuid");
                var secondTestInfo = new GenericTypeInfo("secondTestType", "testGuid");

                var oldTypes = new[] { _firstInfo, _secondInfo, firstTestInfo };
                var newTypes = new[] { _firstInfo, _secondInfo, secondTestInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                var expectedTypesToUpdate = new List<GenericTypeInfoPair>
                {
                    new GenericTypeInfoPair(firstTestInfo, secondTestInfo)
                };

                Assert.IsEmpty(typesToRemove);
                Assert.IsEmpty(typesToAdd);
                Assert.IsTrue(typesToUpdate.SequenceEqual(expectedTypesToUpdate));
            }

            [Test]
            public void When_everything_differs_adds_types_to_needAssemblyRemove_and_needAssemblyAdd()
            {
                var oldTypes = new[] { _firstInfo, _secondInfo, _thirdInfo };
                var newTypes = new[] { _firstInfo, _secondInfo, _fourthInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                var expectedTypesToRemove = new List<GenericTypeInfo> { _thirdInfo };
                var expectedTypesToAdd = new List<GenericTypeInfo> { _fourthInfo };

                Assert.IsEmpty(typesToUpdate);
                Assert.IsTrue(typesToRemove.SequenceEqual(expectedTypesToRemove));
                Assert.IsTrue(typesToAdd.SequenceEqual(expectedTypesToAdd));
            }

            [Test]
            public void When_two_collections_are_empty_returns_empty_collections()
            {
                GenericTypeInfo[] oldTypes = { };
                GenericTypeInfo[] newTypes = { };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                Assert.IsEmpty(typesToRemove);
                Assert.IsEmpty(typesToAdd);
                Assert.IsEmpty(typesToUpdate);
            }

            [Test]
            public void When_old_collection_is_empty_returns_full_needAssemblyAdd()
            {
                GenericTypeInfo[] oldTypes = { };
                GenericTypeInfo[] newTypes = { _firstInfo, _secondInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                Assert.IsEmpty(typesToRemove);
                Assert.IsEmpty(typesToUpdate);
                Assert.IsTrue(typesToAdd.SequenceEqual(newTypes));
            }

            [Test]
            public void When_new_collection_is_empty_returns_full_needAssemblyRemove()
            {
                GenericTypeInfo[] oldTypes = { _firstInfo, _secondInfo };
                GenericTypeInfo[] newTypes = { };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                Assert.IsEmpty(typesToAdd);
                Assert.IsEmpty(typesToUpdate);
                Assert.IsTrue(typesToRemove.SequenceEqual(oldTypes));
            }

            [Test]
            public void When_TypeFullNames_match_and_new_GUID_is_empty_but_old_one_is_not_returns_empty_collections_and_updates_new_GUID()
            {
                var firstTestInfo = new GenericTypeInfo("TestType", "testGUID");
                var secondTestInfo = new GenericTypeInfo("TestType", string.Empty);

                var oldTypes = new[] { _firstInfo, _secondInfo, firstTestInfo };
                var newTypes = new[] { _firstInfo, _secondInfo, secondTestInfo };

                var (typesToRemove, typesToAdd, typesToUpdate) =
                    AssembliesChecker.GetTypesForAssemblyChange(oldTypes, newTypes);

                Assert.IsEmpty(typesToAdd);
                Assert.IsEmpty(typesToUpdate);
                Assert.IsEmpty(typesToRemove);
                Assert.IsTrue(newTypes.Last().GUID == "testGUID");
            }
        }
    }
}