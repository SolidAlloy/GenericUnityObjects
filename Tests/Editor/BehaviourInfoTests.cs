namespace GenericUnityObjects.EditorTests
{
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    public class BehaviourInfoTests
    {
        [Test]
        public void Instances_with_identical_fields_are_equal()
        {
            var firstBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "firstGUID", new[] { "firstArg", "secondArg" });
            var secondBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "firstGUID", new[] { "firstArg", "secondArg" });

            Assert.AreEqual(firstBehaviour, secondBehaviour);
        }

        [Test]
        public void Instances_with_different_typeNames_are_not_equal()
        {
            var firstBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "firstGUID", new[] { "firstArg", "secondArg" });
            var secondBehaviour = new GenericTypeInfo("secondTypeAndAssembly", "firstGUID", new[] { "firstArg", "secondArg" });

            Assert.AreNotEqual(firstBehaviour, secondBehaviour);
        }

        [Test]
        public void Instances_with_different_guids_are_not_equal()
        {
            var firstBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "firstGUID", new[] { "firstArg", "secondArg" });
            var secondBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "secondGUID", new[] { "firstArg", "secondArg" });

            Assert.AreNotEqual(firstBehaviour, secondBehaviour);
        }

        [Test]
        public void Instances_with_different_args_are_not_equal()
        {
            var firstBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "firstGUID", new[] { "firstArg", "secondArg" });
            var secondBehaviour = new GenericTypeInfo("firstTypeAndAssembly", "secondGUID", new[] { "firstArg", "thirdArg" });

            Assert.AreNotEqual(firstBehaviour, secondBehaviour);
        }
    }
}