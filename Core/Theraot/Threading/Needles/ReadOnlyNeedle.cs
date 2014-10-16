﻿#if FAT

using System;
using System.Collections.Generic;
using Theraot.Core;

namespace Theraot.Threading.Needles
{
    [System.Serializable]
    [global::System.Diagnostics.DebuggerNonUserCode]
    public class ReadOnlyNeedle<T> : IReadOnlyNeedle<T>, IEquatable<ReadOnlyNeedle<T>>
    {
        private readonly INeedle<T> _target;

        public ReadOnlyNeedle()
        {
            _target = null;
        }

        public ReadOnlyNeedle(T target)
        {
            _target = new StructNeedle<T>(target);
        }

        public ReadOnlyNeedle(INeedle<T> target)
        {
            _target = target;
        }

        public bool IsAlive
        {
            get
            {
                var target = _target;
                return !ReferenceEquals(_target, null) && target.IsAlive;
            }
        }

        public T Value
        {
            get
            {
                return (T)_target;
            }
        }

        public static explicit operator T(ReadOnlyNeedle<T> needle)
        {
            return Check.NotNullArgument(needle, "needle").Value;
        }

        public static implicit operator ReadOnlyNeedle<T>(T field)
        {
            return new ReadOnlyNeedle<T>(field);
        }

        public static bool operator !=(ReadOnlyNeedle<T> left, ReadOnlyNeedle<T> right)
        {
            return NotEqualsExtracted(left, right);
        }

        public static bool operator ==(ReadOnlyNeedle<T> left, ReadOnlyNeedle<T> right)
        {
            return EqualsExtracted(left, right);
        }

        public override bool Equals(object obj)
        {
            var _obj = obj as ReadOnlyNeedle<T>;
            if (!ReferenceEquals(null, _obj))
            {
                return EqualsExtracted(this, _obj);
            }
            else
            {
                return _target.Equals(obj);
            }
        }

        public bool Equals(ReadOnlyNeedle<T> other)
        {
            return EqualsExtracted(this, other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode((T)_target);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        private static bool EqualsExtracted(ReadOnlyNeedle<T> left, ReadOnlyNeedle<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            else
            {
                return left._target.Equals(right._target);
            }
        }

        private static bool NotEqualsExtracted(ReadOnlyNeedle<T> left, ReadOnlyNeedle<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }
            else
            {
                return !left._target.Equals(right._target);
            }
        }
    }
}

#endif