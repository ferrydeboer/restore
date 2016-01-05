using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class PredefinedQueryReplicatorTest
    {
        [Test]
        public async Task ShouldSimplyMapToPredefinedQuery()
        {
            var localDataEndpoint = new TestAsyncDataEndpoint<LocalTestResource>("Local", new List<LocalTestResource>
                {
                    new LocalTestResource(1, 10),
                    new LocalTestResource(2, 20)
                });
            var remoteDataEndpoint = new TestAsyncDataEndpoint<RemoteTestResource>("Remote", new List<RemoteTestResource>
                {
                    new RemoteTestResource(1, "Remote 1"),
                    new RemoteTestResource(2, "Remote 2"),
                    new RemoteTestResource(3, "Remote 3")
                });
            var replicator = new PredefinedQueryReplicatorAsync<LocalTestResource, RemoteTestResource>(localDataEndpoint, remoteDataEndpoint,
                async rde =>
                {
                    Debug.WriteLine("Loading remote data!");
                    return await rde?.GetAllAsync();
                });

            replicator.QueryReplicated += (sender, args) =>
            {
                Assert.AreEqual(remoteDataEndpoint.Data, args?.ReplicationResults);
            };

            // Trigger a query event on the local data endpoint.
            var replicated = await replicator.Replicate(new DataLoadedEventArgs<LocalTestResource>(localDataEndpoint.Data.AsEnumerable()));
        }
    }

    /// <summary>
    /// <p>
    /// Data Endpoint interface that is being used as a facade and add's notification mechanisms to which one can subscribe for various purposes.
    /// </p>
    /// <p>
    /// Given the current asynchronous nature of a lot of application it is chosed to solely work with asynchronous definitions. Because it is fairly trivial 
    /// to wrap a synchronous method in a Task within a facade implementation.
    /// </p>
    /// </summary>
    /// <typeparam name="T">The data type this endpoint is persisting and retrieving.</typeparam>
    public interface IAsyncDataEndpoint<T>
    {
        /// <summary>
        /// Returns the name the endpoint which can be used to give it a clearer distinction than what the 
        /// channel simply uses.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Thrown when a list of data is being loaded in a data endpoint by a query or some other event. This totally depends on the 
        /// endpoint implementation.
        /// </summary>
        event EventHandler<DataLoadedEventArgs<T>> DataLoaded;

        /// <summary>
        /// Method that should return ALL resources of type T.
        /// </summary>
        /// <returns>All resources of type T.</returns>
        /// <remarks>
        /// Obvisouly task can not be null, but Enumerable should not be null but empty in case of zero results.
        /// </remarks>
        [NotNull]
        Task<IEnumerable<T>> GetAllAsync();
    }

    public class DataLoadedEventArgs<T>
    {
        public IEnumerable<T> LoadedData { get; private set; }

        public DataLoadedEventArgs(IEnumerable<T> loadedData)
        {
            LoadedData = loadedData;
        }
    }

    public class TestAsyncDataEndpoint<T> : IAsyncDataEndpoint<T>
    {
        public TestAsyncDataEndpoint(string name)
        {
            Name = name;
        }

        public TestAsyncDataEndpoint(string name, List<T> data) : this(name)
        {
            Data = data;
        }

        public string Name { get; private set; }

        public event EventHandler<DataLoadedEventArgs<T>> DataLoaded;

        [NotNull] public List<T> Data { get; private set; }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            Debug.WriteLine($"Firing DataLoaded from {Name}");
            try
            {
                OnDataLoaded(new DataLoadedEventArgs<T>(Data));
                Assert.Fail("This should not run!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("This is not catched here");
            }

            // Best
            //return Task.FromResult(Data.AsEnumerable());
            
            // Better
            return new TaskFactory().StartNew<IEnumerable<T>>(() =>
            {
                if (Name == "Remote")
                {
                    throw new Exception("This is not catchable in the event handler that call this.");
                }
                Debug.WriteLine($"Returning data from {Name}");
                return Data;
            });
            /*
            // Bad
            var task = new Task<IEnumerable<T>>(() =>
            {
                Debug.WriteLine($"Returning data from {Name}");
                return Data;
            });
            task.Start();
            return task;
            */
        }

        protected virtual void OnDataLoaded(DataLoadedEventArgs<T> e)
        {
            DataLoaded?.Invoke(this, e);
        }
    }

    public class RemoteTestResource
    {
        public RemoteTestResource(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }

    public class LocalTestResource
    {
        public LocalTestResource(int correlationId, int localId)
        {
            CorrelationId = correlationId;
            LocalId = localId;
        }

        public int CorrelationId { get; }

        public int LocalId { get; }

        public string Name { get; set; }
    }

    public sealed class PredefinedQueryReplicatorAsync<T1, T2>
    {
        [NotNull]
        private readonly Func<IAsyncDataEndpoint<T2>, Task<IEnumerable<T2>>> _endpoint2Query;

        public PredefinedQueryReplicatorAsync(
            [NotNull] IAsyncDataEndpoint<T1> endpoint1,
            [NotNull] IAsyncDataEndpoint<T2> endpoint2,
            [NotNull] Func<IAsyncDataEndpoint<T2>, Task<IEnumerable<T2>>> endpoint2Query)
        {
            if (endpoint1 == null) throw new ArgumentNullException(nameof(endpoint1));
            if (endpoint2 == null) throw new ArgumentNullException(nameof(endpoint2));
            if (endpoint2Query == null) throw new ArgumentNullException(nameof(endpoint2Query));

            Endpoint1 = endpoint1;
            Endpoint1.DataLoaded += Endpoint1_DataLoaded;
            Endpoint2 = endpoint2;
            _endpoint2Query = endpoint2Query;
        }

        private async void Endpoint1_DataLoaded(object sender, DataLoadedEventArgs<T1> e)
        {
            // This is however an async void method. This is probably not working because
            // the method returns to the context on the await
            // ReSharper disable once PossibleNullReferenceException
            await Replicate(e);
        }

        public async Task<IEnumerable<T2>>  Replicate(DataLoadedEventArgs<T1> e)
        {
            IEnumerable<T2> replicationResults;
            try
            {
                replicationResults = await _endpoint2Query(Endpoint2).ConfigureAwait(false);
                OnQueryReplicated(new QueryReplicationEventArgs<T1, T2>(
                    e.LoadedData,
                    replicationResults));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw new Exception("WrappedException, this is not catchable when raise event!");
            }
            return replicationResults;
        }

        public IAsyncDataEndpoint<T1> Endpoint1 { get; }

        public IAsyncDataEndpoint<T2> Endpoint2 { get; }

        /// <summary>
        /// Raised when replication of a query occurs.
        /// </summary>
        public event EventHandler<QueryReplicationEventArgs<T1, T2>> QueryReplicated;

        private void OnQueryReplicated(QueryReplicationEventArgs<T1, T2> e)
        {
            QueryReplicated?.Invoke(this, e);
        }
    }

    public class QueryReplicationEventArgs<T1, T2> : EventArgs
    {
        public QueryReplicationEventArgs(IEnumerable<T1> originResults, IEnumerable<T2> replicationResults)
        {
            OriginResults = originResults;
            ReplicationResults = replicationResults;
        }

        /// <summary>
        /// The results that originated the replication.
        /// </summary>
        public IEnumerable<T1> OriginResults { get; }

        /// <summary>
        /// The results from the opposite endpoint.
        /// </summary>
        public IEnumerable<T2> ReplicationResults { get; }
    }
}
