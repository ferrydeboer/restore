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
        private SynchronizationResolver<LocalTestResource, string> _testSynchronizationResolver;
        private bool _shouldResolve;
        private bool _isResolved;
        private ChangeResolutionStep<LocalTestResource, string> _resolutionStepUnderTest;

        [SetUp]
        public void SetUpTest()
        {
            _shouldResolve = false;
            _testSynchronizationResolver = new SynchronizationResolver<LocalTestResource, string>(
                "bogus config",
                (item, cfg) => _shouldResolve,
                (item, cfg) =>
                {
                    _isResolved = true;
                    return new SynchronizationResult(true);
                });

            var changeResolvers = new List<ISynchronizationResolver<LocalTestResource>>
            {
                _testSynchronizationResolver
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

        // Error handling:
        // In general we can't do much in term of error handling within each specific step.
        // It makes sense to wrap all exceptions so handling at a higher level becomes easier.
        // I think by default we should use a mechanism where the execution of the list simply 
        // halts unless specific rules are provided how to deal with item level exceptions.
        [Test]
        public void ShouldWrapExceptionIntoChangeResolutionException()
        {
            var exception = new Exception("Whatever resolution error");
            _testSynchronizationResolver = new SynchronizationResolver<LocalTestResource, string>(
                "bogus config",
                (item, cfg) =>
                {
                    throw exception;
                },
                (item, cfg) =>
                {
                    _isResolved = true;
                    return new SynchronizationResult(true);
                });

            var changeResolvers = new List<ISynchronizationResolver<LocalTestResource>>
            {
                _testSynchronizationResolver
            };

            _resolutionStepUnderTest = new ChangeResolutionStep<LocalTestResource, string>(
                changeResolvers, "bogus config");

            var localTestResource = new LocalTestResource(1);
            try
            {
                _resolutionStepUnderTest.Resolve(localTestResource);
                Assert.Fail("Expecting ChangeResolutionException");
            }
            catch(ChangeResolutionException ex)
            {
                Assert.AreEqual("Failed to resolve change for LocalTestResource - 1", ex.Message);
                Assert.AreEqual(exception, ex.InnerException);
                Assert.AreEqual(localTestResource, ex.Item);
            }
        }
    }
}
