namespace GenericUnityObjects.EditorTests
{
    using System.Collections.Generic;
    using Editor.MonoBehaviour;
    using NUnit.Framework;
    using SolidUtilities.Extensions;

    public class GenericTypeInfoTests
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
        public void Set_does_not_include_identical_structs()
        {
            var set = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo, _firstInfo };

            Assert.IsTrue(set.Count == 2);
            Assert.IsTrue(set.Contains(_firstInfo));
            Assert.IsTrue(set.Contains(_secondInfo));
        }

        [Test]
        public void Sets_with_same_structs_are_identical()
        {
            var firstSet = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo };
            var secondSet = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo };

            Assert.IsTrue(firstSet.SetEquals(secondSet));
        }

        [Test]
        public void ExceptWith_correctly_works_with_two_different_sets()
        {
            var thirdIncludedSet = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo, _thirdInfo };
            var fourthIncludedSet = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo, _fourthInfo };
            var thirdOnlySet = new HashSet<GenericTypeInfo> { _thirdInfo };

            var newSet = thirdIncludedSet.ExceptWithAndCreateNew(fourthIncludedSet);

            Assert.IsTrue(newSet.SetEquals(thirdOnlySet));
        }

        [Test]
        public void Sets_with_different_guids_are_different()
        {
            var firstGUIDInfo = new GenericTypeInfo("testType", "firstTestGuid");
            var secondGUIDInfo = new GenericTypeInfo("testType", "secondTestGuid");

            var firstSet = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo, firstGUIDInfo };
            var secondSet = new HashSet<GenericTypeInfo> { _firstInfo, _secondInfo, secondGUIDInfo };

            Assert.IsFalse(firstSet.SetEquals(secondSet));
        }

        private class FirstType { }
        private class SecondType { }
    }
}