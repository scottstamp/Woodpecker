using System;

using Woodpecker.Game.Items;
using Woodpecker.Net.Game.Messages;

namespace Woodpecker.Specialized.Fun
{
    public static class FunUtils
    {
        public static serverMessage CreateVoiceSpeakMessage(string text)
        {
            // 'Speaker'
            floorItem pItem = new floorItem();
            pItem.ID = int.MaxValue;
            pItem.Rotation = 0;
            pItem.X = 255;
            pItem.Y = 255;
            pItem.Z = -1f;
            pItem.customData = "voiceSpeak(\"" + text + "\")";
            pItem.Definition = new itemDefinition();
            pItem.Definition.Sprite = "spotlight";
            pItem.Definition.Length = 1;
            pItem.Definition.Width = 1;

            serverMessage msg = new serverMessage(93); // "A]"
            msg.Append(pItem.ToString());

            return msg;
        }
    }
}
