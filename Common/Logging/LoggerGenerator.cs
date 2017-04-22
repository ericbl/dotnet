using System;
using System.Configuration;

namespace Common.Logging
{
    /// <summary>
    /// Static generator of a logger
    /// </summary>
    public static class LoggerGenerator
    {
        /// <summary>
        /// loggingConfiguration
        /// </summary>
        private const string LoggingSectionName = "loggingConfiguration";

        /// <summary>
        /// Creates a Logger if the client is correctly set up (in app.config), otherwise a null logger.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>a logger</returns>
        public static ILogger CreateLogger(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            ILogger log;
            try
            {
                if (ConfigurationManager.GetSection(LoggingSectionName) != null)
                {
                    log = new TraceLogger(name);
                }
                else
                {
                    log = new NullLogger();
                }
            }
            catch (Exception)
            {
                log = new NullLogger();
            }

            return log;
        }

        /// <summary>
        /// Creates the logger if the client is correctly set up, otherwise a null logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>a logger</returns>
        public static ILogger CreateLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return CreateLogger(type.FullName);
        }
    }
}
