namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;
    using SolidUtilities;

    public class TypeInfoTests
    {
        private TypeInfo _firstInfo;
        private TypeInfo _secondInfo;
        private TypeInfo _thirdInfo;
        private TypeInfo _fourthInfo;

        [OneTimeSetUp]
        public void BeforeAllTests()
        {
            _firstInfo = new ArgumentInfo("firstFullName", "firstGUID");
            _secondInfo = new ArgumentInfo("secondFullName", "secondGUID");
            _thirdInfo = new ArgumentInfo("thirdFullName", "thirdGUID");
            _fourthInfo = new ArgumentInfo("fourthFullName", "fourthGUID");
        }

        [Test]
        public void Set_does_not_include_identical_structs()
        {
            var set = new HashSet<TypeInfo> { _firstInfo, _secondInfo, _firstInfo };

            Assert.IsTrue(set.Count == 2);
            Assert.IsTrue(set.Contains(_firstInfo));
            Assert.IsTrue(set.Contains(_secondInfo));
        }

        [Test]
        public void Sets_with_same_structs_are_identical()
        {
            var firstSet = new HashSet<TypeInfo> { _firstInfo, _secondInfo };
            var secondSet = new HashSet<TypeInfo> { _firstInfo, _secondInfo };

            Assert.IsTrue(firstSet.SetEquals(secondSet));
        }

        [Test]
        public void ExceptWith_correctly_works_with_two_different_sets()
        {
            var thirdIncludedSet = new HashSet<TypeInfo> { _firstInfo, _secondInfo, _thirdInfo };
            var fourthIncludedSet = new HashSet<TypeInfo> { _firstInfo, _secondInfo, _fourthInfo };
            var thirdOnlySet = new HashSet<TypeInfo> { _thirdInfo };

            var newSet = thirdIncludedSet.ExceptWithAndCreateNew(fourthIncludedSet);

            Assert.IsTrue(newSet.SetEquals(thirdOnlySet));
        }

        [Test]
        public void Sets_with_different_guids_are_different()
        {
            var firstGUIDInfo = new ArgumentInfo("testType", "firstTestGuid");
            var secondGUIDInfo = new ArgumentInfo("testType", "secondTestGuid");

            var firstSet = new HashSet<TypeInfo> { _firstInfo, _secondInfo, firstGUIDInfo };
            var secondSet = new HashSet<TypeInfo> { _firstInfo, _secondInfo, secondGUIDInfo };

            Assert.IsFalse(firstSet.SetEquals(secondSet));
        }
    }
}