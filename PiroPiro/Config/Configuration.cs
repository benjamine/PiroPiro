using System;
using System.Collections.Generic;

namespace PiroPiro.Config
{
    /// <summary>
    /// Represents a source for configuration settings
    /// </summary>
    public class Configuration
    {
        private static Configuration _Default = null;

        /// <summary>
        /// Default Configuration for this Application
        /// </summary>
        public static Configuration Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new Config.AppEnvConfiguration();
                }
                return _Default;
            }
            set
            {
                _Default = value;
            }
        }

        protected IDictionary<string, object> InternalDict = new Dictionary<string, object>();

        /// <summary>
        /// Gets the setting value associated with the specified key.
        /// </summary>
        /// <param name="name">setting name</param>
        /// <param name="value">setting value</param>
        /// <returns>false if the setting could not be found</returns>
        public virtual bool TryGet(string name, out object value)
        {
            return InternalDict.TryGetValue(name, out value);
        }

        /// <summary>
        /// Gets the setting value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">value type, a conversion will be attempted automatically</typeparam>
        /// <param name="name">setting name</param>
        /// <param name="value">setting value</param>
        /// <returns>false if the setting could not be found</returns>
        public bool TryGet<T>(string name, out T value)
        {
            object val;
            if (TryGet(name, out val))
            {
                if (val == null)
                {
                    value = default(T);
                }
                if (val is T)
                {
                    value = (T)val;
                }
                else
                {
                    value = (T)Convert.ChangeType(val, typeof(T));
                }
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Sets a value in the internal dictionary (will be return on get)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public virtual void Set(string name, object value)
        {
            InternalDict[name] = value;
        }

        /// <summary>
        /// Gets the setting string value associated with the specified key.
        /// </summary>
        /// <param name="name">setting name</param>
        /// <param name="required">if true and the setting could not be found a KeyNotFoundException is thrown</param>
        /// <returns></returns>
        public string Get(string name, bool required = true)
        {
            return Get<string>(name, required);
        }

        /// <summary>
        /// Gets the setting value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type to return, a conversion will be attempted automatically</typeparam>
        /// <param name="name">setting name</param>
        /// <param name="required">if true and the setting could not be found a KeyNotFoundException is thrown</param>
        /// <returns></returns>
        public T Get<T>(string name, bool required = true)
        {
            T value;
            if (TryGet(name, out value))
            {
                return value;
            }
            else
            {
                if (required)
                {
                    throw new KeyNotFoundException(string.Format("Configuration setting '{0}' not found", name));
                }
                return default(T);
            }
        }

        /// <summary>
        /// Get a boolean setting value (true for yes/true/1, false for no/false/0)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public bool? GetFlag(string name, bool required = true)
        {
            string sValue = Get(name, required);
            if (string.IsNullOrWhiteSpace(sValue))
            {
                return null;
            }
            switch (sValue.Trim().ToLowerInvariant())
            {
                case "true":
                case "yes":
                case "1":
                    return true;
                case "false":
                case "no":
                case "0":
                    return false;
                default:
                    throw new Exception(string.Format("Invalid flag value: '{0}', use 'true', 'false', 'yes', 'no', '1', '0'", sValue));
            }
        }
    }
}
