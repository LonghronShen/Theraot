﻿#if FAT
using System;
using System.Collections.Generic;

namespace Theraot.Collections
{
    [System.Diagnostics.DebuggerNonUserCode]
    public class ConversionSet<TInput, TOutput> : ProgressiveSet<TOutput>
    {
        public ConversionSet(IEnumerable<TInput> wrapped, Func<TInput, TOutput> converter)
            : base(BuildEnumerable(wrapped, converter))
        {
            // Empty
        }

        public ConversionSet(IEnumerable<TInput> wrapped, Func<TInput, TOutput> converter, Predicate<TInput> filter)
            : base(BuildEnumerable(wrapped, converter, filter))
        {
            // Empty
        }

        private static IEnumerable<TOutput> BuildEnumerable(IEnumerable<TInput> wrapped, Func<TInput, TOutput> converter)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(wrapped));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            return BuildEnumerableExtracted();

            IEnumerable<TOutput> BuildEnumerableExtracted()
            {
                foreach (var item in wrapped)
                {
                    yield return converter(item);
                }
            }
        }

        private static IEnumerable<TOutput> BuildEnumerable(IEnumerable<TInput> wrapped, Func<TInput, TOutput> converter, Predicate<TInput> filter)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(wrapped));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            return BuildEnumerableExtracted();

            IEnumerable<TOutput> BuildEnumerableExtracted()
            {
                foreach (var item in wrapped)
                {
                    if (filter(item))
                    {
                        yield return converter(item);
                    }
                }
            }
        }
    }
}

#endif