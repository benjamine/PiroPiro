using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Config;

namespace PiroPiro.Contract
{

    /// <summary>
    /// Factory of Browser instances
    /// </summary>
    public abstract class BrowserFactory
    {
        private static IDictionary<string, BrowserFactory> _NamedInstances = new Dictionary<string, BrowserFactory>();

        /// <summary>
        /// Default factory
        /// </summary>
        public static BrowserFactory Default
        {
            get
            {
                BrowserFactory factory;
                if (!_NamedInstances.TryGetValue("_Default", out factory))
                {
                    // create default BrowserFactory using default Configuration
                    (factory = BrowserFactory.FromConfig()).SetAsDefault();
                }
                return factory;
            }
        }

        /// <summary>
        /// Get an instance by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BrowserFactory GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "_Default";
            }
            return _NamedInstances[name];
        }

        /// <summary>
        /// Configuration source for this factory and all its created browsers
        /// </summary>
        public Configuration Configuration { get; private set; }

        /// <summary>
        /// Register a new named factory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factory"></param>
        public static void Register(string name, BrowserFactory factory)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "_Default";
            }
            _NamedInstances[name] = factory;
        }

        /// <summary>
        /// Creates a new browser instance
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public Browser Create(Configuration cfg = null)
        {
            return DoCreate(cfg ?? Configuration);
        }

        protected abstract Browser DoCreate(Configuration cfg);

        /// <summary>
        /// base constructor
        /// </summary>
        /// <param name="cfg"></param>
        protected BrowserFactory(Configuration cfg = null)
        {
            Configuration = cfg ?? Configuration.Default;
        }

        /// <summary>
        /// Set this factory as default (<see cref="BrowserFactory.Default"/>)
        /// </summary>
        public void SetAsDefault()
        {
            BrowserFactory.Register(null, this);
        }

        /// <summary>
        /// Initializes default BrowserFactory using configuration values
        /// <param name="configuration">Configuration source, if null <see cref="Config.Configuration.Default"/> is used</param>
        /// </summary>
        public static BrowserFactory FromConfig(Configuration configuration = null)
        {
            Configuration cfg = configuration ?? Configuration.Default;

            string factoryClassName = cfg.Get("PiroPiro.BrowserFactory");
            var factoryType = Type.GetType(factoryClassName, false);
            if (factoryType == null)
            {
                throw new Exception(string.Format("Type '{0}' not found", factoryClassName));
            }
            var constructor = factoryType.GetConstructor(new[] { typeof(Configuration) });
            if (constructor == null)
            {
                throw new Exception(string.Format("Type '{0}' doesn't have a constructor with a single parameter of type '{1}'",
                    factoryClassName, typeof(Configuration).FullName));
            }
            var factory = (BrowserFactory)constructor.Invoke(new object[] { configuration });

            return factory;
        }
    }
}
