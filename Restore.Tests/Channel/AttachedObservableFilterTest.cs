﻿using System;
using NUnit.Framework;
using Restore.Channel;
using Restore.Channel.Configuration;

namespace Restore.Tests.Channel
{
    [TestFixture]
    public class AttachedObservableFilterTest : IDisposable
    {
        private readonly LocalTestResource _validTestResource = new LocalTestResource(1, 10) { Name = "Ferry de Boer" };
        private InMemoryCrudDataEndpoint<LocalTestResource, int> _dataSource;
        private AttachedObservableCollection<LocalTestResource> _observableUnderTest;

        [SetUp]
        public void SetUpTest()
        {
            _dataSource = new InMemoryCrudDataEndpoint<LocalTestResource, int>(
                new TypeConfiguration<LocalTestResource, int>(
                    ltr => ltr.CorrelationId.HasValue ? ltr.CorrelationId.Value : -1,
                    -1));
            _observableUnderTest = new AttachedObservableCollection<LocalTestResource>(
                _dataSource
                , new LocalTestResourceIdComparer());
            _observableUnderTest.Where(ltr => ltr.Name.Contains("Ferry"));
        }

        [Test]
        public void ShouldIgnoreAddedItemThatDoesNotPassFilter()
        {
            _dataSource.Create(new LocalTestResource(1) { Name = "Bla" });
            Assert.AreEqual(0, _observableUnderTest.Count);
        }

        [Test]
        public void ShouldAddItemIfPassesFilter()
        {
            _dataSource.Create(_validTestResource);
            Assert.AreEqual(1, _observableUnderTest.Count);
        }

        [Test]
        public void ShouldRemoveItemIfUpdateDoesNotMatchFilter()
        {
            var localTestResource = _validTestResource;
            _dataSource.Create(localTestResource);
            localTestResource.Name = "Dirk Drama";
            _dataSource.Update(localTestResource);
            Assert.AreEqual(0, _observableUnderTest.Count);
        }

        public void Dispose()
        {
            _observableUnderTest.Dispose();
        }
    }
}
