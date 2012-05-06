using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro.Contract
{
    /// <summary>
    /// Browser http cookies
    /// </summary>
    public abstract class Cookies
    {
        public abstract Browser Browser { get; }

        /// <summary>
        /// Set an http cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="expiry"></param>
        public abstract void Set(string name, string value, string domain, string path, DateTime? expiry);

        /// <summary>
        /// Get a cookie value by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract string Get(string name);

        /// <summary>
        /// Clear all cookies
        /// </summary>
        public abstract void Clear();
    }
}
