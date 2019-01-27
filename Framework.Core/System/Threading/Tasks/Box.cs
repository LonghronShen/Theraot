﻿#if LESSTHAN_NET40

// BASEDON: https://github.com/dotnet/corefx/blob/e0ba7aa8026280ee3571179cc06431baf1dfaaac/src/System.Threading.Tasks.Parallel/src/System/Threading/Tasks/Box.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System.Threading.Tasks
{
	/// <summary>Utility class for allocating value types as heap variables.</summary>
	internal class Box<T>
	{
		internal T Value;

		internal Box(T value)
		{
			this.Value = value;
		}
	}  // class Box<T>
}  // namespace

#endif