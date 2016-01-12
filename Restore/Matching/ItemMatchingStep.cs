using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore.Matching
{
    public class ItemMatchingStep<T1, T2, TId, TCfg> : SynchronizationStep<T1, ItemMatch<T1, T2>> 
        where TCfg : IChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>> 
        where TId : IEquatable<TId>
    {
        private TCfg _config;
        private readonly IList<T2> _relatedItems;

        public ItemMatchingStep(TCfg config)
        {
            _config = config;
        }

        // The 'problem' with the current step setup is that this step requires state (the second list of items).
        // Second to that is that doing it this way forces me to split up the the first part (item one matches)
        // and the remainder (items 2).
        // - Change the design of a step into two objects/phases 1. Defintion (containing observers) 2. Execution.
        // Where 1. creates 2 with some state. \
        //   - This still doesn't solve the problem of having to merge two lists.
        // - Have Select (single item steps) and SelectMany steps going from one to many. => One IEnumerable to items.
        // Come up later how to solve this problem.
        public override ItemMatch<T1, T2> Process(T1 input)
        {
            var id = _config.Type1EndpointConfiguration.TypeConfig.IdExtractor(input);
            if (id == null)
            {
                return new ItemMatch<T1, T2>(input, default(T2));
            }

            return null;
        }
    }
}
