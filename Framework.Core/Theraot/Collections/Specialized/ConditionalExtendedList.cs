﻿// Needed for NET30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Theraot.Core;

namespace Theraot.Collections.Specialized
{
    [DebuggerNonUserCode]
    public sealed class ConditionalExtendedList<T> : IList<T>
    {
        private readonly IList<T> _append;

        private readonly Func<bool> _enumerateAppend;

        private readonly Func<bool> _enumerateTarget;

        private readonly IList<T> _target;

        public ConditionalExtendedList(IEnumerable<T> target, IEnumerable<T> append, Func<bool> enumerateTarget, Func<bool> enumerateAppend)
        {
            _target = target == null ? ArrayEx.Empty<T>() : target.WrapAsIList();
            _append = append == null ? ArrayEx.Empty<T>() : append.WrapAsIList();
            _enumerateTarget = enumerateTarget ?? (target == null ? FuncHelper.GetFallacyFunc() : FuncHelper.GetTautologyFunc());
            _enumerateAppend = enumerateAppend ?? (append == null ? FuncHelper.GetFallacyFunc() : FuncHelper.GetTautologyFunc());
        }

        public int Count => _target.Count + _append.Count;

        bool ICollection<T>.IsReadOnly => true;

        public T this[int index]
        {
            get
            {
                if (_enumerateTarget())
                {
                    var count = _target.Count;
                    if (index < count)
                    {
                        return _target[index];
                    }

                    index -= count;
                }

                if (_enumerateAppend())
                {
                    return _append[index];
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return _target.Contains(item) || _append.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var enumerateTarget = _enumerateTarget();
            var enumerateAppend = _enumerateAppend();
            if (enumerateTarget)
            {
                var targetCount = _target.Count;
                if (enumerateAppend)
                {
                    var appendCount = _append.Count;
                    Extensions.CanCopyTo(targetCount + appendCount, array, arrayIndex);
                    _target.CopyTo(array, arrayIndex);
                    Extensions.CopyTo(_append, array, targetCount);
                }
                else
                {
                    Extensions.CanCopyTo(targetCount, array, arrayIndex);
                    _target.CopyTo(array, arrayIndex);
                }
            }
            else
            {
                if (!enumerateAppend)
                {
                    return;
                }

                var appendCount = _append.Count;
                Extensions.CanCopyTo(appendCount, array, arrayIndex);
                _append.CopyTo(array);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_enumerateTarget())
            {
                foreach (var item in _target)
                {
                    yield return item;
                }
            }

            if (!_enumerateAppend())
            {
                yield break;
            }

            foreach (var item in _append)
            {
                yield return item;
            }
        }

        public int IndexOf(T item)
        {
            var offset = 0;
            if (_enumerateTarget())
            {
                var targetIndex = _target.IndexOf(item);
                if (targetIndex != -1)
                {
                    return targetIndex;
                }

                offset = _target.Count;
            }

            if (!_enumerateAppend())
            {
                return -1;
            }

            var appendIndex = _append.IndexOf(item);
            if (appendIndex != -1)
            {
                return appendIndex + offset;
            }

            return -1;
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
    }
}