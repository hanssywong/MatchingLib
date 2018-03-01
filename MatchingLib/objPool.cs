using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MatchingLib
{
    public class objPool<T>
    {
        private int INITIAL_POOL_SIZE = 0; // initial size of the pool
        public int poolsize { get { return INITIAL_POOL_SIZE; } }

        // pool of buffers
        private ConcurrentBag<T> m_FreeBuffers;

        private Func<T> newConstructor;

        public objPool(Func<T> newOjb, int _poolSize = 1000)
        {
            INITIAL_POOL_SIZE = _poolSize;
            newConstructor = newOjb;
            m_FreeBuffers = new ConcurrentBag<T>();
            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                m_FreeBuffers.Add(newConstructor());
            }
        }

        /// <summary>
        /// Increase Object pool size by number of count
        /// Thread safe
        /// </summary>
        /// <param name="count"></param>
        public void IncObjPoolSize(int count)
        {
            for (int i = 0; i < count; i++)
            {
                m_FreeBuffers.Add(newConstructor());
            }
            Interlocked.Add(ref INITIAL_POOL_SIZE, count);
        }

        /// <summary>
        ///  check out a buffer, Single thread only
        ///  if resource isn't enough, it will create a new resource
        /// </summary>
        /// <returns></returns>
        public T Checkout()
        {
            T buffer;

            if (!m_FreeBuffers.TryTake(out buffer))
            {
                buffer = newConstructor();
                //Interlocked.Increment(ref INITIAL_POOL_SIZE);
                //Since we are using single thread for this function, so we can ignore the concurrency
                INITIAL_POOL_SIZE++;
            }
            // instead of creating new buffer, 
            // blocking waiting or refusing request may be better
            return buffer;
        }

        /// <summary>
        ///  check out a buffer, Thread safe in multi-thread environment
        ///  this method will not increase total buffer size when buffer runs out instead it will return false
        /// </summary>
        /// <returns></returns>
        public bool CheckoutLimited(out T item)
        {
            return m_FreeBuffers.TryTake(out item);
        }

        /// <summary>
        ///  check out a buffer, For multithread purpose
        ///  if resource isn't enough, it will create a new resource
        /// </summary>
        /// <returns></returns>
        public T CheckoutMT()
        {
            T buffer;

            if (!m_FreeBuffers.TryTake(out buffer))
            {
                buffer = newConstructor();
                Interlocked.Increment(ref INITIAL_POOL_SIZE);
            }
            // instead of creating new buffer, 
            // blocking waiting or refusing request may be better
            return buffer;
        }

        /// <summary>
        /// Thread safe, check in a buffer
        /// </summary>
        /// <param name="buffer"></param>
        public void Checkin(T buffer)
        {
            m_FreeBuffers.Add(buffer);
        }
    }
}
