using System;

namespace Restore.ChangeDispatching
{
    public class ChangeDispatchingStep<TItem> : SynchronizationStep<ISynchronizationAction<TItem>, SynchronizationResult>
    {
        public override SynchronizationResult Process(ISynchronizationAction<TItem> action)
        {
            try
            {
                return action.Execute();
            }
            catch (Exception ex)
            {
                // Error handling scenario's/needs, which currently don't need support for yet.
                // * Rollback a transaction.
                // * Log the error.
                // * Swallow the exception and just continue.
                // * Add information to the catched/rethrown exception?
                throw new DispatchingException($"Failed executing action {action.Name} on {action.Applicant}!", ex);
            }
        }
    }
}