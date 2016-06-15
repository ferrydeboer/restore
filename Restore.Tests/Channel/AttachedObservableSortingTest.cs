using System;
using NUnit.Framework;
using Restore.Channel;
using Restore.Channel.Configuration;

namespace Restore.Tests.Channel
{
    [TestFixture]
    public class AttachedObservableSortingTest : IDisposable
    {
        private AttachedObservableCollection<LocalTestResource> _observableUnderTest;

        private InMemoryCrudDataEndpoint<LocalTestResource, int> _dateSource;
        private LocalTestResource _smallest;
        private LocalTestResource _bigger;
        private LocalTestResource _biggest;

        [SetUp]
        public void SetUpTest()
        {
            _dateSource = new InMemoryCrudDataEndpoint<LocalTestResource, int>(
                new TypeConfiguration<LocalTestResource, int>(ltr => ltr.LocalId));
            _observableUnderTest = new AttachedObservableCollection<LocalTestResource>(
                _dateSource
                , new LocalTestResourceIdComparer());

            _smallest = new LocalTestResource(10) { Name = "Anthony" };
            _bigger = new LocalTestResource(1) { Name = "Bert" };
            _biggest = new LocalTestResource(2) { Name = "Zack" };
        }

        [Test]
        public void ShouldAddBeforeItemOnCreateIfSmallerOnAscending()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Asc();

            _dateSource.Create(_bigger);
            _dateSource.Create(_smallest);

            Assert.AreEqual(_smallest, _observableUnderTest[0]);
        }

        [Test]
        public void ShouldAddAfterItemOnCreateIfSmallerOnDescending()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Desc();

            _dateSource.Create(_smallest);
            _dateSource.Create(_bigger);
            _dateSource.Create(_biggest);

            Assert.AreEqual(_biggest, _observableUnderTest[0]);
            Assert.AreEqual(_smallest, _observableUnderTest[2]);
        }

        [Test]
        public void ShouldMoveItemToTopWhenChangedAndAscending()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Asc();

            _dateSource.Create(_smallest);
            _dateSource.Create(_bigger);
            _dateSource.Create(_biggest);

            _biggest.Name = "Aaron";
            _dateSource.Update(_biggest);

            Assert.AreEqual(_biggest, _observableUnderTest[0]);
        }

        [Test]
        public void ShouldMoveItemToBottomWhenChangedAndDescending()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Desc();

            _dateSource.Create(_biggest);
            _dateSource.Create(_smallest);
            _dateSource.Create(_bigger);

            _biggest.Name = "Aaron";
            _dateSource.Update(_biggest);

            Assert.AreEqual(_biggest, _observableUnderTest[2]);
        }

        [Test]
        public void ShouldReplaceItemOnNewLocationWhenDifferentInstances()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Asc();

            _dateSource.Create(_smallest);
            _dateSource.Create(_bigger);
            _dateSource.Create(_biggest);

            var newBiggest = new LocalTestResource(2) { Name = "Aaron" };
            _dateSource.Update(newBiggest);

            Assert.IsFalse(_observableUnderTest.Contains(_biggest));
            Assert.AreEqual(newBiggest, _observableUnderTest[0]);
        }

        [Test]
        public void ShouldAddEqualItemAfterOtherWhenAscending()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Asc();

            _dateSource.Create(_smallest);
            _dateSource.Create(_bigger);
            _dateSource.Create(_biggest);

            var biggest2 = new LocalTestResource(200) { Name = "Zack" };
            _dateSource.Create(biggest2);

            Assert.AreEqual(biggest2, _observableUnderTest[3]);
        }

        [Test]
        public void ShouldAddEqualItemBeforeOtherWhenDesc()
        {
            _observableUnderTest.OrderBy(ltr => ltr.Name).Desc();

            _dateSource.Create(_smallest);
            _dateSource.Create(_bigger);
            _dateSource.Create(_biggest);

            var biggest2 = new LocalTestResource(200) { Name = "Zack" };
            _dateSource.Create(biggest2);

            Assert.AreEqual(biggest2, _observableUnderTest[0]);
        }

        public void Dispose()
        {
            _observableUnderTest.Dispose();
        }
    }
}