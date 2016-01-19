using NUnit.Framework;
using Restore.Channel;
using Restore.Channel.Configuration;

namespace Restore.Tests.Channel
{
    [TestFixture]
    public class AttachedObservableCollectionTest
    {
        private AttachedObservableCollection<LocalTestResource> _observableUnderTest;
        private InMemoryCrudDataEndpoint<LocalTestResource, int> _dataSource;
        private bool _hasDispatched;

        [SetUp]
        public void SetUpTest()
        {
            _dataSource = new InMemoryCrudDataEndpoint<LocalTestResource, int>(
                new TypeConfiguration<LocalTestResource, int>(
                    ltr => ltr.CorrelationId.HasValue ? ltr.CorrelationId.Value : -1));
            ConstrucTestSubject();
            _hasDispatched = false;
        }

        private void ConstrucTestSubject()
        {
            _observableUnderTest = new AttachedObservableCollection<LocalTestResource>(
                _dataSource
                , new LocalTestResourceIdComparer(),
                act =>
                {
                    _observableUnderTest.CollectionChanged += _observableUnderTest_CollectionChanged;
                    act();
                    _observableUnderTest.CollectionChanged -= _observableUnderTest_CollectionChanged;
                });
        }

        private void _observableUnderTest_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _hasDispatched = true;
        }

        [Test]
        public void ShouldAddItemWhenNotInCollection()
        {
            var addedItem = new LocalTestResource(1, 10) {Name = "TestResource"};
            _dataSource.Create(addedItem);

            Assert.IsTrue(_observableUnderTest.Contains(addedItem));
        }

        [Test]
        public void ShouldDispatchWhenAdded()
        {
            _dataSource.Create(new LocalTestResource(1));
            Assert.IsTrue(_hasDispatched);
        }

        [Test]
        public void ShouldSwapInstanceWhenUpdated()
        {
            var addedItem = new LocalTestResource(1, 10) {Name = "TestResource"};
            _dataSource.Create(addedItem);
            var updatedItem = new LocalTestResource(1, 10) {Name = "Updated TestResource"};
            _dataSource.Update(updatedItem);

            Assert.IsFalse(_observableUnderTest.Contains(addedItem));
            Assert.IsTrue(_observableUnderTest.Contains(updatedItem));
            Assert.AreEqual(1, _observableUnderTest.Count);
        }

        [Test]
        public void ShouldAddUpdatedItemIfNotInTheList()
        {
            _dataSource = new InMemoryCrudDataEndpoint<LocalTestResource, int>(
                new TypeConfiguration<LocalTestResource, int>(
                    ltr => ltr.CorrelationId.HasValue ? ltr.CorrelationId.Value : -1));
            var addedItem = new LocalTestResource(1, 10) { Name = "TestResource" };
            _dataSource.Create(addedItem);
            ConstrucTestSubject();

            _dataSource.Update(addedItem);

            Assert.IsTrue(_observableUnderTest.Contains(addedItem));
        }

        [Test]
        public void ShouldDispatchWhenUpdated()
        {
            var localTestResource = new LocalTestResource(1);
            _dataSource.Create(localTestResource);
            _hasDispatched = false;
            _dataSource.Update(localTestResource);
            Assert.IsTrue(_hasDispatched);
        }

        [Test]
        public void ShouldDeleteItem()
        {
            var addedItem = new LocalTestResource(1, 10) {Name = "TestResource"};
            _dataSource.Create(addedItem);
            _dataSource.Delete(addedItem);

            Assert.IsFalse(_observableUnderTest.Contains(addedItem));
            Assert.AreEqual(0, _observableUnderTest.Count);
        }

        [Test]
        public void ShouldDispatchWhenDeleting()
        {
            var addedItem = new LocalTestResource(1, 10) {Name = "TestResource"};
            _dataSource.Create(addedItem);
            _hasDispatched = false;
            _dataSource.Delete(addedItem);

            Assert.IsTrue(_hasDispatched);
        }
    }
}
