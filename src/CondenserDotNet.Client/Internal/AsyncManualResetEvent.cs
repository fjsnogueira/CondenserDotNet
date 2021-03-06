﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Internal
{
    public class AsyncManualResetEvent<T> where T : IEquatable<T>
    {
        private volatile TaskCompletionSource<T> m_tcs = new TaskCompletionSource<T>();

        public Task<T> WaitAsync() { return m_tcs.Task; }

        public void Set(T result)
        {
            while (true)
            {
                if (!m_tcs.TrySetResult(result) && !m_tcs.Task.Result.Equals(result))
                {
                    Interlocked.Exchange(ref m_tcs, new TaskCompletionSource<T>());
                }
                else
                {
                    return;
                }
            }
        }

        public void Reset()
        {
            while (true)
            {
                var tcs = m_tcs;
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<T>(), tcs) == tcs)
                    return;
            }
        }
    }
}
