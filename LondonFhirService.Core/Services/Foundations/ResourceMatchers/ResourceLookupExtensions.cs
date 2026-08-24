// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers
{
    internal static class ResourceLookupExtensions
    {
        /// <summary>
        /// Builds a match-key lookup, keeping the first resource for a repeated key instead of
        /// throwing.
        ///
        /// The matchers key on clinical identifiers - an NHS number, a SNOMED code - taken from
        /// third-party provider bundles, and nothing upstream guarantees those are unique within
        /// one bundle. With a plain ToDictionary a single repeated key threw, and that exception
        /// was collected and re-thrown for the WHOLE comparison, discarding the diffs already
        /// computed for every other resource type in the pair and marking the record Failed. For
        /// a Condition, two resources sharing a code in one bundle is routine rather than
        /// anomalous.
        ///
        /// First wins, and the extra occurrences are handed to <paramref name="onDuplicate"/>
        /// rather than discarded. That callback is what keeps this honest: a resource that simply
        /// vanished from the lookup would also vanish from the comparison, so a report could read
        /// clean while data was missing from it - a quieter failure than the exception this
        /// replaced, and a worse one for a clinical comparison. Callers surface the extras as
        /// unmatched resources so they stay visible in the diff.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionaryFirstWins<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector,
            Action<TKey, TValue> onDuplicate = null)
            where TKey : notnull
        {
            var lookup = new Dictionary<TKey, TValue>();

            foreach (TSource item in source)
            {
                TKey key = keySelector(item);
                TValue value = valueSelector(item);

                // TryAdd rather than ContainsKey then Add: one hash lookup instead of two, and
                // the return value is exactly the "was this a duplicate" answer we need.
                if (lookup.TryAdd(key, value) == false)
                {
                    onDuplicate?.Invoke(key, value);
                }
            }

            return lookup;
        }
    }
}
