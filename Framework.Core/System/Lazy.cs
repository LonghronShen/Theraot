﻿#if LESSTHAN_NET40

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Theraot.Core;
using Theraot.Reflection;

namespace System
{
    [DebuggerDisplay("IsValueCreated={IsValueCreated}, Value={ValueForDebugDisplay}")]
    [Serializable]
    public class Lazy<T>
    {
        private int _isValueCreated;
        private Func<T> _valueFactory;

        public Lazy()
            : this(LazyThreadSafetyMode.ExecutionAndPublication)
        {
            // Empty
        }

        public Lazy(Func<T> valueFactory)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
            // Empty
        }

        public Lazy(bool isThreadSafe)
            : this(isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
            // Empty
        }

        public Lazy(LazyThreadSafetyMode mode)
            : this(ConstructorHelper.Create<T>, mode, false)
        {
            // Empty
        }

        public Lazy(Func<T> valueFactory, bool isThreadSafe)
            : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
            // Empty
        }

        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
            : this(valueFactory, mode, true)
        {
            // Empty
        }

        private Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode, bool cacheExceptions)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            switch (mode)
            {
                case LazyThreadSafetyMode.None:
                {
                    if (cacheExceptions)
                    {
                        var threads = new HashSet<Thread>();
                        _valueFactory =
                            () => CachingNoneMode(threads);
                    }
                    else
                    {
                        var threads = new HashSet<Thread>();
                        _valueFactory =
                            () => NoneMode(threads);
                    }
                }
                    break;

                case LazyThreadSafetyMode.PublicationOnly:
                {
                    _valueFactory = PublicationOnlyMode;
                }
                    break;

                default: /*LazyThreadSafetyMode.ExecutionAndPublication*/
                {
                    if (cacheExceptions)
                    {
                        Thread thread = null;
                        var waitHandle = new ManualResetEvent(false);
                        _valueFactory =
                            () => CachingFullMode(valueFactory, waitHandle, ref thread);
                    }
                    else
                    {
                        Thread thread = null;
                        var waitHandle = new ManualResetEvent(false);
                        var preIsValueCreated = 0;
                        _valueFactory =
                            () => FullMode(valueFactory, waitHandle, ref thread, ref preIsValueCreated);
                    }
                }
                    break;
            }

            T CachingNoneMode(HashSet<Thread> threads)
            {
                if (Volatile.Read(ref _isValueCreated) != 0)
                {
                    return _valueFactory.Invoke();
                }

                try
                {
                    // lock (threads) // This is meant to not be thread-safe
                    {
                        var currentThread = Thread.CurrentThread;
                        if (threads.Contains(currentThread))
                        {
                            throw new InvalidOperationException();
                        }

                        threads.Add(currentThread);
                    }
                    ValueForDebugDisplay = valueFactory();
                    _valueFactory = FuncHelper.GetReturnFunc(ValueForDebugDisplay);
                    Volatile.Write(ref _isValueCreated, 1);
                    return ValueForDebugDisplay;
                }
                catch (Exception exception)
                {
                    _valueFactory = FuncHelper.GetThrowFunc<T>(exception);
                    throw;
                }
                finally
                {
                    // lock (threads) // This is meant to not be thread-safe
                    {
                        threads.Remove(Thread.CurrentThread);
                    }
                }
            }

            T NoneMode(HashSet<Thread> threads)
            {
                if (Volatile.Read(ref _isValueCreated) != 0)
                {
                    return _valueFactory.Invoke();
                }

                try
                {
                    // lock (threads) // This is meant to not be thread-safe
                    {
                        var currentThread = Thread.CurrentThread;
                        if (threads.Contains(currentThread))
                        {
                            throw new InvalidOperationException();
                        }

                        threads.Add(currentThread);
                    }
                    ValueForDebugDisplay = valueFactory();
                    _valueFactory = FuncHelper.GetReturnFunc(ValueForDebugDisplay);
                    Volatile.Write(ref _isValueCreated, 1);
                    return ValueForDebugDisplay;
                }
                catch (Exception)
                {
                    Volatile.Write(ref _isValueCreated, 0);
                    throw;
                }
                finally
                {
                    // lock (threads) // This is meant to not be thread-safe
                    {
                        threads.Remove(Thread.CurrentThread);
                    }
                }
            }

            T PublicationOnlyMode()
            {
                ValueForDebugDisplay = valueFactory();
                if (Interlocked.CompareExchange(ref _isValueCreated, 1, 0) == 0)
                {
                    _valueFactory = FuncHelper.GetReturnFunc(ValueForDebugDisplay);
                }

                return ValueForDebugDisplay;
            }
        }

        public bool IsValueCreated => _isValueCreated == 1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value => _valueFactory.Invoke();

        internal T ValueForDebugDisplay { get; private set; }

        private T CachingFullMode(Func<T> valueFactory, EventWaitHandle waitHandle, ref Thread thread)
        {
            if (Interlocked.CompareExchange(ref _isValueCreated, 1, 0) == 0)
            {
                try
                {
                    thread = Thread.CurrentThread;
                    GC.KeepAlive(thread);
                    ValueForDebugDisplay = valueFactory.Invoke();
                    _valueFactory = FuncHelper.GetReturnFunc(ValueForDebugDisplay);
                    return ValueForDebugDisplay;
                }
                catch (Exception exc)
                {
                    _valueFactory = FuncHelper.GetThrowFunc<T>(exc);
                    throw;
                }
                finally
                {
                    waitHandle.Set();
                    thread = null;
                }
            }

            if (thread == Thread.CurrentThread)
            {
                throw new InvalidOperationException();
            }

            waitHandle.WaitOne();
            return _valueFactory.Invoke();
        }

        private T FullMode(Func<T> valueFactory, EventWaitHandle waitHandle, ref Thread thread, ref int preIsValueCreated)
        {
            back:
            if (Interlocked.CompareExchange(ref preIsValueCreated, 1, 0) == 0)
            {
                try
                {
                    thread = Thread.CurrentThread;
                    GC.KeepAlive(thread);
                    ValueForDebugDisplay = valueFactory.Invoke();
                    _valueFactory = FuncHelper.GetReturnFunc(ValueForDebugDisplay);
                    Volatile.Write(ref _isValueCreated, 1);
                    return ValueForDebugDisplay;
                }
                catch (Exception)
                {
                    Volatile.Write(ref preIsValueCreated, 0);
                    throw;
                }
                finally
                {
                    waitHandle.Set();
                    thread = null;
                }
            }

            if (thread == Thread.CurrentThread)
            {
                throw new InvalidOperationException();
            }

            waitHandle.WaitOne();
            if (Volatile.Read(ref _isValueCreated) == 1)
            {
                return _valueFactory.Invoke();
            }

            goto back;
        }
    }

    public class Lazy<T, TMetadata> : Lazy<T>
    {
        public Lazy(TMetadata metadata)
        {
            Metadata = metadata;
        }

        public Lazy(Func<T> valueFactory, TMetadata metadata)
            : base(valueFactory)
        {
            Metadata = metadata;
        }

        public Lazy(TMetadata metadata, bool isThreadSafe)
            : base(isThreadSafe)
        {
            Metadata = metadata;
        }

        public Lazy(TMetadata metadata, LazyThreadSafetyMode mode)
            : base(mode)
        {
            Metadata = metadata;
        }

        public Lazy(Func<T> valueFactory, TMetadata metadata, bool isThreadSafe)
            : base(valueFactory, isThreadSafe)
        {
            Metadata = metadata;
        }

        public Lazy(Func<T> valueFactory, TMetadata metadata, LazyThreadSafetyMode mode)
            : base(valueFactory, mode)
        {
            Metadata = metadata;
        }

        public TMetadata Metadata { get; }
    }
}

#endif