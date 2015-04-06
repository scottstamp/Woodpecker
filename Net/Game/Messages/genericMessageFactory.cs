using System;

namespace Woodpecker.Net.Game.Messages
{
    public static class genericMessageFactory
    {
        public static serverMessage createMessageBoxCast(string Text)
        {
            serverMessage retCast = new serverMessage(139); // "BK"
            retCast.Append(Text);

            return retCast;
        }
        public static serverMessage createBanCast(string banReason)
        {
            serverMessage retCast = new serverMessage(35); // "@c"
            retCast.Append(banReason);

            return retCast;
        }
    }
}
