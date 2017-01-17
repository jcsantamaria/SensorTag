using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Tracing;

using Windows.Foundation.Diagnostics;

using SensorTagPi.Core.Interfaces;

namespace SensorTagPi.Log
{
    class Logger : ILogger
    {
        LoggingChannel _channel;

        public Logger()
        {
            _channel = new LoggingChannel("SensorTagPi", null);
        }

        /// <summary>
        /// Builds and writes a message using <c>System.Diagnostics.TraceSource</c> for this component.
        /// <seealso cref="System.Diagnostics.TraceSource"/>
        /// </summary>
        /// <param name="level">Level of the event.</param>
        /// <param name="source">The place where this message originated</param>
        /// <param name="message">The message</param>
        /// <param name="args">optional argument to format with the message</param>
        private void Log(LoggingLevel level, string source, string message, params object[] args)
        {
#if DEBUG
            string msg = string.Format($"{source}: {message}", args);
            _channel.LogMessage(msg, level);
            Debug.WriteLine($"{DateTime.Now:s}: {msg}");
#else
            _channel.LogMessage(string.Format($"{source}: {message}", args), level);
#endif
        }

        protected StringBuilder BuildExceptionMessage(StringBuilder sb, Exception ex)
        {
            if (ex != null)
            {
                sb.AppendFormat("[{0}]: {1} ", ex.GetType().ToString(), ex.Message);
                BuildExceptionMessage(sb, ex.InnerException);
            }
            return sb;
        }

#region ILogger methods
        public void LogVerbose(string source, string message, params object[] args)
        {
            Log(LoggingLevel.Verbose, source, message, args);
        }

        public void LogInfo(string source, string message, params object[] args)
        {
            Log(LoggingLevel.Information, source, message, args);
        }

        public void LogWarning(string source, string message, params object[] args)
        {
            Log(LoggingLevel.Warning, source, message, args);
        }

        public void LogError(string source, string message, params object[] args)
        {
            Log(LoggingLevel.Error, source, message, args);
        }

        public void LogException(string source, Exception exception, string message, params object[] args)
        {
            string msg = args != null ? string.Format(message, args) : message;
            LogError(source, "{0}: {1}", msg, BuildExceptionMessage(new StringBuilder("exception: "), exception));
        }
#endregion
    }
}
