using System;
using System.Collections.Generic;

namespace Restore.Matching
{
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