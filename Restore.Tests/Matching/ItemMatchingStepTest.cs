using System.Collections.Generic;
using NUnit.Framework;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class ItemMatchingStepTest
    {
        [Test]
        public void ShouldWrapProcessStepInPreprocessException()
        {
            // var stepUnderTest = new ItemMatchingStep<,>();
            // Though this is a step, it does not work on a per item basis. It has SelectMany semantics. Though we are feeding it one item at a time. But after that is produces more results.
        }
    }

    public class ItemMatchingStep<T1, T2> : SynchronizationStep<IEnumerable<T1>, IEnumerable<ItemMatch<T1, T2>>>
    {
        public override IEnumerable<ItemMatch<T1, T2>> Process(IEnumerable<T1> input)
        {
            throw new System.NotImplementedException();
        }
    }
}
