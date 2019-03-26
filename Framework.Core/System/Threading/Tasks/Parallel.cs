﻿#if (LESSTHAN_NET40 || NETSTANDARD1_0) && !PROFILE328

#pragma warning disable CC0014 // Use ternary operator
#pragma warning disable CC0031 // Check for null before calling a delegate
// ReSharper disable PossibleNullReferenceException

// BASEDON: https://github.com/dotnet/corefx/blob/e0ba7aa8026280ee3571179cc06431baf1dfaaac/src/System.Threading.Tasks.Parallel/src/System/Threading/Tasks/Parallel.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
// A helper class that contains parallel versions of various looping constructs.  This
// internally uses the task parallel library, but takes care to expose very little
// evidence of this infrastructure being used.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Diagnostics;
using Theraot.Collections.ThreadSafe;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides support for parallel loops and regions.
    /// </summary>
    /// <remarks>
    /// The <see cref="T:System.Threading.Tasks.Parallel"/> class provides library-based data parallel replacements
    /// for common operations such as for loops, for each loops, and execution of a set of statements.
    /// </remarks>
    public static class Parallel
    {
        // We use a stride for loops to amortize the frequency of interlocked operations.
        internal const int Default_Loop_Stride = 16;

        // Static variable to hold default parallel options
        private static readonly ParallelOptions _defaultParallelOptions = new ParallelOptions();

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the iteration count (an Int32) as a parameter.
        /// </remarks>
        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForWorker<object>
            (
                fromInclusive, toExclusive,
                _defaultParallelOptions,
                body,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the iteration count (an Int64) as a parameter.
        /// </remarks>
        public static ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForWorker64<object>
            (
                fromInclusive,
                toExclusive,
                _defaultParallelOptions,
                body,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the iteration count (an Int32) as a parameter.
        /// </remarks>
        public static ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForWorker<object>
            (
                fromInclusive,
                toExclusive,
                parallelOptions,
                body,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the iteration count (an Int64) as a parameter.
        /// </remarks>
        public static ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForWorker64<object>
            (
                fromInclusive,
                toExclusive,
                parallelOptions,
                body,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int32),
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </para>
        /// <para>
        /// Calling <see cref="System.Threading.Tasks.ParallelLoopState.Break()">ParallelLoopState.Break()</see>
        /// informs the For operation that iterations after the current one need not
        /// execute.  However, all iterations before the current one will still need to be executed if they have not already.
        /// Therefore, calling Break is similar to using a break operation within a
        /// conventional for loop in a language like C#, but it is not a perfect substitute: for example, there is no guarantee that iterations
        /// after the current one will definitely not execute.
        /// </para>
        /// <para>
        /// If executing all iterations before the current one is not necessary,
        /// <see cref="System.Threading.Tasks.ParallelLoopState.Stop()">ParallelLoopState.Stop()</see>
        /// should be preferred to using Break.  Calling Stop informs the For loop that it may abandon all remaining
        /// iterations, regardless of whether they're for iterations above or below the current,
        /// since all required work has already been completed.  As with Break, however, there are no guarantees regarding
        /// which other iterations will not execute.
        /// </para>
        /// <para>
        /// When a loop is ended prematurely, the <see cref="T:ParallelLoopState"/> that's returned will contain
        /// relevant information about the loop's completion.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForWorker<object>
            (
                fromInclusive,
                toExclusive,
                _defaultParallelOptions,
                null,
                body,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int64),
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </remarks>
        public static ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForWorker64<object>
            (
                fromInclusive,
                toExclusive,
                _defaultParallelOptions,
                null,
                body,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int32),
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </remarks>
        public static ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForWorker<object>
            (
                fromInclusive,
                toExclusive,
                parallelOptions,
                null,
                body,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int64),
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </remarks>
        public static ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForWorker64<object>
            (
                fromInclusive,
                toExclusive,
                parallelOptions,
                null,
                body,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int32),
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult For<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> localInit, Func<int, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            return ForWorker
            (
                fromInclusive,
                toExclusive,
                _defaultParallelOptions,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.  Supports 64-bit indices.
        /// </summary>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int64),
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult For<TLocal>(long fromInclusive, long toExclusive, Func<TLocal> localInit, Func<long, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            return ForWorker64
            (
                fromInclusive,
                toExclusive,
                _defaultParallelOptions,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int32),
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult For<TLocal>
        (
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<int, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForWorker
            (
                fromInclusive,
                toExclusive,
                parallelOptions,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each value in the iteration range:
        /// [fromInclusive, toExclusive).  It is provided with the following parameters: the iteration count (an Int64),
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult For<TLocal>
        (
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<long, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForWorker64
            (
                fromInclusive,
                toExclusive,
                parallelOptions,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the current element as a parameter.
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForEachWorker<TSource, object>
            (
                source, _defaultParallelOptions, body, null, null, null, null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the current element as a parameter.
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForEachWorker<TSource, object>
            (
                source, parallelOptions, body, null, null, null, null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            IEnumerable<TSource> source,
            Action<TSource, ParallelLoopState> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForEachWorker<TSource, object>
            (
                source, _defaultParallelOptions, null, body, null, null, null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForEachWorker<TSource, object>
            (
                source, parallelOptions, null, body, null, null, null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and the current element's index (an Int64).
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            IEnumerable<TSource> source,
            Action<TSource, ParallelLoopState, long> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return ForEachWorker<TSource, object>
            (
                source, _defaultParallelOptions, null, null, body, null, null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and the current element's index (an Int64).
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState, long> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForEachWorker<TSource, object>
            (
                source, parallelOptions, null, null, body, null, null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>
        (
            IEnumerable<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            return ForEachWorker
            (
                source,
                _defaultParallelOptions,
                null,
                null,
                null, body,
                null,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>
        (
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForEachWorker
            (
                source, parallelOptions, null, null, null, body, null, localInit, localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, the current element's index (an Int64), and some local
        /// state that may be shared amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>
        (
            IEnumerable<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            return ForEachWorker
            (
                source,
                _defaultParallelOptions,
                null,
                null,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on an <see cref="T:System.Collections.IEnumerable{TSource}"/>
        /// in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// enumerable.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, the current element's index (an Int64), and some local
        /// state that may be shared amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>
        (
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return ForEachWorker
            (
                source,
                parallelOptions,
                null,
                null,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">
        /// Partitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The Partitioner that contains the original data source.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> Partitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> Partitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner does not return
        /// the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner returns an IList
        /// with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() method in the <paramref name="source"/> Partitioner returns an
        /// IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the current element as a parameter.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            Partitioner<TSource> source,
            Action<TSource> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return PartitionerForEachWorker<TSource, object>
            (
                source, _defaultParallelOptions, body, null, null, null,
                null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">
        /// Partitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The Partitioner that contains the original data source.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> Partitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> Partitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner does not return
        /// the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner returns an IList
        /// with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() method in the <paramref name="source"/> Partitioner returns an
        /// IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            Partitioner<TSource> source,
            Action<TSource, ParallelLoopState> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            return PartitionerForEachWorker<TSource, object>
            (
                source, _defaultParallelOptions, null, body, null, null,
                null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.OrderablePartitioner{TSource}">
        /// OrderablePartitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The OrderablePartitioner that contains the original data source.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// KeysNormalized property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> OrderablePartitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner do not return the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IList with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() or GetDynamicOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and the current element's index (an Int64).
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            OrderablePartitioner<TSource> source,
            Action<TSource, ParallelLoopState, long> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException("SR.Parallel_ForEach_OrderedPartitionerKeysNotNormalized");
            }

            return PartitionerForEachWorker<TSource, object>
            (
                source, _defaultParallelOptions, null, null, body, null,
                null, null, null
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">
        /// Partitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">The Partitioner that contains the original data source.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> Partitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> Partitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner does not return
        /// the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner returns an IList
        /// with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() method in the <paramref name="source"/> Partitioner returns an
        /// IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>
        (
            Partitioner<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            return PartitionerForEachWorker
            (
                source,
                _defaultParallelOptions,
                null,
                null,
                null,
                body,
                null,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.OrderablePartitioner{TSource}">
        /// OrderablePartitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">The OrderablePartitioner that contains the original data source.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// KeysNormalized property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> OrderablePartitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner do not return the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IList with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() or GetDynamicOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, the current element's index (an Int64), and some local
        /// state that may be shared amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>
        (
            OrderablePartitioner<TSource> source,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException("SR.Parallel_ForEach_OrderedPartitionerKeysNotNormalized");
            }

            return PartitionerForEachWorker
            (
                source,
                _defaultParallelOptions,
                null,
                null,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">
        /// Partitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The Partitioner that contains the original data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> Partitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> Partitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner does not return
        /// the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner returns an IList
        /// with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() method in the <paramref name="source"/> Partitioner returns an
        /// IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the current element as a parameter.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            Partitioner<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return PartitionerForEachWorker<TSource, object>
            (
                source,
                parallelOptions,
                body,
                null,
                null,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">
        /// Partitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The Partitioner that contains the original data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> Partitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> Partitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner does not return
        /// the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner returns an IList
        /// with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() method in the <paramref name="source"/> Partitioner returns an
        /// IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// and a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            Partitioner<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return PartitionerForEachWorker<TSource, object>
            (
                source,
                parallelOptions,
                null,
                body,
                null,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.OrderablePartitioner{TSource}">
        /// OrderablePartitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The OrderablePartitioner that contains the original data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// KeysNormalized property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> OrderablePartitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner do not return the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IList with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() or GetDynamicOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and the current element's index (an Int64).
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource>
        (
            OrderablePartitioner<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource, ParallelLoopState, long> body
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException("SR.Parallel_ForEach_OrderedPartitionerKeysNotNormalized");
            }

            return PartitionerForEachWorker<TSource, object>
            (
                source,
                parallelOptions,
                null,
                null,
                body,
                null,
                null,
                null,
                null
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">
        /// Partitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">The Partitioner that contains the original data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> Partitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> Partitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner does not return
        /// the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() method in the <paramref name="source"/> Partitioner returns an IList
        /// with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() method in the <paramref name="source"/> Partitioner returns an
        /// IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, and some local state that may be shared amongst iterations
        /// that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>(
            Partitioner<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            return PartitionerForEachWorker
            (
                source,
                parallelOptions,
                null,
                null,
                null,
                body,
                null,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes a for each operation on a <see cref="T:System.Collections.Concurrent.OrderablePartitioner{TSource}">
        /// OrderablePartitioner</see> in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in <paramref name="source"/>.</typeparam>
        /// <typeparam name="TLocal">The type of the thread-local data.</typeparam>
        /// <param name="source">The OrderablePartitioner that contains the original data source.</param>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data
        /// for each thread.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each
        /// thread.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="source"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="body"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localInit"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="localFinally"/> argument is null.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when the
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the <paramref name="parallelOptions"/>
        /// argument is set</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// SupportsDynamicPartitions property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// KeysNormalized property in the <paramref name="source"/> OrderablePartitioner returns
        /// false.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when any
        /// methods in the <paramref name="source"/> OrderablePartitioner return null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner do not return the correct number of partitions.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetPartitions() or GetOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IList with at least one null value.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exception that is thrown when the
        /// GetDynamicPartitions() or GetDynamicOrderablePartitions() methods in the <paramref name="source"/>
        /// OrderablePartitioner return an IEnumerable whose GetEnumerator() method returns null.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown to contain an exception
        /// thrown from one of the specified delegates.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult">ParallelLoopResult</see> structure
        /// that contains information on what portion of the loop completed.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="T:System.Collections.Concurrent.Partitioner{TSource}">Partitioner</see> is used to retrieve
        /// the elements to be processed, in place of the original data source.  If the current element's
        /// index is desired, the source must be an <see cref="T:System.Collections.Concurrent.OrderablePartitioner">
        /// OrderablePartitioner</see>.
        /// </para>
        /// <para>
        /// The <paramref name="body"/> delegate is invoked once for each element in the <paramref name="source"/>
        /// Partitioner.  It is provided with the following parameters: the current element,
        /// a <see cref="System.Threading.Tasks.ParallelLoopState">ParallelLoopState</see> instance that may be
        /// used to break out of the loop prematurely, the current element's index (an Int64), and some local
        /// state that may be shared amongst iterations that execute on the same thread.
        /// </para>
        /// <para>
        /// The <paramref name="localInit"/> delegate is invoked once for each thread that participates in the loop's
        /// execution and returns the initial local state for each of those threads.  These initial states are passed to the first
        /// <paramref name="body"/> invocations on each thread.  Then, every subsequent body invocation returns a possibly
        /// modified state value that is passed to the next body invocation.  Finally, the last body invocation on each thread returns a state value
        /// that is passed to the <paramref name="localFinally"/> delegate.  The localFinally delegate is invoked once per thread to perform a final
        /// action on each thread's local state.
        /// </para>
        /// </remarks>
        public static ParallelLoopResult ForEach<TSource, TLocal>(
            OrderablePartitioner<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> localFinally
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (localInit == null)
            {
                throw new ArgumentNullException(nameof(localInit));
            }

            if (localFinally == null)
            {
                throw new ArgumentNullException(nameof(localFinally));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException("SR.Parallel_ForEach_OrderedPartitionerKeysNotNormalized");
            }

            return PartitionerForEachWorker
            (
                source,
                parallelOptions,
                null,
                null,
                null,
                null,
                body,
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Executes each of the provided actions, possibly in parallel.
        /// </summary>
        /// <param name="actions">An array of <see cref="T:System.Action">Actions</see> to execute.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="actions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentException">The exception that is thrown when the
        /// <paramref name="actions"/> array contains a null element.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown when any
        /// action in the <paramref name="actions"/> array throws an exception.</exception>
        /// <remarks>
        /// This method can be used to execute a set of operations, potentially in parallel.
        /// No guarantees are made about the order in which the operations execute or whether
        /// they execute in parallel.  This method does not return until each of the
        /// provided operations has completed, regardless of whether completion
        /// occurs due to normal or exceptional termination.
        /// </remarks>
        public static void Invoke(params Action[] actions)
        {
            Invoke(_defaultParallelOptions, actions);
        }

        /// <summary>
        /// Executes each of the provided actions, possibly in parallel.
        /// </summary>
        /// <param name="parallelOptions">A <see cref="T:System.Threading.Tasks.ParallelOptions">ParallelOptions</see>
        /// instance that configures the behavior of this operation.</param>
        /// <param name="actions">An array of <see cref="T:System.Action">Actions</see> to execute.</param>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="actions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the
        /// <paramref name="parallelOptions"/> argument is null.</exception>
        /// <exception cref="T:System.ArgumentException">The exception that is thrown when the
        /// <paramref name="actions"/> array contains a null element.</exception>
        /// <exception cref="T:System.OperationCanceledExceptionEx">The exception that is thrown when
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> is set.</exception>
        /// <exception cref="T:System.AggregateException">The exception that is thrown when any
        /// action in the <paramref name="actions"/> array throws an exception.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when the
        /// the <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> associated with the
        /// the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> in the
        /// <paramref name="parallelOptions"/> has been disposed.</exception>
        /// <remarks>
        /// This method can be used to execute a set of operations, potentially in parallel.
        /// No guarantees are made about the order in which the operations execute or whether
        /// the they execute in parallel.  This method does not return until each of the
        /// provided operations has completed, regardless of whether completion
        /// occurs due to normal or exceptional termination.
        /// </remarks>
        public static void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            if (parallelOptions == null)
            {
                throw new ArgumentNullException(nameof(parallelOptions));
            }

            // On desktop, we throw an ODE if we're passed a disposed CancellationToken.
            // Here, CancellationToken.ThrowIfSourceDisposed() is not exposed.
            // This is benign, because we'll end up throwing ODE when we register
            // with the token later.

            // Quit early if we're already canceled -- avoid a bunch of work.
            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

            // We must validate that the actions array contains no null elements, and also
            // make a defensive copy of the actions array.
            var actionsCopy = new Action[actions.Length];
            for (var i = 0; i < actionsCopy.Length; i++)
            {
                actionsCopy[i] = actions[i];
                if (actionsCopy[i] == null)
                {
                    throw new ArgumentException("One of the actions was null.");
                }
            }

            InvokeExtracted(parallelOptions, actionsCopy);
        }

        private static bool CheckTimeoutReached(int timeoutOccursAt)
        {
            // Note that both, Environment.TickCount and timeoutOccursAt are ints and can overflow and become negative.
            var currentMillis = Environment.TickCount;

            if (currentMillis < timeoutOccursAt)
            {
                return false;
            }

            return 0 <= timeoutOccursAt || 0 >= currentMillis;
        }

        private static int ComputeTimeoutPoint(int timeoutLength)
        {
            // Environment.TickCount is an int that cycles. We intentionally let the point in time at which the
            // timeout occurs overflow. It will still stay ahead of Environment.TickCount for the comparisons made
            // in CheckTimeoutReached(..):
            unchecked
            {
                return Environment.TickCount + timeoutLength;
            }
        }

        /// <summary>
        /// Performs the major work of the parallel foreach loop. It assumes that argument validation has
        /// already been performed by the caller. This function's whole purpose in life is to enable as much
        /// reuse of common implementation details for the various For overloads we offer. Without it, we'd
        /// end up with lots of duplicate code. It handles: (1) simple foreach loops, (2) foreach loops that
        /// depend on ParallelState, and (3) foreach loops that access indices, (4) foreach loops with thread
        /// local data, and any necessary permutations thereof.
        ///
        /// </summary>
        /// <typeparam name="TSource">The type of the source data.</typeparam>
        /// <typeparam name="TLocal">The type of the local data.</typeparam>
        /// <param name="source">An enumerable data source.</param>
        /// <param name="parallelOptions">ParallelOptions instance to use with this ForEach-loop</param>
        /// <param name="body">The simple loop body.</param>
        /// <param name="bodyWithState">The loop body for ParallelState overloads.</param>
        /// <param name="bodyWithStateAndIndex">The loop body for ParallelState/indexed overloads.</param>
        /// <param name="bodyWithStateAndLocal">The loop body for ParallelState/thread local state overloads.</param>
        /// <param name="bodyWithEverything">The loop body for ParallelState/indexed/thread local state overloads.</param>
        /// <param name="localInit">A selector function that returns new thread local state.</param>
        /// <param name="localFinally">A cleanup function to destroy thread local state.</param>
        /// <remarks>Only one of the bodyXX arguments may be supplied (i.e. they are exclusive).</remarks>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult"/> structure.</returns>
        private static ParallelLoopResult ForEachWorker<TSource, TLocal>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Action<TSource> body,
            Action<TSource, ParallelLoopState> bodyWithState,
            Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex,
            Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything,
            Func<TLocal> localInit,
            Action<TLocal> localFinally
        )
        {
            Debug.Assert
            (
                (body == null ? 0 : 1)
                + (bodyWithState == null ? 0 : 1)
                + (bodyWithStateAndIndex == null ? 0 : 1)
                + (bodyWithStateAndLocal == null ? 0 : 1)
                + (bodyWithEverything == null ? 0 : 1) == 1,
                "expected exactly one body function to be supplied"
            );
            Debug.Assert
            (
                bodyWithStateAndLocal != null
                || bodyWithEverything != null
                || (localInit == null && localFinally == null),
                "thread local functions should only be supplied for loops w/ thread local bodies"
            );

            // Before getting started, do a quick peek to see if we have been canceled already
            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

            switch (source)
            {
                case TSource[] sourceAsArray:
                    // If it's an array, we can use a fast-path that uses ldelems in the IL.
                    return ForEachWorker
                    (
                        sourceAsArray,
                        parallelOptions,
                        body,
                        bodyWithState,
                        bodyWithStateAndIndex,
                        bodyWithStateAndLocal,
                        bodyWithEverything,
                        localInit,
                        localFinally
                    );
                case IList<TSource> sourceAsList:
                    // If we can index into the list, we can use a faster code-path that doesn't result in
                    // contention for the single, shared enumerator object.
                    return ForEachWorker
                    (
                        sourceAsList,
                        parallelOptions,
                        body,
                        bodyWithState,
                        bodyWithStateAndIndex,
                        bodyWithStateAndLocal,
                        bodyWithEverything,
                        localInit,
                        localFinally
                    );
                default:
                    // This is an honest-to-goodness IEnumerable.  Wrap it in a Partitioner and defer to our
                    // ForEach(Partitioner) logic.
                    return PartitionerForEachWorker
                    (
                        Partitioner.Create(source),
                        parallelOptions,
                        body,
                        bodyWithState,
                        bodyWithStateAndIndex,
                        bodyWithStateAndLocal,
                        bodyWithEverything,
                        localInit,
                        localFinally
                    );
            }
        }

        /// <summary>
        /// A fast path for the more general ForEachWorker method above. This uses ldelem instructions to
        /// access the individual elements of the array, which will be faster.
        /// </summary>
        /// <typeparam name="TSource">The type of the source data.</typeparam>
        /// <typeparam name="TLocal">The type of the local data.</typeparam>
        /// <param name="array">An array data source.</param>
        /// <param name="parallelOptions">The options to use for execution.</param>
        /// <param name="body">The simple loop body.</param>
        /// <param name="bodyWithState">The loop body for ParallelState overloads.</param>
        /// <param name="bodyWithStateAndIndex">The loop body for indexed/ParallelLoopState overloads.</param>
        /// <param name="bodyWithStateAndLocal">The loop body for local/ParallelLoopState overloads.</param>
        /// <param name="bodyWithEverything">The loop body for the most generic overload.</param>
        /// <param name="localInit">A selector function that returns new thread local state.</param>
        /// <param name="localFinally">A cleanup function to destroy thread local state.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult"/> structure.</returns>
        private static ParallelLoopResult ForEachWorker<TSource, TLocal>(
            TSource[] array,
            ParallelOptions parallelOptions,
            Action<TSource> body,
            Action<TSource, ParallelLoopState> bodyWithState,
            Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex,
            Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything,
            Func<TLocal> localInit,
            Action<TLocal> localFinally
        )
        {
            Debug.Assert(array != null);
            Debug.Assert(parallelOptions != null, "ForEachWorker(array): parallelOptions is null");

            var from = array.GetLowerBound(0);
            var to = array.GetUpperBound(0) + 1;

            if (body != null)
            {
                return ForWorker<object>
                (
                    from,
                    to,
                    parallelOptions,
                    i => body(array[i]),
                    null,
                    null,
                    null,
                    null
                );
            }

            if (bodyWithState != null)
            {
                return ForWorker<object>
                (
                    from,
                    to,
                    parallelOptions,
                    null,
                    (i, state) => bodyWithState(array[i], state),
                    null,
                    null,
                    null
                );
            }

            if (bodyWithStateAndIndex != null)
            {
                return ForWorker<object>
                (
                    from,
                    to,
                    parallelOptions,
                    null,
                    (i, state) => bodyWithStateAndIndex(array[i], state, i),
                    null,
                    null,
                    null
                );
            }

            if (bodyWithStateAndLocal != null)
            {
                return ForWorker
                (
                    from,
                    to,
                    parallelOptions,
                    null,
                    null,
                    (i, state, local) => bodyWithStateAndLocal(array[i], state, local),
                    localInit,
                    localFinally
                );
            }

            return ForWorker
            (
                from,
                to,
                parallelOptions,
                null,
                null,
                (i, state, local) => bodyWithEverything(array[i], state, i, local),
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// A fast path for the more general ForEachWorker method above. This uses IList&lt;T&gt;'s indexer
        /// capabilities to access the individual elements of the list rather than an enumerator.
        /// </summary>
        /// <typeparam name="TSource">The type of the source data.</typeparam>
        /// <typeparam name="TLocal">The type of the local data.</typeparam>
        /// <param name="list">A list data source.</param>
        /// <param name="parallelOptions">The options to use for execution.</param>
        /// <param name="body">The simple loop body.</param>
        /// <param name="bodyWithState">The loop body for ParallelState overloads.</param>
        /// <param name="bodyWithStateAndIndex">The loop body for indexed/ParallelLoopState overloads.</param>
        /// <param name="bodyWithStateAndLocal">The loop body for local/ParallelLoopState overloads.</param>
        /// <param name="bodyWithEverything">The loop body for the most generic overload.</param>
        /// <param name="localInit">A selector function that returns new thread local state.</param>
        /// <param name="localFinally">A cleanup function to destroy thread local state.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult"/> structure.</returns>
        private static ParallelLoopResult ForEachWorker<TSource, TLocal>(
            IList<TSource> list,
            ParallelOptions parallelOptions,
            Action<TSource> body,
            Action<TSource, ParallelLoopState> bodyWithState,
            Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex,
            Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything,
            Func<TLocal> localInit,
            Action<TLocal> localFinally
        )
        {
            Debug.Assert(list != null);
            Debug.Assert(parallelOptions != null, "ForEachWorker(list): parallelOptions is null");

            if (body != null)
            {
                return ForWorker<object>
                (
                    0,
                    list.Count,
                    parallelOptions,
                    i => body(list[i]),
                    null,
                    null,
                    null,
                    null
                );
            }

            if (bodyWithState != null)
            {
                return ForWorker<object>
                (
                    0,
                    list.Count,
                    parallelOptions,
                    null,
                    (i, state) => bodyWithState(list[i], state),
                    null,
                    null,
                    null
                );
            }

            if (bodyWithStateAndIndex != null)
            {
                return ForWorker<object>
                (
                    0,
                    list.Count,
                    parallelOptions,
                    null,
                    (i, state) => bodyWithStateAndIndex(list[i], state, i),
                    null,
                    null,
                    null
                );
            }

            if (bodyWithStateAndLocal != null)
            {
                return ForWorker
                (
                    0,
                    list.Count,
                    parallelOptions,
                    null,
                    null,
                    (i, state, local) => bodyWithStateAndLocal(list[i], state, local),
                    localInit,
                    localFinally
                );
            }

            return ForWorker
            (
                0,
                list.Count,
                parallelOptions,
                null,
                null,
                (i, state, local) => bodyWithEverything(list[i], state, i, local),
                localInit,
                localFinally
            );
        }

        /// <summary>
        /// Performs the major work of the parallel for loop. It assumes that argument validation has already
        /// been performed by the caller. This function's whole purpose in life is to enable as much reuse of
        /// common implementation details for the various For overloads we offer. Without it, we'd end up
        /// with lots of duplicate code. It handles: (1) simple for loops, (2) for loops that depend on
        /// ParallelState, and (3) for loops with thread local data.
        ///
        /// </summary>
        /// <typeparam name="TLocal">The type of the local data.</typeparam>
        /// <param name="fromInclusive">The loop's start index, inclusive.</param>
        /// <param name="toExclusive">The loop's end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance.</param>
        /// <param name="body">The simple loop body.</param>
        /// <param name="bodyWithState">The loop body for ParallelState overloads.</param>
        /// <param name="bodyWithLocal">The loop body for thread local state overloads.</param>
        /// <param name="localInit">A selector function that returns new thread local state.</param>
        /// <param name="localFinally">A cleanup function to destroy thread local state.</param>
        /// <remarks>Only one of the body arguments may be supplied (i.e. they are exclusive).</remarks>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult"/> structure.</returns>
        private static ParallelLoopResult ForWorker<TLocal>
        (
            int fromInclusive,
            int toExclusive,
            ParallelOptions parallelOptions,
            Action<int> body,
            Action<int, ParallelLoopState> bodyWithState,
            Func<int, ParallelLoopState, TLocal, TLocal> bodyWithLocal,
            Func<TLocal> localInit,
            Action<TLocal> localFinally
        )
        {
            Debug.Assert
            (
                (body == null ? 0 : 1) + (bodyWithState == null ? 0 : 1) + (bodyWithLocal == null ? 0 : 1) == 1,
                "expected exactly one body function to be supplied"
            );
            Debug.Assert
            (
                bodyWithLocal != null || (localInit == null && localFinally == null),
                "thread local functions should only be supplied for loops w/ thread local bodies"
            );

            // Instantiate our result.  Specifics will be filled in later.
            var result = new ParallelLoopResult();

            // We just return immediately if 'to' is smaller (or equal to) 'from'.
            if (toExclusive <= fromInclusive)
            {
                result._completed = true;
                return result;
            }

            // For all loops we need a shared flag even though we don't have a body with state,
            // because the shared flag contains the exceptional bool, which triggers other workers
            // to exit their loops if one worker catches an exception
            var sharedPStateFlags = new ParallelLoopStateFlags32();

            // Before getting started, do a quick peek to see if we have been canceled already
            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

            // initialize ranges with passed in loop arguments and expected number of workers
            var numExpectedWorkers = parallelOptions.EffectiveMaxConcurrencyLevel == -1
                ? Environment.ProcessorCount
                : parallelOptions.EffectiveMaxConcurrencyLevel;
            var rangeManager = new RangeManager(fromInclusive, toExclusive, 1, numExpectedWorkers);

            // Keep track of any cancellations
            OperationCanceledExceptionEx oce = null;

            // if cancellation is enabled, we need to register a callback to stop the loop when it gets signaled
            var ctr = !parallelOptions.CancellationToken.CanBeCanceled
                ? default
                : parallelOptions.CancellationToken.Register
                (
                    _ =>
                    {
                        // Record our cancellation before stopping processing
                        oce = new OperationCanceledExceptionEx("parallelOptions.CancellationToken");
                        // Cause processing to stop
                        sharedPStateFlags.Cancel();
                    }, null, false
                );

            try
            {
                try
                {
                    TaskReplicator.Run
                    (
                        (
                            ref RangeWorker currentWorker,
                            int timeout,
                            out bool replicationDelegateYieldedBeforeCompletion) =>
                        {
                            // First thing we do upon entering the task is to register as a new "RangeWorker" with the
                            // shared RangeManager instance.

                            if (!currentWorker.IsInitialized)
                            {
                                currentWorker = rangeManager.RegisterNewWorker();
                            }

                            // We will need to reset this to true if we exit due to a timeout:
                            replicationDelegateYieldedBeforeCompletion = false;

                            // We need to call FindNewWork32() on it to see whether there's a chunk available.
                            // These are the local index values to be used in the sequential loop.
                            // Their values filled in by FindNewWork32

                            if
                            (
                                !currentWorker.FindNewWork32(out var fromInclusiveLocal, out var toExclusiveLocal)
                                || sharedPStateFlags.ShouldExitLoop(fromInclusiveLocal)
                            )
                            {
                                return; // no need to run
                            }

                            var localValue = default(TLocal);
                            var bLocalValueInitialized =
                                false; // Tracks whether localInit ran without exceptions, so that we can skip localFinally if it wasn't

                            try
                            {
                                // Create a new state object that references the shared "stopped" and "exceptional" flags
                                // If needed, it will contain a new instance of thread-local state by invoking the selector.
                                ParallelLoopState32 state = null;

                                if (bodyWithState != null)
                                {
                                    Debug.Assert(sharedPStateFlags != null);
                                    state = new ParallelLoopState32(sharedPStateFlags);
                                }
                                else if (bodyWithLocal != null)
                                {
                                    Debug.Assert(sharedPStateFlags != null);
                                    state = new ParallelLoopState32(sharedPStateFlags);
                                    if (localInit != null)
                                    {
                                        localValue = localInit();
                                        bLocalValueInitialized = true;
                                    }
                                }

                                // initialize a loop timer which will help us decide whether we should exit early
                                var loopTimeout = ComputeTimeoutPoint(timeout);

                                // Now perform the loop itself.
                                do
                                {
                                    if (body != null)
                                    {
                                        for
                                        (
                                            var j = fromInclusiveLocal;
                                            j < toExclusiveLocal
                                            &&
                                            (
                                                sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone // fast path check as SEL() doesn't inline
                                                || !sharedPStateFlags.ShouldExitLoop()
                                            ); // the no-arg version is used since we have no state
                                            j++
                                        )
                                        {
                                            body(j);
                                        }
                                    }
                                    else if (bodyWithState != null)
                                    {
                                        for
                                        (
                                            var j = fromInclusiveLocal;
                                            j < toExclusiveLocal
                                            &&
                                            (
                                                sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone // fast path check as SEL() doesn't inline
                                                || !sharedPStateFlags.ShouldExitLoop(j)
                                            );
                                            j++
                                        )
                                        {
                                            state.CurrentIteration = j;
                                            bodyWithState(j, state);
                                        }
                                    }
                                    else
                                    {
                                        for
                                        (
                                            var j = fromInclusiveLocal;
                                            j < toExclusiveLocal
                                            &&
                                            (
                                                sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone // fast path check as SEL() doesn't inline
                                                || !sharedPStateFlags.ShouldExitLoop(j)
                                            );
                                            j++
                                        )
                                        {
                                            state.CurrentIteration = j;
                                            localValue = bodyWithLocal(j, state, localValue);
                                        }
                                    }

                                    // Cooperative multitasking:
                                    // Check if allowed loop time is exceeded, if so save current state and return.
                                    // The task replicator will queue up a replacement task. Note that we don't do this on the root task.
                                    if (!CheckTimeoutReached(loopTimeout))
                                    {
                                        continue;
                                    }

                                    replicationDelegateYieldedBeforeCompletion = true;
                                    break;

                                    // Exit DO-loop if we can't find new work, or if the loop was stopped:
                                } while
                                (
                                    currentWorker.FindNewWork32(out fromInclusiveLocal, out toExclusiveLocal)
                                    &&
                                    (
                                        sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone
                                        || !sharedPStateFlags.ShouldExitLoop(fromInclusiveLocal)
                                    )
                                );
                            }
                            catch (Exception ex)
                            {
                                // if we catch an exception in a worker, we signal the other workers to exit the loop, and we rethrow
                                sharedPStateFlags.SetExceptional();
                                ExceptionDispatchInfo.Capture(ex).Throw();
                            }
                            finally
                            {
                                // If a cleanup function was specified, call it. Otherwise, if the type is
                                // IDisposable, we will invoke Dispose on behalf of the user.
                                if (localFinally != null && bLocalValueInitialized)
                                {
                                    localFinally(localValue);
                                }
                            }
                        },
                        parallelOptions,
                        true
                    );
                }
                finally
                {
                    // Dispose the cancellation token registration before checking for a cancellation exception
                    if (parallelOptions.CancellationToken.CanBeCanceled)
                    {
                        ctr.Dispose();
                    }
                }

                // If we got through that with no exceptions, and we were canceled, then
                // throw our cancellation exception
                if (oce != null)
                {
                    throw oce;
                }
            }
            catch (AggregateException aggExp)
            {
                // If we have many cancellation exceptions all caused by the specified user cancel control, then throw only one OCE:
                ThrowSingleCancellationExceptionOrOtherException
                (
                    aggExp.InnerExceptions,
                    aggExp, parallelOptions.CancellationToken
                );
            }
            finally
            {
                var status = sharedPStateFlags.LoopStateFlags;
                result._completed = status == ParallelLoopStateFlags.ParallelLoopStateNone;
                if ((status & ParallelLoopStateFlags.ParallelLoopStateBroken) != 0)
                {
                    result._lowestBreakIteration = sharedPStateFlags.LowestBreakIteration;
                }
            }

            return result;
        }

        /// <summary>
        /// Performs the major work of the 64-bit parallel for loop. It assumes that argument validation has already
        /// been performed by the caller. This function's whole purpose in life is to enable as much reuse of
        /// common implementation details for the various For overloads we offer. Without it, we'd end up
        /// with lots of duplicate code. It handles: (1) simple for loops, (2) for loops that depend on
        /// ParallelState, and (3) for loops with thread local data.
        ///
        /// </summary>
        /// <typeparam name="TLocal">The type of the local data.</typeparam>
        /// <param name="fromInclusive">The loop's start index, inclusive.</param>
        /// <param name="toExclusive">The loop's end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance.</param>
        /// <param name="body">The simple loop body.</param>
        /// <param name="bodyWithState">The loop body for ParallelState overloads.</param>
        /// <param name="bodyWithLocal">The loop body for thread local state overloads.</param>
        /// <param name="localInit">A selector function that returns new thread local state.</param>
        /// <param name="localFinally">A cleanup function to destroy thread local state.</param>
        /// <remarks>Only one of the body arguments may be supplied (i.e. they are exclusive).</remarks>
        /// <returns>A <see cref="T:System.Threading.Tasks.ParallelLoopResult"/> structure.</returns>
        private static ParallelLoopResult ForWorker64<TLocal>
        (
            long fromInclusive,
            long toExclusive,
            ParallelOptions parallelOptions,
            Action<long> body,
            Action<long, ParallelLoopState> bodyWithState,
            Func<long, ParallelLoopState, TLocal, TLocal> bodyWithLocal,
            Func<TLocal> localInit,
            Action<TLocal> localFinally
        )
        {
            Debug.Assert
            (
                (body == null ? 0 : 1)
                + (bodyWithState == null ? 0 : 1)
                + (bodyWithLocal == null ? 0 : 1) == 1,
                "expected exactly one body function to be supplied"
            );
            Debug.Assert
            (
                bodyWithLocal != null
                || (localInit == null && localFinally == null),
                "thread local functions should only be supplied for loops w/ thread local bodies"
            );

            // Instantiate our result.  Specifics will be filled in later.
            var result = new ParallelLoopResult();

            // We just return immediately if 'to' is smaller (or equal to) 'from'.
            if (toExclusive <= fromInclusive)
            {
                result._completed = true;
                return result;
            }

            // For all loops we need a shared flag even though we don't have a body with state,
            // because the shared flag contains the exceptional bool, which triggers other workers
            // to exit their loops if one worker catches an exception
            var sharedPStateFlags = new ParallelLoopStateFlags64();

            // Before getting started, do a quick peek to see if we have been canceled already
            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

            // initialize ranges with passed in loop arguments and expected number of workers
            var numExpectedWorkers = parallelOptions.EffectiveMaxConcurrencyLevel == -1
                ? Environment.ProcessorCount
                : parallelOptions.EffectiveMaxConcurrencyLevel;
            var rangeManager = new RangeManager(fromInclusive, toExclusive, 1, numExpectedWorkers);

            // Keep track of any cancellations
            OperationCanceledExceptionEx oce = null;

            // if cancellation is enabled, we need to register a callback to stop the loop when it gets signaled
            var ctr = !parallelOptions.CancellationToken.CanBeCanceled
                ? default
                : parallelOptions.CancellationToken.Register
                (
                    _ =>
                    {
                        // Record our cancellation before stopping processing
                        oce = new OperationCanceledExceptionEx("parallelOptions.CancellationToken");
                        // Cause processing to stop
                        sharedPStateFlags.Cancel();
                    },
                    null,
                    false
                );

            try
            {
                try
                {
                    TaskReplicator.Run
                    (
                        (
                            ref RangeWorker currentWorker,
                            int timeout,
                            out bool replicationDelegateYieldedBeforeCompletion) =>
                        {
                            // First thing we do upon entering the task is to register as a new "RangeWorker" with the
                            // shared RangeManager instance.

                            if (!currentWorker.IsInitialized)
                            {
                                currentWorker = rangeManager.RegisterNewWorker();
                            }

                            // We will need to reset this to true if we exit due to a timeout:
                            replicationDelegateYieldedBeforeCompletion = false;

                            // These are the local index values to be used in the sequential loop.
                            // Their values filled in by FindNewWork

                            if
                            (
                                !currentWorker.FindNewWork(out var fromInclusiveLocal, out var toExclusiveLocal)
                                || sharedPStateFlags.ShouldExitLoop(fromInclusiveLocal)
                            )
                            {
                                return; // no need to run
                            }

                            var localValue = default(TLocal);
                            var bLocalValueInitialized = false; // Tracks whether localInit ran without exceptions, so that we can skip localFinally if it wasn't

                            try
                            {
                                // Create a new state object that references the shared "stopped" and "exceptional" flags
                                // If needed, it will contain a new instance of thread-local state by invoking the selector.
                                ParallelLoopState64 state = null;

                                if (bodyWithState != null)
                                {
                                    Debug.Assert(sharedPStateFlags != null);
                                    state = new ParallelLoopState64(sharedPStateFlags);
                                }
                                else if (bodyWithLocal != null)
                                {
                                    Debug.Assert(sharedPStateFlags != null);
                                    state = new ParallelLoopState64(sharedPStateFlags);

                                    // If a thread-local selector was supplied, invoke it. Otherwise, use the default.
                                    if (localInit != null)
                                    {
                                        localValue = localInit();
                                        bLocalValueInitialized = true;
                                    }
                                }

                                // initialize a loop timer which will help us decide whether we should exit early
                                var loopTimeout = ComputeTimeoutPoint(timeout);

                                // Now perform the loop itself.
                                do
                                {
                                    if (body != null)
                                    {
                                        for
                                        (
                                            var j = fromInclusiveLocal;
                                            j < toExclusiveLocal
                                            &&
                                            (
                                                sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone // fast path check as SEL() doesn't inline
                                                || !sharedPStateFlags.ShouldExitLoop()
                                            ); // the no-arg version is used since we have no state
                                            j++
                                        )
                                        {
                                            body(j);
                                        }
                                    }
                                    else if (bodyWithState != null)
                                    {
                                        for
                                        (
                                            var j = fromInclusiveLocal;
                                            j < toExclusiveLocal
                                            &&
                                            (
                                                sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone // fast path check as SEL() doesn't inline
                                                || !sharedPStateFlags.ShouldExitLoop(j)
                                            );
                                            j++
                                        )
                                        {
                                            state.CurrentIteration = j;
                                            bodyWithState(j, state);
                                        }
                                    }
                                    else
                                    {
                                        for
                                        (
                                            var j = fromInclusiveLocal;
                                            j < toExclusiveLocal
                                            &&
                                            (
                                                sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone // fast path check as SEL() doesn't inline
                                                || !sharedPStateFlags.ShouldExitLoop(j)
                                            );
                                            j++
                                        )
                                        {
                                            state.CurrentIteration = j;
                                            localValue = bodyWithLocal(j, state, localValue);
                                        }
                                    }

                                    // Cooperative multitasking:
                                    // Check if allowed loop time is exceeded, if so save current state and return.
                                    // The task replicator will queue up a replacement task. Note that we don't do this on the root task.
                                    if (!CheckTimeoutReached(loopTimeout))
                                    {
                                        continue;
                                    }

                                    replicationDelegateYieldedBeforeCompletion = true;
                                    break;

                                    // Exit DO-loop if we can't find new work, or if the loop was stopped:
                                } while
                                (
                                    currentWorker.FindNewWork(out fromInclusiveLocal, out toExclusiveLocal)
                                    &&
                                    (
                                        sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.ParallelLoopStateNone
                                        || !sharedPStateFlags.ShouldExitLoop(fromInclusiveLocal)
                                    )
                                );
                            }
                            catch (Exception ex)
                            {
                                // if we catch an exception in a worker, we signal the other workers to exit the loop, and we rethrow
                                sharedPStateFlags.SetExceptional();
                                ExceptionDispatchInfo.Capture(ex).Throw();
                            }
                            finally
                            {
                                // If a cleanup function was specified, call it. Otherwise, if the type is
                                // IDisposable, we will invoke Dispose on behalf of the user.
                                if (localFinally != null && bLocalValueInitialized)
                                {
                                    localFinally(localValue);
                                }
                            }
                        },
                        parallelOptions,
                        true
                    );
                }
                finally
                {
                    // Dispose the cancellation token registration before checking for a cancellation exception
                    if (parallelOptions.CancellationToken.CanBeCanceled)
                    {
                        ctr.Dispose();
                    }
                }

                // If we got through that with no exceptions, and we were canceled, then
                // throw our cancellation exception
                if (oce != null)
                {
                    throw oce;
                }
            }
            catch (AggregateException aggExp)
            {
                // If we have many cancellation exceptions all caused by the specified user cancel control, then throw only one OCE:
                ThrowSingleCancellationExceptionOrOtherException
                (
                    aggExp.InnerExceptions,
                    aggExp, parallelOptions.CancellationToken
                );
            }
            finally
            {
                var status = sharedPStateFlags.LoopStateFlags;
                result._completed = status == ParallelLoopStateFlags.ParallelLoopStateNone;
                if ((status & ParallelLoopStateFlags.ParallelLoopStateBroken) != 0)
                {
                    result._lowestBreakIteration = sharedPStateFlags.LowestBreakIteration;
                }
            }

            return result;
        }

        private static void InvokeExtracted(ParallelOptions parallelOptions, Action[] actionsCopy)
        {
            // If we have no work to do, we are done.
            if (actionsCopy.Length < 1)
            {
                return;
            }

            // In the algorithm below, if the number of actions is greater than this, we automatically
            // use Parallel.For() to handle the actions, rather than the Task-per-Action strategy.
            const int Small_Action_Count_Limit = 10;

            // If we've gotten this far, it's time to process the actions.

            // This is more efficient for a large number of actions, or for enforcing MaxDegreeOfParallelism:
            if
            (
                actionsCopy.Length > Small_Action_Count_Limit
                || (parallelOptions.MaxDegreeOfParallelism != -1 && parallelOptions.MaxDegreeOfParallelism < actionsCopy.Length)
            )
            {
                // Used to hold any exceptions encountered during action processing
                var exceptionQ = new ThreadSafeQueue<Exception>[1]; // will be lazily initialized if necessary

                // Launch a task replicator to handle the execution of all actions.
                // This allows us to use as many cores as are available, and no more.
                // The exception to this rule is that, in the case of a blocked action,
                // the ThreadPool may inject extra threads, which means extra tasks can run.
                var actionIndex = 0;

                try
                {
                    TaskReplicator.Run
                    (
                        (ref object state, int timeout, out bool replicationDelegateYieldedBeforeCompletion) =>
                        {
                            Theraot.No.Op(state);
                            Theraot.No.Op(timeout);

                            // In this particular case, we do not participate in cooperative multitasking:
                            replicationDelegateYieldedBeforeCompletion = false;

                            // Each for-task will pull an action at a time from the list
                            var myIndex = Interlocked.Increment(ref actionIndex); // = index to use + 1
                            while (myIndex <= actionsCopy.Length)
                            {
                                // Catch and store any exceptions.  If we don't catch them, the self-replicating
                                // task will exit, and that may cause other SR-tasks to exit.
                                // And (absent cancellation) we want all actions to execute.
                                try
                                {
                                    actionsCopy[myIndex - 1]();
                                }
                                catch (Exception e)
                                {
                                    LazyInitializer.EnsureInitialized(ref exceptionQ[0], () => new ThreadSafeQueue<Exception>());
                                    exceptionQ[0].Add(e);
                                }

                                // Check for cancellation.  If it is encountered, then exit the delegate.
                                parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                                // You're still in the game.  Grab your next action index.
                                myIndex = Interlocked.Increment(ref actionIndex);
                            }
                        },
                        parallelOptions,
                        false
                    );
                }
                catch (Exception e)
                {
                    LazyInitializer.EnsureInitialized(ref exceptionQ[0], () => new ThreadSafeQueue<Exception>());

                    // Since we're consuming all action exceptions, there are very few reasons that
                    // we would see an exception here.  Two that come to mind:
                    //   (1) An OCE thrown by one or more actions (AggregateException thrown)
                    //   (2) An exception thrown from the TaskReplicator constructor
                    //       (regular exception thrown).
                    // We'll need to cover them both.

                    switch (e)
                    {
                        case ObjectDisposedException _:
                            throw;

                        case AggregateException ae:
                            // Strip off outer container of an AggregateException, because downstream
                            // logic needs OCEs to be at the top level.
                            foreach (var exc in ae.InnerExceptions)
                            {
                                exceptionQ[0].Add(exc);
                            }

                            break;

                        default:
                            exceptionQ[0].Add(e);
                            break;
                    }
                }

                // If we have encountered any exceptions, then throw.
                if (exceptionQ[0]?.Count > 0)
                {
                    ThrowSingleCancellationExceptionOrOtherException(exceptionQ, new AggregateException(exceptionQ[0]), parallelOptions.CancellationToken);
                }
            }
            else // This is more efficient for a small number of actions and no DOP support:
            {
                // Initialize our array of tasks, one per action.
                var tasks = new Task[actionsCopy.Length];

                // One more check before we begin...
                parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                // Invoke all actions as tasks.  Queue N-1 of them, and run 1 synchronously.
                for (var i = 1; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew
                    (
                        actionsCopy[i], parallelOptions.CancellationToken,
                        TaskCreationOptions.None,
                        parallelOptions.EffectiveTaskScheduler
                    );
                }

                tasks[0] = new Task(actionsCopy[0], parallelOptions.CancellationToken, TaskCreationOptions.None);
                tasks[0].RunSynchronously(parallelOptions.EffectiveTaskScheduler);

                // Now wait for the tasks to complete.  This will not unblock until all of
                // them complete, and it will throw an exception if one or more of them also
                // threw an exception.  We let such exceptions go completely unhandled.
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (AggregateException aggExp)
                {
                    // see if we can combine it into a single OCE. If not propagate the original exception
                    ThrowSingleCancellationExceptionOrOtherException
                    (
                        aggExp.InnerExceptions,
                        aggExp, parallelOptions.CancellationToken
                    );
                }
            }
        }

        // Main worker method for Parallel.ForEach() calls w/ Partitioners.
        private static ParallelLoopResult PartitionerForEachWorker<TSource, TLocal>(
            Partitioner<TSource> source, // Might be OrderablePartitioner
            ParallelOptions parallelOptions,
            Action<TSource> simpleBody,
            Action<TSource, ParallelLoopState> bodyWithState,
            Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex,
            Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything,
            Func<TLocal> localInit,
            Action<TLocal> localFinally)
        {
            Debug.Assert
            (
                (simpleBody == null ? 0 : 1)
                + (bodyWithState == null ? 0 : 1)
                + (bodyWithStateAndIndex == null ? 0 : 1)
                + (bodyWithStateAndLocal == null ? 0 : 1)
                + (bodyWithEverything == null ? 0 : 1) == 1,
                "PartitionForEach: expected exactly one body function to be supplied"
            );
            Debug.Assert
            (
                bodyWithStateAndLocal != null
                || bodyWithEverything != null
                || (localInit == null && localFinally == null),
                "PartitionForEach: thread local functions should only be supplied for loops w/ thread local bodies"
            );

            var orderedSource = source as OrderablePartitioner<TSource>;
            Debug.Assert
            (
                orderedSource != null
                || (bodyWithStateAndIndex == null && bodyWithEverything == null),
                "PartitionForEach: bodies with indices are only allowable for OrderablePartitioner"
            );

            if (!source.SupportsDynamicPartitions)
            {
                throw new InvalidOperationException("SR.Parallel_ForEach_PartitionerNotDynamic");
            }

            // Before getting started, do a quick peek to see if we have been canceled already
            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

            // For all loops we need a shared flag even though we don't have a body with state,
            // because the shared flag contains the exceptional bool, which triggers other workers
            // to exit their loops if one worker catches an exception
            var sharedPStateFlags = new ParallelLoopStateFlags64();

            // Instantiate our result.  Specifics will be filled in later.
            var result = new ParallelLoopResult();

            // Keep track of any cancellations
            OperationCanceledExceptionEx oce = null;

            // if cancellation is enabled, we need to register a callback to stop the loop when it gets signaled
            var ctr = !parallelOptions.CancellationToken.CanBeCanceled
                ? default
                : parallelOptions.CancellationToken.Register
                (
                    _ =>
                    {
                        // Record our cancellation before stopping processing
                        oce = new OperationCanceledExceptionEx("parallelOptions.CancellationToken");
                        // Cause processing to stop
                        sharedPStateFlags.Cancel();
                    },
                    null,
                    false
                );

            // Get our dynamic partitioner -- depends on whether source is castable to OrderablePartitioner
            // Also, do some error checking.
            IEnumerable<TSource> partitionerSource = null;
            IEnumerable<KeyValuePair<long, TSource>> orderablePartitionerSource = null;
            if (orderedSource != null)
            {
                orderablePartitionerSource = orderedSource.GetOrderableDynamicPartitions();
                if (orderablePartitionerSource == null)
                {
                    throw new InvalidOperationException("SR.Parallel_ForEach_PartitionerReturnedNull");
                }
            }
            else
            {
                partitionerSource = source.GetDynamicPartitions();
                if (partitionerSource == null)
                {
                    throw new InvalidOperationException("SR.Parallel_ForEach_PartitionerReturnedNull");
                }
            }

            try
            {
                try
                {
                    TaskReplicator.Run
                    (
                        (
                            ref IEnumerator partitionState,
                            int timeout,
                            out bool replicationDelegateYieldedBeforeCompletion) =>
                        {
                            // We will need to reset this to true if we exit due to a timeout:
                            replicationDelegateYieldedBeforeCompletion = false;

                            var localValue = default(TLocal);
                            var bLocalValueInitialized = false; // Tracks whether localInit ran without exceptions, so that we can skip localFinally if it wasn't

                            try
                            {
                                // Create a new state object that references the shared "stopped" and "exceptional" flags.
                                // If needed, it will contain a new instance of thread-local state by invoking the selector.
                                ParallelLoopState64 state = null;

                                if (bodyWithState != null || bodyWithStateAndIndex != null)
                                {
                                    state = new ParallelLoopState64(sharedPStateFlags);
                                }
                                else if (bodyWithStateAndLocal != null || bodyWithEverything != null)
                                {
                                    state = new ParallelLoopState64(sharedPStateFlags);
                                    // If a thread-local selector was supplied, invoke it. Otherwise, stick with the default.
                                    if (localInit != null)
                                    {
                                        localValue = localInit();
                                        bLocalValueInitialized = true;
                                    }
                                }

                                // initialize a loop timer which will help us decide whether we should exit early
                                var loopTimeout = ComputeTimeoutPoint(timeout);

                                if (orderedSource != null) // Use this path for OrderablePartitioner:
                                {
                                    // first check if there's saved state from a previous replica that we might be replacing.
                                    // the only state to be passed down in such a transition is the enumerator
                                    if (!(partitionState is IEnumerator<KeyValuePair<long, TSource>> myPartition))
                                    {
                                        myPartition = orderablePartitionerSource.GetEnumerator();
                                        partitionState = myPartition;
                                    }

                                    if (myPartition == null)
                                    {
                                        throw new InvalidOperationException("SR.Parallel_ForEach_NullEnumerator");
                                    }

                                    while (myPartition.MoveNext())
                                    {
                                        var kvp = myPartition.Current;
                                        var index = kvp.Key;
                                        var value = kvp.Value;

                                        // Update our iteration index
                                        if (state != null)
                                        {
                                            state.CurrentIteration = index;
                                        }

                                        if (simpleBody != null)
                                        {
                                            simpleBody(value);
                                        }
                                        else if (bodyWithState != null)
                                        {
                                            bodyWithState(value, state);
                                        }
                                        else if (bodyWithStateAndIndex != null)
                                        {
                                            bodyWithStateAndIndex(value, state, index);
                                        }
                                        else if (bodyWithStateAndLocal != null)
                                        {
                                            localValue = bodyWithStateAndLocal(value, state, localValue);
                                        }
                                        else
                                        {
                                            localValue = bodyWithEverything(value, state, index, localValue);
                                        }

                                        if (sharedPStateFlags.ShouldExitLoop(index))
                                        {
                                            break;
                                        }

                                        // Cooperative multitasking:
                                        // Check if allowed loop time is exceeded, if so save current state and return.
                                        // The task replicator will queue up a replacement task. Note that we don't do this on the root task.
                                        if (!CheckTimeoutReached(loopTimeout))
                                        {
                                            continue;
                                        }

                                        replicationDelegateYieldedBeforeCompletion = true;
                                        break;
                                    }
                                }
                                else // Use this path for Partitioner that is not OrderablePartitioner:
                                {
                                    // first check if there's saved state from a previous replica that we might be replacing.
                                    // the only state to be passed down in such a transition is the enumerator
                                    if (!(partitionState is IEnumerator<TSource> myPartition))
                                    {
                                        myPartition = partitionerSource.GetEnumerator();
                                        partitionState = myPartition;
                                    }

                                    if (myPartition == null)
                                    {
                                        throw new InvalidOperationException("SR.Parallel_ForEach_NullEnumerator");
                                    }

                                    // I'm not going to try to maintain this
                                    if (state != null)
                                    {
                                        state.CurrentIteration = 0;
                                    }

                                    while (myPartition.MoveNext())
                                    {
                                        var t = myPartition.Current;

                                        if (simpleBody != null)
                                        {
                                            simpleBody(t);
                                        }
                                        else if (bodyWithState != null)
                                        {
                                            bodyWithState(t, state);
                                        }
                                        else if (bodyWithStateAndLocal != null)
                                        {
                                            localValue = bodyWithStateAndLocal(t, state, localValue);
                                        }
                                        else
                                        {
                                            DebugEx.Fail("PartitionerForEach: illegal body type in Partitioner handler");
                                        }

                                        // Any break, stop or exception causes us to halt
                                        // We don't have the global indexing information to discriminate whether or not
                                        // we are before or after a break point.
                                        if (sharedPStateFlags.LoopStateFlags != ParallelLoopStateFlags.ParallelLoopStateNone)
                                        {
                                            break;
                                        }

                                        // Cooperative multitasking:
                                        // Check if allowed loop time is exceeded, if so save current state and return.
                                        // The task replicator will queue up a replacement task. Note that we don't do this on the root task.
                                        if (!CheckTimeoutReached(loopTimeout))
                                        {
                                            continue;
                                        }

                                        replicationDelegateYieldedBeforeCompletion = true;
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Inform other tasks of the exception, then rethrow
                                sharedPStateFlags.SetExceptional();
                                ExceptionDispatchInfo.Capture(ex).Throw();
                            }
                            finally
                            {
                                if (localFinally != null && bLocalValueInitialized)
                                {
                                    localFinally(localValue);
                                }

                                if (!replicationDelegateYieldedBeforeCompletion)
                                {
                                    if (partitionState is IDisposable partitionToDispose)
                                    {
                                        partitionToDispose.Dispose();
                                    }
                                }
                            }
                        },
                        parallelOptions,
                        true
                    );
                }
                finally
                {
                    // Dispose the cancellation token registration before checking for a cancellation exception
                    if (parallelOptions.CancellationToken.CanBeCanceled)
                    {
                        ctr.Dispose();
                    }
                }

                // If we got through that with no exceptions, and we were canceled, then
                // throw our cancellation exception
                if (oce != null)
                {
                    throw oce;
                }
            }
            catch (AggregateException aggExp)
            {
                // If we have many cancellation exceptions all caused by the specified user cancel control, then throw only one OCE:
                ThrowSingleCancellationExceptionOrOtherException
                (
                    aggExp.InnerExceptions,
                    aggExp, parallelOptions.CancellationToken
                );
            }
            finally
            {
                var status = sharedPStateFlags.LoopStateFlags;
                result._completed = status == ParallelLoopStateFlags.ParallelLoopStateNone;
                if ((status & ParallelLoopStateFlags.ParallelLoopStateBroken) != 0)
                {
                    result._lowestBreakIteration = sharedPStateFlags.LowestBreakIteration;
                }

                //dispose the partitioner source if it implements IDisposable
                var d = orderablePartitionerSource != null ? orderablePartitionerSource as IDisposable : partitionerSource as IDisposable;

                d?.Dispose();
            }

            return result;
        }

        private static OperationCanceledExceptionEx ReduceToSingleCancellationException(
            ICollection exceptions,
            CancellationToken cancelToken)
        {
            // If collection is empty - no match:
            if (exceptions == null || exceptions.Count == 0)
            {
                return null;
            }

            // If token is not cancelled, it can not be part of an exception:
            if (!cancelToken.IsCancellationRequested)
            {
                return null;
            }

            // Check all exceptions:
            Exception first = null;
            foreach (var exObj in exceptions)
            {
                Debug.Assert(exObj is Exception);
                var ex = (Exception)exObj;

                if (first == null)
                {
                    first = ex;
                }

                // If mismatch found, fail-fast:
                if (!(ex is OperationCanceledExceptionEx ocEx) || !cancelToken.Equals(ocEx.CancellationToken))
                {
                    return null;
                }
            }

            // All exceptions are OCEs with this token, let's just pick the first:
            Debug.Assert(first is OperationCanceledExceptionEx);
            return (OperationCanceledExceptionEx)first;
        }

        private static void ThrowSingleCancellationExceptionOrOtherException(ICollection exceptions, Exception otherException, CancellationToken cancelToken)
        {
            // IF exceptions are all OperationCanceledExceptionExs with the specified cancelToken,
            // THEN throw that unique OperationCanceledExceptionEx (pick any);
            // OTHERWISE throw the specified otherException.
            var reducedCancelEx = ReduceToSingleCancellationException(exceptions, cancelToken);
            ExceptionDispatchInfo.Capture(reducedCancelEx ?? otherException).Throw();
        }
    }
}

#endif