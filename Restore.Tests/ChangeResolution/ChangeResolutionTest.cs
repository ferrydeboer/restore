using System;
using System.Collections.Generic;
using NUnit.Framework;
using Restore.ChangeResolution;
using Restore.RxProto;

namespace Restore.Tests.ChangeResolution
{
    [TestFixture]
    public class ChangeResolutionTest
    {
        private ChangeResolver<LocalTestResource, string> _testChangeResolver;
        private bool _shouldResolve;
        private bool _isResolved;
        private ChangeResolutionStep<LocalTestResource, string> _resolutionStepUnderTest;

        [SetUp]
        public void SetUpTest()
        {
            _shouldResolve = false;
            _testChangeResolver = new ChangeResolver<LocalTestResource, string>(
                "bogus config",
                (item, cfg) => _shouldResolve,
                (item, cfg) =>
                {
                    _isResolved = true;
                    return new SynchronizationResult(true);
                });

            var changeResolvers = new List<IChangeResolver<LocalTestResource>>
            {
                _testChangeResolver
            };

            _resolutionStepUnderTest = new ChangeResolutionStep<LocalTestResource, string>(
                changeResolvers, "bogus config");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowChangeResolutionExceptionIfConfiguredWithoutResolvers()
        {
            new ChangeResolutionStep<LocalTestResource, string>(null, "bogus config");
        }

        [Test]
        public void ShouldReturnActionWhenApplicable()
        {
            _shouldResolve = true;
            var localTestResource = new LocalTestResource(1);
            var action = _resolutionStepUnderTest.Resolve(localTestResource);

            Assert.IsNotNull(action);
            action.Execute();
            Assert.IsTrue(_isResolved);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldReturnNullSynchActionWhenUnResolved()
        {
            var localTestResource = new LocalTestResource(1);
            var action = _resolutionStepUnderTest.Resolve(localTestResource);

            Assert.IsNotNull(action);
            action.Execute();
        }
    }
}
