using System;
using System.Collections.Generic;

namespace Restore.Configuration
{
    // REFACTOR: There's a confusing API where there's both IdResolvers and IdExtractors. The former is needed however for instantiation.
    // REFACTOR: It's only that the channels are working with Extraction functions.
    public class Source<T, TId> : ISource<T, TId>
        where TId : IEquatable<TId>
    {
        private readonly IDictionary<Type, object> _endpoints = new Dictionary<Type, object>();
        private Type _resolverType;

        public string Name { get; }
        public Func<T, TId> IdExtractor { get; set; }

        /// <summary>
        /// Gets the default value the extractor return.
        /// Used to determine if <typeparam name="T"></typeparam> is an already existing instance.
        /// </summary>
        public TId DefaultExtractorValue { get; }

        public IIdResolver<T, TId> IdResolver { get; set; }

        protected void SetResolver(Type idResolverType)
        {
            if (idResolverType == null) { throw new ArgumentNullException(nameof(idResolverType)); }
            _resolverType = idResolverType;
        }

        public void SetIdResolver<TR>()
            where TR : IIdResolver<T, TId>
        {
            var resolverClosedType = typeof(TR);

            // as long as T matches it's fine. Regardless of it being an open or closed type. Though that does
            // have impact on the creation as well!
            if (resolverClosedType.IsConstructedGenericType)
            {
                // When we have a resolver for a specific type this does not work such as is the case with countries.
                var openType = resolverClosedType.GetGenericTypeDefinition();
                SetResolver(openType);
            }
            else
            {
                IdResolver = Activator.CreateInstance<TR>();
            }
        }

        public IIdResolver<TDerived, TId> CreateResolver<TDerived>()
            where TDerived : T
        {
            if (IdResolver != null)
            {
                if (typeof(TDerived) != typeof(T))
                {
                    throw new ArgumentException("This source is configured to only support a single type!");
                }

                return IdResolver as IIdResolver<TDerived, TId>;
            }

            var resolverType = _resolverType.MakeGenericType(typeof(TDerived));
            object instance = Activator.CreateInstance(resolverType);
            var idResolver = instance as IIdResolver<TDerived, TId>;
            return idResolver;
        }

        /// <summary>
        /// Sources contain their endpoint which provides the possibility to create various channels
        /// on the same endpoint types. This does imply multiple channels could be writing to the same
        /// endpoint. In the future it might be appropriate to provide endpoint factories instead of
        /// instances.
        /// </summary>
        /// <typeparam name="TDerived">The type this endpoint serves.</typeparam>
        /// <param name="endpoint">The endpoint instance.</param>
        public void AddEndpoint<TDerived>(ICrudEndpoint<TDerived, TId> endpoint)
            where TDerived : T
        {
            _endpoints.Add(typeof(TDerived), endpoint);
        }

        public ICrudEndpoint<TDerived, TId> GetEndpoint<TDerived>()
        {
            return GetEndpoint(typeof(TDerived)) as ICrudEndpoint<TDerived, TId>;
        }

        public object GetEndpoint(Type endpointType)
        {
            if (_endpoints.ContainsKey(endpointType))
            {
                return _endpoints[endpointType];
            }

            return null;
        }

        public Source(string name, Func<T, TId> idExtractor, TId defaultExtractorValue)
        {
            DefaultExtractorValue = defaultExtractorValue;
            Name = name;
            IdExtractor = idExtractor;
        }

        public Source(Func<T, TId> idExtractor)
        {
            IdExtractor = idExtractor;
        }
    }
}