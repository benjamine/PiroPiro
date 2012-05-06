using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using System.Configuration;
using System.Dynamic;

namespace PiroPiro.Config
{
    /// <summary>
    /// Lookups for settings in environment variables, config files, internal dict or a set of default values (in this order)
    /// </summary>
    public class AppEnvConfiguration : Configuration
    {
        /// <summary>
        /// Indicates if settings can be obtained from Environment variables (<see cref="System.Environment"/>)
        /// </summary>
        public bool UseEnvironmentVariables { get; set; }

        /// <summary>
        /// Indicates if settings can be obtained from configuration files (<see cref="System.Configuration.ConfigurationManager"/>)
        /// </summary>
        public bool UseConfigFiles { get; set; }

        /// <summary>
        /// Indicates if settings can be obtained from DefaultValues dictionary
        /// </summary>
        public bool UseDefaultValues { get; set; }

        /// <summary>
        /// Dictionary of default values
        /// </summary>
        public IDictionary<string, object> DefaultValues { get; set; }

        public AppEnvConfiguration()
        {
            UseConfigFiles = true;
            UseEnvironmentVariables = true;
            UseDefaultValues = true;
        }

        public override bool TryGet(string name, out object value)
        {
            if (!base.TryGet(name, out value))
            {
                // not found on internal dictionary
                string sValue;

                if (UseEnvironmentVariables)
                {
                    // try environment variable
                    sValue = Environment.GetEnvironmentVariable(name);
                    if (!string.IsNullOrEmpty(sValue))
                    {
                        value = sValue;
                        return true;
                    }
                }

                if (UseConfigFiles)
                {
                    // try config files (app, user or machine .config)
                    sValue = ConfigurationManager.AppSettings[name];
                    if (!string.IsNullOrEmpty(sValue))
                    {
                        value = sValue;
                        return true;
                    }
                }

                if (UseDefaultValues && DefaultValues != null)
                {
                    return DefaultValues.TryGetValue(name, out value);
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
