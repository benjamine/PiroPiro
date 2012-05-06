using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro.DbCleanup
{
    /// <summary>
    /// Null strategy, performs no action
    /// </summary>
    public class NullDbCleanupStrategy : DbCleanupStrategy
    {
        private static NullDbCleanupStrategy _Instance;

        public static NullDbCleanupStrategy Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new NullDbCleanupStrategy();
                }
                return _Instance;
            }
        }

        private NullDbCleanupStrategy()
        {
        }

        protected override void DoExecute()
        {
            // no action
        }
    }

}
