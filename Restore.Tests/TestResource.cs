using System;
using System.Collections.Generic;


namespace Restore.Tests
{
    public class TestResource
    {
        private string _description;
        private List<string> _lastUpdates = new List<string>();

        public TestResource(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    _lastUpdates.Add("Description");
                }
            }
        }

        public List<string> LastUpdates
        {
            get { return _lastUpdates; }
        }

        public string CorrelationId { get; set; }
        public bool Deleted { get; set; }
        public DateTime ServerModifiedAt { get; set; }

        /// <summary>
        /// Copy method to help creating distincts objects at both data endpoints.
        /// </summary>
        public TestResource Copy()
        {
            return new TestResource(Id)
            {
                CorrelationId = CorrelationId,
                Description = Description
            };
        }

        /// <summary>
        /// Update method that needs to be called when storing to test actual updates.
        /// </summary>
        public void Update(TestResource source)
        {
            _lastUpdates = new List<string>();
            Description = source.Description;
        }
    }
}