using System;

namespace Common.Server.Logging
{
    /// <summary>
    /// Interface implemented by all logger factories
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Creates a logger object initialized with a type
        /// </summary>
        /// <param name="type">Type to initialize the logger</param>
        /// <returns>Logger object</returns>
        Common.Logging.ILogger Create(Type type);
    }
}
