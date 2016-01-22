using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Restore.ChangeDispatching;
using Restore.ChangeResolution;

namespace Restore.Tests.ChangeDispatching
{
    [TestFixture]
    public class ChangeDispatchingStepTest
    {
        private ChangeDispatchingStep<LocalTestResource> _stepUnderTest;
        private ISynchronizationAction<LocalTestResource> _testAction;
        private SynchronizationResult _testActionResult;

        [SetUp]
        public void SetUpTest()
        {
            _stepUnderTest = new ChangeDispatchingStep<LocalTestResource>();
            _testActionResult = new SynchronizationResult(true);
            _testAction = new SynchronizationAction<LocalTestResource, string>(
                "bogus",
                (resource, s) => _testActionResult,
                new LocalTestResource(1));
        }

        [Test]
        public void ShouldExecuteAction()
        {
            var actualResult = _stepUnderTest.Process(_testAction);
            Assert.AreEqual(_testActionResult, actualResult);
        }

        [Test]
        public void ShouldWrapExceptionInDispatchingException()
        {
            var raisedException = new InvalidOperationException("Update not possible");
            ISynchronizationAction<LocalTestResource> action = new SynchronizationAction<LocalTestResource, string>(
                "bogus"
                , (resource, s) => { throw raisedException; }
                , new LocalTestResource(1)
                , "TestAction");

            try
            {
                _stepUnderTest.Process(action);
                Assert.Fail("Expecting DispatchingException");
            }
            catch (DispatchingException ex)
            {
                Assert.AreEqual(raisedException, ex.InnerException);
                Assert.AreEqual($"Failed executing action TestAction on {action.Applicant}!", ex.Message);
            }
        }

        [Test]
        public void ShouldCallBeforeDispatchingObserversInComposedPipeline()
        {
            bool isCalled = false;
            _stepUnderTest.AddOutputObserver(action =>
            {
                isCalled = true;
            });

            var input = new List<ISynchronizationAction<LocalTestResource>>()
            {
                _testAction
            };
            _stepUnderTest.Compose(input).ToList();

            Assert.IsTrue(isCalled);
        }
    }
}