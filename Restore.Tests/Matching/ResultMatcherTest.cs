using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Restore.Extensions;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class ResultMatcherTest
    {
        // To big of a unit test, should be splitted up. Created to test first concept.
        [Test]
        public void ShouldMatch()
        {
            var localTcfg = new TypeConfiguration<LocalTestResource, int>(ltr => ltr.CorrelationId);
            var remoteTcfg = new TypeConfiguration<RemoteTestResource, int>(ltr => ltr.Id);
            var resultMatcher = new ResultMatcher<LocalTestResource, RemoteTestResource, int>(localTcfg, remoteTcfg);

            var localResults = new List<LocalTestResource>
            {
                new LocalTestResource(1, 10) { Name = "Local 1" },
                new LocalTestResource(2) { Name = "Only Local 2" }
            };
            var remoteResults = new List<RemoteTestResource>()
            {
                new RemoteTestResource(1, "Remote 1"),
                new RemoteTestResource(3, "Only Remote 2")
            };

            var matches = resultMatcher.Match(localResults, remoteResults).ToList();

            Assert.IsNotNull(matches);
            foreach (var match in matches)
            {
                Debug.WriteLine("{0} - {1}", match?.Result1?.Name, match?.Result2?.Name);
            }
            Assert.AreEqual(matches[0].Result1, localResults[0]);
            Assert.AreEqual(matches.ElementAt(0).Result2, remoteResults[0]);

            Assert.AreEqual(matches.ElementAt(1).Result1, localResults[1]);
            Assert.IsNull(matches.ElementAt(1).Result2);

            Assert.IsNull(matches.ElementAt(2).Result1);
            Assert.AreEqual(matches.ElementAt(2).Result2, remoteResults[1]);
        }

        [Test]
        public void ShouldNotReturnNull()
        {
            var emptyList = new List<int>().AsEnumerable();
            Assert.IsNotNull(emptyList.ToList());
        }
    }

    public class ResultMatcher<T1, T2, TId>
    {
        [NotNull] private readonly TypeConfiguration<T1, TId> _t1Config;
        [NotNull] private readonly TypeConfiguration<T2, TId> _t2Config;

        public ResultMatcher(
            [NotNull] TypeConfiguration<T1, TId> t1Config,
            [NotNull] TypeConfiguration<T2, TId> t2Config)
        {
            if (t1Config == null) throw new ArgumentNullException(nameof(t1Config));
            if (t2Config == null) throw new ArgumentNullException(nameof(t2Config));

            _t1Config = t1Config;
            _t2Config = t2Config;
        }

        public IEnumerable<ResultMatch<T1, T2>> Match(
            [NotNull] IEnumerable<T1> result1,
            [NotNull] IEnumerable<T2> result2)
        {
            if (result1 == null) throw new ArgumentNullException(nameof(result1));
            if (result2 == null) throw new ArgumentNullException(nameof(result2));

            // The disadvantage is that this can be blocking countrary to IObservable.
            var result1List = result1.ToList();
            var result2List = result2.ToList();

            // I can not use this because it can extract a null id. In that case I should just take the item. Just returning a negative Id in
            // that case also doesn't give me the desired result. Because they all need to be different id's in that case.
            /*
            var allIds =
                result1List.Select(r1 => _t1Config.IdExtractor(r1)).Union(result2List.Select(r2 => _t2Config.IdExtractor(r2)));

            var matches = from id in allIds
            join t1r in result1List on id equals _t1Config.IdExtractor(t1r) into t1join
            from t1r in t1join.DefaultIfEmpty()
            join t2r in result2List on id equals _t2Config.IdExtractor(t2r) into t2join
            from t2r in t2join.DefaultIfEmpty()
            select new ResultMatch<T1, T2>(t1r, t2r);

            return matches;*/

            foreach (T1 item1 in result1List)
            {
                var item1Id = _t1Config.IdExtractor(item1);
                if (item1Id == null)
                {
                    // No id to match on could be extracted, it's same to assume it can not be matched because it still requires synchronization.
                    yield return new ResultMatch<T1, T2>(item1, default(T2));
                    continue;
                }

                //var item2Match = result2List.FirstOrDefault(item2 => _t1Config.IdExtractor(item1).Equals(item1Id));
                var item2Match = result2List.Extract(item2 => _t1Config.IdExtractor(item1).Equals(item1Id));
                if (EqualityComparer<T2>.Default.Equals(item2Match, default(T2)))
                {
                    yield return new ResultMatch<T1, T2>(item1, default(T2));
                }
                else
                {
                    yield return new ResultMatch<T1, T2>(item1, item2Match);
                }
            }

            foreach (var item2 in result2List)
            {
                yield return new ResultMatch<T1, T2>(default(T1), item2);
            }
        }
    }

//    public static class ResultMatcherExtension
//    {
//        public static IEnumerable<ResultMatch<T1, T2>> Match<T1, T2, Tid>(
//            [NotNull] this IEnumerable<T1> result1,
//            [NotNull] IEnumerable<T2> result2
//           /* EqualityComparer<T1, T2> */ )
//        {
//            //var matcher = new
//        }
//    }

    public class TypeConfiguration<T, TId>
    {
        [NotNull] public readonly Func<T, IEquatable<TId>> IdExtractor;

        public TypeConfiguration([NotNull] Func<T, IEquatable<TId>> idExtractor)
        {
            if (idExtractor == null) throw new ArgumentNullException(nameof(idExtractor));

            IdExtractor = idExtractor;
        }
    }

    public class ResultMatch<T1, T2>
    {
        public readonly T1 Result1;
        public readonly T2 Result2;

        public ResultMatch(T1 result1, T2 result2)
        {
            if (EqualityComparer<T1>.Default.Equals(result1, default(T1))
                && EqualityComparer<T2>.Default.Equals(result2, default(T2)))
            {
                throw new ArgumentException("A match can never contain two items that contain no value!");
            }

                Result1 = result1;
            Result2 = result2;
        }
    }
}
