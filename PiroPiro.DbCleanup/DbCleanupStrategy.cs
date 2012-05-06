using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Config;
using System.ComponentModel;

namespace PiroPiro.DbCleanup
{
    /// <summary>
    /// Represents a strategy to clean up a database (restore to an original state)
    /// </summary>
    public abstract class DbCleanupStrategy
    {
        /// <summary>
        /// Occurs before this strategy is executed
        /// </summary>
        public event EventHandler<CancelEventArgs> Executing;

        public class FinishedTaskEventArgs : EventArgs
        {
            /// <summary>
            /// Elapsed time executing task
            /// </summary>
            public TimeSpan Elapsed { get; set; }

            public FinishedTaskEventArgs(TimeSpan elapsed)
            {
                this.Elapsed = elapsed;
            }
        }

        /// <summary>
        /// Occurs after this strategy is executed
        /// </summary>
        public event EventHandler<FinishedTaskEventArgs> Executed;

        private static DbCleanupStrategy _Default;

        /// <summary>
        /// Default strategy based on <see cref="Configuration.Default"/>
        /// </summary>
        public static DbCleanupStrategy Default
        {
            get
            {
                if (_Default == null)
                {
                    FromConfig().SetAsDefault();
                }
                return _Default;
            }
        }

        /// <summary>
        /// Configuration source for this strategy
        /// </summary>
        public Configuration Configuration { get; private set; }

        /// <summary>
        /// Create a strategy based on configuration settings
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static DbCleanupStrategy FromConfig(Configuration configuration = null)
        {
            Configuration cfg = configuration ?? Configuration.Default;

            string strategyName = cfg.Get("PiroPiro.DbCleanup.Strategy", false);

            if (string.IsNullOrWhiteSpace(strategyName))
            {
                return NullDbCleanupStrategy.Instance;
            }

            var strategyType = Type.GetType(strategyName, false);
            if (strategyType == null)
            {
                string defaultPrefix = typeof(DbCleanupStrategy).Namespace ?? "";
                if (!strategyName.StartsWith(defaultPrefix))
                {
                    strategyName = defaultPrefix + "." + strategyName;
                    strategyType = Type.GetType(strategyName, false);
                }
            }

            if (strategyType == null)
            {
                throw new Exception(string.Format("Strategy '{0}' not found", strategyName));
            }
            var constructor = strategyType.GetConstructor(new[] { typeof(Configuration) });
            if (constructor == null)
            {
                throw new Exception(string.Format("Type '{0}' doesn't have a constructor with a single parameter of type '{1}'",
                    strategyType.FullName, typeof(Configuration).FullName));
            }
            var strategy = (DbCleanupStrategy)constructor.Invoke(new object[] { configuration });

            return strategy;
        }

        protected DbCleanupStrategy(Configuration configuration = null)
        {
            this.Configuration = configuration ?? Configuration.Default;
        }

        /// <summary>
        /// Execute this strategy cleaning up the database
        /// </summary>
        public void Execute()
        {
            var executingHandler = Executing;
            if (executingHandler != null)
            {
                var cancelEventArgs = new CancelEventArgs();
                executingHandler(this, cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return;
                }
            }

            Console.WriteLine(string.Format("[DbCleanup:Begin] Strategy: {0}", this.GetType().FullName));

            DateTime startTime = DateTime.Now;

            try
            {
                DoExecute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[DbCleanup:Failed] Error: {0}", ex));
                throw;
            }

            TimeSpan elapsed = DateTime.Now.Subtract(startTime);

            Console.WriteLine(string.Format("[DbCleanup:Complete] Elapsed: {0}", elapsed));

            var executedHandler = Executed;
            if (executedHandler != null)
            {
                executedHandler(this, new FinishedTaskEventArgs(elapsed));
            }
        }

        protected abstract void DoExecute();

        /// <summary>
        /// Set this strategy as default
        /// </summary>
        public void SetAsDefault()
        {
            _Default = this;
        }

    }
}
