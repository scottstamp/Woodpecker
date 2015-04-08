using System;

namespace Woodpecker.Net.Game.Messages
{
    /// <summary>
    /// Provides the target method names for client>server messages. Method names are required for reflecting and invoking methods to process requests from clients. This class is static.
    /// </summary>
    public static class clientMessageTargetMethodNames
    {
        /// <summary>
        /// Returns the target method name for a certain client>server message as a string. If no results found, then 'UNKNOWN' is returned.
        /// </summary>
        /// <param name="messageID">The ID of the message to get the method name for.</param>
        public static string getMessageTargetMethodName(int messageID)
        {
            switch (messageID)
            {
                case 2:
                    return "ROOM_DIRECTORY";

                case 4:
                    return "TRY_LOGIN";

                case 5:
                    return "VERSIONCHECK";

                case 6:
                    return "UNIQUEID";

                case 7:
                    return "GET_INFO";

                case 8:
                    return "GET_CREDITS";

                case 9:
                    return "GETAVAILABLESETS";

                case 12:
                    return "MESSENGERINIT";

                case 13:
                    return "SBUSYF";

                case 15:
                    return "MESSENGER_UPDATE";

                case 16:
                    return "SUSERF";

                case 17:
                    return "SRCHF";

                case 18:
                    return "GETFVRF";

                case 19:
                    return "ADD_FAVORITE_ROOM";

                case 20:
                    return "DEL_FAVORITE_ROOM";

                case 21:
                    return "GETFLATINFO";

                case 23:
                    return "DELETEFLAT";

                case 24:
                    return "UPDATEFLAT";

                case 25:
                    return "SETFLATINFO";

                case 26:
                    return "SCR_GET_USER_INFO";

                case 28:
                    return "GETDOORFLAT";

                case 29:
                    return "CREATEFLAT";

                case 30:
                    return "MESSENGER_C_CLICK";

                case 31:
                    return "MESSENGER_C_READ";

                case 32:
                    return "MESSENGER_MARKREAD";

                case 33:
                    return "MESSENGER_SENDMSG";

                case 36:
                    return "MESSENGER_ASSIGNPERSMSG";

                case 37:
                    return "MESSENGER_ACCEPTBUDDY";

                case 38:
                    return "MESSENGER_DECLINEBUDDY";

                case 39:
                    return "MESSENGER_REQUESTBUDDY";

                case 40:
                    return "MESSENGER_REMOVEBUDDY";

                case 41:
                    return "FINDUSER";

                case 42:
                    return "APPROVENAME";

                case 43:
                    return "REGISTER";

                case 44:
                    return "UPDATE";

                case 46:
                    return "AC";

                case 47:
                    return "GET_PASSWORD";

                case 48:
                    return "PICK_CRYFORHELP";

                case 49:
                    return "GDATE";

                case 52:
                    return "CHAT";

                case 53:
                    return "QUIT";

                case 54:
                    return "GOVIADOOR";

                case 55:
                    return "SHOUT";

                case 56:
                    return "WHISPER";

                case 57:
                    return "TRYFLAT";

                case 58:
                    return "LANGCHECK";

                case 59:
                    return "GOTOFLAT";

                case 60:
                    return "G_HMAP";

                case 61:
                    return "G_USRS";

                case 62:
                    return "G_OBJS";

                case 63:
                    return "G_ITEMS";

                case 64:
                    return "G_STAT";

                case 65:
                    return "GETSTRIP";

                case 66:
                    return "FLATPROPBYITEM";

                case 67:
                    return "ADDSTRIPITEM";

                case 68:
                    return "TRADE_UNACCEPT";

                case 69:
                    return "TRADE_ACCEPT";

                case 70:
                    return "TRADE_CLOSE";

                case 71:
                    return "TRADE_OPEN";

                case 72:
                    return "TRADE_ADDITEM";

                case 73:
                    return "MOVESTUFF";

                case 74:
                    return "SETSTUFFDATA";

                case 75:
                    return "MOVE";

                case 76:
                    return "THROW_DICE";

                case 77:
                    return "DICE_OFF";

                case 78:
                    return "PRESENTOPEN";

                case 79:
                    return "LOOKTO";

                case 80:
                    return "CARRYDRINK";

                case 81:
                    return "INTODOOR";

                case 82:
                    return "DOORGOIN";

                case 83:
                    return "G_IDATA";

                case 84:
                    return "SETITEMDATA";

                case 85:
                    return "REMOVEITEM";

                case 86:
                    return "CRYFORHELP";

                case 87:
                    return "CARRYITEM";

                case 88:
                    return "STOP";

                case 89:
                    return "USEITEM";

                case 90:
                    return "PLACESTUFF";

                case 93:
                    return "DANCE";

                case 94:
                    return "WAVE";

                case 95:
                    return "KICKUSER";

                case 96:
                    return "ASSIGNRIGHTS";

                case 97:
                    return "REMOVERIGHTS";

                case 98:
                    return "LETUSERIN";

                case 99:
                    return "REMOVESTUFF";

                case 100:
                    return "GRPC";

                case 101:
                    return "GCIX";

                case 102:
                    return "GCAP";

                case 103:
                    return "JUMPSTART";

                case 104:
                    return "SIGN";

                case 105:
                    return "BUY_TICKETS";

                case 106:
                    return "JUMPPERF";

                case 107:
                    return "SPLASH_POSITION";

                case 108:
                    return "CLOSE_UIMAKOPPI";

                case 112:
                    return "HUBU_VOTE";

                case 114:
                    return "PAALU_MOVE";

                case 115:
                    return "GOAWAY";

                case 116:
                    return "SWIMSUIT";

                case 126:
                    return "GETROOMAD";

                case 127:
                    return "GETUSERCREDITLOG";

                case 128:
                    return "GETPETSTAT";

                case 129:
                    return "REDEEM_VOUCHER";

                case 130:
                    return "COPPA_REG_CHECKTIME";

                case 131:
                    return "COPPA_REG_GETREALTIME";

                case 146:
                    return "PARENT_EMAIL_REQUIRED";

                case 147:
                    return "VALIDATE_PARENT_EMAIL";

                case 148:
                    return "SEND_PARENT_EMAIL";

                case 149:
                    return "UPDATE_ACCOUNT";

                case 150:
                    return "NAVIGATE";

                case 151:
                    return "GETUSERFLATCATS";

                case 152:
                    return "GETFLATCAT";

                case 153:
                    return "SETFLATCAT";

                case 154:
                    return "GETSPACENODEUSERS";

                case 155:
                    return "REMOVEALLRIGHTS";

                case 156:
                    return "GETPARENTCHAIN";

                case 157:
                    return "GETAVAILABLEBADGES";

                case 158:
                    return "SETBADGE";

                case 159:
                    return "GET_GAME_LIST";

                case 160:
                    return "START_OBSERVING_GAME";

                case 162:
                    return "GET_CREATE_GAME_INFO";

                case 163:
                    return "CREATE_GAME";

                case 164:
                    return "JOIN_GAME";

                case 165:
                    return "SPECTATE_GAME";

                case 167:
                    return "LEAVE_GAME";

                case 168:
                    return "KICK_USER";

                case 170:
                    return "START_GAME";

                case 171:
                   return "GAME_INTERACT";

                case 172:
                   return "PLAY_AGAIN";

                case 181:
                    return "GET_SESSION_PARAMETERS";

                case 182:
                    return "GETINTERST";

                case 183:
                    return "CONVERT_FURNI_TO_CREDITS";

                case 190:
                    return "SCR_BUY";

                case 191:
                    return "MESSENGER_GETMESSAGES";

                case 196:
                    return "PONG";

                case 197:
                    return "APPROVEEMAIL";

                case 198:
                    return "CHANGECALLCATEGORY";

                case 199:
                    return "MESSAGETOCALLER";

                case 200:
                    return "MODERATORACTION";

                case 201:
                    return "MESSENGER_REPORTMESSAGE";

                case 202:
                    return "GENERATEKEY";

                case 203:
                    return "APPROVE_PASSWORD";

                case 204:
                    return "TRY_SSO_LOGIN";

                case 206:
                    return "INIT_CRYPTO";

                case 213:
                    return "GET_FURNI_REVISIONS";

                case 214:
                    return "SETITEMSTATE";

                case 215:
                    return "GET_ALIAS_LIST";
                    
                case 217:
                    return "SONG_EDIT_LOAD";

                case 218:
                    return "SAVESONG";

                case 219:
                    return "INSERT_SOUND_PACKAGE";

                case 220:
                    return "EJECT_SOUND_PACKAGE";

                case 221:
                    return "GET_SONG_INFO";

                case 230:
                    return "GET_GROUP_BADGES";

                case 233:
                    return "MESSENGER_GETREQUESTS";

                case 246:
                    return "SONG_EDIT_CLOSE";

                case 323:
                    return "CRYFORHELP_PICKUPGO";

                default:
                    return "UNKNOWN";
            }
        }
    }
}
