using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensorTagPi.Core.Interfaces
{
    /// <summary>
    /// Interface to define common instrummentation methods supported by a logging service.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log a verbose message.
        /// </summary>
        /// <param name="source">The object originating the message</param>
        /// <param name="message">The message format.</param>
        /// <param name="args">optional arguments to use in the message format</param>
        void LogVerbose(string source, string message, params object[] args);

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="source">The object originating the message</param>
        /// <param name="message">The message format.</param>
        /// <param name="args">optional arguments to use in the message format</param>
        void LogInfo(string source, string message, params object[] args);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="source">The object originating the message</param>
        /// <param name="message">The message format.</param>
        /// <param name="args">optional arguments to use in the message format</param>
        void LogWarning(string source, string message, params object[] args);

        /// <summary>
        /// Logs a critical error message.
        /// </summary>
        /// <param name="source">The object originating the message</param>
        /// <param name="message">The message format.</param>
        /// <param name="args">optional arguments to use in the message format</param>
        void LogError(string source, string message, params object[] args);

        /// <summary>
        /// Logs an exception message.
        /// </summary>
        /// <param name="source">The object originating the message</param>
        /// <param name="exception">The exception instance that originated the message</param>
        /// <param name="message">The message format.</param>
        /// <param name="args">optional arguments to use in the message format</param>
        void LogException(string source, Exception exception, string message, params object[] args);
    }
}
