using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Specialized;
using Woodpecker.Security.Cryptography;

using Woodpecker.Net.Game;
using Woodpecker.Net.Remote;
using Woodpecker.Sessions;

using Woodpecker.Game.Externals;
using Woodpecker.Game.Users;
using Woodpecker.Game.Users.Roles;
using Woodpecker.Game.Store;
using Woodpecker.Game.Messenger;
using Woodpecker.Game.Moderation;
using Woodpecker.Game.Rooms;
using Woodpecker.Game.Items;
using Woodpecker.Game.Arcade;

namespace Woodpecker
{
    /// <summary>
    /// The 'hearth' of Woodpecker, containing references to all kinds of objects.
    /// </summary>
    public static class Engine
    {
        #region Fields
        private static mainForm mMainForm;
        private static sessionManager mSessionManager = new sessionManager();
        private static gameConnectionManager mGameConnectionManager = new gameConnectionManager();
        private static remoteConnectionManager mRemoteConnectionManager = new remoteConnectionManager();
        private static externalManager mExternalManager = new externalManager();
        private static userManager mUserManager = new userManager();
        private static roleManager mRoleManager = new roleManager();
        private static storeManager mStoreManager = new storeManager();
        private static messengerManager mMessengerManager = new messengerManager();
        private static moderationManager mModerationManager = new moderationManager();
        private static roomManager mRoomManager = new roomManager();
        private static itemManager mItemManager = new itemManager();
        private static arcadeManager mArcadeManager = new arcadeManager();
        private static md5Provider mMD5Provider = new md5Provider();
        #endregion

        #region Properties
        public static class Program
        {
            #region Properties
            /// <summary>
            /// A reference to the main form with the user interface.
            /// </summary>
            public static mainForm GUI
            {
                get { return mMainForm; }
                set { mMainForm = value; }
            }
            /// <summary>
            /// The product version of this copy of Woodpecker as a string.
            /// </summary>
            public static string Version
            {
                get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Starts the booting procedure of Woodpecker and handles exceptions.
            /// </summary>
            public static void Start()
            {
                Logging.setDefaultLogSessings();

                Logging.Log("Woodpecker v" + Version);
                Logging.Log(null);

                Logging.Log("Checking db.config...");
                if (!Configuration.configFileExists)
                {
                    Logging.Log("db.config was not found in the same directory as the executable of this copy of Woodpecker!");
                    Stop("db.config not found");
                    return;
                }

                Configuration.createConnectionString();
                Logging.Log("Running database connectivity check...");

                Database Check = new Database(true, true);
                if (Check.Ready)
                {
                    Check.runQuery("INSERT INTO server_startups(startup) VALUES (NOW())"); // Log server startup
                    Check = null;
                    Logging.Log("Database is contactable!");
                }
                else
                {
                    Logging.Log("Failed to contact database, please check the error shown above!", Logging.logType.commonWarning);
                    Stop("failed to contact database");
                    return;
                }
                Logging.Log(null);

                Configuration.loadConfiguration();
                mMD5Provider = new md5Provider();
                mMD5Provider.baseSalt = Configuration.getConfigurationValue("cryptography.md5.salt");
                mExternalManager.loadEntries();

                mRoleManager.loadRoles();
                mMessengerManager.setConfiguration();

                mModerationManager.logChat = (Configuration.getConfigurationValue("moderation.logchat") == "1");
                mItemManager.loadDefinitions();

                mStoreManager.loadSales();
                mStoreManager.loadCataloguePages();

                mRoomManager.initRoomCategories();
                mRoomManager.initRoomModels();

                int Port = Configuration.getNumericConfigurationValue("connections.game.port");
                if (Port > 0)
                {
                    int maxConnectionsPerIP = Configuration.getNumericConfigurationValue("connections.game.maxperip");
                    if (maxConnectionsPerIP == 0)
                    {
                        Logging.Log("The field connections.game.maxperip in the `configuration` table of the database is not holding a valid value. The default value '3' will be used.", Logging.logType.commonWarning);
                        maxConnectionsPerIP = 3;
                    }
                    if (!mGameConnectionManager.startListening(Port, maxConnectionsPerIP))
                    {
                        Configuration.setConfigurationValue("server.status", "offline");
                        Logging.Log("Failed to setup game connection listener, the port (" + Port + ") is probably in use by another application.", Logging.logType.commonWarning);
                        Stop("failed to setup game connection listener");
                        return;
                    }
                }
                else
                {
                    Configuration.setConfigurationValue("server.status", "offline");
                    Logging.Log("The field 'connections.game.port' in the `configuration` table of the database is not present. Please fill in a numeric entry, which represents the port to listen on for game connections.", Logging.logType.commonWarning);
                    Stop("No valid port for game connection listener set.");
                    return;
                }

                Port = 3080;
                if (Port > 0)
                {
                    bool allowExternalHosts = false;
                    if (!mRemoteConnectionManager.startListening(Port, allowExternalHosts))
                    {
                        Configuration.setConfigurationValue("server.status", "offline");
                        Logging.Log("Failed to setup remote connection listener, the port (" + Port + ") is probably in use by another application.", Logging.logType.commonWarning);
                        Stop("failed to setup remote connection listener");
                        return;
                    }
                }

                mSessionManager.startPingChecker();
                mSessionManager.startReceivedBytesChecker();
                Game.startUpdater();

                Logging.Log(null);
                Logging.Log("Ready.");
                Configuration.setConfigurationValue("server.status", "online");
                mMainForm.logSacredText(null);
                mMainForm.stripMenu.Enabled = true;
            }
            /// <summary>
            /// Sets Woodpecker in the 'allowed to close window'-mode, after shutting down all other tasks etc. A message is being displayed with the reason of shutdown.
            /// </summary>
            /// <param name="Message">The reason of the shutdown.</param>
            public static void Stop(string Message)
            {
                Configuration.setConfigurationValue("server.status", "offline");
                Logging.Log("Shutting down, reason: " + Message);
                mGameConnectionManager.stopListening();
                mSessionManager.stopPingChecker();
                mSessionManager.stopMessageSizeChecker();
                mSessionManager.Clear();
                //_roomManager.saveInstances();
                Game.stopUpdater();

                mMainForm.stripMenu.Enabled = false;
                mMainForm.mShutdown = true;
                Logging.Log("Woodpecker safely shutdown! You can now close the application.");
            }
            #endregion
        }
        /// <summary>
        /// Provides various functions for enciphering and hashing data.
        /// </summary>
        public static class Security
        {
            /// <summary>
            /// Provides functions for hashing data.
            /// </summary>
            public static class Cryptography
            {
                public static md5Provider MD5
                {

                    get { return mMD5Provider; }
                }
            }
            /// <summary>
            /// Provides functions for ciphering data and generating public keys. Also features a factory for creating RC4 classes.
            /// </summary>
            public static class Ciphering
            {
                /// <summary>
                /// Generates a total random public key with a random length for the RC4 ciphering, containing a-z and 0-9 and returns it as a string.
                /// </summary>
                public static string generateKey()
                {
                    int keyLength = new Random(DateTime.Now.Millisecond).Next(60, 65);
                    Random v = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + keyLength);
                    StringBuilder sb = new StringBuilder(keyLength);

