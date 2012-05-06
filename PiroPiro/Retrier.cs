using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro
{
    /// <summary>
    /// Executes a function retrying (with timeout or max count) on certain exceptions or default values
    /// </summary>
    internal class Retrier
    {
        internal class RetryOptions
        {
            /// <summary>
            /// If true, retry when the function returns a default value (for the return type)
            /// </summary>
            public bool RetryIfDefaultValue { get; set; }

            /// <summary>
            /// Maximum elapsed time for the last retry
            /// </summary>
            public TimeSpan? Timeout { get; set; }

            /// <summary>
            /// Retry if an exception of one of these types occur (rethrow otherwise)
            /// </summary>
            public Type[] RetryOnExceptions { get; set; }

            /// <summary>
            /// Interval between retry attempts
            /// </summary>
            public int Interval { get; set; }

            /// <summary>
            /// Maximum number of retries
            /// </summary>
            public int MaxRetries { get; set; }

            /// <summary>
            /// Creates retry options with default values
            /// </summary>
            public RetryOptions()
            {
                Interval = 500;
                MaxRetries = 120;
            }
        }

        public RetryOptions Options { get; set; }

        /// <summary>
        /// Creates a new retrier
        /// </summary>
        /// <param name="options"></param>
        public Retrier(RetryOptions options)
        {
            this.Options = options ?? new RetryOptions();
        }

        /// <summary>
        /// Creates a new retrier specifying retry options (for parameter info check <seealso cref="Retrier.RetryOptions"/>)
        /// </summary>
        /// <param name="retryIfDefaultValue"></param>
        /// <param name="timeout"></param>
        /// <param name="retryOnExceptions"></param>
        /// <param name="maxRetries"></param>
        public Retrier(bool retryIfDefaultValue = false, TimeSpan? timeout = null,
            IEnumerable<Type> retryOnExceptions = null, int? maxRetries = null)
        {
            Options = new RetryOptions();
            Options.RetryIfDefaultValue = retryIfDefaultValue;
            if (timeout != null)
            {
                Options.Timeout = timeout.Value;
            }
            if (retryOnExceptions != null)
            {
                Options.RetryOnExceptions = retryOnExceptions.ToArray();
            }
            if (maxRetries != null)
            {
                Options.MaxRetries = maxRetries.Value;
            }
        }

        /// <summary>
        /// Executes a function, retrying if necessary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <returns></returns>
        public T Get<T>(Func<T> fn)
        {
            DateTime start = DateTime.Now;
            T value = default(T);
            bool retry = false;
            int count = 0;

            // first attempt
            try
            {
                value = fn();
                if (Options.RetryIfDefaultValue &&
                    ((default(T) == null && value == null)
                    || (default(T) != null && default(T).Equals(value))))
                {
                    retry = true;
                }
            }
            catch (Exception ex)
            {
                if (Options.RetryOnExceptions == null || !Options.RetryOnExceptions.Any(t => t.IsAssignableFrom(ex.GetType())))
                {
                    throw;
                }
                retry = true;
            }

            while (retry)
            {
                // retry with sleep interval until we get a valid return or timeout is reached
                retry = false;
                try
                {
                    System.Threading.Thread.Sleep(Options.Interval > 0 ? Options.Interval : 500);
                    count++;
                    value = fn();

                    if (Options.RetryIfDefaultValue &&
                        ((default(T) == null && value == null)
                        || (default(T) != null && default(T).Equals(value))))
                    {
                        // default value obtained, retry
                        retry = true;

                        if (count >= Options.MaxRetries)
                        {
                            // too many retries! return last value (even if invalid)
                            retry = false;
                        }
                        else if (Options.Timeout != null && Options.Timeout.Value.TotalMilliseconds > 0 &&
                           DateTime.Now.Subtract(start) > Options.Timeout)
                        {
                            // timeout! return last value (even if invalid)
                            retry = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Options.RetryOnExceptions == null || !Options.RetryOnExceptions.Any(t => t.IsAssignableFrom(ex.GetType())))
                    {
                        // unexpected exception
                        throw;
                    }

                    if (count >= Options.MaxRetries)
                    {
                        // too many retries!
                        throw;
                    }
                    else if (Options.Timeout != null && Options.Timeout.Value.TotalMilliseconds > 0 &&
                       DateTime.Now.Subtract(start) > Options.Timeout)
                    {
                        // timeout!
                        throw;
                    }

                    // expected exception, retry
                    retry = true;
                }
            }

            return value;
        }

        /// <summary>
        /// Executes a function, retrying if necessary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static T Get<T>(Func<T> fn, RetryOptions options)
        {
            return new Retrier(options).Get(fn);
        }

        /// <summary>
        /// Executes a function, retrying if necessary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <param name="retryIfDefaultValue">retry if default value for <typeparam name="T"> is obtained</typeparam></param>
        /// <param name="timeout">timeout for retrying (will retry every 500ms)</param>
        /// <param name="retryOnExceptions">retry if an exception of one of this types is thrown</param>
        /// <param name="maxRetries">maximum number of retries</param>
        /// <returns></returns>
        public static T Get<T>(Func<T> fn, bool retryIfDefaultValue = false, TimeSpan? timeout = null,
            IEnumerable<Type> retryOnExceptions = null, int? maxRetries = null)
        {
            return new Retrier(
                retryIfDefaultValue: retryIfDefaultValue,
                timeout: timeout,
                retryOnExceptions: retryOnExceptions,
                maxRetries: maxRetries).Get(fn);
        }
    }
}
