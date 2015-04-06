//#define DEBUG
using System;
using System.Net.Sockets;
using System.Reflection;

using Woodpecker.Core;
using Woodpecker.Storage;
using Woodpecker.Game;
using Woodpecker.Game.Users;
using Woodpecker.Sessions;
using Woodpecker.Net.Game.Messages;
using Woodpecker.Security.Ciphering;
using Woodpecker.Specialized.Encoding;

namespace Woodpecker.Net.Game
{
    /// <summary>
    /// Represents a game connection of a Habbo client to the server.
    /// </summary>
    public class gameConnection : Reactor
    {
        #region Fields
        /// <summary>
        /// The System.Net.Sockets.Socket object of this connection.
        /// </summary>
        private Socket Socket;
        /// <summary>
        /// The IP address of this connection.
        /// </summary>
        public string ipAddress
        {
            get { return this.Socket.RemoteEndPoint.ToString().Split(':')[0]; }
        }
        /// <summary>
        /// A byte array which contains the data of the incoming message.
        /// </summary>
        private byte[] dataBuffer;
        private int receivedBytesCounter;
        /// <summary>
        /// The total amount of bytes that the server has received from this game connection since the last counter reset.
        /// </summary>
        public int receivedBytes
        {
            get { return this.receivedBytesCounter; }
        }
        /// <summary>
        /// The Woodpecker.Game.reactorHandler object, which keeps and manages all inheritances of the Reactor class.
        /// </summary>
        public reactorHandler reactorHandler;
        /// <summary>
        /// The unique instance of the encryption client for this game connection.
        /// </summary>
        private rc4Provider encryptionClient;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new game connection instance.
        /// </summary>
        /// <param name="Session">The Woodpecker.Sessions.Session object for this connection.</param>
        /// <param name="gameClient">The System.Net.Sockets.Socket object of the game client that has been accepted by the game connection listener.</param>
        public gameConnection(Session Session, Socket gameClient)
        {
            this.Session = Session;            
            this.Socket = gameClient;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if this IP address is banned, if so, the ban is processed, otherwise, the default reactors will be registered and handshake with client is performed..
        /// </summary>
        public void Initialize()
        {
            this.dataBuffer = new byte[1024]; // Max 1024 characters per packet
            this.reactorHandler = new reactorHandler(this.Session);
            this.reactorHandler.Register(new securityReactor());
            this.reactorHandler.Register(new globalReactor());

            this.Socket.BeginReceive(this.dataBuffer, 0, this.dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.dataArrival), null);

            this.Response.Initialize(0); // "@@" (handshake)
            this.sendMessage(Response);

            // Quick ping
            this.Response.Initialize(50); // "@r"
            this.sendMessage(Response);
        }
        /// <summary>
        /// Immediately aborts the connection and releases all resources.
        /// </summary>
        public void Abort()
        {
            try { this.Socket.Close(); }
            catch { }
            this.Session.isValid = false;
            this.Socket = null;
            this.Session = null;
            this.dataBuffer = null;
            this.Request = null;
            this.Response = null;
            this.reactorHandler = null;
        }
        /// <summary>
        /// Resets the received bytes counter to zero.
        /// </summary>
        public void resetReceivedBytesCounter()
        {
            this.receivedBytesCounter = 0;
        }

        #region Normal message sending
        /// <summary>
        /// Sends a message in string format to the client.
        /// </summary>
        /// <param name="Message">The string representing the message. Use char1 to break messages.</param>
        public void sendMessage(string Message)
        {
            try
            {
                byte[] Data = Configuration.charTable.GetBytes(Message);
                this.Socket.Send(Data);
            }
            catch { }
        }
        /// <summary>
        /// Sends a single message in a serverMessage object to the client.
        /// </summary>
        /// <param name="Message">The serverMessage object representing the message.</param>
        public void sendMessage(serverMessage Message)
        {
            Logging.Log("Session " + this.Session.ID + ">> " + Message, Logging.logType.serverMessageEvent);
            this.sendMessage(Message.ToString());
        }
        #endregion

