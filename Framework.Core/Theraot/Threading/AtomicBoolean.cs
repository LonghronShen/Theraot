﻿// AtomicBoolean.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// Needed for NET30

using System.Runtime.CompilerServices;
using System.Threading;

namespace Theraot.Threading
{
    public class AtomicBoolean
    {
        private const int Set = 1;
        private const int Unset = 0;
        private int _value;

        public bool Value
        {
            get => _value == Set;

            set => Exchange(value);
        }

        public static implicit operator AtomicBoolean(bool value)
        {
            return new AtomicBoolean
                   {
                       Value = value
                   };
        }

        public static implicit operator bool(AtomicBoolean atomicBoolean)
        {
            return atomicBoolean.Value;
        }

        public static bool operator !=(AtomicBoolean left, AtomicBoolean right)
        {
            if (left is null)
            {
                return !(right is null);
            }

            if (right is null)
            {
                return true;
            }

            return left._value != right._value;
        }

        public static bool operator ==(AtomicBoolean left, AtomicBoolean right)
        {
            if (left is null)
            {
                return right is null;
            }

            if (right is null)
            {
                return false;
            }

            return left._value == right._value;
        }

        public bool CompareExchange(bool expected, bool newVal)
        {
            var newTemp = newVal ? Set : Unset;
            var expectedTemp = expected ? Set : Unset;

            return Interlocked.CompareExchange(ref _value, newTemp, expectedTemp) == expectedTemp;
        }

        public bool Equals(AtomicBoolean obj)
        {
            if (obj is null)
            {
                return false;
            }

            return _value == obj._value;
        }

        public override bool Equals(object obj)
        {
            return obj is AtomicBoolean boolean && Equals(boolean);
        }

        public bool Exchange(bool newVal)
        {
            var newTemp = newVal ? Set : Unset;
            return Interlocked.Exchange(ref _value, newTemp) == Set;
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }

        public bool TrySet()
        {
            return !Exchange(true);
        }

        internal bool TryRelaxedSet()
        {
            return _value == Unset && !Exchange(true);
        }
    }
}