using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public void ShouldReturnNullSynchActionWhenUnresolved()
        {
            _shouldResolve = false;
            var localTestResource = new LocalTestResource(1);
            var action = _resolutionStepUnderTest.Resolve(localTestResource);

            Assert.IsNotNull(action);
            Assert.AreEqual(typeof(NullSynchAction<LocalTestResource>), action.GetType());
        }

        [Test]
        public void ShouldBuildPipelineWithObservers()
        {
            _shouldResolve = true;
            LocalTestResource isCalled = null;
            _resolutionStepUnderTest.AddResultObserver(action =>
            {
                isCalled = action.Applicant;
            });

            var compositionSource = new List<LocalTestResource>
            {
                new LocalTestResource(1)
            };
            var pipeline = _resolutionStepUnderTest.Compose(compositionSource);

            foreach (var synchronizationAction in pipeline)
            {
                Debug.WriteLine(synchronizationAction.Applicant);
            }
            //var drained = pipeline.ToList();
            Assert.AreEqual(compositionSource[0], isCalled);
        }
    }
}
