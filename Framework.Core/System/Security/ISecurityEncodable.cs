﻿#if (LESSTHAN_NETCOREAPP20 || LESSTHAN_NETSTANDARD20) && !PROFILE328
#pragma warning disable CA1041 // Provide ObsoleteAttribute message

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Security
{
    [Runtime.InteropServices.ComVisible(true)]
    public interface ISecurityEncodable
    {
        SecurityElement ToXml();

        void FromXml(SecurityElement e);
    }
}
#endif