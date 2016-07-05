using System;

namespace Restore.Configuration
{
    public interface ISource<T, TId>
        where TId : IEquatable<TId>
    {
        string Name { get; }

        // REFACTOR: Fix confusing API with both Extractors and Resolvers!
        Func<T, TId> IdExtractor { get; }

        /// <summary>
        /// Sets the Id resolver for this source. Instances with a derived type of <typeparam name="T"></typeparam>T
        /// will be instantiated on channel creation.
        /// </summary>
        /// <typeparam name="TR">The resolver type. Provide a </typeparam>
        void SetIdResolver<TR>()
            where TR : IIdResolver<T, TId>;

        IIdResolver<TDerived, TId> CreateResolver<TDerived>()
            where TDerived : T;

        // Having a generic list of endpoints is probably not going to play well with invariant IdExtractor.
        void AddEndpoint<TDerived>(ICrudEndpoint<TDerived, TId> endpoint)
            where TDerived : T;
        object GetEndpoint(Type endpointType);
        ICrudEndpoint<TDerived, TId> GetEndpoint<TDerived>();
    }
}