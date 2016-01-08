using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Restore.Matching
{
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

        public async Task<IEnumerable<T2>> Replicate(DataLoadedEventArgs<T1> e)
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
                throw new Exception("WrappedException, this is not catchable when using raised events!");
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
}