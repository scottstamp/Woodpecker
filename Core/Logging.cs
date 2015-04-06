using System;

namespace Woodpecker.Core
{
    /// <summary>
    /// Provides various functions for logging events.
    /// </summary>
    public static class Logging
    {
        #region Fields
        public enum logType
        {
            debugEvent,
            commonWarning,
            commonError,
            reactorError,
            targetMethodNotFoundEvent,
            sessionConnectionEvent,
            serverMessageEvent,
            clientMessageEvent,
            userVisitEvent,
            roomInstanceEvent,
            roomUserRegisterEvent,
            moderationEvent,
            connectionBlacklistEvent,
            haxEvent
        }
        private static bool[] logActionStatus = new bool[14];
        #endregion

        #region Methods
        public static void setDefaultLogSessings()
        {
            logActionStatus[0] = false; // Debug
            logActionStatus[1] = true; // Common warning
            logActionStatus[2] = true; // Common error
            logActionStatus[3] = false; // Reactor error
            logActionStatus[4] = false; // Listener not found
            logActionStatus[5] = true; // Session connection event
            logActionStatus[6] = false; // Server>client message
            logActionStatus[7] = false; // Client>server message
            logActionStatus[8] = true; // User login/logout
            logActionStatus[9] = true; // Room instance create/destroy
            logActionStatus[10] = false; // Room user enter/leave
            logActionStatus[11] = true; // Moderation event
            logActionStatus[12] = true; // Connection blacklist event
            logActionStatus[13] = true; // Hax alert
        }
        /// <summary>
        /// Toggles the log yes/no status of a certain log type.
        /// </summary>
        /// <param name="Type">The logtype to toggle the status of/.</param>
        public static void toggleLogStatus(logType lType)
        {
            int iType = (int)lType;
            logActionStatus[iType] = !logActionStatus[iType];
        }
        public static void Log(string Text)
        {
            Engine.Program.GUI.logSacredText(Text);
        }
        public static void Log(string Text, logType Type)
        {
            if (logActionStatus[(int)Type])
                Engine.Program.GUI.logText(Text, Type);
        }
        #endregion
    }
}
