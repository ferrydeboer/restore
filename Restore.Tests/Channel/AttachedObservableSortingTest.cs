using JetBrains.Annotations;
using NUnit.Framework;
using Restore.Channel;
using Restore.Channel.Configuration;

namespace Restore.Tests.Channel
{
    [TestFixture]
    public class AttachedObservableSortingTest
    {
        private AttachedObservableCollection<LocalTestResource> _observableUnderTest;

        private InMemoryCrudDataEndpoint<LocalTestResource, int> _dateSource;
        private readonly LocalTestResource _smallest = new LocalTestResource(10) { Name = "Anthony" };
        private readonly LocalTestResource _bigger = new LocalTestResource(1) { Name = "Bert" };
        private readonly LocalTestResource _biggest = new LocalTestResource(2) { Name = "Zack" };
        
        [SetUp]
        public void SetUpTest()
        {
            _dateSource = new InMemoryCrudDataEndpoint<LocalTestResource, int>(
                new TypeConfiguration<LocalTestResource, int>(ltr => ltr.LocalId));
            _observableUnderTest = new AttachedObservableCollection<LocalTestResource>(
                _dateSource
                , new LocalTestResourceIdComparer());

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
    }
}