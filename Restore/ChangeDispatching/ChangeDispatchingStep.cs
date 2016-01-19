using System;
using JetBrains.Annotations;

namespace Restore.ChangeDispatching
{
    public class ChangeDispatchingStep<TItem> : SynchronizationStep<ISynchronizationAction<TItem>, SynchronizationResult>
    {
        public override SynchronizationResult Process([NotNull] ISynchronizationAction<TItem> input)
        {
            if (input == null) { throw new ArgumentNullException(nameof(input)); }

            try
            {
                return input.Execute();
            }
            catch (Exception ex)
            {
                // Error handling scenario's/needs, which currently don't need support for yet.
                // * Rollback a transaction.
                // * Log the error.
                // * Swallow the exception and just continue.
                // * Add information to the catched/rethrown exception?
                throw new DispatchingException($"Failed executing action {input.Name} on {input.Applicant}!", ex);
            }
        }
    }
}