                    for (int i = 0; i < keyLength; i++)
                    {
                        int j = 0;
                        if (v.Next(0, 2) == 1)
                            j = v.Next(97, 123);
                        else
                            j = v.Next(48, 58);
                        sb.Append((char)j);
                    }

                    return sb.ToString();
                }
            }
        }
        /// <summary>
        /// Provides various functions regarding player sessions.
        /// </summary>
        public static sessionManager Sessions
        {
            get { return mSessionManager; }
        }

        /// <summary>
        /// Provides various functions for connections.
        /// </summary>
        public static class Net
        {
            /// <summary>
            /// Provides various functions for game connections.
            /// </summary>
            public static gameConnectionManager Game
            {
                get { return mGameConnectionManager; }
            }
            /// <summary>
            /// Provides various functions for remote connections.
            /// </summary>
            public static remoteConnectionManager Remote
            {
                get { return mRemoteConnectionManager; }
            }
        }
        /// <summary>
        /// Provides management for all kinds of in-game related things.
        /// </summary>
        public static class Game
        {
            #region Properties
            private static Thread mUpdateWorker;
            /// <summary>
            /// Provides management for cached external texts.
            /// </summary>
            public static externalManager Externals
            {
                get { return mExternalManager; }
            }
            /// <summary>
            /// Provides management and functions for logged in users, user data etc.
            /// </summary>
            public static userManager Users
            {
                get { return mUserManager; }
            }
            /// <summary>
            /// Provides management and functions for user roles, such as rights, badges etc.
            /// </summary>
            public static roleManager Roles
            {
                get { return mRoleManager; }
            }
            /// <summary>
            /// Contains various functions and fields for the catalogue and it's items, as well as other financial things.
            /// </summary>
            public static storeManager Store
            {
                get { return mStoreManager; }
            }
            /// <summary>
            /// Provides various functions for the in-game messenger ('Console') for sending messages to users, adding users to buddylist etc. Also features a postmaster.
            /// </summary>
            public static messengerManager Messenger
            {
                get { return mMessengerManager; }
            }
            /// <summary>
            /// Provides all kinds of functions for moderation in the virtual hotel, eg, bans etc.
            /// </summary>
            public static moderationManager Moderation
            {
                get { return mModerationManager; }
            }
            /// <summary>
            /// Provides all kinds of functions for rooms, room categories and the navigator.
            /// </summary>
            public static roomManager Rooms
            {
                get { return mRoomManager; }
            }
            public static itemManager Items
            {
                get { return mItemManager; }
            }
            public static arcadeManager Arcade
            {
                get { return mArcadeManager; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Attempts to start the game updater worker thread.
            /// </summary>
            public static void startUpdater()
            {
                if(mUpdateWorker == null)
                {
                    mUpdateWorker = new Thread(new ThreadStart(Updater));
                    mUpdateWorker.Start();
                    Logging.Log("Game update worker started.");
                    Rooms.resetRoomUserAmounts();
                }
            }
            /// <summary>
            /// Stops the game updater worker thread.
            /// </summary>
            public static void stopUpdater()
            {
                if (mUpdateWorker != null)
                {
                    mUpdateWorker.Abort();
                    mUpdateWorker = null;
                    Logging.Log("Game update worker stopped.");
                }
            }
            /// <summary>
            /// An infinite loop at a configured interval, updating various things in game.
            /// </summary>
            private static void Updater()
            {
                while (true)
                {
                    Moderation.dropExpiredBans();
                    Messenger.Postmaster.dropInvalidMessages();
                    Rooms.dropInvalidFavoriteRoomEntries();
                    Rooms.destroyInactiveRoomInstances();

                    GC.Collect(); // Force garbage collecting
                    Thread.Sleep(3 * 60000); // 3 minutes interval
                }
            }
            #endregion
        }
        #endregion
    }
}
