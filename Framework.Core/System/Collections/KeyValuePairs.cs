#if LESSTHAN_NETSTANDARD13
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable CA2235 // Mark all non-serializable fields
#pragma warning disable CC0021 // Use nameof
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
// ReSharper disable once VirtualMemberCallInConstructor

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Collections
{
    [DebuggerDisplay("{_value}", Name = "[{_key}]")]
    internal class KeyValuePairs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object _key;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly object _value;

        public KeyValuePairs(object key, object value)
        {
            _value = value;
            _key = key;
        }
    }
}

#endif