        #region Async message sending
        /// <summary>
        /// Sends a message in string format to the client via an asynchronous System.Net.Sockets.Socket.BeginSend action.
        /// </summary>
        /// <param name="Message">The string object of the message. Use char1 to break messages.</param>
        public void sendAsyncMessage(string Message)
        {
            if (this.Socket == null)
                return;
            
            byte[] Data = Configuration.charTable.GetBytes(Message);
            this.Socket.BeginSend(Data, 0, Data.Length, SocketFlags.None, new AsyncCallback(sentAsyncMessage), null);
        }
        /// <summary>
        /// Sends a single message in a serverMessage object to the client via an asynchronous System.Net.Sockets.Socket.BeginSend action.
        /// </summary>
        /// <param name="Message">The serverMessage object of the message.</param>
        public void sendAsyncMessage(serverMessage Message)
        {
            Logging.Log("Session " + this.Session.ID + ">> " + Message, Logging.logType.serverMessageEvent);
            this.sendMessage(Message.ToString());
        }
        /// <summary>
        /// Ends an asynchronous BeginSend operation.
        /// </summary>
        /// <param name="iAr">The IAsyncResult object which contains the data of the asynchronous operation.</param>
        private void sentAsyncMessage(IAsyncResult iAr)
        {
            if(this.Socket != null)
                this.Socket.EndSend(iAr);
        }
        #endregion
        private void dataArrival(IAsyncResult iAr)
        {
            if (Session == null || !Session.isValid) // Safety above all
                return;

            string Data = "";
            try
            {
                int bytesReceived = this.Socket.EndReceive(iAr);
                this.receivedBytesCounter += bytesReceived;

                Data = Configuration.charTable.GetString(this.dataBuffer, 0, bytesReceived);
                Logging.Log("Session " + this.Session.ID + "<< " + Data, Logging.logType.debugEvent);
                    
                if (this.encryptionClient != null)
                    Data = this.encryptionClient.Decipher(Data);

                while (Data.Length > 0)
                {
                    int v = base64Encoding.Decode(Data.Substring(1, 2)); // Decode length of this message
                    this.Request = new clientMessage(Data.Substring(3, v)); // Get message content and create clientMessage object

                    Logging.Log("Session " + this.Session.ID + "<< [" + this.Request.ID + ": " + this.Request.methodName + "] \"" + this.Request.encodedID + "\", \"" + this.Request.Content + "\"", Logging.logType.clientMessageEvent);
                    this.processMessage();
                    Data = Data.Substring(v + 3);
                }

                if (Session != null && Session.isValid) // Session is still allowed to process messages
                    this.Socket.BeginReceive(this.dataBuffer, 0, this.dataBuffer.Length, SocketFlags.None, new AsyncCallback(this.dataArrival), null);
            }

            catch (ObjectDisposedException) { }
            catch (SocketException)
            {
                if (this.Session != null)
                    Engine.Sessions.destroySession(this.Session.ID);

                return;
            }
            catch (ArgumentOutOfRangeException) // Wrong structured packet
            {
                // Saddam Hussein solution

                //Logging.Log("Session " + this.Session.ID + " sent wrong structured packet '" + Data + "'. Session destroyed and attempt to blacklist IP address sent.", Logging.logType.haxEvent);
                //Engine.Net.Game.blackListIpAddress(this.Session.ipAddress);
                
                // Nah!
                Engine.Sessions.destroySession(this.Session.ID); // Something is wrong with client, weird data?
                return;
            }
            catch (Exception ex)
            {
                Logging.Log("Unhandled exception occurred in dataArrival method of session, stack trace: \r\n" + ex.StackTrace, Logging.logType.commonError);
            }
        }
        private void processMessage()
        {
            if (this.Request.Content.Contains("\x01"))
            {
                Session.isValid = false;
                Engine.Sessions.destroySession(Session.ID);
                Engine.Net.Game.blackListIpAddress(this.ipAddress);
                Logging.Log("Session " + Session.ID + " (ip: " + this.ipAddress + ") sent message with char1 (message breaker), hax! IP address blacklisted.");
                return;
            }

            string requiredMethod = this.Request.methodName;
            foreach (Reactor lReactor in this.reactorHandler.Reactors)
            {
                if (lReactor != null)
                {
                    try
                    {
                        Type t = lReactor.GetType();
                        MethodInfo pMethod = t.GetMethod(requiredMethod);
                        if (pMethod != null) // Target method is inside this reactor
                        {
                            lReactor.setRequest(Request);
                            pMethod.Invoke(lReactor, null);
                            lReactor.unsetRequest();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Log("Unhandled error occurred in " + requiredMethod + " in " + lReactor.GetType().ToString() + ". Error: " + e.InnerException.Message);

                        // Transmit client error report
                        Response.Initialize(299); // "Dk"
                        Response.appendWired(1337); // Server ID
                        Response.appendWired(Request.ID); // Last message ID
                        Response.appendClosedValue(DateTime.Now.ToString());
                        sendResponse();
                        return;
                    }
                }
            }

            // Target method could not be reflected
            if (Request.methodName == "UNKNOWN") // Non-existing packet
            {
                /*
                Logging.Log("Session " + this.Session.ID + " sent message with ID " + Request.ID + ", this message is not present in the packet protocol, hax!", Logging.logType.haxEvent);
                ObjectTree.Net.Game.blackListIpAddress(this.Session.ipAddress);
                ObjectTree.Sessions.destroySession(this.Session.ID); */
            }
            else
                Logging.Log("No target method for client message " + this.Request.ID + " [\"" + Request.encodedID + "\": " + Request.methodName + "] found in any of the registered reactors.", Logging.logType.targetMethodNotFoundEvent);
        }
        /// <summary>
        /// Sends the key of an error, whose description value is inside the external_texts of the client.
        /// </summary>
        /// <param name="localizedKey">The external_texts key of the error description.</param>
        public void sendLocalizedError(string localizedKey)
        {
            serverMessage Message = new serverMessage(33); // "@a"
            Message.Append(localizedKey);
            this.sendMessage(Message);
        }
        /// <summary>
        /// Initializes the RC4 encryption instance of this connection with a given public key.
        /// </summary>
        /// <param name="Key">The public key to set.</param>
        public void initializeEncryption(string Key)
        {
            this.encryptionClient = new rc4Provider(Key);
        }
        #endregion
    }
}
