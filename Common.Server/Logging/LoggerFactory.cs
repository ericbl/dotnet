using Common.Generic;
using Common.Logging;
using System;

namespace Common.Server.Logging
{
    /// <summary>
    /// Singelton Factory zum erzeugen von Logger Objekten vom Typ Logger im gleichen Namespace
    /// </summary>
    public class LoggerFactory : AbstractGenericSingleton<LoggerFactory>, ILoggerFactory
    {
        #region ILoggerFactory Members

        /// <summary>
        /// Returns a new logger object
        /// </summary>
        /// <param name="type">Type used to initialize the logger</param>
        /// <returns>New Logger object</returns>
        public ILogger Create(Type type)
        {
            return new TraceLogger(type.ToString());
        }

        #endregion
    }
}
