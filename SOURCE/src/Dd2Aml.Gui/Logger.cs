/*
 *  Copyright (C) 2019 GSD2AML Team (Nico Dietz, Steffen Gerdes, Constantin Ruhdorfer,
 *  Jonas Komarek, Phuc Quang Vu, Michael Weidmann)
 *  2020 DD2AML Team (Antonia Wermerskirch, Nora Baitinger,
 *  Bastiane Storz, Lara Mack)
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation version 3 of the License.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Dd2Aml.Lib.Logging;
using NLog;
using LogLevel = Dd2Aml.Lib.Logging.LogLevel;

namespace Dd2Aml.Gui
{
    /// <summary>
    /// The logger instance of the CLI.
    /// </summary>
    public class Logger : ILoggingService
    {
        private NLog.Logger NlogLogger { get; } = LogManager.GetLogger("gui_logger");

        /// <summary>
        /// Logs a message to the specified log level using NLog.
        /// </summary>
        /// <param name="level">The level on which to log on.</param>
        /// <param name="message">The log message.</param>
        public void Log(LogLevel level, string message)
        {
            NLog.LogLevel logLevel;
            switch (level)
            {
                case LogLevel.Error:
                    logLevel = NLog.LogLevel.Error;
                    break;

                case LogLevel.Info:
                    logLevel = NLog.LogLevel.Info;
                    break;

                case LogLevel.Trace:
                    logLevel = NLog.LogLevel.Trace;
                    break;

                case LogLevel.Warning:
                    logLevel = NLog.LogLevel.Warn;
                    break;

                case LogLevel.Debug:
                    logLevel = NLog.LogLevel.Debug;
                    break;

                case LogLevel.Fatal:
                    logLevel = NLog.LogLevel.Fatal;
                    break;
                    
                default:
                    logLevel = NLog.LogLevel.Off;
                    break;
            }

            NlogLogger.Log(logLevel, message);
        }
    }
}
